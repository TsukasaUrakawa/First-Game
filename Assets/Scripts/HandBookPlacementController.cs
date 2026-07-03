using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandBookPlacementController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private GameObject[] _bookPrefabs;
    [SerializeField] private Color _selectedColor = new Color(1f, 0.92f, 0.65f, 1f);

    public static HandBookPlacementController Instance { get; private set; }
    public bool IsSelected { get; private set; }

    private Image _bookImage;
    private Color _normalColor = Color.white;

    private void Awake()
    {
        _bookImage = GetComponent<Image>();

        if (_bookImage != null)
        {
            _normalColor = _bookImage.color;
        }

        Instance = this;
        SetSelected(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (!_handItemController.HasBook)
        {
            SetSelected(false);
            return;
        }

        SetSelected(!IsSelected);
        Debug.Log(IsSelected ? "手持ち本を選択" : "手持ち本の選択を解除");
    }

    public bool TryPlaceBook(BookSlot slot)
    {
        if (!IsSelected || !_handItemController.HasBook || slot == null || slot.IsFilled)
        {
            return false;
        }

        Sprite sprite = _handItemController.CurrentBookSprite;
        int correctSlotIndex = _handItemController.CurrentCorrectSlotIndex;
        int bookColorIndex = GetColorIndexFromSpriteName(sprite.name);

        if (bookColorIndex < 0 || bookColorIndex != slot.ShelfColorIndex)
        {
            Debug.Log("この本は別の色の棚には配置できません");
            return false;
        }

        GameObject prefab = GetPrefabFromSpriteName(sprite.name);

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
        bookObject.SetCorrectSlotIndex(correctSlotIndex);
        slot.PlaceBook(book.transform);

        _handItemController.ClearHandBook();
        SetSelected(false);
        return true;
    }

    public bool TryReturnBookToHand(BookSlot slot)
    {
        if (_handItemController.HasBook || slot == null || !slot.IsFilled)
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

        _handItemController.SetHandBook(
            spriteRenderer.sprite,
            bookObject.CorrectSlotIndex
        );

        slot.ClearSlot();
        Destroy(bookObject.gameObject);
        SetSelected(false);
        return true;
    }

    private void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (_bookImage != null)
        {
            _bookImage.color = selected
                ? _selectedColor
                : _normalColor;
        }
    }

    private GameObject GetPrefabFromSpriteName(string spriteName)
    {
        return GetPrefab(GetColorIndexFromSpriteName(spriteName));
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
