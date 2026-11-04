using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShelfBookMoveUI : MonoBehaviour
{
    [SerializeField] private ShelfBookMoveController _movement;
    [SerializeField] private HandItemController _hand;
    [SerializeField] private GameObject _normalActions;
    [SerializeField] private GameObject _moveActions;
    [SerializeField] private Image _selectedBookImage;
    [SerializeField] private Button _returnButton;
    [SerializeField] private TMP_Text _returnLabel;

    private void OnEnable()
    {
        _movement.SelectionChanged += Refresh;
        _hand.InventoryChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        _movement.SelectionChanged -= Refresh;
        _hand.InventoryChanged -= Refresh;
    }

    private void Refresh()
    {
        if (_normalActions == null || _moveActions == null) return;
        bool selected = _movement.HasSelection;
        _normalActions.SetActive(!selected);
        _moveActions.SetActive(selected);
        _selectedBookImage.sprite = _movement.SelectedSprite;
        _returnButton.interactable = _movement.CanReturnToHand;
        _returnLabel.text = _hand.IsFull ? "手持ちがいっぱい" : "手持ちに戻す";
    }
}
