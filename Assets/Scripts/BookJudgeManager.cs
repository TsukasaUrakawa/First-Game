using UnityEngine;
using UnityEngine.Events;

public class BookJudgeManager : MonoBehaviour
{
    [SerializeField] private BookShelfJudge[] _shelves;
    [SerializeField] private GameObject _clearPanel;
    [SerializeField] private bool _hideClearPanelOnStart = true;
    [SerializeField] private UnityEvent _onGameClear;

    private bool _isGameCleared;

    private void Awake()
    {
        if (_shelves == null || _shelves.Length == 0)
        {
            _shelves = FindObjectsByType<BookShelfJudge>(FindObjectsSortMode.None);
        }

        if (_clearPanel != null && _hideClearPanelOnStart)
        {
            _clearPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 全本棚をまとめて判定します。
    /// デバッグ用ボタンや一括判定ボタンから呼ぶ想定です。
    /// </summary>
    public void CheckAllShelves()
    {
        foreach (BookShelfJudge shelf in _shelves)
        {
            if (shelf == null)
            {
                continue;
            }

            shelf.CheckShelf();
        }

        CheckGameClear();
    }

    /// <summary>
    /// エフェクトを出さずに、ゲーム全体がクリア状態か調べます。
    /// </summary>
    public bool CheckGameClear()
    {
        if (_isGameCleared)
        {
            return true;
        }

        if (_shelves == null || _shelves.Length == 0)
        {
            Debug.LogWarning("判定対象の本棚が登録されていません");
            return false;
        }

        foreach (BookShelfJudge shelf in _shelves)
        {
            if (shelf == null || !shelf.IsShelfCorrect())
            {
                Debug.Log("まだ完成していない本棚があります");
                return false;
            }
        }

        CompleteGame();
        return true;
    }

    private void CompleteGame()
    {
        if (_isGameCleared)
        {
            return;
        }

        _isGameCleared = true;

        if (_clearPanel != null)
        {
            _clearPanel.SetActive(true);
        }

        _onGameClear?.Invoke();
        Debug.Log("すべての本棚が完成しました。ゲームクリア！");
    }
}
