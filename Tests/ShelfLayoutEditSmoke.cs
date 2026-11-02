// Run in Edit mode with ShelfCare open. Temporary test objects live in a separate scene.
if (UnityEditor.EditorApplication.isPlaying)
    throw new System.Exception("Run the layout check in Edit mode.");
var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (originalScene.path != "Assets/Scenes/ShelfCare.unity")
    throw new System.Exception("Open the ShelfCare scene first.");
int checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new System.Exception("FAIL: " + label);
    checks++;
}
bool Near(UnityEngine.Vector3 a, UnityEngine.Vector3 b) => UnityEngine.Vector3.Distance(a, b) < .00002f;
var shelves = UnityEngine.Object.FindObjectsByType<BookShelfSlotAligner>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).OrderBy(s => s.name).ToArray();
var slots = shelves.SelectMany(s => s.GetComponentsInChildren<BookSlot>(true)).ToArray();
Check(shelves.Length == 8 && slots.Length == 140, "Eight shelves still have 140 slots");
Check(slots.Select(s => s.SlotIndex).OrderBy(i => i).SequenceEqual(Enumerable.Range(0, 140)), "Slot identities remain exactly 0 through 139");
Check(!originalScene.GetRootGameObjects().Any(g => g.name == "Splines"), "Unused spline hierarchy is removed");
var manifest = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("Packages/manifest.json"));
Check(manifest["dependencies"]["com.unity.splines"] == null, "Splines is no longer a direct package dependency");
float[] levels = { 7.95f, 4f, .08f, -3.6f };
for (int i = 0; i < shelves.Length; i++)
{
    var row = shelves[i].GetComponentsInChildren<BookSlot>(true).OrderBy(s => s.SlotIndex).ToArray();
    int count = i % 2 == 0 ? 18 : 17;
    float firstX = i % 2 == 0 ? -8.6f : 4.15f;
    float lastX = i % 2 == 0 ? 2.85f : 15.7f;
    float y = levels[i / 2];
    Check(row.Length == count && shelves[i].SlotCount == count, shelves[i].name + " preserves its book count");
    Check(Near(row[0].transform.position, new UnityEngine.Vector3(firstX, y, 0)), shelves[i].name + " preserves the left endpoint");
    Check(Near(row.Last().transform.position, new UnityEngine.Vector3(lastX, y, 0)), shelves[i].name + " preserves the right endpoint");
    Check(row.All(s => System.Math.Abs(s.transform.position.y - y) < .00002f && System.Math.Abs(s.transform.position.z) < .00002f), shelves[i].name + " remains level");
    var gaps = row.Zip(row.Skip(1), (a, b) => b.transform.localPosition.x - a.transform.localPosition.x).ToArray();
    Check(gaps.Max() - gaps.Min() < .00002f && gaps.Min() > 0, shelves[i].name + " has uniform increasing spacing");
}

// Exercise the reusable component independently of the game's existing scene.
var testScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
try
{
    UnityEngine.SceneManagement.SceneManager.SetActiveScene(testScene);
    var root = new UnityEngine.GameObject("BookShelf01");
    var layout = root.AddComponent<BookShelfSlotAligner>();
    var so = new UnityEditor.SerializedObject(layout);
    so.FindProperty("_startLocalPosition").vector3Value = new UnityEngine.Vector3(2, -1, .3f);
    so.FindProperty("_slotSpacing").floatValue = .75f;
    so.ApplyModifiedPropertiesWithoutUndo();
    layout.AlignChildSlots();
    Check(layout.SlotCount == 0, "Empty shelf is safe");
    var decoration = new UnityEngine.GameObject("Decoration");
    decoration.transform.SetParent(root.transform, false);
    decoration.transform.localPosition = new UnityEngine.Vector3(9, 8, 7);
    BookSlot AddSlot(string name)
    {
        var go = new UnityEngine.GameObject(name);
        go.transform.SetParent(root.transform, false);
        return go.AddComponent<BookSlot>();
    }
    var third = AddSlot("Slot03");
    layout.AlignChildSlots();
    Check(Near(third.transform.localPosition, new UnityEngine.Vector3(2, -1, .3f)), "Single book uses the start position");
    var first = AddSlot("Slot01");
    var second = AddSlot("Slot02");
    second.gameObject.SetActive(false);
    layout.AlignChildSlots();
    Check(layout.SlotCount == 3, "Inactive books are counted");
    Check(Near(first.transform.localPosition, new UnityEngine.Vector3(2, -1, .3f)) && Near(second.transform.localPosition, new UnityEngine.Vector3(2.75f, -1, .3f)) && Near(third.transform.localPosition, new UnityEngine.Vector3(3.5f, -1, .3f)), "Slot numbers define order, independently of sibling order");
    first.transform.SetAsFirstSibling();
    third.transform.SetAsLastSibling();
    layout.AlignChildSlots();
    Check(Near(second.transform.localPosition, new UnityEngine.Vector3(2.75f, -1, .3f)), "Reordering the hierarchy preserves numbered positions");
    Check(Near(decoration.transform.localPosition, new UnityEngine.Vector3(9, 8, 7)), "Decorations are not moved");
    var nested = new UnityEngine.GameObject("Slot04");
    nested.transform.SetParent(decoration.transform, false);
    nested.AddComponent<BookSlot>();
    nested.transform.localPosition = new UnityEngine.Vector3(1, 2, 3);
    layout.AlignChildSlots();
    Check(layout.SlotCount == 3 && Near(nested.transform.localPosition, new UnityEngine.Vector3(1, 2, 3)), "Only direct child slots belong to this row");
    root.transform.position = new UnityEngine.Vector3(10, 20, -2);
    root.transform.rotation = UnityEngine.Quaternion.Euler(0, 0, 90);
    root.transform.localScale = new UnityEngine.Vector3(2, 3, 4);
    layout.AlignChildSlots();
    Check(Near(first.transform.position, new UnityEngine.Vector3(13, 24, -.8f)), "Layout follows the shelf's local coordinate space");
    Check(System.Math.Abs(UnityEngine.Vector3.Distance(first.transform.position, second.transform.position) - 1.5f) < .00002f, "Scaled shelves retain proportional spacing");

    // Invoke the actual Editor context action to verify its Undo integration.
    first.transform.localPosition = new UnityEngine.Vector3(99, 98, 97);
    UnityEditor.Undo.IncrementCurrentGroup();
    var action = typeof(BookShelfSlotAlignerEditor).GetMethod("AlignFromContextMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    action.Invoke(null, new object[] { new UnityEditor.MenuCommand(layout) });
    UnityEditor.Undo.FlushUndoRecordObjects();
    Check(Near(first.transform.localPosition, new UnityEngine.Vector3(2, -1, .3f)), "Editor action aligns the row");
    UnityEditor.Undo.PerformUndo();
    Check(Near(first.transform.localPosition, new UnityEngine.Vector3(99, 98, 97)), "Undo restores the previous slot position");
}
finally
{
    UnityEngine.SceneManagement.SceneManager.SetActiveScene(originalScene);
    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(testScene, true);
}
return new { passed = checks, shelves = shelves.Length, slots = slots.Length };
