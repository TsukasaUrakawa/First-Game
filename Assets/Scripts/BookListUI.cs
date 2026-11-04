using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 本一覧の管理
/// </summary>
public class BookListUI : MonoBehaviour
{
    [SerializeField] private BookManager _bookManager;
    [SerializeField] private BookSelectionController _bookSelectionController;
    [SerializeField] private BookButton _bookButtonPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private ScrollRect _scroll;
    private readonly HashSet<string> _ownedBooks = new HashSet<string>();
    private readonly List<BookButton> _buttons = new List<BookButton>();
    private readonly int[] _remainingByColor = new int[BookColorUtility.ShelfCount];
    private bool _countsReady;
    private int _remainingTotal;
    public int ActiveColorIndex { get; private set; } = -1;
    public event System.Action StateChanged;

    public int GetRemainingCount(int colorIndex = -1)
    {
        if (!_countsReady) RefreshCounts();
        return colorIndex == -1 ? _remainingTotal : colorIndex >= 0 && colorIndex < _remainingByColor.Length
            ? _remainingByColor[colorIndex] : 0;
    }

    public void ShowColor(int colorIndex)
    {
        if (colorIndex < -1 || colorIndex >= BookColorUtility.ShelfCount || ActiveColorIndex == colorIndex ||
            !_content.gameObject.activeInHierarchy || _bookSelectionController.IsConfirmationOpen) return;
        ActiveColorIndex = colorIndex;
        ApplyFilter(true);
        StateChanged?.Invoke();
    }

    public void ExcludeOwnedBooks(IEnumerable<string> names)
    {
        _ownedBooks.UnionWith(names);
        for (int i = _buttons.Count - 1; i >= 0; i--)
        {
            var button = _buttons[i];
            if (button == null) { _buttons.RemoveAt(i); continue; }
            if (_ownedBooks.Contains(button.BookSprite.name))
            {
                // Remove it immediately from the layout and counts, before deferred Destroy.
                button.gameObject.SetActive(false);
                _buttons.RemoveAt(i);
                Destroy(button.gameObject);
            }
        }
        RefreshCounts();
        StateChanged?.Invoke();
    }

    private void Start()
    {
        CreateBookButton();
        RefreshCounts();
        ApplyFilter(false);
        StateChanged?.Invoke();
    }

    private void RefreshCounts()
    {
        System.Array.Clear(_remainingByColor, 0, _remainingByColor.Length);
        _remainingTotal = 0;
        var sprites = _bookManager._allBookSprites;
        _countsReady = sprites != null && sprites.Length > 0;
        if (!_countsReady) return;
        foreach (var sprite in sprites)
        {
            if (sprite == null || _ownedBooks.Contains(sprite.name)) continue;
            int color = BookColorUtility.GetColorIndexFromSpriteName(sprite.name);
            if (color < 0 || color >= _remainingByColor.Length) continue;
            _remainingByColor[color]++;
            _remainingTotal++;
        }
    }

    private void ApplyFilter(bool resetScroll)
    {
        foreach (var button in _buttons)
            if (button != null) button.gameObject.SetActive(ActiveColorIndex < 0 ||
                BookColorUtility.GetColorIndexFromSpriteName(button.BookSprite.name) == ActiveColorIndex);
        if (resetScroll && _scroll != null)
        {
            _scroll.StopMovement();
            if (_content is RectTransform rect) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            _scroll.verticalNormalizedPosition = 1f;
        }
    }

    /// <summary>
    /// 本ボタンを生成
    /// </summary>
    private void CreateBookButton()
    {
        //全ての本画像配列のコピーを作る
        Sprite[] books = (Sprite[])_bookManager._allBookSprites.Clone();

        //本画像をランダムに並べ替える
        for (int i = 0; i < books.Length; i++)
        {
            int randomIndex = Random.Range(i, books.Length);
            Sprite temp = books[i];
            books[i] = books[randomIndex];
            books[randomIndex] = temp;
        }

        //並び替え後の本ボタンを生成
        foreach (Sprite bookSprite in books)
        {
            if (_ownedBooks.Contains(bookSprite.name)) continue;
            BookButton button = Instantiate(_bookButtonPrefab, _content);
            button.SetUp(bookSprite, _bookSelectionController);
            _buttons.Add(button);
        }
    }
}
