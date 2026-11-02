// Run through Unity CLI's script eval_file in a fresh ShelfCare Play session.
// First open the book list and allow a frame for BookListUI.Start to create books.
// This exercises the actual scene references and persistent button listeners.
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
void Visible(UnityEngine.CanvasGroup group, bool visible, string label)
{
    Check(group.alpha == (visible ? 1f : 0f) && group.interactable == visible && group.blocksRaycasts == visible, label);
}

var ui = Find<GameplayUIController>();
var camera = Find<CameraController>();
var selection = Find<BookSelectionController>();
var popup = Find<BookPopUpController>();
var hand = Find<HandItemController>();
var placement = Find<HandBookPlacementController>();
var answer = Find<ShelfAnswerPanelController>();
var action = Find<ShelfActionMenuController>();
var shelfHud = Named("ShelfButtonPanel").GetComponent<UnityEngine.CanvasGroup>();
var handHud = Named("HandItemPanel").GetComponent<UnityEngine.CanvasGroup>();
var back = Named("BackToRoomButton");
var greenShelf = Named("GreenShelfButton").GetComponent<BookShelfZoomButton>();
var books = UnityEngine.Object.FindObjectsByType<BookButton>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)
    .Where(b => b.BookSprite != null && b.BookSprite.name.Contains("Green")).Take(6).ToArray();
Check(books.Length == 6 && hand.BookCount == 0, "Fresh scene has six selectable green books and an empty hand");

// Cancel the confirmation, then close the list using its real Close button.
books[0].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
selection.ShowSelectionConfirmation();
Check(popup.IsOpen && selection.SelectedCount == 1, "Selected book opens confirmation");
ui.Back();
Check(ui.IsBookListOpen && !popup.IsOpen && selection.SelectedCount == 1 && books[0].IsSelected, "Back closes only confirmation and preserves selection for editing");
selection.ShowSelectionConfirmation();
Button("CloseButton").onClick.Invoke();
Check(!ui.IsBookListOpen && !popup.IsOpen && selection.SelectedCount == 0 && !books[0].IsSelected, "Close list also closes confirmation and clears selection");
Visible(shelfHud, true, "Room restores shelf HUD");

// Do not lose the selection by closing before the Take listener runs.
ui.OpenBookList();
foreach (var book in books) book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
ui.OpenBookList();
Check(selection.SelectedCount == 6, "Repeated open does not discard an active selection");
selection.ShowSelectionConfirmation();
Button("TakeButton").onClick.Invoke();
Check(hand.BookCount == 6 && selection.SelectedCount == 0 && !ui.IsBookListOpen && !popup.IsOpen, "Actual Take button transfers six books before closing");
Visible(handHud, true, "Taking books restores hand HUD");

// Placement cannot leak through a modal, even outside its graphic bounds.
hand.SelectBook(0);
Check(hand.TryGetSelectedBook(out var held), "Hand book can be selected");
var slot = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).Single(s => s.SlotIndex == held.CorrectSlotIndex);
var emptySlot = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).First(s => s != slot && s.ShelfColorIndex == slot.ShelfColorIndex);
greenShelf.ShowCompletedImage();
Check(answer.IsOpen && GameplayUIController.IsWorldInputBlocked, "Answer panel blocks world input");
Check(!placement.TryPlaceBook(slot) && hand.BookCount == 6 && !slot.IsFilled, "Answer panel prevents placing through UI");
Check(!placement.CanPlaceSelectedBookOnSlot(slot, out _), "Answer panel suppresses ghost previews");
Visible(shelfHud, false, "Answer hides shelf HUD");
Visible(handHud, false, "Answer hides hand HUD");
ui.Back();
Check(!answer.IsOpen && !GameplayUIController.IsWorldInputBlocked, "Back closes answer and restores world input");
Check(placement.TryPlaceBook(slot) && slot.IsFilled && hand.BookCount == 5, "Book placement still works after closing answer");
Check(slot.IsCorrect(), "Placed book retains its correct slot identity");
greenShelf.ShowCompletedImage();
Check(!placement.TryReturnBookToHand(slot) && slot.IsFilled, "Answer prevents returning a placed book through UI");
ui.Back();
Check(placement.TryReturnBookToHand(slot) && !slot.IsFilled && hand.BookCount == 6, "Returning a book still works after closing answer");

// Opening and closing a list must preserve the underlying zoom.
greenShelf.ZoomShelf();
Check(camera.IsZoomed && back.activeInHierarchy, "Zoom exposes the visible Close button");
Visible(shelfHud, false, "Zoom hides shelf navigation");
ui.OpenBookList();
Check(!back.activeInHierarchy && GameplayUIController.IsWorldInputBlocked, "Book list covers zoom navigation and world input");
Visible(handHud, false, "Book list hides hand HUD");
ui.Back();
Check(!ui.IsBookListOpen && camera.IsZoomed && back.activeInHierarchy, "Back closes list without resetting zoom");
Visible(shelfHud, false, "Closing list while zoomed does not reveal shelf navigation");
Visible(handHud, true, "Closing list restores hand HUD");
back.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(!camera.IsZoomed && !back.activeSelf, "Visible Close button returns to room");
Visible(shelfHud, true, "Returning to room restores shelf navigation");

Button("GreenShelfButton").onClick.Invoke();
Check(action.IsOpen && camera.IsZoomed && !GameplayUIController.IsWorldInputBlocked, "Shelf card directly zooms and shows nonblocking toolbar");
ui.Back();
Check(!action.IsOpen && !camera.IsZoomed && !GameplayUIController.IsWorldInputBlocked, "Back dismisses toolbar and returns to room together");
greenShelf.ZoomShelf();
greenShelf.ShowCompletedImage();
ui.Back();
Check(!answer.IsOpen && camera.IsZoomed && action.IsOpen && back.activeInHierarchy, "Closing answer restores shelf toolbar and underlying zoom");

// Clear locks immediately, before the delayed clear panel is displayed.
Find<GameClearEffectController>().PlayClearEffect();
ui.Back();
ui.OpenBookList();
greenShelf.ShowCompletedImage();
Button("GreenShelfButton").onClick.Invoke();
Check(ui.IsGameCleared && GameplayUIController.IsWorldInputBlocked, "Clear locks gameplay immediately and Back cannot dismiss it");
Check(!ui.IsBookListOpen && !answer.IsOpen && !action.IsOpen && !back.activeSelf, "Clear prevents reopening gameplay panels");
hand.SelectBook(0);
Check(!placement.TryPlaceBook(emptySlot), "Clear prevents further placement");
Visible(shelfHud, false, "Clear hides shelf HUD");
Visible(handHud, false, "Clear hides hand HUD");

return new { passed = checks.Count, checks };
