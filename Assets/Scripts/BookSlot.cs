using UnityEngine;
using UnityEngine.EventSystems;

public class BookSlot : MonoBehaviour
{
    public bool IsFilled { get; private set; }
    public bool IsLocked { get; private set; }

    [SerializeField] private int _slotIndex;
    [Header("Ghost Preview")]
    [SerializeField] private SpriteRenderer _ghostBookRenderer;
    [SerializeField, Min(0.01f)] private float _ghostTargetWorldWidth = 0.58f;
    [SerializeField, Min(0.01f)] private float _ghostTargetWorldHeight = 2.3f;
    [SerializeField, Range(0f, 1f)] private float _ghostAlpha = 0.35f;
    [SerializeField] private int _ghostSortingOrder = 9;

    public int SlotIndex => _slotIndex;
    public BookObject PlacedBook => _placedBook;
    public int ShelfColorIndex { get; private set; } = -1;

    private BookObject _placedBook;
    private Collider2D _clickCollider;

    private void Awake()
    {
        _clickCollider = GetComponent<Collider2D>();
        UpdateSlotIndexFromName();
        UpdateShelfColorIndex();
        PrepareGhostRenderer();
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

        if (HandBookPlacementController.Instance.TryPlaceBook(this))
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

        bookTransform.position = transform.position;
        IsFilled = true;
        _placedBook = bookObject;

        if (_clickCollider != null)
        {
            _clickCollider.enabled = false;
        }

        HideGhostPreview();

        PlacedBookClick bookClick = bookTransform.GetComponent<PlacedBookClick>();

        if (bookClick != null)
        {
            bookClick.SetCurrentSlot(this);
        }
    }

    public void ClearSlot()
    {
        IsFilled = false;
        _placedBook = null;

        if (_clickCollider != null)
        {
            _clickCollider.enabled = !IsLocked;
        }
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

        Collider2D placedBookCollider = _placedBook.GetComponent<Collider2D>();

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

    private void PrepareGhostRenderer()
    {
        if (_ghostBookRenderer == null)
        {
            GameObject ghostObject = new GameObject("GhostBookPreview");
            ghostObject.transform.SetParent(transform);
            ghostObject.transform.localPosition = Vector3.zero;
            ghostObject.transform.localRotation = Quaternion.identity;
            ghostObject.transform.localScale = Vector3.one;

            _ghostBookRenderer = ghostObject.AddComponent<SpriteRenderer>();
        }

        _ghostBookRenderer.sortingOrder = _ghostSortingOrder;
        HideGhostPreview();
    }

    private void UpdateGhostPreview()
    {
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
        if (_ghostBookRenderer == null || sprite == null)
        {
            return;
        }

        _ghostBookRenderer.sprite = sprite;
        _ghostBookRenderer.color = new Color(1f, 1f, 1f, _ghostAlpha);
        _ghostBookRenderer.enabled = true;

        NormalizeGhostSize(sprite);
    }

    private void HideGhostPreview()
    {
        if (_ghostBookRenderer != null)
        {
            _ghostBookRenderer.enabled = false;
        }
    }

    private void NormalizeGhostSize(Sprite sprite)
    {
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;

        if (spriteWidth <= 0f || spriteHeight <= 0f)
        {
            return;
        }

        Transform ghostTransform = _ghostBookRenderer.transform;
        float parentScaleX = ghostTransform.parent != null
            ? Mathf.Abs(ghostTransform.parent.lossyScale.x)
            : 1f;
        float parentScaleY = ghostTransform.parent != null
            ? Mathf.Abs(ghostTransform.parent.lossyScale.y)
            : 1f;

        Vector3 scale = ghostTransform.localScale;
        scale.x = _ghostTargetWorldWidth / (spriteWidth * parentScaleX);
        scale.y = _ghostTargetWorldHeight / (spriteHeight * parentScaleY);
        ghostTransform.localScale = scale;
    }
}
