// Run after opening the list in a fresh ShelfCare Play session (see README).
if (!UnityEditor.EditorApplication.isPlaying)
    throw new System.InvalidOperationException("Run this check in Play mode.");
var checks = new System.Collections.Generic.List<string>();
void Check(bool condition, string label)
{
    if (!condition) throw new System.Exception("FAIL: " + label);
    checks.Add(label);
}
UnityEngine.GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
UnityEngine.UI.Button Button(string name) => Named(name).GetComponent<UnityEngine.UI.Button>();
var selection = UnityEngine.Object.FindFirstObjectByType<BookSelectionController>();
var hand = UnityEngine.Object.FindFirstObjectByType<HandItemController>();
var popup = UnityEngine.Object.FindFirstObjectByType<BookPopUpController>();
var ui = GameplayUIController.Instance;
var status = Named("SelectionCountLabel").GetComponent<TMPro.TMP_Text>();
var hint = Named("SelectionHintLabel").GetComponent<TMPro.TMP_Text>();
var confirm = Button("ConfirmSelectionButton");
var clear = Button("ClearSelectionButton");
var books = UnityEngine.Object.FindObjectsByType<BookButton>(UnityEngine.FindObjectsSortMode.None).OrderBy(b => b.transform.GetSiblingIndex()).ToArray();
Check(ui.IsBookListOpen && hand.BookCount == 0 && hand.Capacity == 9 && books.Length > 10, "Fresh list has books and nine-slot inventory");
Check(status.text.Contains("選択 0 / 9") && status.text.Contains("手持ち 0 / 9"), "Japanese counter starts at zero");
Check(!confirm.interactable && !clear.interactable, "No selection disables both actions");
confirm.onClick.Invoke();
Check(!popup.IsOpen, "Empty confirmation does not open a popup");

books[0].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(confirm.interactable && clear.interactable && status.text.Contains("選択 1 / 9"), "Selecting one book immediately updates counter and actions");
books[0].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(selection.SelectedCount == 0 && !confirm.interactable, "Deselecting updates the counter and actions");
foreach (var book in books.Take(10)) book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(selection.SelectedCount == 9 && hint.text.Contains("上限"), "Selection is capped at nine with visible guidance");
clear.onClick.Invoke();
Check(selection.SelectedCount == 0 && books.Take(10).All(b => !b.IsSelected), "Clear button resets every selection highlight");
Check(!clear.interactable && !confirm.interactable, "Clearing disables the actions again");

// Native scrolling moves the books without moving the title, Close or actions.
var scroll = Named("Scroll View").GetComponent<UnityEngine.UI.ScrollRect>();
UnityEngine.Canvas.ForceUpdateCanvases();
var titlePosition = Named("BookListTitle").transform.position;
var closePosition = Named("CloseButton").transform.position;
var footerPosition = Named("BookListFooter").transform.position;
scroll.verticalNormalizedPosition = 0;
UnityEngine.Canvas.ForceUpdateCanvases();
Check(Named("BookListTitle").transform.position == titlePosition && Named("CloseButton").transform.position == closePosition && Named("BookListFooter").transform.position == footerPosition, "Title, Close and actions remain fixed at the bottom of the list");
var viewportCorners = new UnityEngine.Vector3[4];
var lastCorners = new UnityEngine.Vector3[4];
scroll.viewport.GetWorldCorners(viewportCorners);
books.Last().GetComponent<UnityEngine.RectTransform>().GetWorldCorners(lastCorners);
Check(lastCorners[0].y >= viewportCorners[0].y - .1f && lastCorners[1].y <= viewportCorners[1].y + .1f, "Last row can be fully scrolled into view");
Check(lastCorners[0].x >= viewportCorners[0].x - .1f && lastCorners[2].x <= viewportCorners[2].x + .1f, "Last book fits the grid horizontally");
books.Last().GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
confirm.onClick.Invoke();
Check(popup.IsOpen && selection.SelectedCount == 1, "Visible confirmation button opens the selected-book popup");
ui.Back();
Check(!popup.IsOpen && ui.IsBookListOpen && selection.SelectedCount == 1 && books.Last().IsSelected, "Cancelling confirmation preserves selection in the list");
clear.onClick.Invoke();

foreach (var book in books.Take(9)) book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
confirm.onClick.Invoke();
Button("TakeButton").onClick.Invoke();
Check(hand.BookCount == 9 && !ui.IsBookListOpen, "New confirmation path transfers nine books through the real Take button");
ui.OpenBookList();
Check(selection.SelectionLimit == 0 && hint.text.Contains("いっぱい"), "Full hand shows the capacity guidance when reopening");
Check(status.text.Contains("選択 0 / 0") && status.text.Contains("手持ち 9 / 9") && !confirm.interactable, "Full hand has a zero selection limit and disabled confirmation");
books[9].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(selection.SelectedCount == 0, "A full hand cannot select another book");

// Inventory changes should update the open controls without frame-by-frame polling.
hand.SelectBook(0);
hand.RemoveSelectedBook();
Check(status.text.Contains("選択 0 / 1") && status.text.Contains("手持ち 8 / 9"), "Inventory change updates remaining capacity immediately");
books[9].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(selection.SelectedCount == 1 && confirm.interactable && hint.text.Contains("上限"), "One free hand slot permits one selection");
confirm.onClick.Invoke();
Button("TakeButton").onClick.Invoke();
Check(hand.BookCount == 9 && !ui.IsBookListOpen, "Taking the remaining capacity succeeds and closes the list");
ui.OpenBookList();
ui.CloseBookList();
ui.OpenBookList();
Check(status.text.Contains("手持ち 9 / 9") && !confirm.interactable && !clear.interactable, "Repeated reopening refreshes the controls correctly");

foreach (var label in Named("BookListFooter").GetComponentsInChildren<TMPro.TMP_Text>())
    Check(label.text.Where(c => !char.IsControl(c)).All(c => label.font.HasCharacter(c)), "Static atlas contains every glyph for " + label.name);
Check(status.font.atlasPopulationMode == TMPro.AtlasPopulationMode.Static && status.font.sourceFontFile == null, "Japanese UI uses a baked atlas without a runtime source font");
return new { passed = checks.Count, checks };
