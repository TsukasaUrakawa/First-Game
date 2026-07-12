using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "ShelfCare";

    public void StartGame()
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