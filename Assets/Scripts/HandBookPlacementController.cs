using UnityEngine;

public class HandBookPlacementController : MonoBehaviour
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private GameObject[] _bookPrefabs;

    public static HandBookPlacementController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryPlaceBook(BookSlot slot)
    {
        if (slot == null || slot.IsFilled ||
            !_handItemController.TryGetSelectedBook(out HandBookData handBook))
        {
            return false;
        }

        Sprite sprite = handBook.Sprite;
        int bookColorIndex = GetColorIndexFromSpriteName(sprite.name);

        if (bookColorIndex < 0 || bookColorIndex != slot.ShelfColorIndex)
        {
            Debug.Log("この本は別の色の棚には配置できません");
            return false;
        }

        GameObject prefab = GetPrefab(bookColorIndex);

        if (prefab == null)
        {
            Debug.LogWarning($"対応する本Prefabが見つかりません: {sprite.name}");
            return false;
        }

        GameObject book = Instantiate(prefab, slot.transform.position, Quaternion.identity);
        BookObject bookObject = book.GetComponent<BookObject>();

        if (bookObject == null)
        {
            Debug.LogError("生成した本PrefabにBookObjectがありません");
            Destroy(book);
            return false;
        }

        bookObject.SetSprite(sprite);
        bookObject.SetCorrectSlotIndex(handBook.CorrectSlotIndex);
        slot.PlaceBook(book.transform);
        _handItemController.RemoveSelectedBook();
        return true;
    }

    public bool TryReturnBookToHand(BookSlot slot)
    {
        if (_handItemController.IsFull || slot == null || !slot.IsFilled)
        {
            return false;
        }

        BookObject bookObject = slot.PlacedBook;

        if (bookObject == null)
        {
            slot.ClearSlot();
            return false;
        }

        SpriteRenderer spriteRenderer = bookObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogError("配置された本からSpriteを取得できません");
            return false;
        }

        if (!_handItemController.TryAddBook(
                spriteRenderer.sprite,
                bookObject.CorrectSlotIndex))
        {
            return false;
        }

        slot.ClearSlot();
        Destroy(bookObject.gameObject);
        return true;
    }

    private int GetColorIndexFromSpriteName(string spriteName)
    {
        if (spriteName.Contains("Green")) return 0;
        if (spriteName.Contains("Blue")) return 1;
        if (spriteName.Contains("Beige")) return 2;
        if (spriteName.Contains("Red")) return 3;
        if (spriteName.Contains("Purple")) return 4;
        if (spriteName.Contains("Brown")) return 5;
        if (spriteName.Contains("White")) return 6;
        if (spriteName.Contains("Black")) return 7;
        return -1;
    }

    private GameObject GetPrefab(int index)
    {
        if (index < 0 || index >= _bookPrefabs.Length)
        {
            return null;
        }

        return _bookPrefabs[index];
    }
}
