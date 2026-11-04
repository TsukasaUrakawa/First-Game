using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Coordinates gameplay panels and the input they cover. Panel controllers keep
/// their own presentation; visibility of the shared HUD is decided here.
/// </summary>
public class GameplayUIController : MonoBehaviour
{
    [SerializeField] private GameObject _bookListPanel;
    [SerializeField] private CanvasGroup _handItemCanvasGroup;
    [SerializeField] private CanvasGroup _shelfButtonCanvasGroup;
    [SerializeField] private GameObject _zoomBackButton;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private BoxOpenAndClose _boxController;
    [SerializeField] private BookSelectionController _bookSelectionController;
    [SerializeField] private BookPopUpController _bookPopUpController;
    [SerializeField] private ShelfAnswerPanelController _answerController;
    [SerializeField] private ShelfActionMenuController _actionMenuController;
    [SerializeField] private ShelfBookMoveController _bookMovement;
    [SerializeField] private SettingsPanelController _settings;
    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private PauseMenuController _pause;
    [SerializeField] private GameObject _pauseButton;
    [SerializeField] private RectTransform _menuRow;

    public static GameplayUIController Instance { get; private set; }
    public static bool IsWorldInputBlocked => Instance != null && Instance.BlocksWorldInput;

    public bool IsBookListOpen => _bookListPanel.activeSelf;
    public bool IsGameCleared { get; private set; }
    public bool IsSettingsOpen => _settings != null && _settings.IsOpen;
    public bool IsPauseOpen => _pause != null && _pause.IsOpen;
    public bool IsMenuOpen => IsSettingsOpen || IsPauseOpen;
    public bool HasModal => IsMenuOpen || IsGameCleared || IsBookListOpen || _answerController.IsOpen;
    public bool BlocksWorldInput => HasModal;
    public event System.Action StateChanged;
    public event System.Action PauseChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Always start in the room, even if a panel was left open while editing.
        CloseBookList();
        _answerController.Close();
        _actionMenuController.Close();
        RefreshVisibility();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
        else if (Input.GetMouseButtonDown(1) && !BlocksWorldInput &&
                 ((_bookMovement != null && _bookMovement.HasSelection) ||
                  (_cameraController.IsZoomed && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))))
        {
            Back();
        }
    }

    public void OpenBookList()
    {
        if (IsGameCleared || IsBookListOpen || IsMenuOpen)
        {
            return;
        }

        _answerController.Close();
        _bookSelectionController.ClearSelection();
        _bookListPanel.SetActive(true);
        RefreshVisibility();
    }

    public void CloseBookList()
    {
        if (IsPauseOpen) return;
        _bookSelectionController.ClearSelection();
        _bookListPanel.SetActive(false);
        _boxController.CloseLid();
        RefreshVisibility();
    }

    public void OnAnswerOpened()
    {
        CloseBookList();
        RefreshVisibility();
    }

    public void OnSettingsOpened()
    {
        // Cancel a box-opening animation whose event has not shown the list yet.
        _boxController.CloseLid();
        RefreshVisibility();
    }

    public void OnGameClear()
    {
        // Lock gameplay immediately, including the delay before the clear fade.
        IsGameCleared = true;
        if (IsPauseOpen) _pause.CloseForClear();
        if (IsSettingsOpen) _settings.Close();
        CloseBookList();
        _answerController.Close();
        _actionMenuController.Close();
        RefreshVisibility();
    }

    public void Back()
    {
        if (IsPauseOpen) { _pause.Resume(); return; }
        if (IsSettingsOpen) { _settings.Close(); return; }
        if (IsGameCleared)
        {
            return;
        }

        if (!HasModal && _bookMovement != null && _bookMovement.HasSelection)
        {
            _bookMovement.CancelSelection();
            return;
        }

        // One input closes only the topmost layer.
        if (_bookPopUpController.IsOpen)
        {
            _bookSelectionController.ReturnToSelection();
        }
        else if (IsBookListOpen)
        {
            CloseBookList();
        }
        else if (_answerController.IsOpen)
        {
            _answerController.Close();
        }
        else if (_cameraController.IsZoomed)
        {
            ReturnToRoom();
        }
        else
        {
            _pause?.Open();
        }
    }

    public void ReturnToRoom()
    {
        if (HasModal) return;
        _bookMovement?.CancelSelection();
        _actionMenuController.Close();
        _cameraController.ResetCamera();
    }

    public void RefreshVisibility()
    {
        ApplyVisibility();
        StateChanged?.Invoke();
    }

    public void RefreshPauseVisibility()
    {
        // A temporary pause must not cancel a selected shelf book.
        if (IsPauseOpen && !IsBookListOpen) _boxController.CloseLid();
        ApplyVisibility();
        PauseChanged?.Invoke();
    }

    private void ApplyVisibility()
    {
        if (_menuRow != null)
        {
            _menuRow.anchoredPosition = new Vector2(IsBookListOpen ? -248 : -24, -24);
            if (!IsMenuOpen && !IsGameCleared) _menuRow.SetAsLastSibling();
        }
        if (_pauseButton != null) _pauseButton.SetActive(!IsGameCleared && !IsMenuOpen);
        if (_settingsButton != null) _settingsButton.SetActive(!HasModal);
        SetVisible(_handItemCanvasGroup, !BlocksWorldInput);
        SetVisible(_shelfButtonCanvasGroup, !HasModal && !_cameraController.IsZoomed);
        _zoomBackButton.SetActive(_cameraController.IsZoomed && !BlocksWorldInput);
        _actionMenuController.SetVisible(_cameraController.IsZoomed && !HasModal);
    }

    private static void SetVisible(CanvasGroup group, bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }
}
