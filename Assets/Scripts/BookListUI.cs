using UnityEngine;

public class BookListUI : MonoBehaviour
{
    [SerializeField] private BookManager _bookManager;
    [SerializeField] private BookSelectionController _bookSelectionController;
    [SerializeField] private BookButton _bookButtonPrefab;
    [SerializeField] private Transform _content;

    private void Start()
    {
        CreateBookButton();
    }

    private void CreateBookButton()
    {
        Sprite[] books = (Sprite[])_bookManager._allBookSprites.Clone();

        for (int i = 0; i < books.Length; i++)
        {
            int randomIndex = Random.Range(i, books.Length);
            Sprite temp = books[i];
            books[i] = books[randomIndex];
            books[randomIndex] = temp;
        }

        foreach (Sprite bookSprite in books)
        {
            BookButton button = Instantiate(_bookButtonPrefab, _content);
            button.SetUp(bookSprite, _bookSelectionController);
        }
    }
}
