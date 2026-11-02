// Run after opening the list in a fresh ShelfCare Play session (see README).
if (!UnityEditor.EditorApplication.isPlaying)
    throw new System.InvalidOperationException("Run this check in Play mode.");
var checks = new System.Collections.Generic.List<string>();
void Check(bool condition, string label)
{
    if (!condition) throw new System.Exception("FAIL: " + label);
    checks.Add(label);
}
T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>(UnityEngine.FindObjectsInactive.Include);
UnityEngine.GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
UnityEngine.UI.Button Button(string name) => Named(name).GetComponent<UnityEngine.UI.Button>();
TMPro.TMP_Text Label(string name) => Named(name).GetComponent<TMPro.TMP_Text>();
bool Inside(UnityEngine.RectTransform child, UnityEngine.RectTransform parent)
{
    var points = new UnityEngine.Vector3[4];
    child.GetWorldCorners(points);
    return points.All(p => parent.rect.Contains(parent.InverseTransformPoint(p)));
}
var ui = Find<GameplayUIController>();
var camera = Find<CameraController>();
var action = Find<ShelfActionMenuController>();
var selection = Find<BookSelectionController>();
var popup = Find<BookPopUpController>();
var hand = Find<HandItemController>();
var placement = Find<HandBookPlacementController>();
var grid = Named("ShelfCardGrid").GetComponent<UnityEngine.UI.GridLayoutGroup>();
var cards = grid.GetComponentsInChildren<BookShelfZoomButton>();
var books = UnityEngine.Object.FindObjectsByType<BookButton>(UnityEngine.FindObjectsSortMode.None).OrderBy(b => b.transform.GetSiblingIndex()).ToArray();
Check(ui.IsBookListOpen && hand.BookCount == 0 && books.Length == 140, "Fresh list contains all 140 books");
Check(grid.constraint == UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount && grid.constraintCount == 2 && cards.Length == 8, "Navigation has two columns and four rows");
Check(cards.Select(c => c.DisplayName).SequenceEqual(new[] { "緑の本棚", "青の本棚", "ベージュの本棚", "赤の本棚", "紫の本棚", "茶の本棚", "白の本棚", "黒の本棚" }), "Card order follows physical shelves");
Check(cards.All(c => c.transform.localScale == UnityEngine.Vector3.one), "Every shelf card has the same scale");
Check(UnityEngine.Object.FindObjectsByType<RoundedPanelGraphic>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).All(g => g.GetComponent<UnityEngine.CanvasRenderer>() != null), "Every rounded panel has its renderer");

var dialog = Named("BookConfirmationCard").GetComponent<UnityEngine.RectTransform>();
var row = Named("SelectedBookRow").GetComponent<UnityEngine.RectTransform>();
foreach (int count in new[] { 1, 3, 6 })
{
    selection.ClearSelection();
    foreach (var book in books.Take(count)) book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Button("ConfirmSelectionButton").onClick.Invoke();
    UnityEngine.Canvas.ForceUpdateCanvases();
    var previews = row.GetComponentsInChildren<UnityEngine.RectTransform>().Where(t => t.parent == row).ToArray();
    Check(popup.IsOpen && previews.Length == count, count + " selected books show exactly that many cards");
    Check(Label("ConfirmationSelectionCount").text.Contains(count.ToString()) && Label("ConfirmationCapacity").text.Contains(count + " / 6"), count + " selected books show correct counts and capacity");
    Check(previews.All(t => Inside(t, dialog)) && Inside(Button("TakeButton").GetComponent<UnityEngine.RectTransform>(), dialog) && Inside(Button("CancelButton").GetComponent<UnityEngine.RectTransform>(), dialog), count + " preview cards and both actions fit inside the dialog");
    var left = new UnityEngine.Vector3[4]; var right = new UnityEngine.Vector3[4];
    previews.First().GetWorldCorners(left); previews.Last().GetWorldCorners(right);
    Check(UnityEngine.Mathf.Abs((left[0].x + right[2].x) * .5f - row.position.x) < .1f, count + " book cards are centered as a group");
    // A programmatic list click is also ignored while the confirmation is open.
    books.Last().GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Check(selection.SelectedCount == count, count + " confirmation cannot change selection behind it");
    if (count == 3) Button("CancelButton").onClick.Invoke(); else ui.Back();
    Check(!popup.IsOpen && selection.SelectedCount == count && books.Take(count).All(b => b.IsSelected), count + " Cancel or Back preserves selected books and highlights");
}
Button("CloseButton").onClick.Invoke();
Check(selection.SelectedCount == 0 && books.Take(6).All(b => !b.IsSelected), "Closing the list still clears selection");

