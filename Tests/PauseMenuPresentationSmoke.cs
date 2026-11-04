// Pause open; let a frame render first. Check normal/error states at both desktop sizes.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play.");
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
GameObject Named(string n) => UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == n).gameObject;
var pause = UnityEngine.Object.FindFirstObjectByType<PauseMenuController>(); var panel = Named("PausePanel").transform;
var card = Named("PauseCard").GetComponent<RectTransform>(); var canvas = Named("Canvas").GetComponent<RectTransform>();
Check(pause.IsOpen && Time.timeScale == 0, "Pause visible with clock stopped");
var corners = new Vector3[4]; card.GetWorldCorners(corners);
Check(corners.All(p => canvas.rect.Contains(canvas.InverseTransformPoint(p))), "Card fits the screen");
foreach (var label in card.GetComponentsInChildren<TMPro.TMP_Text>())
{
    label.ForceMeshUpdate(); Check(!label.isTextOverflowing, label.name + " fits");
    Check(label.text.Where(c => !char.IsControl(c)).All(c => label.font.HasCharacter(c)), label.name + " has all glyphs");
    Check(label.font.atlasPopulationMode == TMPro.AtlasPopulationMode.Static && label.font.sourceFontFile == null, "Static Japanese font");
}
foreach (string name in new[] { "ResumePlayButton", "SaveReturnTitleButton" })
{
    var button = Named(name); var point = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
    Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>()?.gameObject == button, name + " receives pointer input");
}
var dimmerHits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = new Vector2(10, Screen.height - 10) }, dimmerHits);
Check(dimmerHits.Count > 0 && dimmerHits[0].gameObject.transform.IsChildOf(panel), "Dimmer blocks outside clicks");
Check(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Selectable>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None).Where(c => !c.transform.IsChildOf(panel)).All(c => !c.IsInteractable()), "Keyboard cannot navigate to covered controls");
return new { passed = checks, width = Screen.width, height = Screen.height, error = Named("PauseStatus").GetComponent<TMPro.TMP_Text>().text };
