// Edit mode. Audit every authored Button, including hidden panels.
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Stop Play first.");
int checks = 0, buttonCount = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
foreach (string sceneName in new[] { "TitleScene", "ShelfCare" })
{
    var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity");
    var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
    var audio = all.Select(t => t.GetComponent<SceneAudioSettings>()).Single(c => c != null);
    var serialized = new UnityEditor.SerializedObject(audio);
    var source = (AudioSource)serialized.FindProperty("_effectsSource").objectReferenceValue;
    Check(source != null && serialized.FindProperty("_clickSound").objectReferenceValue != null && audio.ClickDuration > 0, sceneName + " has UI feedback audio");
    Check(serialized.FindProperty("_musicSources").GetArrayElementAtIndex(0).objectReferenceValue != source, "Clicks use an effect source, separate from music");
    var settings = new UnityEditor.SerializedObject(audio.GetComponent<SettingsPanelController>());
    Check(settings.FindProperty("_uiAudio").objectReferenceValue == audio && settings.FindProperty("_effectsSource").objectReferenceValue == source, "Settings uses the same effect gain/mute");
    foreach (var button in all.Select(t => t.GetComponent<UnityEngine.UI.Button>()).Where(b => b != null))
    {
        var callbacks = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Select(i => button.onClick.GetPersistentTarget(i)?.GetType().Name + "." + button.onClick.GetPersistentMethodName(i)).ToArray();
        bool click = callbacks.Contains("SceneAudioSettings.PlayClick");
        bool existing = callbacks.Any(c => new[] { "TitleSceneController.StartGame", "TitleSceneController.QuitGame", "TitleSceneController.StartNewGame", "TitleSceneController.ConfirmNewGame", "SettingsPanelController.PreviewSound", "BookPopUpController.PlayClickSE", "BookShelfZoomButton.OpenActionMenu", "BookSelectionController.ReturnToSelection", "ShelfBookMoveController.ReturnSelectedToHand", "SceneButtonController.LoadTitleSceneWithSound", "SceneButtonController.LoadGameSceneWithSound", "PauseMenuController.Open", "PauseMenuController.Resume", "PauseMenuController.SaveAndReturnToTitle" }.Contains(c));
        bool bookListAction = button.name == "ConfirmSelectionButton" || button.name == "ClearSelectionButton";
        Check(click || existing || bookListAction, sceneName + "/" + button.name + " has audio feedback");
        Check(callbacks.Count(c => c == "SceneAudioSettings.PlayClick") <= 1 && !(click && existing), button.name + " has no duplicate generic click");
        buttonCount++;
    }
    if (sceneName == "ShelfCare")
    {
        var selection = new UnityEditor.SerializedObject(all.Select(t => t.GetComponent<BookSelectionController>()).Single(c => c != null));
        var popup = new UnityEditor.SerializedObject(selection.FindProperty("_bookPopUpController").objectReferenceValue);
        foreach (string field in new[] { "_clickSound", "_bookSelectSound", "_bookDeselectSound", "_bookLimitSound" })
            Check(popup.FindProperty(field).objectReferenceValue != null, "Book selection feedback " + field);
        var transition = new UnityEditor.SerializedObject(all.Select(t => t.GetComponent<SceneButtonController>()).Single(c => c != null));
        Check(transition.FindProperty("_uiAudio").objectReferenceValue == audio && transition.FindProperty("_transitionButtons").arraySize == 2, "Both clear actions share the guarded transition");
    }
    Check(!scene.isDirty, "Audit does not change scene");
}
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/TitleScene.unity");
return new { passed = checks, buttons = buttonCount };
