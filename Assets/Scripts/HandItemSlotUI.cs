using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _bookImage;
    [SerializeField] private Color _selectedColor = new Color(1f, 0.92f, 0.65f, 1f);

    private HandItemController _controller;
    private int _index;
    private Color _normalColor = Color.white;
    private bool _hasCachedNormalColor;

    public void Initialize(HandItemController controller, int index)
    {
        _controller = controller;
        _index = index;
        CacheNormalColor();
    }

    public void SetView(Sprite sprite, bool selected)
    {
        if (_bookImage == null)
        {
            return;
        }

        CacheNormalColor();

        bool hasBook = sprite != null;
        _bookImage.sprite = sprite;
        _bookImage.gameObject.SetActive(hasBook);
        _bookImage.color = selected ? _selectedColor : _normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _controller?.SelectBook(_index);
        }
    }

    private void CacheNormalColor()
    {
        if (_hasCachedNormalColor || _bookImage == null)
        {
            return;
        }

        _normalColor = _bookImage.color;
        _hasCachedNormalColor = true;
    }
}
