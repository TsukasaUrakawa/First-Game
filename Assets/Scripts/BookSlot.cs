using UnityEngine;
using UnityEngine.EventSystems;

public class BookSlot : MonoBehaviour
{
    public bool IsFilled { get; private set; }
    public bool IsLocked { get; private set; }

    [SerializeField] private int _slotIndex;
    [Header("Ghost Preview")]
    [SerializeField] private BookVisual3D _ghostPrefab;
    private BookVisual3D _ghost;
    public BookVisual3D GhostPreview => _ghost;

    public int SlotIndex => _slotIndex;
    public BookObject PlacedBook => _placedBook;
    public int ShelfColorIndex { get; private set; } = -1;
    public BookShelfJudge ShelfJudge => _shelfJudge;
    public bool CanRearrange => !IsLocked && _shelfJudge != null && !_shelfJudge.IsCompleted;

    private BookObject _placedBook;
    private Collider _clickCollider;
    private BookShelfJudge _shelfJudge;

    private void Awake()
    {
        _clickCollider = GetComponent<Collider>();
        _shelfJudge = GetComponentInParent<BookShelfJudge>();
        UpdateSlotIndexFromName();
        UpdateShelfColorIndex();
    }

    private void OnValidate()
    {
        UpdateSlotIndexFromName();
        UpdateShelfColorIndex();
    }

    private void UpdateSlotIndexFromName()
    {
        string objectName = gameObject.name;

        if (objectName.StartsWith("Slot"))
        {
            string numberPart = objectName.Substring(4);

            if (int.TryParse(numberPart, out int number))
            {
                _slotIndex = number - 1;
            }
        }
    }

    private void UpdateShelfColorIndex()
    {
        if (transform.parent == null)
        {
            ShelfColorIndex = -1;
            return;
        }

        ShelfColorIndex =
            BookColorUtility.GetShelfColorIndexFromObjectName(
                transform.parent.name
            );
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (IsLocked || IsFilled || HandBookPlacementController.Instance == null)
        {
            return;
        }

        var movement = ShelfBookMoveController.Instance;
        bool placed = movement != null && movement.HasSelection
            ? movement.TryMoveSelectedBookTo(this)
            : HandBookPlacementController.Instance.TryPlaceBook(this);
        if (placed)
        {
            HideGhostPreview();
        }
    }

    private void OnMouseEnter()
    {
        UpdateGhostPreview();
    }

    private void OnMouseOver()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            HideGhostPreview();
            return;
        }

        UpdateGhostPreview();
    }

    private void OnMouseExit()
    {
        HideGhostPreview();
    }

    public void PlaceBook(Transform bookTransform)
    {
        if (bookTransform == null || IsFilled)
        {
            return;
        }

        BookObject bookObject = bookTransform.GetComponent<BookObject>();

        if (bookObject == null)
        {
            Debug.LogWarning("配置しようとしたオブジェクトにBookObjectがありません");
            return;
        }

        AssignBook(bookObject);
        _shelfJudge?.NotifyBookChanged();
    }

    public void ClearSlot()
    {
        if (_placedBook != null) _placedBook.GetComponent<PlacedBookClick>()?.SetCurrentSlot(null);
        AssignBook(null);
        _shelfJudge?.NotifyBookChanged();
    }

    public bool CanMoveOrSwapTo(BookSlot target) => !GameplayUIController.IsWorldInputBlocked &&
        IsFilled && _placedBook != null && CanRearrange && target != null && target != this &&
        target.CanRearrange && target._shelfJudge == _shelfJudge && target.ShelfColorIndex == ShelfColorIndex;

    public bool TryMoveOrSwapTo(BookSlot target)
    {
        if (!CanMoveOrSwapTo(target)) return false;
        BookObject moving = _placedBook;
        BookObject other = target._placedBook;
        // Both positions and click handlers are final before observers can save or judge.
        AssignBook(other);
        target.AssignBook(moving);
        _shelfJudge.NotifyBookChanged();
        return true;
    }

    private void AssignBook(BookObject book)
    {
        _placedBook = book;
        IsFilled = book != null;
        if (_clickCollider != null) _clickCollider.enabled = !IsLocked && !IsFilled;
        HideGhostPreview();
        if (book == null) return;
        book.transform.SetParent(transform, false);
        book.transform.localPosition = Vector3.zero;
        book.transform.localRotation = Quaternion.identity;
        book.transform.localScale = Vector3.one;
        book.GetComponent<PlacedBookClick>()?.SetCurrentSlot(this);
        var collider = book.GetComponent<Collider>();
        if (collider != null) collider.enabled = !IsLocked;
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        HideGhostPreview();

        if (_clickCollider != null)
        {
            _clickCollider.enabled = !locked && !IsFilled;
        }

        if (_placedBook == null)
        {
            return;
        }

        Collider placedBookCollider = _placedBook.GetComponent<Collider>();

        if (placedBookCollider != null)
        {
            placedBookCollider.enabled = !locked;
        }
    }

    public bool IsCorrect()
    {
        if (_placedBook == null)
        {
            return false;
        }

        return _placedBook.CorrectSlotIndex == _slotIndex;
    }

    private void UpdateGhostPreview()
    {
        var movement = ShelfBookMoveController.Instance;
        if (movement != null && movement.HasSelection)
        {
            if (!IsFilled && movement.TryGetPreview(this, out Sprite sprite)) ShowGhostPreview(sprite);
            else HideGhostPreview();
            return;
        }
        if (IsLocked ||
            IsFilled ||
            HandBookPlacementController.Instance == null ||
            !HandBookPlacementController.Instance.CanPlaceSelectedBookOnSlot(
                this,
                out HandBookData selectedBook
            ))
        {
            HideGhostPreview();
            return;
        }

        ShowGhostPreview(selectedBook.Sprite);
    }

    private void ShowGhostPreview(Sprite sprite)
    {
        if (_ghostPrefab == null || sprite == null) return;
        if (_ghost == null)
        {
            _ghost = Instantiate(_ghostPrefab, transform, false);
            _ghost.name = "GhostBookPreview";
        }
        _ghost.SetSprite(sprite);
        _ghost.gameObject.SetActive(true);
    }

    private void HideGhostPreview()
    {
        if (_ghost != null) _ghost.gameObject.SetActive(false);
    }
}
