using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Restores after scene initialization and saves complete operations at frame end.</summary>
[DefaultExecutionOrder(100)]
public sealed class GameProgressController : MonoBehaviour
{
    [SerializeField] private BookManager _books;
    [SerializeField] private BookListUI _list;
    [SerializeField] private HandItemController _hand;
    [SerializeField] private HandBookPlacementController _placement;
    [SerializeField] private BookJudgeManager _judge;
    [SerializeField] private GameplayUIController _ui;
    [SerializeField] private BookShelfJudge[] _shelves;
    [SerializeField] private TMP_Text _saveStatus;

    private BookSlot[] _slots;
    private bool _initialized, _dirty, _resetHistory, _saveFailed;
    private string _lastSavedJson;
    private float _statusUntil;
    public bool IsReady => _initialized;

    private void Start()
    {
        _slots = _shelves.SelectMany(s => s.GetComponentsInChildren<BookSlot>(true)).OrderBy(s => s.SlotIndex).ToArray();
        _resetHistory = GameProgressStore.ConsumeNewGameRequest();
        var result = GameProgressStore.Read(out var data);
        if (!_resetHistory && result == ProgressReadResult.Invalid)
        {
            SetStatus("保存データを読み込めませんでした。\nタイトルから新しくはじめてください。", true);
            return;
        }
        if (!_resetHistory && data != null)
        {
            if (!CanRestore(data))
            {
                SetStatus("保存データを読み込めませんでした。\nタイトルから新しくはじめてください。", true);
                return;
            }
            Restore(data);
        }
        _hand.InventoryChanged += MarkDirty;
        foreach (var shelf in _shelves) shelf.StateChanged += MarkDirty;
        _judge.GameCleared += MarkDirty;
        _initialized = true;
        if (!_resetHistory && data != null && data.cleared) _judge.CheckGameClear();
        if (_resetHistory || data == null || result == ProgressReadResult.Recovered) _dirty = true;
        else _lastSavedJson = JsonUtility.ToJson(Capture());
    }

    private bool CanRestore(GameProgressData data)
    {
        if (_slots.Length != BookColorUtility.TotalBooks || _slots.Select(s => s.SlotIndex).Distinct().Count() != _slots.Length ||
            _shelves.Length != BookColorUtility.ShelfCount || data.hand.Length > _hand.Capacity) return false;
        var sprites = _books._allBookSprites.ToDictionary(s => s.name);
        return data.hand.All(sprites.ContainsKey) && data.placed.All(p => sprites.ContainsKey(p.book) &&
            _slots[p.slot].SlotIndex == p.slot && _placement.CanRestoreBook(_slots[p.slot], sprites[p.book]));
    }

    private void Restore(GameProgressData data)
    {
        var sprites = _books._allBookSprites.ToDictionary(s => s.name);
        foreach (string name in data.hand)
        {
            BookColorUtility.TryGetBookIndex(name, out int correct);
            _hand.TryAddBook(sprites[name], correct);
        }
        foreach (var book in data.placed) _placement.RestoreBook(_slots[book.slot], sprites[book.book]);
        foreach (var shelf in _shelves)
        {
            int color = shelf.GetComponentsInChildren<BookSlot>(true)[0].ShelfColorIndex;
            shelf.RestoreCompleted(data.completedShelves.Contains(color));
        }
        _list.ExcludeOwnedBooks(data.hand.Concat(data.placed.Select(p => p.book)));
    }

    private GameProgressData Capture() => new GameProgressData
    {
        version = 1,
        hand = _hand.Books.Select(b => b.Sprite.name).ToArray(),
        placed = _slots.Where(s => s.IsFilled).Select(s => new SavedBookPlacement
        {
            book = s.PlacedBook.GetComponent<SpriteRenderer>().sprite.name, slot = s.SlotIndex
        }).ToArray(),
        completedShelves = _shelves.Where(s => s.IsCompleted).Select(s =>
            s.GetComponentsInChildren<BookSlot>(true)[0].ShelfColorIndex).OrderBy(c => c).ToArray(),
        cleared = _judge.IsGameCleared
    };

    private void MarkDirty() => _dirty = true;

    private void LateUpdate()
    {
        if (_dirty) SaveNow();
        if (_saveStatus != null)
            _saveStatus.gameObject.SetActive(_saveFailed || (Time.unscaledTime < _statusUntil && !_ui.HasModal));
    }

    public bool SaveNow()
    {
        if (!_initialized) return false;
        _dirty = false;
        var data = Capture();
        string json = JsonUtility.ToJson(data);
        if (!_saveFailed && !_resetHistory && json == _lastSavedJson) return true;
        bool saved = GameProgressStore.Write(data, _resetHistory);
        if (saved)
        {
            _lastSavedJson = json;
            _resetHistory = false;
        }
        SetStatus(saved ? "保存しました" : "保存できませんでした。\nブラウザの保存設定をご確認ください。", !saved);
        return saved;
    }

    private void SetStatus(string message, bool failed)
    {
        _saveFailed = failed;
        _statusUntil = Time.unscaledTime + 2.5f;
        if (_saveStatus != null) _saveStatus.text = message;
    }

    private void OnApplicationFocus(bool focused) { if (!focused && (_dirty || _saveFailed)) SaveNow(); }
    private void OnApplicationPause(bool paused) { if (paused && (_dirty || _saveFailed)) SaveNow(); }
    private void OnApplicationQuit() => SaveNow();

    private void OnDisable()
    {
        if (!_initialized) return;
        // Scene buttons flush before unloading. Avoid reading objects during teardown.
        _hand.InventoryChanged -= MarkDirty;
        foreach (var shelf in _shelves) if (shelf != null) shelf.StateChanged -= MarkDirty;
        _judge.GameCleared -= MarkDirty;
    }
}
