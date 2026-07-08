using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandItemSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image _bookImage;
    [SerializeField] private GameObject _frameImage;

    [SerializeField, Range(0f, 1f)]
    private float _hoverBrightness = 0.8f;

    private HandItemController _controller;
    private int _index;

    private Color _normalColor = Color.white;
    private bool _hasCachedNormalColor;

    private bool _hasBook;
    private bool _isSelected;
    private bool _isPointerOver;

    public void Initialize(
        HandItemController controller,
        int index)
    {
        _controller = controller;
        _index = index;

        CacheNormalColor();
        UpdateVisual();
    }

    public void SetView(Sprite sprite, bool selected)
    {
        if (_bookImage == null)
        {
            return;
        }

        CacheNormalColor();

        _hasBook = sprite != null;
        _isSelected = selected;

        if (!_hasBook)
        {
            _isPointerOver = false;
        }

        _bookImage.sprite = sprite;
        _bookImage.gameObject.SetActive(_hasBook);

        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_hasBook)
        {
            return;
        }

        if (eventData.button ==
            PointerEventData.InputButton.Left)
        {
            _controller?.SelectBook(_index);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_hasBook)
        {
            return;
        }

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
            _frameImage.SetActive(
                _hasBook && _isSelected
            );
        }

        if (_bookImage == null || !_hasBook)
        {
            return;
        }

        if (_isPointerOver)
        {
            _bookImage.color = new Color(
                _normalColor.r * _hoverBrightness,
                _normalColor.g * _hoverBrightness,
                _normalColor.b * _hoverBrightness,
                _normalColor.a
            );
        }
        else
        {
            _bookImage.color = _normalColor;
        }
    }

    private void CacheNormalColor()
    {
        if (_hasCachedNormalColor ||
            _bookImage == null)
        {
            return;
        }

        _normalColor = _bookImage.color;
        _hasCachedNormalColor = true;
    }
}