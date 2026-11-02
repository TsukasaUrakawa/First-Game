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

    public static GameplayUIController Instance { get; private set; }
    public static bool IsWorldInputBlocked => Instance != null && Instance.BlocksWorldInput;

    public bool IsBookListOpen => _bookListPanel.activeSelf;
    public bool IsGameCleared { get; private set; }
    public bool HasModal => IsGameCleared || IsBookListOpen || _answerController.IsOpen;
    public bool BlocksWorldInput => HasModal;

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
        else if (Input.GetMouseButtonDown(1) &&
                 _cameraController.IsZoomed && !BlocksWorldInput &&
                 (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            Back();
        }
    }

    public void OpenBookList()
    {
        if (IsGameCleared || IsBookListOpen)
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

    public void OnGameClear()
    {
        // Lock gameplay immediately, including the delay before the clear fade.
        IsGameCleared = true;
        CloseBookList();
        _answerController.Close();
        _actionMenuController.Close();
        RefreshVisibility();
    }

    public void Back()
    {
        if (IsGameCleared)
        {
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
            _actionMenuController.Close();
            _cameraController.ResetCamera();
        }
    }

    public void RefreshVisibility()
    {
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
