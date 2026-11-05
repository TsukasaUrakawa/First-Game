// Edit mode. Verify real book geometry and exact reuse of all original puzzle artwork.
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Edit mode.");
int checks = 0;
void Check(bool ok, string message) { if (!ok) throw new System.Exception("FAIL: " + message); checks++; }
const string folder = "Assets/Presentation/Gameplay3D/";
var library = UnityEditor.AssetDatabase.LoadAssetAtPath<BookVisualLibrary>(folder + "BookVisualLibrary.asset");
Check(library != null, "3D library exists");
var so = new UnityEditor.SerializedObject(library); var entries = so.FindProperty("_spines");
Check(entries.arraySize == 140, "All 140 spine identities remain available");
var textures = new System.Collections.Generic.HashSet<Texture>();
for (int i = 0; i < entries.arraySize; i++)
{
    var entry = entries.GetArrayElementAtIndex(i);
    var sprite = (Sprite)entry.FindPropertyRelative("Sprite").objectReferenceValue;
    var mesh = (Mesh)entry.FindPropertyRelative("Mesh").objectReferenceValue;
    int color = entry.FindPropertyRelative("ColorIndex").intValue;
    Check(sprite != null && BookColorUtility.TryGetBookIndex(sprite.name, out int index) && index == i, "Original identity " + i);
    Check(mesh != null && mesh.vertexCount == 4 && mesh.triangles.Length == 6, "Baked spine quad " + sprite.name);
    var actual = mesh.uv; var expected = sprite.uv;
    Check(Mathf.Abs(actual.Min(p => p.x) - expected.Min(p => p.x)) < .000001f && Mathf.Abs(actual.Max(p => p.x) - expected.Max(p => p.x)) < .000001f &&
        Mathf.Abs(actual.Min(p => p.y) - expected.Min(p => p.y)) < .000001f && Mathf.Abs(actual.Max(p => p.y) - expected.Max(p => p.y)) < .000001f,
        "Exact original image region: " + sprite.name);
    var material = library.SpineMaterial(color, false);
    Check(material.GetTexture("_BaseMap") == sprite.texture, "Uses the original atlas instead of a replacement image: " + sprite.name);
    Check(mesh.bounds.size.z == 0 && Mathf.Abs(mesh.bounds.size.x - .68f) < .00001f && Mathf.Abs(mesh.bounds.size.y - 3) < .00001f, "Consistent physical spine dimensions");
    textures.Add(sprite.texture);
}
Check(textures.Count == 8, "Eight original atlas textures are shared across 140 books");
Check(textures.Cast<Texture2D>().All(t => t.mipmapCount > 1 && t.filterMode == FilterMode.Trilinear), "Distant geometric lines use mipmapped filtering");
string[] colors = { "Green", "Blue", "Beige", "Red", "Purple", "Brown", "White", "Black" };
var body = library.BodyMesh;
Check(body != null && body.bounds.size.z > .8f && body.bounds.size.y > body.bounds.size.z, "Covers and paper have real depth");
for (int i = 0; i < colors.Length; i++)
{
    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BookPrefabs/" + colors[i] + "BookObjectPrefab.prefab");
    var book = prefab.GetComponent<BookObject>();
    Check(book != null && book.Sprite != null && BookColorUtility.GetColorIndexFromSpriteName(book.Sprite.name) == i, colors[i] + " identity/presentation references");
    Check(prefab.GetComponentsInChildren<SpriteRenderer>(true).Length == 0 && prefab.GetComponentsInChildren<Collider2D>(true).Length == 0, colors[i] + " has no 2D fallback");
    var collider = prefab.GetComponent<BoxCollider>();
    Check(collider != null && collider.size.z > 1 && collider.enabled, colors[i] + " has physical 3D input");
    Check(prefab.GetComponentsInChildren<MeshFilter>().Any(f => f.sharedMesh == body), colors[i] + " shares the same binding/page geometry");
    Check(prefab.transform.localScale == Vector3.one, colors[i] + " does not stretch the book when its sprite changes");
}
var ghost = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(folder + "GhostBook3D.prefab");
Check(ghost != null && ghost.GetComponent<BookVisual3D>() != null && ghost.GetComponentsInChildren<Collider>(true).Length == 0, "3D preview cannot steal clicks");
Check(ghost.GetComponentsInChildren<MeshRenderer>().All(r => r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off && r.sharedMaterials.All(m => m.renderQueue >= 3000 && m.GetFloat("_ZWrite") == 0)), "Preview stays translucent and does not cast a solid shadow");
var previewMaterials=Enumerable.Range(0,8).Select(i=>library.SpineMaterial(i,true)).Concat(library.BodyMaterials(0,true)).Distinct().ToArray();
Check(previewMaterials.All(m=>m.GetColor("_BaseColor").a>0&&m.GetColor("_BaseColor").a<1),"Every 3D preview surface remains translucent");
Check(previewMaterials.All(m=>m.GetFloat("_SrcBlend")== (float)(m.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON")?UnityEngine.Rendering.BlendMode.One:UnityEngine.Rendering.BlendMode.SrcAlpha)),"URP applies alpha once; opening the material inspector cannot change the preview blending");
return new { passed = checks, books = entries.arraySize, atlases = textures.Count, bodyVertices = body.vertexCount };
