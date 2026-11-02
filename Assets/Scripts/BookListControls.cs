using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Fixed book-list actions, updated only when the selection or hand changes.</summary>
public class BookListControls : MonoBehaviour
{
    [SerializeField] private BookSelectionController _selection;
    [SerializeField] private HandItemController _hand;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _clearButton;
    [SerializeField] private TMP_Text _selectionLabel;
    [SerializeField] private TMP_Text _hintLabel;

    private void OnEnable()
    {
        _selection.SelectionChanged += Refresh;
        _hand.InventoryChanged += Refresh;
        _confirmButton.onClick.AddListener(_selection.ShowSelectionConfirmation);
        _clearButton.onClick.AddListener(_selection.CancelSelection);
        Refresh();
    }

    private void OnDisable()
    {
        _selection.SelectionChanged -= Refresh;
        _hand.InventoryChanged -= Refresh;
        _confirmButton.onClick.RemoveListener(_selection.ShowSelectionConfirmation);
        _clearButton.onClick.RemoveListener(_selection.CancelSelection);
    }

    private void Refresh()
    {
        int count = _selection.SelectedCount;
        int limit = _selection.SelectionLimit;
        _selectionLabel.text = $"選択 {count} / {limit} 冊　手持ち {_hand.BookCount} / {_hand.Capacity} 冊";
        _confirmButton.interactable = count > 0 && count <= limit;
        _clearButton.interactable = count > 0;

        if (limit == 0)
        {
            _hintLabel.text = "手持ちがいっぱいです。一度閉じて、棚に本を置きましょう。";
        }
        else if (count >= limit)
        {
            _hintLabel.text = "選べる上限です。「選んだ本を確認」へ進みましょう。";
        }
        else
        {
            _hintLabel.text = "本をクリックして選択・解除。右クリックでも確認できます。";
        }
    }
}
