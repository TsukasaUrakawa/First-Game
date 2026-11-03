// Hide the guide for this Play session's baseline UI checks; preserve saved data.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play mode.");
var tutorial = UnityEngine.Object.FindFirstObjectByType<BookPlacementTutorial>();
bool hadKey = UnityEngine.PlayerPrefs.HasKey(BookPlacementTutorial.PreferenceKey);
int original = UnityEngine.PlayerPrefs.GetInt(BookPlacementTutorial.PreferenceKey, 0);
try { tutorial.Dismiss(); }
finally
{
    if (hadKey) UnityEngine.PlayerPrefs.SetInt(BookPlacementTutorial.PreferenceKey, original);
    else UnityEngine.PlayerPrefs.DeleteKey(BookPlacementTutorial.PreferenceKey);
    UnityEngine.PlayerPrefs.Save();
}
return new { suspended = !tutorial.IsRunning };
