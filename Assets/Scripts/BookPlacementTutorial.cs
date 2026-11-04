using TMPro;
using UnityEngine;

/// <summary>Nonblocking guidance derived from the player's current actions.</summary>
public class BookPlacementTutorial : MonoBehaviour
{
    public const string PreferenceKey = "ShelfCare.BookPlacementTutorial.v1";
    public enum Step { OpenBox, SelectBook, ConfirmBooks, TakeBooks, CloseList, SelectHandBook, ChooseShelf, PlaceBook, Completed }

    [SerializeField] private GameplayUIController _ui;
    [SerializeField] private BookSelectionController _selection;
    [SerializeField] private BookPopUpController _popup;
    [SerializeField] private HandItemController _hand;
    [SerializeField] private HandBookPlacementController _placement;
    [SerializeField] private CameraController _camera;
    [SerializeField] private ShelfActionMenuController _shelfMenu;
    [SerializeField] private ShelfAnswerPanelController _answer;
    [SerializeField] private Collider _boxCollider;
    [SerializeField] private BookShelfZoomButton[] _shelfButtons;
    [SerializeField] private RectTransform _handRow;
    [SerializeField] private RectTransform _bookViewport;
    [SerializeField] private RectTransform _confirmButton;
    [SerializeField] private RectTransform _takeButton;
    [SerializeField] private RectTransform _closeListButton;
    [SerializeField] private RectTransform _backButton;
    [SerializeField] private RectTransform _layer;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private RectTransform _focus;
    [SerializeField] private TutorialFocusEffect _focusEffect;
    [SerializeField] private GameObject _helpButton;
    [SerializeField] private TMP_Text _heading;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private TMP_Text _dismissLabel;

    public bool IsRunning { get; private set; }
    public Step CurrentStep { get; private set; }
    private bool _completed;
    private RectTransform _focusTarget;
    private Bounds? _worldFocus;
    private bool _focusBox;
    private readonly Vector3[] _corners = new Vector3[4];
    private static readonly string[] ColorNames = { "緑", "青", "ベージュ", "赤", "紫", "茶", "白", "黒" };

