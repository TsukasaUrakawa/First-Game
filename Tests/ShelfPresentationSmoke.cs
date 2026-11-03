// Run in Play after opening any shelf's completion picture and letting a frame render.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play mode.");
UnityEngine.GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
var ui = GameplayUIController.Instance;
var answer = ShelfAnswerPanelController.Instance;
var camera = UnityEngine.Object.FindFirstObjectByType<CameraController>();
var menu = ShelfActionMenuController.Instance;
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
bool Fits(UnityEngine.RectTransform child, UnityEngine.RectTransform parent)
{
    var corners = new UnityEngine.Vector3[4]; child.GetWorldCorners(corners);
    return corners.All(p => parent.rect.Contains(parent.InverseTransformPoint(p)));
}
var canvas = Named("Canvas").GetComponent<UnityEngine.RectTransform>();
Check(answer.IsOpen, "Completion picture is already rendered");
Check(Fits(Named("ShelfAnswerCard").GetComponent<UnityEngine.RectTransform>(), canvas), "Paper card fits screen");
Check(Fits(Named("ShelfAnswerImage").GetComponent<UnityEngine.RectTransform>(), Named("AnswerWoodenFrame").GetComponent<UnityEngine.RectTransform>()), "Picture fits wooden display");
foreach (var p in new[] { new UnityEngine.Vector2(1, 1), new UnityEngine.Vector2(UnityEngine.Screen.width - 1, 1), new UnityEngine.Vector2(1, UnityEngine.Screen.height - 1), new UnityEngine.Vector2(UnityEngine.Screen.width - 1, UnityEngine.Screen.height - 1) })
{
    var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = p }, hits);
    Check(hits.Count > 0 && hits[0].gameObject == Named("ShelAnswerPanel"), "Modal backdrop intercepts corner " + p);
}
var cards = UnityEngine.Object.FindObjectsByType<BookShelfZoomButton>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).OrderBy(b => b.ShelfJudge.name).ToArray();
foreach (var shelf in cards)
{
    ui.Back(); shelf.ZoomShelf(); menu.ShowSelectedShelfAnswer();
    UnityEngine.Canvas.ForceUpdateCanvases();
    Check(Named("ShelfAnswerTitle").GetComponent<TMPro.TMP_Text>().text == shelf.DisplayName + "の完成図", shelf.DisplayName + " correct title");
    Check(Named("ShelfAnswerImage").GetComponent<UnityEngine.UI.Image>().sprite.name == "Books" + (Array.IndexOf(cards, shelf) + 1), shelf.DisplayName + " correct pattern");
    Check(GameplayUIController.IsWorldInputBlocked && !Named("BackToRoomButton").activeInHierarchy, shelf.DisplayName + " modal protects underlying shelf");
    foreach (var label in Named("ShelfAnswerCard").GetComponentsInChildren<TMPro.TMP_Text>())
    {
        label.ForceMeshUpdate();
        Check(!label.isTextOverflowing && label.text.Where(ch => !char.IsControl(ch)).All(ch => label.font.HasCharacter(ch)), label.name + " fits and has glyphs");
    }
}
Named("CrossButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(!answer.IsOpen && camera.IsZoomed && menu.SelectedShelf == cards.Last() && menu.IsOpen, "Close returns to the same shelf for placement");
Check(Fits(Named("BackToRoomButton").GetComponent<UnityEngine.RectTransform>(), canvas), "Room button fits screen");
Check(Named("BackToRoomButtonLabel").GetComponent<TMPro.TMP_Text>().text == "部屋に戻る", "Room button uses Japanese paper-style label");
Named("BackToRoomButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(!camera.IsZoomed && !menu.IsOpen && !GameplayUIController.IsWorldInputBlocked, "Room button returns to gameplay room");
return new { passed = checks, width = UnityEngine.Screen.width, height = UnityEngine.Screen.height };
