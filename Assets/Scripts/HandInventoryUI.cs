using TMPro;
using UnityEngine;

/// <summary>Hand count and placement guidance, refreshed when inventory or selection changes.</summary>
public class HandInventoryUI : MonoBehaviour
{
    [SerializeField] private HandItemController _hand;
    [SerializeField] private TMP_Text _countLabel;
    [SerializeField] private TMP_Text _selectionLabel;
    [SerializeField] private TMP_Text _hintLabel;

    private void OnEnable()
    {
        _hand.InventoryChanged += Refresh;
        _hand.SelectionChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        _hand.InventoryChanged -= Refresh;
        _hand.SelectionChanged -= Refresh;
    }

    private void Refresh()
    {
        _countLabel.text = $"{_hand.BookCount} / {_hand.Capacity} 冊";
        _selectionLabel.text = _hand.HasSelectedBook ? $"選択中：{_hand.SelectedIndex + 1}番の本" : "本をクリックして選択";
        _hintLabel.text = _hand.BookCount == 0
            ? "箱をクリックして、本を取り出しましょう。"
            : _hand.HasSelectedBook
                ? "同じ色の棚の空いている場所をクリック。もう一度本をクリックすると選択解除。"
                : "手持ちの本を選ぶ → 同じ色の棚の空いている場所に置く";
    }
}
