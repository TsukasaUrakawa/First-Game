// In a fresh Play session, open the list, let a frame create its books, close it,
// then run this file. The saved tutorial preference is restored even on failure.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play mode.");
var checks = new System.Collections.Generic.List<string>();
void Check(bool ok, string message) { if (!ok) throw new System.Exception("FAIL: " + message); checks.Add(message); }
T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>(UnityEngine.FindObjectsInactive.Include);
UnityEngine.GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<UnityEngine.Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
UnityEngine.UI.Button Button(string name) => Named(name).GetComponent<UnityEngine.UI.Button>();
TMPro.TMP_Text Label(string name) => Named(name).GetComponent<TMPro.TMP_Text>();
var tutorial = Find<BookPlacementTutorial>();
var ui = Find<GameplayUIController>();
var hand = Find<HandItemController>();
var selection = Find<BookSelectionController>();
var popup = Find<BookPopUpController>();
var camera = Find<CameraController>();
var placement = Find<HandBookPlacementController>();
var boxRenderer = Find<BoxOpenAndClose>().GetComponentInChildren<UnityEngine.Renderer>();
var boxMaterials = boxRenderer.sharedMaterials;
var originalBoxColors = boxMaterials.Select(m => m.GetColor("_BaseColor")).ToArray();
bool BoxColorsRestored()
{
    var properties = new UnityEngine.MaterialPropertyBlock();
    for (int i = 0; i < boxMaterials.Length; i++)
    {
        boxRenderer.GetPropertyBlock(properties, i);
        if (!properties.isEmpty || boxMaterials[i].GetColor("_BaseColor") != originalBoxColors[i]) return false;
    }
    return true;
}
bool hadPreference = UnityEngine.PlayerPrefs.HasKey(BookPlacementTutorial.PreferenceKey);
int savedPreference = UnityEngine.PlayerPrefs.GetInt(BookPlacementTutorial.PreferenceKey, 0);
void Step(BookPlacementTutorial.Step step, string message)
{
    Check(tutorial.IsRunning && tutorial.CurrentStep == step && Named("TutorialPanel").activeInHierarchy, message);
    foreach (var label in Named("TutorialPanel").GetComponentsInChildren<TMPro.TMP_Text>())
    {
        label.ForceMeshUpdate();
        Check(!label.isTextOverflowing && label.text.Where(c => !char.IsControl(c)).All(c => label.font.HasCharacter(c)), message + " / " + label.name + " fits and has glyphs");
    }
}
try
{
    Check(hand.BookCount == 0 && !ui.IsBookListOpen && !camera.IsZoomed, "Fresh scene is in the room");
    Check(tutorial.IsRunning == (savedPreference == 0), "Startup follows the saved tutorial preference");
    tutorial.BeginTutorial();
    Step(BookPlacementTutorial.Step.OpenBox, "Guide starts at the box");
    Check(!GameplayUIController.IsWorldInputBlocked, "Guide does not make gameplay modal");
    Check(Named("TutorialFocus").GetComponentsInChildren<UnityEngine.UI.Graphic>().All(g => !g.raycastTarget), "Focus outline never intercepts clicks");
    Button("GreenShelfButton").onClick.Invoke();
    Step(BookPlacementTutorial.Step.OpenBox, "Opening a shelf before taking books gives recovery guidance");
    Check(Label("TutorialBody").text.Contains("部屋に戻る"), "Recovery directs the player back to the room");
    ui.Back();
    Button("TutorialDismissButton").onClick.Invoke();
    Check(!tutorial.IsRunning && Named("TutorialHelpButton").activeInHierarchy && UnityEngine.PlayerPrefs.GetInt(BookPlacementTutorial.PreferenceKey) == 1, "Skip hides the guide and records that it was seen");
    Check(!Named("TutorialDimmer").activeInHierarchy && BoxColorsRestored(), "Skip removes the dimmer and restores all box materials");
    Button("TutorialHelpButton").onClick.Invoke();
    Step(BookPlacementTutorial.Step.OpenBox, "Help button reopens the guide");
    ui.OpenBookList();
    Step(BookPlacementTutorial.Step.SelectBook, "Opening the list advances to choosing a book");
    Check(Named("TutorialDimmer").activeInHierarchy && BoxColorsRestored(), "Changing target restores box colors while keeping list spotlight");
    Check(!Named("TutorialHelpButton").activeInHierarchy, "Help button does not cover the list Close button");
    var book = UnityEngine.Object.FindObjectsByType<BookButton>(UnityEngine.FindObjectsSortMode.None).Single(b => b.BookSprite.name.Contains("Green") && b.BookSprite.name.EndsWith("01"));
    book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Step(BookPlacementTutorial.Step.ConfirmBooks, "Selecting a book advances to confirmation");
    book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Step(BookPlacementTutorial.Step.SelectBook, "Deselecting returns to choosing a book");
    book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Button("ConfirmSelectionButton").onClick.Invoke();
    Step(BookPlacementTutorial.Step.TakeBooks, "Opening confirmation explains how to take the book");
    ui.Back();
    Step(BookPlacementTutorial.Step.ConfirmBooks, "Back returns to confirmation guidance with selection preserved");
    Check(selection.SelectedCount == 1 && book.IsSelected, "Tutorial does not clear the selected book");
    ui.Back();
    Step(BookPlacementTutorial.Step.OpenBox, "Closing the list returns to the box step");
    ui.OpenBookList();
    book.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Button("ConfirmSelectionButton").onClick.Invoke();
    Button("TakeButton").onClick.Invoke();
    Check(hand.BookCount == 1 && !popup.IsOpen && !ui.IsBookListOpen, "Actual Take transfers the tutorial book");
    Step(BookPlacementTutorial.Step.SelectHandBook, "Taking a book advances to hand selection");
    var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left };
    Named("HandItemSlot0").GetComponent<HandItemSlotUI>().OnPointerClick(pointer);
    Step(BookPlacementTutorial.Step.ChooseShelf, "Selecting a hand book identifies its shelf");
    Check(Label("TutorialBody").text.Contains("緑"), "Guide uses the book's color");
    Button("BlueShelfButton").onClick.Invoke();
    Step(BookPlacementTutorial.Step.ChooseShelf, "Wrong shelf gives a route back instead of advancing");
    Check(Label("TutorialBody").text.Contains("部屋に戻る"), "Wrong-shelf guidance points to Back");
    var wrongSlot = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).First(s => s.ShelfColorIndex == 1);
    Check(!placement.TryPlaceBook(wrongSlot) && tutorial.CurrentStep != BookPlacementTutorial.Step.Completed, "Rejected placement cannot complete the tutorial");
    ui.Back();
    Button("GreenShelfButton").onClick.Invoke();
    Step(BookPlacementTutorial.Step.PlaceBook, "Correct shelf advances to actual placement");
    Button("AnswerButton").onClick.Invoke();
    Check(!Named("TutorialPanel").activeInHierarchy && !Named("TutorialFocus").activeInHierarchy, "Completion image temporarily hides the guide");
    Check(!Named("TutorialDimmer").activeInHierarchy, "Completion image is not darkened by the tutorial");
    ui.Back();
    Step(BookPlacementTutorial.Step.PlaceBook, "Closing completion image restores placement guidance");
    hand.SelectBook(0);
    Step(BookPlacementTutorial.Step.SelectHandBook, "Deselecting a hand book returns to hand guidance");
    hand.SelectBook(0);
    ui.OpenBookList();
    var target = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).Single(s => s.SlotIndex == 0);
    Check(!placement.TryPlaceBook(target) && !target.IsFilled, "Tutorial does not bypass a modal's placement guard");
    ui.Back();
    Step(BookPlacementTutorial.Step.PlaceBook, "Closing a detour list restores the same shelf and hand selection");
    Check(placement.TryPlaceBook(target) && target.IsCorrect(), "Book is placed through the real gameplay controller");
    Step(BookPlacementTutorial.Step.Completed, "Successful placement completes the guide");
    Check(!Named("TutorialDimmer").activeInHierarchy && BoxColorsRestored(), "Successful placement clears all highlighting");
    Check(UnityEngine.PlayerPrefs.GetInt(BookPlacementTutorial.PreferenceKey) == 1 && hand.BookCount == 0, "Completion records progress without replacing game inventory");
    Button("TutorialDismissButton").onClick.Invoke();
    Check(!tutorial.IsRunning && Named("TutorialHelpButton").activeInHierarchy, "Completed guide can be closed");
    Button("TutorialHelpButton").onClick.Invoke();
    Check(target.IsFilled && hand.BookCount == 0 && camera.IsZoomed, "Replay preserves shelf, inventory and camera");
    Check(placement.TryReturnBookToHand(target), "Existing book can be returned while the guide is active");
    Step(BookPlacementTutorial.Step.SelectHandBook, "Returning a book is not mistaken for successful placement");
    hand.SelectBook(0);
    Check(placement.TryPlaceBook(target) && tutorial.CurrentStep == BookPlacementTutorial.Step.Completed, "Replay also completes on a real placement");
    tutorial.Dismiss();
    // Returning an endpoint while another book is selected expands the focus
    // to include that newly freed position after ClearSlot has actually run.
    var other = Find<BookManager>()._allBookSprites.Single(s => s.name.Contains("Green") && s.name.EndsWith("02"));
    hand.TryAddBook(other, 1); hand.SelectBook(0); tutorial.BeginTutorial();
    var focusCorners = new UnityEngine.Vector3[4];
    Named("TutorialFocus").GetComponent<UnityEngine.RectTransform>().GetWorldCorners(focusCorners);
    float previousLeft = focusCorners[0].x;
    Check(placement.TryReturnBookToHand(target) && hand.SelectedIndex == 0, "Returning another book preserves the currently selected book");
    Named("TutorialFocus").GetComponent<UnityEngine.RectTransform>().GetWorldCorners(focusCorners);
    Check(focusCorners[0].x < previousLeft - .1f && tutorial.CurrentStep == BookPlacementTutorial.Step.PlaceBook, "Focus immediately includes the freed shelf endpoint");
    hand.RemoveSelectedBook(); hand.SelectBook(0);
    Check(placement.TryPlaceBook(target) && tutorial.CurrentStep == BookPlacementTutorial.Step.Completed, "Guide still completes after returning another book");
    tutorial.Dismiss();
    ui.Back();
    var sprites = Find<BookManager>()._allBookSprites.Where(s => s.name.Contains("Blue")).Take(9).ToArray();
    foreach (var sprite in sprites) hand.TryAddBook(sprite, 18 + int.Parse(sprite.name.Substring(sprite.name.Length - 2)) - 1);
    tutorial.BeginTutorial();
    ui.OpenBookList();
    Step(BookPlacementTutorial.Step.CloseList, "A full hand gives instructions to close the list");
    tutorial.Dismiss();
    Check(ui.IsBookListOpen && hand.BookCount == 9 && GameplayUIController.IsWorldInputBlocked, "Skipping preserves the open list and existing inventory");
    ui.Back();
    tutorial.BeginTutorial();
    Step(BookPlacementTutorial.Step.SelectHandBook, "Replay with books already held resumes at hand selection");
    Find<GameClearEffectController>().PlayClearEffect();
    tutorial.BeginTutorial();
    Check(!tutorial.IsRunning && !Named("TutorialPanel").activeInHierarchy && !Named("TutorialHelpButton").activeInHierarchy, "Clear closes the guide and prevents reopening");
    Check(!Named("TutorialDimmer").activeInHierarchy, "Game clear removes the dimmer");
}
finally
{
    if (hadPreference) UnityEngine.PlayerPrefs.SetInt(BookPlacementTutorial.PreferenceKey, savedPreference);
    else UnityEngine.PlayerPrefs.DeleteKey(BookPlacementTutorial.PreferenceKey);
    UnityEngine.PlayerPrefs.Save();
}
return new { passed = checks.Count, checks };
