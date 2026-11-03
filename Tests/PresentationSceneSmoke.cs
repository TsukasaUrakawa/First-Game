// Run in Play in TitleScene, or after the clear panel has finished fading in.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play mode.");
UnityEngine.GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
bool title = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene";
var root = Named(title ? "TitlePresentation" : "ClearCard");
var canvas = Named("Canvas").GetComponent<UnityEngine.RectTransform>();
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
bool Fits(UnityEngine.RectTransform child, UnityEngine.RectTransform parent)
{
    var corners = new UnityEngine.Vector3[4]; child.GetWorldCorners(corners);
    return corners.All(p => parent.rect.Contains(parent.InverseTransformPoint(p)));
}
Check(root.activeInHierarchy && root.GetComponent<UnityEngine.CanvasGroup>().alpha >= .99f, "Presentation is visible after intro");
foreach (var label in root.GetComponentsInChildren<TMPro.TMP_Text>())
{
    label.ForceMeshUpdate();
    Check(!label.isTextOverflowing, label.name + " has no text overflow");
    Check(label.text.Where(c => !char.IsControl(c)).All(c => label.font.HasCharacter(c)), label.name + " has every glyph");
    Check(label.font.atlasPopulationMode == TMPro.AtlasPopulationMode.Static && label.font.sourceFontFile == null, label.name + " uses a baked font");
    Check(Fits(label.rectTransform, canvas), label.name + " stays on screen");
}
foreach (string name in title ? new[] { "StartButton", "QuitButton" } : new[] { "ReplayButton", "TitleButton" })
{
    var button = Named(name).GetComponent<UnityEngine.UI.Button>();
    var rect = button.GetComponent<UnityEngine.RectTransform>();
    Check(Fits(rect, canvas) && button.interactable, name + " fits and is enabled");
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = UnityEngine.RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)) }, hits);
    Check(hits.Count > 0 && hits[0].gameObject.transform.IsChildOf(button.transform), name + " receives pointer input above background effects");
}
var ambient = UnityEngine.Object.FindFirstObjectByType<AmbientGeometryGraphic>();
Check(ambient != null && !ambient.raycastTarget, "Ambient geometry does not intercept input");
var mesh = ambient.canvasRenderer.GetMesh();
Check(mesh != null && mesh.vertexCount > 0, "Ambient geometry is actually rendered");
var background = Named(title ? "Background" : "ClearImage").GetComponent<UnityEngine.RectTransform>();
Check(Fits(canvas, background), "Overscanned moving background covers every screen corner");
if (!title)
{
    Check(GameplayUIController.Instance.IsGameCleared && GameplayUIController.IsWorldInputBlocked, "Clear keeps gameplay input locked");
    Check(Fits(root.GetComponent<UnityEngine.RectTransform>(), canvas), "Completion card fits screen");
    var images = Named("CompletedPatternGallery").GetComponentsInChildren<UnityEngine.UI.Image>();
    Check(images.Length == 8 && images.Select(i => i.sprite.name).SequenceEqual(Enumerable.Range(1, 8).Select(i => "Books" + i)), "Gallery shows all eight completed patterns in shelf order");
    Check(Named("ClearPanel").GetComponent<UnityEngine.CanvasGroup>().interactable, "Clear actions become available after fade");
}
return new { passed = checks, scene = title ? "title" : "clear", width = UnityEngine.Screen.width, height = UnityEngine.Screen.height };
