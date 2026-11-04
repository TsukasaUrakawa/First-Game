// Play in Title or a fresh game. Back up progress/settings first. Uses native UGUI clicks.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play.");
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
GameObject Named(string n) => UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == n).gameObject;
var settings = UnityEngine.Object.FindFirstObjectByType<SettingsPanelController>();
if (settings.IsOpen) settings.Close();
var source = (AudioSource)new UnityEditor.SerializedObject(settings).FindProperty("_effectsSource").objectReferenceValue;
GamePreferences.SetMusicVolume(0); GamePreferences.SetEffectsVolume(1); GamePreferences.SetMuted(false);
float authoredGain = source.volume; GamePreferences.SetEffectsVolume(.25f);
void Click(string name, UnityEngine.EventSystems.PointerEventData.InputButton input = UnityEngine.EventSystems.PointerEventData.InputButton.Left)
{
    Named(name).GetComponent<UnityEngine.UI.Button>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = input });
}
source.Stop(); Click("OpenSettingsButton", UnityEngine.EventSystems.PointerEventData.InputButton.Right);
Check(!settings.IsOpen && !source.isPlaying, "Right click is silent");
var access = Named("OpenSettingsButton").GetComponent<UnityEngine.UI.Button>();
access.interactable = false; Click("OpenSettingsButton");
Check(!settings.IsOpen && !source.isPlaying, "Disabled button is silent"); access.interactable = true;
Click("OpenSettingsButton"); Check(settings.IsOpen && source.isPlaying, "Opening settings plays the click");
source.Stop(); Click("CloseSettingsButton"); Check(!settings.IsOpen && source.isPlaying, "Closing settings plays the click after hiding its panel");
Click("OpenSettingsButton");
foreach (string name in new[] { "MuteAudioToggle", "MuteAudioToggle", "ReducedMotionToggle" })
{
    source.Stop(); var toggle = Named(name).GetComponent<UnityEngine.UI.Toggle>();
    bool before = toggle.isOn;
    toggle.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left });
    Check(toggle.isOn != before && source.isPlaying, name + " toggles and requests feedback");
    Check(source.mute == GamePreferences.Muted && Mathf.Abs(source.volume - authoredGain * .25f) < .0001f, "Feedback follows the chosen effect level and mute");
}
source.Stop(); Named("EffectsVolumeSlider").GetComponent<UnityEngine.UI.Slider>().value = .4f;
Check(!source.isPlaying && Mathf.Abs(source.volume - authoredGain * .4f) < .0001f, "Continuous slider movement does not produce repeated clicks");
settings.Close();
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "ShelfCare")
{
    var ui = GameplayUIController.Instance; ui.OpenBookList();
    for (int i = 0; i < 9; i++)
    {
        source.Stop(); Click("BookColorFilter" + i);
        Check(source.isPlaying && UnityEngine.Object.FindFirstObjectByType<BookListUI>().ActiveColorIndex == i - 1, "Color filter " + i + " plays and switches");
    }
    ui.CloseBookList();
    var hand = UnityEngine.Object.FindFirstObjectByType<HandItemController>();
    var sprite = UnityEngine.Object.FindFirstObjectByType<BookManager>()._allBookSprites.Single(s => s.name == "Green01");
    Check(hand.TryAddBook(sprite, 0), "Prepare one book"); hand.SelectBook(0);
    var slot = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).Single(s => s.SlotIndex == 0);
    Check(HandBookPlacementController.Instance.TryPlaceBook(slot) && ShelfBookMoveController.Instance.ClickBook(slot), "Select the placed book");
    source.Stop(); Click("CancelShelfMoveButton");
    Check(!ShelfBookMoveController.Instance.HasSelection && source.isPlaying, "Shelf selection cancellation plays a click");
}
else
{
    var title = UnityEngine.Object.FindFirstObjectByType<TitleSceneController>(); title.StartNewGame();
    Check(Named("NewGameDialog").activeSelf, "New Game confirmation opens");
    source.Stop(); Click("CancelNewGameButton");
    Check(!Named("NewGameDialog").activeSelf && source.isPlaying, "New Game cancellation plays its click");
}
return new { passed = checks, scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name };
