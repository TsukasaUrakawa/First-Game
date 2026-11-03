// Run while the guide is visible, after a frame has rendered its current step.
var tutorial = UnityEngine.Object.FindFirstObjectByType<BookPlacementTutorial>();
if (!UnityEditor.EditorApplication.isPlaying || !tutorial.IsRunning) throw new System.Exception("Show the tutorial first.");
UnityEngine.Transform Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name);
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
bool Fits(UnityEngine.RectTransform child, UnityEngine.RectTransform parent)
{
    var corners = new UnityEngine.Vector3[4]; child.GetWorldCorners(corners);
    return corners.All(p => parent.rect.Contains(parent.InverseTransformPoint(p)));
}
var panel = Named("TutorialPanel").GetComponent<UnityEngine.RectTransform>();
var layer = Named("TutorialLayer").GetComponent<UnityEngine.RectTransform>();
var focus = Named("TutorialFocus").GetComponent<UnityEngine.RectTransform>();
var dimmer = Named("TutorialDimmer").GetComponent<TutorialSpotlightGraphic>();
Check(panel.gameObject.activeInHierarchy && Fits(panel, layer), "Guide fits screen");
Check(!focus.gameObject.activeInHierarchy || Fits(focus, layer), "Spotlight opening fits screen");
foreach (var label in panel.GetComponentsInChildren<TMPro.TMP_Text>())
    Check(!label.isTextOverflowing && label.text.Where(c => !char.IsControl(c)).All(c => label.font.HasCharacter(c)), "Label fits: " + label.name);
Check(focus.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Length == 0, "No frame or rectangular fill is drawn over the target");
Check(dimmer.gameObject.activeInHierarchy == focus.gameObject.activeInHierarchy, "Dimmer follows the current target");
Check(!dimmer.raycastTarget, "Spotlight cannot consume input");
Check(dimmer.transform.GetSiblingIndex() < focus.GetSiblingIndex() && focus.GetSiblingIndex() < panel.GetSiblingIndex(), "Target and guide render above the dimmer");
System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> Hits(UnityEngine.Vector2 point)
{
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
    return hits;
}
if (tutorial.CurrentStep == BookPlacementTutorial.Step.OpenBox && !UnityEngine.Object.FindFirstObjectByType<CameraController>().IsZoomed)
{
    var box = UnityEngine.Object.FindFirstObjectByType<BoxOpenAndClose>();
    Check(Hits(UnityEngine.Camera.main.WorldToScreenPoint(box.GetComponent<UnityEngine.Collider>().bounds.center)).Count == 0, "Pulsing box remains clickable through the dimmer");
}
if (tutorial.CurrentStep == BookPlacementTutorial.Step.TakeBooks)
{
    foreach (string name in new[] { "TakeButton", "CancelButton" })
    {
        var button = Named(name).GetComponent<UnityEngine.RectTransform>();
        var hits = Hits(UnityEngine.RectTransformUtility.WorldToScreenPoint(null, button.TransformPoint(button.rect.center)));
        Check(hits.Count > 0 && hits[0].gameObject.transform.IsChildOf(button), name + " stays reachable while pulsing");
    }
    var dialog = Named("BookConfirmationCard").GetComponent<UnityEngine.RectTransform>();
    var panelCorners = new UnityEngine.Vector3[4]; var dialogCorners = new UnityEngine.Vector3[4];
    panel.GetWorldCorners(panelCorners); dialog.GetWorldCorners(dialogCorners);
    Check(panelCorners[0].y > dialogCorners[1].y, "Guide stays above confirmation card");
}
if (tutorial.CurrentStep == BookPlacementTutorial.Step.PlaceBook)
{
    var hand = UnityEngine.Object.FindFirstObjectByType<HandItemController>();
    if (!hand.TryGetSelectedBook(out var book)) throw new System.Exception("Expected selected book.");
    int color = BookColorUtility.GetColorIndexFromSpriteName(book.Sprite.name);
    foreach (var slot in UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).Where(s => s.ShelfColorIndex == color && !s.IsFilled))
        Check(Hits(UnityEngine.Camera.main.WorldToScreenPoint(slot.transform.position)).Count == 0, slot.name + " placement center remains reachable");
}
return new { passed = checks, step = tutorial.CurrentStep.ToString(), width = UnityEngine.Screen.width, height = UnityEngine.Screen.height };
