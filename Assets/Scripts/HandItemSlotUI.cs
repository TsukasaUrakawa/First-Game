using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HandItemSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image _bookImage;
    [SerializeField] private GameObject _frameImage;

    [SerializeField] private Graphic _background;
    [SerializeField] private TMP_Text _stateLabel;
    [SerializeField] private GameObject _emptyLabel;

    private HandItemController _controller;
    private int _index;

    private bool _hasBook;
    private bool _isSelected;
    private bool _isPointerOver;

    public void Initialize(
        HandItemController controller,
        int index)
    {
        _controller = controller;
        _index = index;

        UpdateVisual();
    }

    public void SetView(Sprite sprite, bool selected)
    {
        if (_bookImage == null)
        {
            return;
        }

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
        if (!_hasBook || GameplayUIController.IsWorldInputBlocked)
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
            _frameImage.SetActive(_hasBook && (_isSelected || _isPointerOver));
        }

        if (_emptyLabel != null) _emptyLabel.SetActive(!_hasBook);
        if (_stateLabel != null)
        {
            _stateLabel.text = _isSelected && _hasBook ? "選択中" : (_index + 1).ToString();
            _stateLabel.color = _isSelected ? new Color(.98f, .88f, .67f) : new Color(.69f, .76f, .72f);
        }
        if (_background != null)
            _background.color = _isSelected ? new Color(.22f, .36f, .35f)
                : _hasBook ? new Color(.12f, .23f, .25f) : new Color(.08f, .16f, .18f);
    }

    private void OnDisable()
    {
        _isPointerOver = false;
        UpdateVisual();
    }
}
