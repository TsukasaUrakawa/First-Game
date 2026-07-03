using UnityEngine;
using UnityEngine.UI;

public class HandItemController : MonoBehaviour
{
    [SerializeField] private Image _bookImage;

    private Sprite _currentBookSprite;
    private int _currentCorrectSlotIndex;
    private bool _hasBook;

    public bool HasBook => _hasBook;
    public Sprite CurrentBookSprite => _currentBookSprite;
    public int CurrentCorrectSlotIndex => _currentCorrectSlotIndex;

    public void SetHandBook(Sprite sprite, int correctSlotIndex)
    {
        _currentBookSprite = sprite;
        _currentCorrectSlotIndex = correctSlotIndex;
        _hasBook = true;

        _bookImage.sprite = sprite;
        _bookImage.gameObject.SetActive(true);
    }

    public void ClearHandBook()
    {
        _currentBookSprite = null;
        _currentCorrectSlotIndex = 0;
        _hasBook = false;

        _bookImage.sprite = null;
        _bookImage.gameObject.SetActive(false);
    }
}