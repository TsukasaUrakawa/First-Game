using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookShelfZoomButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private GameObject _bookShelf;
    [SerializeField] private float _zoomSize = 3f;

    [Header("Visual")]
    [SerializeField] private Image _buttonImage;
    [SerializeField] private GameObject _frameImage;
    [SerializeField, Range(0f, 1f)] private float _hoverBrightness = 0.8f;

    [Header("Answer")]
    [SerializeField] private Sprite _completedImageSprite;

    [Header("Action Menu")]
    [SerializeField] private Sprite _actionPanelSprite;
    [SerializeField] private RectTransform _actionMenuPoint;

    public Sprite ActionPanelSprite
    {
        get
        {
            if (_actionPanelSprite != null)
            {
                return _actionPanelSprite;
            }

            return _buttonImage != null ? _buttonImage.sprite : null;
        }
    }

    public RectTransform ActionMenuPoint
    {
        get
        {
            return _actionMenuPoint;
        }
    }

    private Color _normalColor = Color.white;
    private bool _isPointerOver;

    private void Awake()
    {
        if (_buttonImage == null)
        {
            _buttonImage = GetComponent<Image>();
        }

        if (_buttonImage != null)
        {
            _normalColor = _buttonImage.color;
        }

        UpdateVisual();
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
            _frameImage.SetActive(_isPointerOver);
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
