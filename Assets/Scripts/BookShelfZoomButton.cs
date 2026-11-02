using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BookShelfZoomButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private GameObject _bookShelf;
    [SerializeField] private float _zoomSize = 3f;

    [Header("Visual")]
    [SerializeField] private Graphic _buttonImage;
    [SerializeField] private GameObject _frameImage;
    [SerializeField, Range(0f, 1f)] private float _hoverBrightness = 0.8f;

    [Header("Answer")]
    [SerializeField] private Sprite _completedImageSprite;

    [Header("Shelf Card")]
    [SerializeField] private string _shelfName = "本棚";
    [SerializeField] private TMP_Text _progressLabel;
    [SerializeField] private Image _progressFill;
    public string DisplayName => _shelfName;
    public BookShelfJudge ShelfJudge => _bookShelf != null ? _bookShelf.GetComponent<BookShelfJudge>() : null;

    private Color _normalColor = Color.white;
    private bool _isPointerOver;
    private bool _isSelected;

    private void Awake()
    {
        if (_buttonImage == null)
        {
            _buttonImage = GetComponent<Graphic>();
        }

        if (_buttonImage != null)
        {
            _normalColor = _buttonImage.color;
        }

        UpdateVisual();
    }

    private void OnEnable()
    {
        if (ShelfJudge != null) ShelfJudge.StateChanged += RefreshProgress;
        RefreshProgress();
    }

    private void OnDisable()
    {
        if (ShelfJudge != null) ShelfJudge.StateChanged -= RefreshProgress;
        _isPointerOver = false;
        UpdateVisual();
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisual();
    }

    private void RefreshProgress()
    {
        BookShelfJudge shelf = ShelfJudge;
        if (shelf == null) return;
        if (_progressLabel != null)
            _progressLabel.text = shelf.IsCompleted ? "完成" : $"{shelf.PlacedCount} / {shelf.SlotCount} 冊";
        if (_progressFill != null)
            _progressFill.fillAmount = shelf.SlotCount == 0 ? 0 : (float)shelf.PlacedCount / shelf.SlotCount;
    }

    // ShelfButton の OnClick から呼ぶ
    public void OpenActionMenu()
    {
        if (ShelfActionMenuController.Instance == null)
        {
            Debug.LogWarning("ShelfActionMenuController が見つかりません");
            return;
        }

        ShelfActionMenuController.Instance.Open(this);
    }

    public void ZoomShelf()
    {
        if (_cameraController == null || _bookShelf == null)
        {
            Debug.LogWarning("CameraController または本棚が設定されていません");
            return;
        }

        _isPointerOver = false;
        UpdateVisual();
        ShelfActionMenuController.Instance?.SetShelfContext(this);

        _cameraController.ZoomTo(
            _bookShelf.transform.position,
            _zoomSize
        );
    }

    public bool JudgeShelf()
    {
        if (_bookShelf == null)
        {
            Debug.LogWarning("判定対象の本棚が設定されていません");
            return false;
        }

        BookShelfJudge shelfJudge = _bookShelf.GetComponent<BookShelfJudge>();

        if (shelfJudge == null)
        {
            Debug.LogWarning($"{_bookShelf.name} に BookShelfJudge がありません");
            return false;
        }

        return shelfJudge.CheckShelf();
    }

    public void ShowCompletedImage()
    {
        if (ShelfAnswerPanelController.Instance == null)
        {
            Debug.LogWarning("ShelfAnswerPanelController が見つかりません");
            return;
        }

        ShelfAnswerPanelController.Instance.Open(_completedImageSprite);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_frameImage != null)
        {
            _frameImage.SetActive(_isPointerOver || _isSelected);
        }

        if (_buttonImage == null)
        {
            return;
        }

        if (_isPointerOver)
        {
            _buttonImage.color = new Color(
                _normalColor.r * _hoverBrightness,
                _normalColor.g * _hoverBrightness,
                _normalColor.b * _hoverBrightness,
                _normalColor.a
            );
        }
        else
        {
            _buttonImage.color = _normalColor;
        }
    }
}
