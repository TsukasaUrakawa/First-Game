using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonController : MonoBehaviour
{
    [SerializeField] private string _titleSceneName = "TitleScene";
    [SerializeField] private string _gameSceneName = "ShelfCare";

    public void LoadTitleScene()
    {
        SceneManager.LoadScene(_titleSceneName);
    }

    public void LoadGameScene()
    {
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