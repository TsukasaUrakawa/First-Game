using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 本一覧に表示される、それぞれの本ボタン。
/// </summary>
public class BookButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _frameImage;

    private Sprite _bookSprite;
    private BookSelectionController _selectionController;
    private Image _bookImage;

    public Sprite BookSprite => _bookSprite;
    public bool IsSelected { get; private set; }

    private void Awake()
    {
        _bookImage = GetComponent<Image>();
    }

    public void SetUp(Sprite bookSprite, BookSelectionController selectionController)
    {
        _bookSprite = bookSprite;
        _selectionController = selectionController;
        _bookImage.sprite = bookSprite;
        SetSelected(false);
    }

    // 既存のButton OnClickからそのまま呼べるよう、メソッド名を維持する。
    public void SendSprite()
    {
        _selectionController.ToggleSelection(this);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (_frameImage != null)
        {
            _frameImage.SetActive(selected);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_frameImage != null)
        {
            _frameImage.SetActive(true);
        }

        _bookImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_frameImage != null)
        {
            _frameImage.SetActive(IsSelected);
        }

        _bookImage.color = Color.white;
    }
}