cards[0].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(camera.IsZoomed && action.IsOpen && !GameplayUIController.IsWorldInputBlocked, "One shelf click zooms with a nonblocking toolbar");
Check(Label("CurrentShelfTitle").text == cards[0].DisplayName && Label("CurrentShelfStatus").text.Contains("0 / 18"), "Toolbar identifies the selected shelf and current storage");
Check(Named("ShelfButtonPanel").GetComponent<UnityEngine.CanvasGroup>().alpha == 0, "Zoom hides room navigation");
Button("AnswerButton").onClick.Invoke();
Check(Find<ShelfAnswerPanelController>().IsOpen && !action.IsOpen && GameplayUIController.IsWorldInputBlocked, "Completion image covers the toolbar and blocks placement");
ui.Back();
Check(camera.IsZoomed && action.IsOpen, "Closing completion image restores the same shelf toolbar");
Button("JudgeButton").onClick.Invoke();
Check(action.IsOpen && Label("CurrentShelfStatus").text.Contains("違う"), "Checking an empty shelf gives visible feedback without closing the toolbar");

var bookSprite = Find<BookManager>()._allBookSprites.First(s => s.name.Contains("Green") && s.name.EndsWith("01"));
var slot = cards[0].ShelfJudge.GetComponentsInChildren<BookSlot>().Single(s => s.SlotIndex == 0);
Check(hand.TryAddBook(bookSprite, 0), "Test book enters the hand");
hand.SelectBook(0);
Check(placement.TryPlaceBook(slot), "Toolbar permits actual placement");
Check(cards[0].transform.Find("ShelfProgressLabel").GetComponent<TMPro.TMP_Text>().text == "1 / 18 冊" && Label("CurrentShelfStatus").text.Contains("1 / 18"), "Placement updates card and toolbar immediately");
var fill = cards[0].transform.Find("ProgressTrack/ProgressFill").GetComponent<UnityEngine.UI.Image>();
Check(fill.sprite != null && UnityEngine.Mathf.Abs(fill.fillAmount - 1f / 18) < .0001f, "Progress bar renders the actual fraction");
Check(cards.Skip(1).All(c => c.transform.Find("ShelfProgressLabel").GetComponent<TMPro.TMP_Text>().text.StartsWith("0 /")), "Placement leaves other shelf counters unchanged");
Check(placement.TryReturnBookToHand(slot) && Label("CurrentShelfStatus").text.Contains("0 / 18"), "Returning a book updates the toolbar immediately");
ui.Back();
Check(!camera.IsZoomed && !action.IsOpen && Named("ShelfButtonPanel").GetComponent<UnityEngine.CanvasGroup>().alpha == 1, "Back returns to room navigation in one action");
cards[1].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(Label("CurrentShelfTitle").text == cards[1].DisplayName && Label("CurrentShelfStatus").text.Contains("0 / 17"), "Another shelf replaces the toolbar context");
ui.OpenBookList();
Check(!action.IsOpen, "Book list hides the toolbar");
ui.Back();
Check(action.IsOpen && camera.IsZoomed && Label("CurrentShelfTitle").text == cards[1].DisplayName, "Closing list restores its shelf context");
ui.Back();

// With one book already held, the projected inventory must include it.
ui.OpenBookList();
foreach (var book in books.Take(5)) book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Button("ConfirmSelectionButton").onClick.Invoke();
Check(Label("ConfirmationCapacity").text.Contains("6 / 6") && Button("TakeButton").interactable, "Projected hand count includes existing inventory");
Button("TakeButton").onClick.Invoke();
Button("TakeButton").onClick.Invoke();
Check(hand.BookCount == 6 && selection.SelectedCount == 0 && !popup.IsOpen && !ui.IsBookListOpen, "Taking books transfers once even if invoked twice");
foreach (var label in Named("ShelfButtonPanel").GetComponentsInChildren<TMPro.TMP_Text>(true)
    .Concat(Named("ShelfActionPanel").GetComponentsInChildren<TMPro.TMP_Text>(true))
    .Concat(dialog.GetComponentsInChildren<TMPro.TMP_Text>(true)))
    Check(label.text.Where(c => !char.IsControl(c)).All(c => label.font.HasCharacter(c)), "Japanese atlas covers " + label.name);
Check(Label("ConfirmationSelectionCount").font.atlasPopulationMode == TMPro.AtlasPopulationMode.Static && Label("ConfirmationSelectionCount").font.sourceFontFile == null, "UI uses the baked atlas without a runtime font source");
return new { passed = checks.Count, checks };
