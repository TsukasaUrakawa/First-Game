using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Color filters and box counts; card order and cross-color selections stay intact.</summary>
public sealed class BookColorFilterUI : MonoBehaviour
{
    [SerializeField] private BookListUI _list;
    [SerializeField] private Button[] _buttons;
    [SerializeField] private TMP_Text[] _names;
    [SerializeField] private TMP_Text[] _counts;
    [SerializeField] private GameObject[] _selectedMarkers;
    [SerializeField] private TMP_Text _remainingLabel;
    [SerializeField] private GameObject _emptyState;
    [SerializeField] private TMP_Text _emptyMessage;
    private UnityAction[] _handlers;

    private void OnEnable()
    {
        if (_handlers == null)
        {
            _handlers = new UnityAction[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++)
            {
                int color = i - 1;
                _handlers[i] = () => _list.ShowColor(color);
            }
        }
        _list.StateChanged += Refresh;
        for (int i = 0; i < _buttons.Length; i++) _buttons[i].onClick.AddListener(_handlers[i]);
        Refresh();
    }

    private void OnDisable()
    {
        _list.StateChanged -= Refresh;
        for (int i = 0; i < _buttons.Length; i++) _buttons[i].onClick.RemoveListener(_handlers[i]);
    }

    private void Refresh()
    {
        int total = _list.GetRemainingCount();
        _remainingLabel.text = $"色で絞り込み　箱にある本：{total}冊";
        for (int i = 0; i < _buttons.Length; i++)
        {
            bool selected = _list.ActiveColorIndex == i - 1;
            _buttons[i].targetGraphic.color = selected ? new Color(.286f, .455f, .318f) : new Color(.95f, .90f, .81f);
            _names[i].color = selected ? new Color(1, .98f, .91f) : new Color(.33f, .23f, .15f);
            _counts[i].color = selected ? new Color(.91f, .94f, .85f) : new Color(.53f, .44f, .33f);
            _counts[i].text = $"残り {_list.GetRemainingCount(i - 1)} 冊";
            _selectedMarkers[i].SetActive(selected);
        }
        _emptyState.SetActive(_list.GetRemainingCount(_list.ActiveColorIndex) == 0);
        _emptyMessage.text = total == 0 ? "箱の本はすべて取り出しました。\n本棚の模様を確かめてみましょう。"
            : "この色の本は、すべて取り出しました。\nほかの色か「すべて」を選んでください。";
    }
}
