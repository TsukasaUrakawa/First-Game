// Room view in ShelfCare Play, no modal. Check the physical bookcase and real 3D input.
if (!UnityEditor.EditorApplication.isPlaying || GameplayUIController.Instance.HasModal || UnityEngine.Object.FindFirstObjectByType<CameraController>().IsZoomed)
    throw new System.Exception("Run in the room with no modal.");
int checks = 0;
void Check(bool ok, string message) { if (!ok) throw new System.Exception("FAIL: " + message); checks++; }
var camera = Camera.main; Physics.SyncTransforms();
Check(UnityEngine.Object.FindObjectsByType<Collider2D>(UnityEngine.FindObjectsSortMode.None).Length == 0, "World input is fully 3D");
Check(!UnityEngine.Object.FindObjectsByType<SpriteRenderer>(UnityEngine.FindObjectsSortMode.None).Any(r => r.enabled), "No flat shelf/background sprite is rendering");
var cabinet = GameObject.Find("GameplayRoom3D").transform.Find("Bookcase");
Check(cabinet != null && cabinet.GetComponentsInChildren<MeshRenderer>().Length >= 20, "Real boards, backs and trim form the bookcase");
foreach (var filter in cabinet.GetComponentsInChildren<MeshFilter>())
{
    var projected = filter.sharedMesh.vertices.Select(v => camera.WorldToViewportPoint(filter.transform.TransformPoint(v))).ToArray();
    Check(projected.All(v => v.z > 0 && v.x >= .20f && v.x <= .99f && v.y >= .27f && v.y <= .96f), "Cabinet fits between the HUD and window edges: " + filter.name);
}
var slots = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None);
Check(slots.Length == 140 && slots.Select(s => s.SlotIndex).Distinct().Count() == 140, "All original placement identities remain unique");
foreach (var slot in slots.Where(s => !s.IsLocked))
{
    var point = camera.WorldToScreenPoint(slot.transform.position);
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
    Check(hits.Count == 0, "Room slot is not beneath UI: " + slot.SlotIndex);
    Check(Physics.Raycast(camera.ScreenPointToRay(point), out var hit, 250) && hit.collider.GetComponentInParent<BookSlot>() == slot, "Room ray reaches the intended book/place: " + slot.SlotIndex);
}
return new { passed = checks, slots = slots.Length, width = Screen.width, height = Screen.height };
