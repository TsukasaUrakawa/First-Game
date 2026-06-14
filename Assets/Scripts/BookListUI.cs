using UnityEngine;
using UnityEngine.UI;

public class BookListUI : MonoBehaviour
{
    [Header("本のプレハブ")]
    [SerializeField] private GameObject BookItemPrefab;

    [Header("Content")]
    [SerializeField] private Transform Content;

    private void Start()
    {
        CreateBooks();
    }

    void CreateBooks()
    {
        // Booksフォルダ内の画像を全部取得
        Sprite[] books = Resources.LoadAll<Sprite>("Books");

        foreach (Sprite sprite in books)
        {
            GameObject item = Instantiate(BookItemPrefab, Content);

            Image image = item.transform.Find("BookImage")
                                        .GetComponent<Image>();

            image.sprite = sprite;
        }
    }
}