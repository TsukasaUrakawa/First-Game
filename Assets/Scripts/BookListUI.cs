using UnityEngine;

/// <summary>
/// 本一覧の管理
/// </summary>
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
            BookButton button = Instantiate(_bookButtonPrefab, _content);
            button.SetUp(bookSprite, _bookSelectionController);
        }
    }
}
