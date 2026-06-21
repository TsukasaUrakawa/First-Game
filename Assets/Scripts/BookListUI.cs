using UnityEngine;

public class BookListUI : MonoBehaviour
{
    [SerializeField] BookManager _bookManager;
    [SerializeField] BookPopUpController _bookPopUpController;
    [SerializeField] BookButton _bookButtonPrefab;
    [SerializeField] Transform _content;
    void Start()
    {
        CreateBookButton();
    }

    private void CreateBookButton()
    {
        for (int i = 0; i < _bookManager._allBookSprites.Length; i++)
        {
            BookButton button = Instantiate(_bookButtonPrefab, _content);
            Sprite bookSprite = _bookManager._allBookSprites[i];
            button.SetUp(bookSprite, _bookPopUpController);
        }
    }
}
