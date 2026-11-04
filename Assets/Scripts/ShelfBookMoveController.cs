using UnityEngine;

/// <summary>Two-click rearranging keeps books on their shelf without consuming hand capacity.</summary>
public sealed class ShelfBookMoveController : MonoBehaviour
{
    [SerializeField] private GameplayUIController _ui;
    [SerializeField] private HandItemController _hand;
    [SerializeField] private HandBookPlacementController _placement;
    [SerializeField] private BookShelfZoomButton[] _shelfButtons;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _selectSound;
    [SerializeField] private AudioClip _moveSound;

    public static ShelfBookMoveController Instance { get; private set; }
    public BookSlot SelectedSlot { get; private set; }
    public bool HasSelection => SelectedSlot != null;
    public Sprite SelectedSprite => _book != null ? _book.Sprite : null;
    public bool CanReturnToHand => HasSelection && SelectedSlot.CanRearrange && !_hand.IsFull && !_ui.BlocksWorldInput;
    public event System.Action SelectionChanged;
    private BookObject _book;

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        _ui.StateChanged += CancelSelection;
        _hand.SelectionChanged += OnHandSelectionChanged;
    }

    private void OnDisable()
    {
        CancelSelection();
        _ui.StateChanged -= CancelSelection;
        _hand.SelectionChanged -= OnHandSelectionChanged;
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    public bool ClickBook(BookSlot slot)
    {
        if (_ui.BlocksWorldInput || slot == null || !slot.IsFilled || !slot.CanRearrange) return false;
        if (HasSelection)
        {
            if (SelectedSlot == slot) { CancelSelection(); return true; }
            return TryMoveSelectedBookTo(slot);
        }
        if (slot.PlacedBook == null) return false;
        // A book clicked from the room opens its shelf before showing the actions.
        var button = System.Array.Find(_shelfButtons, b => b != null && b.ShelfJudge == slot.ShelfJudge);
        if (button == null) return false;
        button.ZoomShelf();
        _hand.ClearSelection();
        SelectedSlot = slot;
        _book = slot.PlacedBook;
        slot.ShelfJudge.StateChanged += ValidateSelection;
        Play(_selectSound);
        SelectionChanged?.Invoke();
        return true;
    }

    public bool TryMoveSelectedBookTo(BookSlot target)
    {
        if (!HasSelection || !SelectedSlot.TryMoveOrSwapTo(target)) return false;
        CancelSelection();
        Play(_moveSound);
        return true;
    }

    public bool TryGetPreview(BookSlot target, out Sprite sprite)
    {
        sprite = SelectedSprite;
        return HasSelection && SelectedSlot.CanMoveOrSwapTo(target) && sprite != null;
    }

    public void ReturnSelectedToHand()
    {
        if (CanReturnToHand && _placement.TryReturnBookToHand(SelectedSlot)) CancelSelection();
    }

    public void CancelSelection()
    {
        bool hadSelection = HasSelection;
        if (_book != null) _book.Visual.SetHighlight(0);
        if (SelectedSlot != null && SelectedSlot.ShelfJudge != null)
            SelectedSlot.ShelfJudge.StateChanged -= ValidateSelection;
        SelectedSlot = null;
        _book = null;
        if (hadSelection) SelectionChanged?.Invoke();
    }

    private void OnHandSelectionChanged() { if (_hand.HasSelectedBook) CancelSelection(); }

    private void ValidateSelection()
    {
        if (HasSelection && (!SelectedSlot.CanRearrange || SelectedSlot.PlacedBook != _book)) CancelSelection();
    }

    private void Update()
    {
        ValidateSelection();
        if (_book != null)
        {
            float pulse = GamePreferences.ReducedMotion ? .45f : .45f + .15f * Mathf.Sin(Time.unscaledTime * 3.5f);
            _book.Visual.SetHighlight(pulse);
        }
    }

    private void Play(AudioClip sound) { if (_audioSource != null && sound != null) _audioSource.PlayOneShot(sound, .6f); }
}
