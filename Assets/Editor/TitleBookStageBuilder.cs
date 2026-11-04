using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>Bakes editable, ordinary hardbacks and title-only printed geometry into TitleScene.</summary>
public static class TitleBookStageBuilder
{
    private const string Folder = "Assets/Presentation/TitleBooks";
    private const string MeshPath = Folder + "/BookMeshes.asset";
    private const int BookCount = 26;
    private const float Width = .40f, Height = 2.70f, Depth = 1.40f;
    private static readonly int[] Palette = { 0, 0, 1, 3, 0, 2, 0, 1 };

    [MenuItem("Tools/Shelf Care/Rebuild Title Book Sculpture")]
    public static void Rebuild()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/TitleScene.unity" || scene.isDirty)
            throw new System.InvalidOperationException("Open the saved TitleScene in Edit mode first.");
        System.IO.Directory.CreateDirectory("Assets/.utmp");
        EditorSceneManager.SaveScene(scene, "Assets/.utmp/before-kinetic-title.unity", true);
        EnsureFolder("Assets", "Presentation");
        EnsureFolder("Assets/Presentation", "TitleBooks");

        var grain = ClothGrain();
        var cloth = new[] {
            Material("ClothInk", "#173A40", .05f, .20f, grain),
            Material("ClothVermilion", "#913E29", .02f, .18f, grain),
            Material("ClothOat", "#D9C7A2", .02f, .20f, grain),
            Material("ClothOchre", "#82603C", .03f, .22f, grain)
        };
        var paper = Material("Paper", "#EADDC2", 0, .15f);
        var edges = Material("PageEdges", "#9E8A66", 0, .1f);
        var copper = Material("Copper", "#D9A66B", .30f, .48f, grain);
        var ivory = Material("IvoryInk", "#F4DFB1", .05f, .38f, grain);
        var floorMat = Material("StageFloor", "#102A30", .12f, .30f, grain);
        // Unity names a main asset after its file; reuse that name on subsequent builds.
        var body = SaveMesh(Body(), "BookMeshes");

        var previous = GameObject.Find("TitleBookStage");
        if (previous != null) Object.DestroyImmediate(previous);
        var stage = new GameObject("TitleBookStage");
        var composition = Child("Composition", stage.transform);
        var orbit = Child("Books", composition);
        var books = new Transform[BookCount];
        for (int i = 0; i < BookCount; i++)
        {
            float t = i / (float)(BookCount - 1);
            var rotation = Quaternion.identity;
            // Ordinary rectangular volumes can touch along a stepped wave without
            // the wedge-shaped covers a tightly packed radial fan would require.
            var position = new Vector3(Mathf.Lerp(-5.18f, 5.18f, t),
                .7f + 1.6f * Mathf.Sin(t * Mathf.PI * 2 + .3f), 0);
            var book = Child("TitleBook" + (i + 1).ToString("00"), orbit);
            book.localPosition = position;
            book.localRotation = rotation;
            books[i] = book;
            MeshObject("BindingAndPages", book, body, new[] { cloth[PaletteIndex(i)], paper, copper, edges });
            MeshObject("PrintedPattern", book, SaveMesh(Pattern(position, rotation), "Pattern" + i.ToString("00")), new[] { copper, ivory });
        }

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "StageFloor";
        Object.DestroyImmediate(floor.GetComponent<Collider>());
        floor.transform.SetParent(composition, false);
        floor.transform.localPosition = new Vector3(0, -5.3f, 0);
        floor.transform.localScale = new Vector3(60, .12f, 60);
        floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

