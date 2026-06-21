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
        //元の配列をコピー
        Sprite[]books = (Sprite[])_bookManager._allBookSprites.Clone();

        // 本のシャッフル
        for (int i = 0; i < books.Length; i++)
        {
            int randomIndex = Random.Range(i, books.Length);

            Sprite temp = books[i];
            books[i] = books[randomIndex];
            books[randomIndex] = temp;
        }

        //本ボタンの生成
        for (int i = 0; i < books.Length; i++)
        {
            BookButton button = Instantiate(_bookButtonPrefab, _content);
            Sprite bookSprite = books[i];
            button.SetUp(bookSprite, _bookPopUpController);
        }
    }
}
