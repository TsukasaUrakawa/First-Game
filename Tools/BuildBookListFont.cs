// Run in Edit mode: unity command script eval_file --file Tools/BuildBookListFont.cs
// Bakes the UI corpus and printable ASCII; the full source font stays editor-only.
if (UnityEditor.EditorApplication.isPlaying)
    throw new System.InvalidOperationException("Stop Play mode before rebuilding the font.");

const string assetPath = "Assets/Fonts/BookList/BookListJapanese.asset";
var source = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/Fonts/BookList/Editor/ZenMaruGothic-Medium.ttf");
if (source == null) throw new System.Exception("Import the editor source font first.");
string corpus = System.IO.File.ReadAllText("Assets/Fonts/BookList/Characters.txt");
corpus += new string(Enumerable.Range(32, 95).Select(c => (char)c).ToArray());
corpus = new string(corpus.Where(c => !char.IsControl(c)).Distinct().ToArray());
var font = TMPro.TMP_FontAsset.CreateFontAsset(source, 48, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, TMPro.AtlasPopulationMode.Dynamic, false);
if (!font.TryAddCharacters(corpus, out string missing))
    throw new System.Exception("Font is missing characters or atlas space: " + missing);
font.name = "BookListJapanese";
font.atlasPopulationMode = TMPro.AtlasPopulationMode.Static;
var serializedFont = new UnityEditor.SerializedObject(font);
serializedFont.FindProperty("m_ClearDynamicDataOnBuild").boolValue = false;
serializedFont.ApplyModifiedPropertiesWithoutUndo();
font.atlasTextures[0].name = "BookListJapanese Atlas";
font.material.name = "BookListJapanese Material";
// Bake the same padding ratios TMP computes on first display so using the
// generated font does not dirty its material after it has been committed.
TMPro.ShaderUtilities.UpdateShaderRatios(font.material);

// Preserve the existing GUID and subasset references when rebuilding.
var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(assetPath);
if (existing == null)
{
    UnityEditor.AssetDatabase.CreateAsset(font, assetPath);
    UnityEditor.AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
    UnityEditor.AssetDatabase.AddObjectToAsset(font.material, font);
}
else
{
    var texture = font.atlasTextures[0];
    var material = font.material;
    UnityEditor.EditorUtility.CopySerialized(texture, existing.atlasTextures[0]);
    UnityEditor.EditorUtility.CopySerialized(material, existing.material);
    font.atlasTextures = new[] { existing.atlasTextures[0] };
    font.material = existing.material;
    font.material.mainTexture = font.atlasTextures[0];
    UnityEditor.EditorUtility.CopySerialized(font, existing);
    // TMP_FontAsset.OnDestroy owns its material and atlas textures. Restore
    // temporary resources before destroying it so saved subassets stay intact.
    font.atlasTextures = new[] { texture };
    font.material = material;
    UnityEngine.Object.DestroyImmediate(font);
    font = existing;
}
UnityEditor.EditorUtility.SetDirty(font);
UnityEditor.EditorUtility.SetDirty(font.material);
UnityEditor.EditorUtility.SetDirty(font.atlasTextures[0]);
UnityEditor.AssetDatabase.SaveAssets();
return new { assetPath, characters = font.characterTable.Count, mode = font.atlasPopulationMode.ToString(), runtimeSourceFont = font.sourceFontFile != null };
