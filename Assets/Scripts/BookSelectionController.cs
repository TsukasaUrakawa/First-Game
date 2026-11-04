using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本の選択を管理
/// </summary>
public class BookSelectionController : MonoBehaviour
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private BookPopUpController _bookPopUpController;
    [SerializeField] private GameObject _bookListPanel;
    [SerializeField] private GameplayUIController _gameplayUI;
    [SerializeField] private BookListUI _bookList;

    private readonly List<BookButton> _selectedBooks = new List<BookButton>();

    public int SelectedCount => _selectedBooks.Count;
    public int SelectionLimit => _handItemController.AvailableSpace;
    public bool IsConfirmationOpen => _bookPopUpController.IsOpen;
    public event System.Action SelectionChanged;

    public int CountSelectedOutsideColor(int colorIndex)
    {
        if (colorIndex < 0) return 0;
        int count = 0;
        foreach (var button in _selectedBooks)
            if (button != null && BookColorUtility.GetColorIndexFromSpriteName(button.BookSprite.name) != colorIndex) count++;
        return count;
    }

    private void Update()
    {
        if (_gameplayUI.IsSettingsOpen || _selectedBooks.Count == 0 || _bookPopUpController.IsOpen || Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (_bookListPanel != null && !_bookListPanel.activeInHierarchy)
        {
            return;
        }

        //マウスの右クリックで選択した本のポップアップを表示
        if (Input.GetMouseButtonDown(1))
        {
            ShowSelectionConfirmation();
        }
    }

    public void ShowSelectionConfirmation()
    {
        if (!_gameplayUI.IsSettingsOpen && _selectedBooks.Count > 0 && _bookListPanel.activeInHierarchy && !_bookPopUpController.IsOpen)
        {
            _bookPopUpController.ShowSelectedBooks(_selectedBooks);
            _bookPopUpController.PlayClickSE();
        }
    }

    public void ToggleSelection(BookButton bookButton)
    {
        if (_gameplayUI.IsSettingsOpen || bookButton == null || !bookButton.isActiveAndEnabled || !_bookListPanel.activeInHierarchy || _bookPopUpController.IsOpen)
        {
            return;
        }

        if (_selectedBooks.Contains(bookButton))
        {
            _selectedBooks.Remove(bookButton);
            bookButton.SetSelected(false);
            _bookPopUpController.PlayBookDeselectSE();
            SelectionChanged?.Invoke();
            return;
        }

        if (_selectedBooks.Count >= SelectionLimit)
        {
            _bookPopUpController.PlayBookLimitSE();
            return;
        }

        _selectedBooks.Add(bookButton);
        bookButton.SetSelected(true);
        _bookPopUpController.PlayBookSelectSE();
        SelectionChanged?.Invoke();
    }

    /// <summary>
    /// 選択した本を手持ちに追加
    /// </summary>
    public void TakeSelectedBooks()
    {
        if (_gameplayUI.IsSettingsOpen || _selectedBooks.Count == 0)
        {
            return;
        }

        if (_selectedBooks.Count > _handItemController.AvailableSpace)
        {
            _bookPopUpController.PlayBookLimitSE();
            return;
        }

        foreach (BookButton button in _selectedBooks)
        {
            if (!BookColorUtility.TryGetBookIndex(button.BookSprite.name, out _))
            {
                Debug.LogError($"本の名前から正解スロットを取得できません: {button.BookSprite.name}");
                return;
            }
        }

        var takenNames = new List<string>();
        foreach (BookButton button in _selectedBooks)
        {
            Sprite sprite = button.BookSprite;
            BookColorUtility.TryGetBookIndex(sprite.name, out int correctSlotIndex);

            if (_handItemController.TryAddBook(sprite, correctSlotIndex))
            {
                takenNames.Add(sprite.name);
                if (_bookList == null) Destroy(button.gameObject);
            }
        }

        _selectedBooks.Clear();
        _bookList?.ExcludeOwnedBooks(takenNames);
        _bookPopUpController.CloseBookPopUp();
        _gameplayUI.CloseBookList();
    }

    /// <summary>
    /// キャンセルボタンを押したときの処理
    /// </summary>
    public void CancelSelection()
    {
        ClearSelection();
        _bookPopUpController.PlayClickSE();
    }

    public void ReturnToSelection()
    {
        _bookPopUpController.CloseBookPopUp();
        _bookPopUpController.PlayClickSE();
    }

    public void ClearSelection()
    {
        foreach (BookButton button in _selectedBooks)
        {
            if (button != null)
            {
                button.SetSelected(false);
            }
        }

        _selectedBooks.Clear();
        _bookPopUpController.CloseBookPopUp();
        SelectionChanged?.Invoke();
    }

}