        var camera = Camera.main;
        camera.transform.position = new Vector3(-4.3f, 4.6f, -19.5f);
        camera.transform.LookAt(Vector3.zero);
        camera.orthographic = true;
        camera.orthographicSize = 5.9f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ColorOf("#0D242A");
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 100;
        var cameraData = camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
        var animation = stage.AddComponent<TitleBookSculpture>();
        Reference(animation, "_camera", camera);
        Reference(animation, "_composition", composition);
        Reference(animation, "_orbit", orbit);
        var serialized = new SerializedObject(animation);
        var array = serialized.FindProperty("_books"); array.arraySize = books.Length;
        for (int i = 0; i < books.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = books[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
        animation.Frame(camera.aspect);

        var key = Named("Directional Light").GetComponent<Light>();
        key.transform.rotation = Quaternion.Euler(32, -28, 0);
        key.color = ColorOf("#FFE8C3"); key.intensity = 2.1f;
        key.shadows = LightShadows.Soft; key.shadowBias = .03f; key.shadowNormalBias = .12f;
        LightObject("CoolFill", stage.transform, new Vector3(-6, 3, -7), ColorOf("#A5D0D2"), 18, LightType.Point);
        LightObject("CopperRim", stage.transform, new Vector3(8, 6, 2), ColorOf("#FFCA87"), 40, LightType.Point);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ColorOf("#728B91");
        RenderSettings.ambientEquatorColor = ColorOf("#3F535B");
        RenderSettings.ambientGroundColor = ColorOf("#18282E");
        // Fade the distant floor into the stage instead of exposing a hard horizon.
        floor.transform.localScale = new Vector3(400, .12f, 400);
        camera.farClipPlane = 300;
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = ColorOf("#0D242A"); RenderSettings.fogDensity = .035f;

        ConfigureUI();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(MeshPath)) AssetDatabase.SaveAssetIfDirty(asset);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Title sculpture rebuilt: 26 rigid books, shared cloth/paper materials and independent title artwork.");
    }

    private static int PaletteIndex(int i) => Palette[i % Palette.Length];
    private static Color ColorOf(string hex) { ColorUtility.TryParseHtmlString(hex, out var color); return color; }
    private static GameObject Named(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
    private static Transform Child(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
    private static void EnsureFolder(string parent, string child) { if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child); }
    private static void Reference(Object target, string field, Object value) { var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }

    private static Material Material(string name, string hex, float metallic, float smoothness, Texture grain = null)
    {
        string path = Folder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", ColorOf(hex));
        material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness);
        material.SetTexture("_BaseMap", grain);
        material.SetTextureScale("_BaseMap", grain == null ? Vector2.one : new Vector2(3, 10));
        material.enableInstancing = true;
        EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        return material;
    }

