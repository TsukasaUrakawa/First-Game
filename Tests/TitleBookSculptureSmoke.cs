// TitleScene Play mode. Back up preferences, then disable reduced motion before running.
// Sample the full assembly/idle cycle. Check physical book separation and real projected vertices.
if (!UnityEditor.EditorApplication.isPlaying || GamePreferences.ReducedMotion)
    throw new System.Exception("Run in TitleScene Play with normal motion; back up preferences first.");
var stage = UnityEngine.Object.FindFirstObjectByType<TitleBookSculpture>();
if (stage == null) throw new System.Exception("Title sculpture missing.");
var serialized = new UnityEditor.SerializedObject(stage);
var property = serialized.FindProperty("_books");
var books = Enumerable.Range(0, property.arraySize).Select(i => (Transform)property.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
var camera = (Camera)serialized.FindProperty("_camera").objectReferenceValue;
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var elapsed = typeof(TitleBookSculpture).GetField("_elapsed", flags);
var assembled = typeof(TitleBookSculpture).GetField("_assembled", flags);
var apply = typeof(TitleBookSculpture).GetMethod("ApplyPose", flags);
float beforeTime = (float)elapsed.GetValue(stage);
bool beforeAssembled = (bool)assembled.GetValue(stage);
int checks = 0, poses = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
var body = books[0].Find("BindingAndPages").GetComponent<MeshFilter>().sharedMesh;
Check(books.Length >= 20 && books.Length <= 32, "A substantial sculpture uses a bounded number of books");
Check(stage.GetComponentsInChildren<Collider>().Length == 0, "Decorative books do not intercept gameplay input");
Check(stage.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0, "Books are rigid, with no skinned deformation");
Check(books.All(b => b.Find("BindingAndPages").GetComponent<MeshFilter>().sharedMesh == body), "Every volume has the same ordinary binding/page geometry");
Check(body.bounds.size.y > body.bounds.size.z && body.bounds.size.z > body.bounds.size.x, "Book proportions distinguish height, page depth and thickness");
foreach (var renderer in stage.GetComponentsInChildren<MeshRenderer>())
    Check(renderer.sharedMaterials.All(m => m != null && m.shader.isSupported), renderer.name + " has supported materials");
var meshes = books.SelectMany(b => b.GetComponentsInChildren<MeshFilter>()).Select(m => m.sharedMesh).Distinct().ToArray();
Check(meshes.Sum(m => m.vertexCount) < 20000, "Baked geometry stays small enough for a browser title screen");
Check(meshes.All(m => UnityEditor.AssetDatabase.GetAssetPath(m).StartsWith("Assets/Presentation/TitleBooks/")), "Title artwork is independent of gameplay book textures");
bool Separated(Transform a, Transform b)
{
    var centerA = a.TransformPoint(body.bounds.center); var centerB = b.TransformPoint(body.bounds.center);
    var axesA = new[] { a.right, a.up, a.forward }; var axesB = new[] { b.right, b.up, b.forward };
    var halfA = Vector3.Scale(body.bounds.extents, a.lossyScale); var halfB = Vector3.Scale(body.bounds.extents, b.lossyScale);
    var axes = axesA.Concat(axesB).Concat(axesA.SelectMany(x => axesB.Select(y => Vector3.Cross(x, y))));
    foreach (var axis in axes)
    {
        if (axis.sqrMagnitude < .000001f) continue;
        float radiusA = Enumerable.Range(0, 3).Sum(i => halfA[i] * Mathf.Abs(Vector3.Dot(axis, axesA[i])));
        float radiusB = Enumerable.Range(0, 3).Sum(i => halfB[i] * Mathf.Abs(Vector3.Dot(axis, axesB[i])));
        if (Mathf.Abs(Vector3.Dot(centerB - centerA, axis)) >= radiusA + radiusB - .00001f) return true;
    }
    return false;
}
void Pose(float time, bool isAssembled)
{
    elapsed.SetValue(stage, time); assembled.SetValue(stage, isAssembled); apply.Invoke(stage, null); poses++;
    foreach (var book in books)
    {
        var scale = book.lossyScale;
        Check(Mathf.Abs(scale.x - scale.y) < .0001f && Mathf.Abs(scale.y - scale.z) < .0001f, "No squash/stretch: " + book.name);
        foreach (var filter in book.GetComponentsInChildren<MeshFilter>())
        {
            var points = filter.sharedMesh.vertices.Select(p => camera.WorldToViewportPoint(filter.transform.TransformPoint(p))).ToArray();
            Check(points.All(p => p.z > 0 && p.x >= .355f && p.x <= .99f && p.y >= .015f && p.y <= .96f), "Book stays visible and clear of the left menu: " + book.name + " at " + time);
        }
    }
    for (int i = 0; i < books.Length; i++) for (int j = i + 1; j < books.Length; j++)
        Check(Separated(books[i], books[j]), "Solid books do not intersect: " + books[i].name + "/" + books[j].name + " at " + time);
}
try
{
    foreach (float time in new[] { 0, .5f, 1, 1.8f, 2.8f }) Pose(time, false);
    float intro = serialized.FindProperty("_assemblyDuration").floatValue + (books.Length - 1) * serialized.FindProperty("_bookDelay").floatValue;
    for (int i = 0; i < 44; i++) Pose(intro + i * .5f, true);
}
finally
{
    elapsed.SetValue(stage, beforeTime); assembled.SetValue(stage, beforeAssembled); apply.Invoke(stage, null);
}
return new { passed = checks, poses, books = books.Length, vertices = meshes.Sum(m => m.vertexCount), width = Screen.width, height = Screen.height };
