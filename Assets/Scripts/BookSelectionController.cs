using System.Collections.Generic;
using UnityEngine;

public class BookSelectionController : MonoBehaviour
{
    private const int MaxSelection = 5;

    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private BookPopUpController _bookPopUpController;
    [SerializeField] private GameObject _bookListPanel;

    private readonly List<BookButton> _selectedBooks = new List<BookButton>();

    public int SelectedCount => _selectedBooks.Count;

    private void Update()
    {
        if (_selectedBooks.Count == 0 || _bookPopUpController.IsOpen)
        {
            return;
        }

        if (_bookListPanel != null && !_bookListPanel.activeInHierarchy)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            _bookPopUpController.ShowSelectedBooks(_selectedBooks);
        }
    }

    public void ToggleSelection(BookButton bookButton)
    {
        if (bookButton == null || _bookPopUpController.IsOpen)
        {
            return;
        }

        if (_selectedBooks.Contains(bookButton))
        {
            _selectedBooks.Remove(bookButton);
            bookButton.SetSelected(false);
            _bookPopUpController.PlayBookSelectSE();
            return;
        }

        int selectableCount = Mathf.Min(MaxSelection, _handItemController.AvailableSpace);

        if (_selectedBooks.Count >= selectableCount)
        {
            Debug.Log("これ以上本を選択できません");
            return;
        }

        _selectedBooks.Add(bookButton);
        bookButton.SetSelected(true);
        _bookPopUpController.PlayBookSelectSE();
    }

    public void TakeSelectedBooks()
    {
        if (_selectedBooks.Count == 0)
        {
            return;
        }

        if (_selectedBooks.Count > _handItemController.AvailableSpace)
        {
            Debug.Log("手持ちアイテム欄の空きが足りません");
            return;
        }

        foreach (BookButton button in _selectedBooks)
        {
            if (GetSlotIndexFromSpriteName(button.BookSprite.name) < 0)
            {
                Debug.LogError($"本の名前から正解スロットを取得できません: {button.BookSprite.name}");
                return;
            }
        }

        foreach (BookButton button in _selectedBooks)
        {
            Sprite sprite = button.BookSprite;
            int correctSlotIndex = GetSlotIndexFromSpriteName(sprite.name);

            if (_handItemController.TryAddBook(sprite, correctSlotIndex))
            {
                Destroy(button.gameObject);
            }
        }

        _selectedBooks.Clear();
        _bookPopUpController.PlayTakeBookSE();
        _bookPopUpController.CloseBookPopUp();
    }

    public void CancelSelection()
    {
        foreach (BookButton button in _selectedBooks)
        {
            if (button != null)
            {
                button.SetSelected(false);
            }
        }

        _selectedBooks.Clear();
        _bookPopUpController.PlayClickSE();
        _bookPopUpController.CloseBookPopUp();
    }

    private int GetSlotIndexFromSpriteName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName) || spriteName.Length < 2)
        {
            return -1;
        }

        string numberPart = spriteName.Substring(spriteName.Length - 2);

        if (!int.TryParse(numberPart, out int number))
        {
            return -1;
        }

        int offset;

        if (spriteName.Contains("Green")) offset = 0;
        else if (spriteName.Contains("Blue")) offset = 18;
        else if (spriteName.Contains("Beige")) offset = 35;
        else if (spriteName.Contains("Red")) offset = 53;
        else if (spriteName.Contains("Purple")) offset = 70;
        else if (spriteName.Contains("Brown")) offset = 88;
        else if (spriteName.Contains("White")) offset = 105;
        else if (spriteName.Contains("Black")) offset = 123;
        else return -1;

        return offset + number - 1;
    }
}