    private static Texture2D ClothGrain()
    {
        // A tiny procedural weave, not a photographic or gameplay-pattern texture.
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true) { name = "ClothGrain", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            int grain = (x * 73856093 ^ y * 19349663) & 7;
            byte value = (byte)(224 + grain + ((x + y / 2) % 4 == 0 ? -10 : 5) + (y % 4 == 0 ? -8 : 3));
            pixels[y * size + x] = new Color32(value, value, value, 255);
        }
        texture.SetPixels32(pixels); texture.Apply(true, false);
        string path = Folder + "/ClothGrain.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing == null) { AssetDatabase.CreateAsset(texture, path); existing = texture; }
        else { EditorUtility.CopySerialized(texture, existing); Object.DestroyImmediate(texture); EditorUtility.SetDirty(existing); }
        AssetDatabase.SaveAssetIfDirty(existing); return existing;
    }

    private static Mesh SaveMesh(Mesh mesh, string name)
    {
        mesh.name = name;
        var existing = AssetDatabase.LoadAllAssetsAtPath(MeshPath).OfType<Mesh>().SingleOrDefault(m => m.name == name);
        if (existing != null)
        {
            // Update through Mesh's native API as well as its serialized data. This
            // also refreshes the renderer's vertex/index buffers when rebuilding.
            existing.Clear(); existing.vertices = mesh.vertices; existing.normals = mesh.normals; existing.uv = mesh.uv;
            existing.subMeshCount = mesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++) existing.SetTriangles(mesh.GetTriangles(i), i);
            existing.RecalculateBounds(); existing.UploadMeshData(false);
            Object.DestroyImmediate(mesh); EditorUtility.SetDirty(existing); return existing;
        }
        if (AssetDatabase.LoadMainAssetAtPath(MeshPath) == null) AssetDatabase.CreateAsset(mesh, MeshPath);
        else AssetDatabase.AddObjectToAsset(mesh, MeshPath);
        return mesh;
    }

    private static void MeshObject(string name, Transform parent, Mesh mesh, Material[] materials)
    {
        var go = Child(name, parent).gameObject;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterials = materials;
        renderer.shadowCastingMode = name == "PrintedPattern" ? ShadowCastingMode.Off : ShadowCastingMode.On;
    }

    private static void LightObject(string name, Transform parent, Vector3 position, Color color, float intensity, LightType type)
    {
        var light = Child(name, parent).gameObject.AddComponent<Light>();
        light.transform.position = position; light.type = type; light.color = color;
        light.intensity = intensity; light.range = 30; light.shadows = LightShadows.None;
    }

    private static Mesh Body()
    {
        var b = new Geometry(4);
        // Identical, parallel rectangular covers around a rectangular paper block.
        b.Box(new Vector3(-Width / 2 + .018f, 0, 0), new Vector3(.036f, Height + .06f, Depth + .10f), 0);
        b.Box(new Vector3(Width / 2 - .018f, 0, 0), new Vector3(.036f, Height + .06f, Depth + .10f), 0);
        b.Box(new Vector3(0, 0, .022f), new Vector3(Width - .062f, Height - .09f, Depth - .03f), 1);
        b.Box(new Vector3(0, 0, -Depth / 2 - .020f), new Vector3(Width - .02f, Height + .018f, .065f), 0);
        for (int sign = -1; sign <= 1; sign += 2)
            b.Box(new Vector3(0, sign * (Height / 2 - .11f), -.758f), new Vector3(Width - .06f, .012f, .004f), 2);
        // Fine page separations on the head, foot and fore edge. They are flat printed lines.
        for (int i = 1; i < 13; i++)
        {
            float x = Mathf.Lerp(-Width / 2 + .04f, Width / 2 - .04f, i / 13f);
            b.Quad(new Vector3(x, Height / 2 - .044f, -.65f), new Vector3(x, Height / 2 - .044f, .70f),
                new Vector3(x + .002f, Height / 2 - .044f, .70f), new Vector3(x + .002f, Height / 2 - .044f, -.65f), Vector3.up, 3);
            b.Quad(new Vector3(x, -Height / 2 + .044f, -.65f), new Vector3(x + .002f, -Height / 2 + .044f, -.65f),
                new Vector3(x + .002f, -Height / 2 + .044f, .70f), new Vector3(x, -Height / 2 + .044f, .70f), Vector3.down, 3);
            b.Quad(new Vector3(x, -Height / 2 + .05f, .708f), new Vector3(x + .002f, -Height / 2 + .05f, .708f),
                new Vector3(x + .002f, Height / 2 - .05f, .708f), new Vector3(x, Height / 2 - .05f, .708f), Vector3.forward, 3);
        }
        return b.Mesh();
    }

    private static Vector2 Polar(float radius, float angle) => new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;

    private static Mesh Pattern(Vector3 position, Quaternion rotation)
    {
        var geometry = new Geometry(2);
        // A title-only faceted pinwheel. Clip the same global artwork onto each rigid spine.
        // The artwork is baked into book-local coordinates, so it travels with that volume.
        void Print(int ink, params Vector2[] polygon)
        {
            // Clip in the artwork's flat coordinates, then follow the wave at the
            // exact left/right edge of each spine, so neighboring strokes meet.
            var strip = Clip(polygon.ToList(), 0, position.x - Width / 2 + .012f, true);
            strip = Clip(strip, 0, position.x + Width / 2 - .012f, false);
            var points = strip.Select(p => {
                float t = (p.x + 5.18f) / 10.36f;
                float wave = .7f + 1.6f * Mathf.Sin(t * Mathf.PI * 2 + .3f);
                return (Vector2)(Quaternion.Inverse(rotation) * (new Vector3(p.x, p.y + wave, 0) - position));
            }).ToList();
            points = Clip(points, 0, -Width / 2 + .012f, true);
            points = Clip(points, 0, Width / 2 - .012f, false);
            points = Clip(points, 1, -Height / 2 + .16f, true);
            points = Clip(points, 1, Height / 2 - .16f, false);
            geometry.Polygon(points, -.756f, ink);
        }
        for (int i = 0; i < 9; i++)
        {
            float a = 7 + i * 40;
            Print(1, Polar(.10f, a + 39), Polar(6.4f, a), Polar(6.4f, a + 12));
            Print(0, Polar(.15f, a + 40), Polar(6.4f, a + 16), Polar(6.4f, a + 20));
            for (int j = 0; j < 3; j++)
                Print(0, Polar(2.3f, a + 43 + j * 2), Polar(6.4f, a + 25 + j * 2),
                    Polar(6.4f, a + 26 + j * 2), Polar(2.3f, a + 44 + j * 2));
        }
        Print(0, new Vector2(-6, -.91f), new Vector2(6, -.91f), new Vector2(6, -.88f), new Vector2(-6, -.88f));
        Print(0, new Vector2(-6, .88f), new Vector2(6, .88f), new Vector2(6, .91f), new Vector2(-6, .91f));
        return geometry.Mesh();
    }

    private static List<Vector2> Clip(List<Vector2> input, int axis, float edge, bool minimum)
    {
        var output = new List<Vector2>();
        if (input.Count == 0) return output;
        bool Inside(Vector2 p) => minimum ? p[axis] >= edge : p[axis] <= edge;
        Vector2 previous = input[input.Count - 1]; bool wasInside = Inside(previous);
        foreach (var point in input)
        {
            bool inside = Inside(point);
            if (inside != wasInside) output.Add(Vector2.Lerp(previous, point, (edge - previous[axis]) / (point[axis] - previous[axis])));
            if (inside) output.Add(point);
            previous = point; wasInside = inside;
        }
        return output;
    }

    private sealed class Geometry
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uv = new List<Vector2>();
        private readonly List<int>[] _triangles;
        public Geometry(int materials) { _triangles = Enumerable.Range(0, materials).Select(_ => new List<int>()).ToArray(); }
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, int material)
        {
            int start = _vertices.Count;
            _vertices.AddRange(new[] { a, b, c, d });
            for (int i = 0; i < 4; i++) _normals.Add(normal);
            _uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
            bool forward = Vector3.Dot(Vector3.Cross(b - a, c - a), normal) > 0;
            _triangles[material].AddRange(forward ? new[] { start, start + 1, start + 2, start, start + 2, start + 3 } : new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
        }
        public void Box(Vector3 center, Vector3 size, int material)
        {
            var min = center - size / 2; var max = center + size / 2;
            Quad(new Vector3(min.x,min.y,min.z),new Vector3(min.x,max.y,min.z),new Vector3(max.x,max.y,min.z),new Vector3(max.x,min.y,min.z),Vector3.back,material);
            Quad(new Vector3(min.x,min.y,max.z),new Vector3(max.x,min.y,max.z),new Vector3(max.x,max.y,max.z),new Vector3(min.x,max.y,max.z),Vector3.forward,material);
            Quad(new Vector3(min.x,min.y,min.z),new Vector3(min.x,min.y,max.z),new Vector3(min.x,max.y,max.z),new Vector3(min.x,max.y,min.z),Vector3.left,material);
            Quad(new Vector3(max.x,min.y,min.z),new Vector3(max.x,max.y,min.z),new Vector3(max.x,max.y,max.z),new Vector3(max.x,min.y,max.z),Vector3.right,material);
            Quad(new Vector3(min.x,max.y,min.z),new Vector3(min.x,max.y,max.z),new Vector3(max.x,max.y,max.z),new Vector3(max.x,max.y,min.z),Vector3.up,material);
            Quad(new Vector3(min.x,min.y,min.z),new Vector3(max.x,min.y,min.z),new Vector3(max.x,min.y,max.z),new Vector3(min.x,min.y,max.z),Vector3.down,material);
        }
        public void Polygon(List<Vector2> points, float z, int material)
        {
            if (points.Count < 3) return;
            int start = _vertices.Count;
            foreach (var p in points) { _vertices.Add(new Vector3(p.x, p.y, z)); _normals.Add(Vector3.back); _uv.Add(new Vector2(p.x, p.y)); }
            float winding = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 a = points[i], b = points[(i + 1) % points.Count];
                winding += a.x * b.y - b.x * a.y;
            }
            for (int i = 1; i < points.Count - 1; i++)
                _triangles[material].AddRange(winding < 0 ? new[] { start, start + i, start + i + 1 } : new[] { start, start + i + 1, start + i });
        }
        public Mesh Mesh()
        {
            var mesh = new Mesh(); mesh.SetVertices(_vertices); mesh.SetNormals(_normals); mesh.SetUVs(0, _uv);
            mesh.subMeshCount = _triangles.Length;
            for (int i = 0; i < _triangles.Length; i++) mesh.SetTriangles(_triangles[i], i);
            mesh.RecalculateBounds(); return mesh;
        }
    }

    private static void ConfigureUI()
    {
        Color ivory = ColorOf("#F5E6C7"), secondary = ColorOf("#BEC7BC");
        var background = Named("Background");
        var image = background.GetComponent<Image>(); if (image != null) Object.DestroyImmediate(image);
        var backdrop = background.GetComponent<TitleBackdropGraphic>() ?? background.AddComponent<TitleBackdropGraphic>();
        backdrop.color = ColorOf("#091C22"); backdrop.raycastTarget = false;
        var brand = Named("TitleBrand").GetComponent<RectTransform>();
        brand.anchorMin = brand.anchorMax = new Vector2(.205f, .64f);
        brand.sizeDelta = new Vector2(580, 460);
        foreach (string name in new[] { "TitleEyebrow", "TitleHeadline", "TitleTagline" })
        {
            var text = Named(name).GetComponent<TMP_Text>();
            text.rectTransform.sizeDelta = new Vector2(580, text.rectTransform.sizeDelta.y);
            text.color = name == "TitleHeadline" ? ivory : secondary;
        }
        Named("TitleEyebrow").GetComponent<TMP_Text>().color = ColorOf("#CBAA7C");
        Named("TitleTagline").GetComponent<TMP_Text>().fontSize = 27;
        var summary = Named("SaveSummary").GetComponent<TMP_Text>();
        summary.rectTransform.anchorMin = summary.rectTransform.anchorMax = new Vector2(.205f, .39f);
        summary.rectTransform.sizeDelta = new Vector2(570, 70); summary.color = secondary;
        foreach (string name in new[] { "StartButton", "NewGameButton", "QuitButton" })
        {
            var button = Named(name).GetComponent<Button>();
            var rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.205f, rect.anchorMin.y);
            var surface = button.GetComponent<RoundedPanelGraphic>();
            surface.color = name == "StartButton" ? ivory : ColorOf("#244249"); surface.CornerRadius = 7;
            button.GetComponentInChildren<TMP_Text>(true).color = name == "StartButton" ? ColorOf("#16313A") : ivory;
            var shadow = button.GetComponent<Shadow>(); if (shadow != null) { shadow.effectColor = new Color(0, 0, 0, .20f); shadow.effectDistance = new Vector2(0, -3); }
            var colors = button.colors; colors.highlightedColor = ColorOf("#E1C49A"); colors.pressedColor = ColorOf("#BC9F79"); colors.selectedColor = Color.white; button.colors = colors;
        }
        var settings = Named("OpenSettingsButton").GetComponent<Button>();
        settings.GetComponent<RoundedPanelGraphic>().color = ColorOf("#244249");
        settings.GetComponentInChildren<TMP_Text>(true).color = ivory;
        Named("FadePanel").GetComponent<Image>().color = ColorOf("#091C22");
        var ambient = Object.FindFirstObjectByType<AmbientGeometryGraphic>();
        var so = new SerializedObject(ambient); so.FindProperty("_moteCount").intValue = 8; so.FindProperty("_ornamentAlpha").floatValue = 0; so.ApplyModifiedPropertiesWithoutUndo();
        ambient.color = new Color(.8f, .65f, .4f, .20f);
    }
}
