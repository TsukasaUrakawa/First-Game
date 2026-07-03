using UnityEngine;
using UnityEngine.EventSystems;

public class BookSlot : MonoBehaviour
{
    public bool IsFilled { get; private set; }

    [SerializeField] private int _slotIndex;
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

        const string shelfNamePrefix = "BookShelf";
        string shelfName = transform.parent.name;

        if (!shelfName.StartsWith(shelfNamePrefix))
        {
            ShelfColorIndex = -1;
            return;
        }

        string numberPart = shelfName.Substring(shelfNamePrefix.Length);

        if (int.TryParse(numberPart, out int shelfNumber))
        {
            ShelfColorIndex = shelfNumber - 1;
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (IsFilled || HandBookPlacementController.Instance == null)
        {
            return;
        }

        HandBookPlacementController.Instance.TryPlaceBook(this);
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
            _clickCollider.enabled = true;
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
}
