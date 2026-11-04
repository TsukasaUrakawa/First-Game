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
    [SerializeField] private BookListUI _bookList;

    private void OnEnable()
    {
        _selection.SelectionChanged += Refresh;
        _hand.InventoryChanged += Refresh;
        if (_bookList != null) _bookList.StateChanged += Refresh;
        _confirmButton.onClick.AddListener(_selection.ShowSelectionConfirmation);
        _clearButton.onClick.AddListener(_selection.CancelSelection);
        Refresh();
    }

    private void OnDisable()
    {
        _selection.SelectionChanged -= Refresh;
        _hand.InventoryChanged -= Refresh;
        if (_bookList != null) _bookList.StateChanged -= Refresh;
        _confirmButton.onClick.RemoveListener(_selection.ShowSelectionConfirmation);
        _clearButton.onClick.RemoveListener(_selection.CancelSelection);
    }

    private void Refresh()
    {
        int count = _selection.SelectedCount;
        int limit = _selection.SelectionLimit;
        int hidden = _bookList != null ? _selection.CountSelectedOutsideColor(_bookList.ActiveColorIndex) : 0;
        _selectionLabel.text = $"選択 {count} / {limit} 冊　手持ち {_hand.BookCount} / {_hand.Capacity} 冊";
        _confirmButton.interactable = count > 0 && count <= limit;
        _clearButton.interactable = count > 0;

        if (limit == 0)
        {
            _hintLabel.text = "手持ちがいっぱいです。一度閉じて、棚に本を置きましょう。";
        }
        else if (count >= limit)
        {
            _hintLabel.text = hidden > 0 ? $"選べる上限です。ほかの色で{hidden}冊選択中。「選んだ本を確認」へ。"
                : "選べる上限です。「選んだ本を確認」へ進みましょう。";
        }
        else if (hidden > 0)
        {
            _hintLabel.text = $"ほかの色で{hidden}冊選択中です。確認画面には、すべての選択が表示されます。";
        }
        else if (_bookList != null && _bookList.GetRemainingCount(_bookList.ActiveColorIndex) == 0)
        {
            _hintLabel.text = _bookList.GetRemainingCount() == 0
                ? "箱の本はすべて取り出しました。「閉じる」で本棚に戻りましょう。"
                : "ほかの色か「すべて」を選んで、本を探しましょう。";
        }
        else
        {
            _hintLabel.text = "本をクリックして選択・解除。右クリックでも確認できます。";
        }
    }
}
