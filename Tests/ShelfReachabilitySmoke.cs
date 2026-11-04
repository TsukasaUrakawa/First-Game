// Fresh Play game, no menu/modal open. Run at each supported aspect ratio.
if (!UnityEditor.EditorApplication.isPlaying || GameplayUIController.Instance.HasModal) throw new System.Exception("Close menus first.");
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
var camera = UnityEngine.Object.FindFirstObjectByType<CameraController>().GetComponent<Camera>();
var shelves = UnityEngine.Object.FindObjectsByType<BookShelfZoomButton>(UnityEngine.FindObjectsSortMode.None).OrderBy(s => s.ShelfJudge.name).ToArray();
Check(shelves.Length == 8, "All shelves present");
void CheckShelf(BookShelfZoomButton shelf)
{
    Physics.SyncTransforms();
    foreach (var slot in shelf.ShelfJudge.GetComponentsInChildren<BookSlot>(true))
    {
        var point = (Vector2)camera.WorldToScreenPoint(slot.transform.position);
        Check(point.x >= 12 && point.y >= 12 && point.x <= Screen.width-12 && point.y <= Screen.height-12, shelf.name + " endpoint/slot " + slot.SlotIndex + " fits the viewport");
        var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
        Check(hits.Count == 0, shelf.name + " slot " + slot.SlotIndex + " is not covered by UI");
        var ray = camera.ScreenPointToRay(point);
        Check(Physics.Raycast(ray, out var hit, 200) && hit.collider.GetComponentInParent<BookSlot>() == slot,
            shelf.name + " slot " + slot.SlotIndex + " receives a real 3D physics ray");
    }
}
// After a resize, check the current view before reopening any shelf.
var previous = ShelfActionMenuController.Instance.SelectedShelf;
if (previous != null) CheckShelf(previous);
foreach (var shelf in shelves) { shelf.OpenActionMenu(); Canvas.ForceUpdateCanvases(); CheckShelf(shelf); }
return new { passed = checks, width = Screen.width, height = Screen.height, resizedViewChecked = previous != null };
