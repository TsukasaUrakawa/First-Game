// Show Clear in Play, wait for its fade, and select TitleButton or ReplayButton
// in EventSystem first. Back up preferences. Verify the destination in a later call.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play.");
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
GameObject Named(string n) => UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == n).gameObject;
var title = Named("TitleButton").GetComponent<UnityEngine.UI.Button>();
var replay = Named("ReplayButton").GetComponent<UnityEngine.UI.Button>();
var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
Check(selected == title.gameObject || selected == replay.gameObject, "Select a clear action first");
var chosen = selected.GetComponent<UnityEngine.UI.Button>();
Check(chosen.IsActive() && chosen.IsInteractable(), "Clear action is available");
var source = (AudioSource)new UnityEditor.SerializedObject(UnityEngine.Object.FindFirstObjectByType<SceneAudioSettings>()).FindProperty("_effectsSource").objectReferenceValue;
GamePreferences.SetMusicVolume(0); GamePreferences.SetEffectsVolume(.25f); GamePreferences.SetMuted(false);
UnityEngine.Object.FindFirstObjectByType<SettingsPanelController>().SaveNow();
UnityEngine.Object.FindFirstObjectByType<GameProgressController>().SaveNow();
string progress = PlayerPrefs.GetString(GameProgressStore.PreferenceKey);
string preferences = PlayerPrefs.GetString(GamePreferences.PreferenceKey);
Time.timeScale = 0; source.Stop();
chosen.OnSubmit(new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current));
Check(source.isPlaying && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "ShelfCare", "Click starts before unloading its source");
Check(!title.IsInteractable() && !replay.IsInteractable(), "Both clear actions lock immediately");
// Deliberately bypass Selectable's guard to exercise the handler's re-entry guard.
(chosen == title ? replay : title).onClick.Invoke();
return new { passed = checks, destination = chosen == title ? "TitleScene" : "ShelfCare", progress, preferences };
