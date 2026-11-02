// Open a book confirmation first, let a rendered frame run, then evaluate this file.
// GraphicRaycaster uses the rendered Canvas depth, so do not combine with Show.
var popup = UnityEngine.Object.FindFirstObjectByType<BookPopUpController>();
if (!UnityEditor.EditorApplication.isPlaying || !popup.IsOpen)
    throw new System.Exception("Open a selection confirmation and let a frame render first.");
UnityEngine.Transform Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name);
var overlay = Named("BookPopUp");
var checks = new System.Collections.Generic.List<string>();
void Hit(UnityEngine.Vector2 position, UnityEngine.Transform expected, string label)
{
    var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = position };
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
    if (hits.Count == 0 || !hits[0].gameObject.transform.IsChildOf(expected))
        throw new System.Exception("FAIL: " + label);
    checks.Add(label);
}
Hit(UnityEngine.RectTransformUtility.WorldToScreenPoint(null, Named("CloseButton").position), overlay, "Confirmation intercepts underlying list Close");
foreach (var point in new[] { new UnityEngine.Vector2(1, 1), new UnityEngine.Vector2(UnityEngine.Screen.width - 1, 1), new UnityEngine.Vector2(1, UnityEngine.Screen.height - 1), new UnityEngine.Vector2(UnityEngine.Screen.width - 1, UnityEngine.Screen.height - 1) })
    Hit(point, overlay, "Confirmation covers screen corner " + point);
foreach (string button in new[] { "TakeButton", "CancelButton" })
{
    var transform = Named(button);
    var rect = transform.GetComponent<UnityEngine.RectTransform>();
    Hit(UnityEngine.RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), transform, button + " is reachable above the backdrop");
}
return new { passed = checks.Count, checks };