    private void OnEnable()
    {
        _ui.StateChanged += Refresh;
        _selection.SelectionChanged += Refresh;
        _popup.VisibilityChanged += Refresh;
        _hand.InventoryChanged += Refresh;
        _hand.SelectionChanged += Refresh;
        _placement.BookPlaced += OnBookPlaced;
        _placement.BookReturned += OnBookReturned;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(PreferenceKey, 0) == 0) BeginTutorial();
        else Refresh();
    }

    private void OnDisable()
    {
        _ui.StateChanged -= Refresh;
        _selection.SelectionChanged -= Refresh;
        _popup.VisibilityChanged -= Refresh;
        _hand.InventoryChanged -= Refresh;
        _hand.SelectionChanged -= Refresh;
        _placement.BookPlaced -= OnBookPlaced;
        _placement.BookReturned -= OnBookReturned;
        _focus.gameObject.SetActive(false);
    }

    public void BeginTutorial()
    {
        if (_ui.IsGameCleared || _ui.IsSettingsOpen) return;
        ShelfBookMoveController.Instance?.CancelSelection();
        IsRunning = true;
        _completed = false;
        Refresh();
    }

    public void Dismiss()
    {
        if (!IsRunning) return;
        RememberSeen();
        IsRunning = false;
        Refresh();
    }

    private static void RememberSeen()
    {
        PlayerPrefs.SetInt(PreferenceKey, 1);
        PlayerPrefs.Save();
    }

    private void OnBookPlaced(BookSlot slot)
    {
        if (!IsRunning || _completed) return;
        _completed = true;
        RememberSeen();
        Refresh();
    }

    private void OnBookReturned(BookSlot slot) => Refresh();

    private Step ResolveStep(int color)
    {
        if (_completed) return Step.Completed;
        if (_popup.IsOpen) return Step.TakeBooks;
        if (_ui.IsBookListOpen)
        {
            if (_hand.IsFull) return Step.CloseList;
            return _selection.SelectedCount > 0 ? Step.ConfirmBooks : Step.SelectBook;
        }
        if (_hand.BookCount == 0) return Step.OpenBox;
        if (!_hand.HasSelectedBook) return Step.SelectHandBook;
        return color >= 0 && _camera.IsZoomed && _shelfMenu.SelectedShelf == _shelfButtons[color]
            ? Step.PlaceBook : Step.ChooseShelf;
    }

    private void Refresh()
    {
        if (_ui.IsGameCleared) IsRunning = false;
        bool visible = IsRunning && !_ui.IsGameCleared && !_ui.IsSettingsOpen && !_answer.IsOpen;
        _panel.gameObject.SetActive(visible);
        _helpButton.SetActive(!IsRunning && !_ui.HasModal);
        _focus.gameObject.SetActive(false);
        _focusTarget = null;
        _worldFocus = null;
        _focusBox = false;
        if (!visible) return;

        int color = _hand.TryGetSelectedBook(out var book) ? BookColorUtility.GetColorIndexFromSpriteName(book.Sprite.name) : -1;
        CurrentStep = ResolveStep(color);
        string colorName = color >= 0 ? ColorNames[color] : "同じ色";
        int number;
        switch (CurrentStep)
        {
            case Step.OpenBox:
                number = 1;
                _body.text = _camera.IsZoomed
                    ? "手持ちの本がありません。\n「部屋に戻る」で戻りましょう。"
                    : "左下の箱をクリックして、\n本の一覧を開きましょう。";
                _focusBox = !_camera.IsZoomed;
                if (_camera.IsZoomed) _focusTarget = _backButton;
                break;
            case Step.SelectBook:
                number = 2;
                _body.text = "気になる本を1冊クリック。\n最大9冊まで選べます。";
                _focusTarget = _bookViewport;
                break;
            case Step.ConfirmBooks:
                number = 3;
                _body.text = "下の「選んだ本を確認」を\nクリックしましょう。";
                _focusTarget = _confirmButton;
                break;
            case Step.TakeBooks:
                number = 4;
                _body.text = "選んだ本を確かめて、\n「この本を持っていく」をクリック。";
                _focusTarget = _takeButton;
                break;
            case Step.CloseList:
                number = 4;
                _body.text = "手持ちに本があります。\n右上の「閉じる」をクリック。";
                _focusTarget = _closeListButton;
                break;
            case Step.SelectHandBook:
                number = 5;
                _body.text = "画面下の手持ちから、\n棚に置く本をクリックしましょう。";
                _focusTarget = _handRow;
                break;
            case Step.ChooseShelf:
                number = 6;
                _body.text = _camera.IsZoomed
                    ? $"「部屋に戻る」で戻り、\n{colorName}の棚を選びましょう。"
                    : $"{colorName}の本を選択中です。\n左の{colorName}の棚をクリックして拡大。";
                _focusTarget = _camera.IsZoomed ? _backButton : color >= 0 ? _shelfButtons[color].GetComponent<RectTransform>() : _handRow;
                break;
            case Step.PlaceBook:
                number = 7;
                _body.text = $"拡大した{colorName}の棚の、\n空いている場所をクリック。";
                var slots = _shelfButtons[color].ShelfJudge.GetComponentsInChildren<BookSlot>();
                foreach (var slot in slots)
                {
                    if (slot.IsFilled) continue;
                    Bounds bounds = slot.GetComponent<Collider2D>().bounds;
                    if (_worldFocus.HasValue) { var combined = _worldFocus.Value; combined.Encapsulate(bounds); _worldFocus = combined; }
                    else _worldFocus = bounds;
                }
                break;
            default:
                number = 7;
                _body.text = "本を置けました！\n完成図を見ながら並べてみましょう。";
                break;
        }
        _heading.text = CurrentStep == Step.Completed ? "できました！" : $"あそびかた　{number} / 7";
        _dismissLabel.text = CurrentStep == Step.Completed ? "閉じる" : "スキップ";
        Vector2 corner = _ui.IsBookListOpen ? new Vector2(.5f, 1) : Vector2.one;
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = corner;
        _panel.anchoredPosition = _ui.IsBookListOpen ? new Vector2(120, -10) : new Vector2(-24, -180);
        UpdateFocus();
    }

    private void LateUpdate()
    {
        if (_panel.gameObject.activeInHierarchy) UpdateFocus();
    }

    private void UpdateFocus()
    {
        bool hasTarget = _focusTarget != null || _worldFocus.HasValue || _focusBox;
        _focus.gameObject.SetActive(hasTarget);
        if (!hasTarget) return;

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        if (_focusTarget != null)
        {
            _focusTarget.GetWorldCorners(_corners);
            for (int i = 0; i < _corners.Length; i++) Include(RectTransformUtility.WorldToScreenPoint(null, _corners[i]), ref min, ref max);
        }
        else
        {
            Bounds bounds = _focusBox ? _boxCollider.bounds : _worldFocus.Value;
            Camera camera = _camera.GetComponent<Camera>();
            Include(camera.WorldToScreenPoint(bounds.min), ref min, ref max);
            Include(camera.WorldToScreenPoint(bounds.max), ref min, ref max);
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, min, null, out var localMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, max, null, out var localMax);
        localMin = Vector2.Max(localMin - Vector2.one * 8, _layer.rect.min + Vector2.one * 4);
        localMax = Vector2.Min(localMax + Vector2.one * 8, _layer.rect.max - Vector2.one * 4);
        if (localMax.x <= localMin.x || localMax.y <= localMin.y)
        {
            _focus.gameObject.SetActive(false);
            return;
        }
        _focus.anchoredPosition = (localMin + localMax) * .5f;
        _focus.sizeDelta = localMax - localMin;
        _focusEffect.SetTarget(Rect.MinMaxRect(localMin.x, localMin.y, localMax.x, localMax.y), _focusBox, _focusTarget);
    }

    private static void Include(Vector2 point, ref Vector2 min, ref Vector2 max)
    {
        min = Vector2.Min(min, point);
        max = Vector2.Max(max, point);
    }
}
