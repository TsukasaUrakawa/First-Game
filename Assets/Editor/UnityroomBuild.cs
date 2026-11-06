using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Reproducible release files for unityroom, with a readable local build report.</summary>
[InitializeOnLoad]
public static class UnityroomBuild
{
    public const string OutputDirectory = "Builds/Unityroom/ShelfCare";
    public const string StatusPath = ".utmp/unityroom-build-status.json";
    private static readonly string[] Scenes = { "Assets/Scenes/TitleScene.unity", "Assets/Scenes/ShelfCare.unity" };
    private const string PendingKey = "ShelfCare.UnityroomBuild.Pending";
    static UnityroomBuild() { EditorApplication.update += BuildWhenReady; }

    [MenuItem("Tools/Shelf Care/Prepare unityroom Settings")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before preparing a build.");
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.nameFilesAsHashes = false;
        PlayerSettings.WebGL.initialMemorySize = 128;
        if (EditorUserBuildSettings.webGLBuildSubtarget != WebGLTextureSubtarget.Generic)
            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.Generic;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.connectProfiler = false;
        Debug.Log("unityroom: Gzip, no fallback/development build, 960 x 540.");
    }

    [MenuItem("Tools/Shelf Care/Build for unityroom")]
    public static void QueueBuild()
    {
        if (SessionState.GetBool(PendingKey, false) || BuildPipeline.isBuildingPlayer) throw new InvalidOperationException("A build is already running.");
        if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
            throw new InvalidOperationException("Wait for the Editor to finish loading, then stop Play.");
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            throw new InvalidOperationException("Switch the active Build Profile to Web before building.");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save your scene changes before building.");
        if (Scenes.Any(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null))
            throw new InvalidOperationException("The title/gameplay scene is missing.");
        // Pipeline is useful in the Editor, but its runtime instrumentation is not a release dependency.
        var symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WebGL).Split(';');
        if (symbols.Any(s => s.Trim() == "ENABLE_RUNTIME_PIPELINE" || s.Trim() == "ENABLE_RUNTIME_PIPELINE_DEV"))
            throw new InvalidOperationException("Disable Pipeline runtime instrumentation for the Web release.");
        Configure();
        SessionState.SetBool(PendingKey, true);
        WriteStatus(new Status { status = "queued" });
        EditorApplication.QueuePlayerLoopUpdate();
    }

    private static void BuildWhenReady()
    {
        if (!SessionState.GetBool(PendingKey, false) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying || BuildPipeline.isBuildingPlayer) return;
        SessionState.SetBool(PendingKey, false);
        Build();
    }

    private static void Build()
    {
        string editorProduct = PlayerSettings.productName;
        var status = new Status { status = "building", startedUtc = DateTime.UtcNow.ToString("o") };
        WriteStatus(status);
        try
        {
            // Brand only the player. Keep the Editor's existing PlayerPrefs namespace intact.
            PlayerSettings.productName = "Shelf Care";
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = OutputDirectory,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                subtarget = (int)WebGLTextureSubtarget.Generic,
                options = BuildOptions.DetailedBuildReport
            });
            status.seconds = report.summary.totalTime.TotalSeconds;
            status.bytes = (long)report.summary.totalSize;
            status.warnings = (int)report.summary.totalWarnings;
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Web build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors).");
            status.files = VerifyArtifacts(OutputDirectory);
            status.status = "succeeded";
            File.WriteAllText(Path.Combine(OutputDirectory, "unityroom-files.json"), JsonUtility.ToJson(status, true));
            Debug.Log("unityroom release ready: " + Path.GetFullPath(OutputDirectory));
        }
        catch (Exception ex)
        {
            status.status = "failed"; status.error = ex.Message;
            Debug.LogException(ex);
        }
        finally
        {
            PlayerSettings.productName = editorProduct;
            AssetDatabase.SaveAssets();
            WriteStatus(status);
        }
    }

    public static UploadFile[] VerifyArtifacts(string directory)
    {
        string prefix = Path.Combine(directory, "Build", "ShelfCare");
        return new[] { ".data.gz", ".framework.js.gz", ".wasm.gz" }.Select(suffix =>
        {
            string path = prefix + suffix;
            if (!File.Exists(path)) throw new FileNotFoundException("Missing unityroom upload file.", path);
            using (var input = File.OpenRead(path))
            {
                if (input.ReadByte() != 0x1f || input.ReadByte() != 0x8b) throw new InvalidDataException("Not Gzip: " + path);
                input.Position = 0;
                using (var gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    if (suffix == ".wasm.gz")
                    {
                        var header = new byte[8];
                        int read = 0;
                        while (read < header.Length) { int n = gzip.Read(header, read, header.Length - read); if (n == 0) break; read += n; }
                        if (read != 8 || !header.SequenceEqual(new byte[] { 0, 97, 115, 109, 1, 0, 0, 0 }))
                            throw new InvalidDataException("Invalid WebAssembly module: " + path);
                    }
                    gzip.CopyTo(Stream.Null); // Also reject truncated/corrupt compressed output.
                }
            }
            using (var sha = SHA256.Create()) using (var input = File.OpenRead(path))
                return new UploadFile { path = "Build/ShelfCare" + suffix, bytes = input.Length,
                    sha256 = BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant() };
        }).ToArray();
    }

    private static void WriteStatus(Status status)
    {
        Directory.CreateDirectory(".utmp");
        File.WriteAllText(StatusPath, JsonUtility.ToJson(status, true));
    }
    [Serializable] private sealed class Status
    {
        public string status, error, startedUtc;
        public string output = OutputDirectory;
        public string product = "Shelf Care";
        public string unityVersion = Application.unityVersion;
        public double seconds;
        public long bytes;
        public int warnings;
        public UploadFile[] files;
    }
    [Serializable] public sealed class UploadFile { public string path, sha256; public long bytes; }
}
