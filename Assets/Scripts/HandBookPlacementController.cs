using UnityEngine;

public class HandBookPlacementController : MonoBehaviour
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private GameObject[] _bookPrefabs;
    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _placeSound;
    [SerializeField] private AudioClip _returnSound;
    [SerializeField] private AudioClip _wrongShelfSound;
    [SerializeField, Range(0f, 1f)] private float _placeSoundVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _returnSoundVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _wrongShelfSoundVolume = 1f;

    public static HandBookPlacementController Instance { get; private set; }
    public event System.Action<BookSlot> BookPlaced;
    public event System.Action<BookSlot> BookReturned;

    private void Awake()
    {
        Instance = this;

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null && (_placeSound != null || _returnSound != null || _wrongShelfSound != null))
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
        if (GameplayUIController.IsWorldInputBlocked || slot == null || slot.IsFilled || slot.IsLocked ||
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
            PlayWrongShelfSound();
            return false;
        }

        if (!CreatePlacedBook(slot, sprite, handBook.CorrectSlotIndex, bookColorIndex)) return false;
        PlayPlaceSound();
        _handItemController.RemoveSelectedBook();
        BookPlaced?.Invoke(slot);
        return true;
    }

    public bool CanPlaceSelectedBookOnSlot(
        BookSlot slot,
        out HandBookData handBook)
    {
        handBook = null;

        if (GameplayUIController.IsWorldInputBlocked || slot == null || slot.IsFilled || slot.IsLocked || _handItemController == null)
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
        if (GameplayUIController.IsWorldInputBlocked || _handItemController == null ||
            _handItemController.IsFull ||
            slot == null ||
            slot.IsLocked ||
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
        PlayReturnSound();
        BookReturned?.Invoke(slot);
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

    internal bool CanRestoreBook(BookSlot slot, Sprite sprite)
    {
        if (slot == null || sprite == null || !BookColorUtility.TryGetBookIndex(sprite.name, out int index)) return false;
        int color = BookColorUtility.GetColorIndexFromSlotIndex(index);
        GameObject prefab = GetPrefab(color);
        return slot.ShelfColorIndex == color && prefab != null && prefab.GetComponent<BookObject>() != null;
    }

    internal bool RestoreBook(BookSlot slot, Sprite sprite)
    {
        if (!CanRestoreBook(slot, sprite)) return false;
        BookColorUtility.TryGetBookIndex(sprite.name, out int index);
        return CreatePlacedBook(slot, sprite, index, BookColorUtility.GetColorIndexFromSlotIndex(index));
    }

    private bool CreatePlacedBook(BookSlot slot, Sprite sprite, int correctIndex, int color)
    {
        GameObject prefab = GetPrefab(color);
        if (slot.IsFilled || prefab == null || prefab.GetComponent<BookObject>() == null) return false;
        GameObject book = Instantiate(prefab, slot.transform.position, Quaternion.identity);
        BookObject bookObject = book.GetComponent<BookObject>();
        bookObject.SetSprite(sprite);
        bookObject.SetCorrectSlotIndex(correctIndex);
        slot.PlaceBook(book.transform);
        return true;
    }

    private void PlayPlaceSound()
    {
        if (_audioSource != null && _placeSound != null)
        {
            _audioSource.PlayOneShot(_placeSound, _placeSoundVolume);
        }
    }

    private void PlayReturnSound()
    {
        if (_audioSource != null && _returnSound != null)
        {
            _audioSource.PlayOneShot(_returnSound, _returnSoundVolume);
        }
    }

    private void PlayWrongShelfSound()
    {
        if (_audioSource != null && _wrongShelfSound != null)
        {
            _audioSource.PlayOneShot(_wrongShelfSound, _wrongShelfSoundVolume);
        }
    }
}
