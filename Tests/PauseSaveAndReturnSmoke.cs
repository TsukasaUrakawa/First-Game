// Gameplay fixture ready. Backs up no preferences; use the backup helpers first.
// Leaves via the real Save button. Verify Title and Continue in later calls.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play.");
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>(UnityEngine.FindObjectsInactive.Include);
GameObject Named(string n) => UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == n).gameObject;
var hand = Find<HandItemController>(); var slots = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).OrderBy(s => s.SlotIndex).ToArray();
var pause = Find<PauseMenuController>(); var movement = Find<ShelfBookMoveController>(); bool inflight = false;
if (!pause.IsOpen && movement.HasSelection)
{
    var origin = movement.SelectedSlot;
    var target = origin.ShelfJudge.GetComponentsInChildren<BookSlot>(true).FirstOrDefault(s => s != origin && origin.CanMoveOrSwapTo(s));
    Check(target != null && movement.TryMoveSelectedBookTo(target), "Make a final transfer in this frame before saving");
    inflight = true;
}
var expected = new GameProgressData { version = 1, hand = hand.Books.Select(b => b.Sprite.name).ToArray(),
    placed = slots.Where(s => s.IsFilled).Select(s => new SavedBookPlacement { book = s.PlacedBook.Sprite.name, slot = s.SlotIndex }).ToArray(),
    completedShelves = UnityEngine.Object.FindObjectsByType<BookShelfJudge>(UnityEngine.FindObjectsSortMode.None).Where(s => s.IsCompleted).Select(s => s.GetComponentsInChildren<BookSlot>(true)[0].ShelfColorIndex).OrderBy(c => c).ToArray(), cleared = false };
Check(expected.IsValid(), "Live fixture is valid");
System.IO.File.WriteAllText(".utmp/save-smoke-expected.json", JsonUtility.ToJson(expected));
GamePreferences.SetMusicVolume(.25f); GamePreferences.SetEffectsVolume(.6f); GamePreferences.SetMuted(true); GamePreferences.SetReducedMotion(true);
if (!pause.IsOpen) Named("OpenPauseButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(pause.IsOpen && Time.timeScale == 0, "Open Pause from the current view");
var button = Named("SaveReturnTitleButton").GetComponent<UnityEngine.UI.Button>();
button.OnSubmit(new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current));
Check(!button.IsInteractable() && !Named("ResumePlayButton").GetComponent<UnityEngine.UI.Button>().IsInteractable(), "Both actions lock during the transition");
pause.SaveAndReturnToTitle(); Find<GameplayUIController>().Back();
Check(pause.IsOpen && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "ShelfCare", "Repeat submit and Back cannot interrupt or repeat the transition");
Check(GameProgressStore.Read(out var saved) == ProgressReadResult.Loaded && JsonUtility.ToJson(saved) == JsonUtility.ToJson(expected), "Save flushes the full live layout before leaving");
Check(!GamePreferences.IsDirty && GamePreferences.TryParse(PlayerPrefs.GetString(GamePreferences.PreferenceKey), out var preferences) && preferences.music == .25f && preferences.effects == .6f && preferences.muted && preferences.reducedMotion, "Settings save independently before leaving");
return new { passed = checks, hand = expected.hand.Length, placed = expected.placed.Length, completed = expected.completedShelves.Length, inflight };
