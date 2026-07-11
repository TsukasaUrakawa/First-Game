using UnityEngine;

public class HandBookPlacementController : MonoBehaviour
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private GameObject[] _bookPrefabs;
    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _placeSound;
    [SerializeField, Range(0f, 1f)] private float _placeSoundVolume = 1f;

    public static HandBookPlacementController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null && _placeSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
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
            _handItemController == null ||
            !_handItemController.TryGetSelectedBook(out HandBookData handBook))
        {
            return false;
        }

        Sprite sprite = handBook.Sprite;

        if (sprite == null)
        {
            return false;
        }

        int bookColorIndex =
            BookColorUtility.GetColorIndexFromSpriteName(sprite.name);

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
        PlayPlaceSound();
        _handItemController.RemoveSelectedBook();
        return true;
    }

    public bool CanPlaceSelectedBookOnSlot(
        BookSlot slot,
        out HandBookData handBook)
    {
        handBook = null;

        if (slot == null || slot.IsFilled || _handItemController == null)
        {
            return false;
        }

        if (!_handItemController.TryGetSelectedBook(out handBook) ||
            handBook?.Sprite == null)
        {
            return false;
        }

        int bookColorIndex =
            BookColorUtility.GetColorIndexFromSpriteName(
                handBook.Sprite.name
            );

        return bookColorIndex >= 0 &&
               bookColorIndex == slot.ShelfColorIndex;
    }

    public bool TryReturnBookToHand(BookSlot slot)
    {
        if (_handItemController == null ||
            _handItemController.IsFull ||
            slot == null ||
            !slot.IsFilled)
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

    private GameObject GetPrefab(int index)
    {
        if (index < 0 || index >= _bookPrefabs.Length)
        {
            return null;
        }

        return _bookPrefabs[index];
    }

    private void PlayPlaceSound()
    {
        if (_audioSource != null && _placeSound != null)
        {
            _audioSource.PlayOneShot(_placeSound, _placeSoundVolume);
        }
    }
}
