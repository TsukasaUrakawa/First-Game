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
    private readonly System.Collections.Generic.HashSet<string> _ownedBooks = new System.Collections.Generic.HashSet<string>();

    public void ExcludeOwnedBooks(System.Collections.Generic.IEnumerable<string> names)
    {
        _ownedBooks.UnionWith(names);
        foreach (var button in _content.GetComponentsInChildren<BookButton>(true))
            if (button.BookSprite != null && _ownedBooks.Contains(button.BookSprite.name))
                Destroy(button.gameObject);
    }

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
            if (_ownedBooks.Contains(bookSprite.name)) continue;
            BookButton button = Instantiate(_bookButtonPrefab, _content);
            button.SetUp(bookSprite, _bookSelectionController);
        }
    }
}
