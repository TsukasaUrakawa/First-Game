using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonController : MonoBehaviour
{
    [SerializeField] private string _titleSceneName = "TitleScene";
    [SerializeField] private string _gameSceneName = "ShelfCare";
    [SerializeField] private SceneAudioSettings _uiAudio;
    [SerializeField] private UnityEngine.UI.Button[] _transitionButtons;
    private bool _isLoading;

    public void LoadTitleSceneWithSound() => BeginButtonTransition(false);
    public void LoadGameSceneWithSound() => BeginButtonTransition(true);

    private void BeginButtonTransition(bool replay)
    {
        if (_isLoading) return;
        _isLoading = true;
        foreach (var button in _transitionButtons) button.interactable = false;
        _uiAudio.PlayClick();
        StartCoroutine(LoadAfterClick(replay));
    }

    private System.Collections.IEnumerator LoadAfterClick(bool replay)
    {
        // Keep the scene's effect source alive until the short click has finished.
        yield return new WaitForSecondsRealtime(_uiAudio.ClickDuration + .05f);
        if (replay) LoadGameScene();
        else LoadTitleScene();
    }

    public void LoadTitleScene()
    {
        FindFirstObjectByType<GameProgressController>()?.SaveNow();
        SceneManager.LoadScene(_titleSceneName);
    }

    public void LoadGameScene()
    {
        GameProgressStore.RequestNewGame();
        SceneManager.LoadScene(_gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("ゲーム終了");
#else
        Application.Quit();
#endif
    }
}
