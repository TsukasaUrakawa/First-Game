using TMPro;
using UnityEngine;

/// <summary>Contextual actions for the zoomed shelf; does not block book placement.</summary>
public class ShelfActionMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _actionPanel;
    [SerializeField] private TMP_Text _shelfTitleLabel;
    [SerializeField] private TMP_Text _shelfStatusLabel;
    [SerializeField] private AudioClip _shelfButtonClickSound;
    [SerializeField] private GameplayUIController _gameplayUI;
    [SerializeField] private AudioSource _audioSource;

    public static ShelfActionMenuController Instance { get; private set; }
    public bool IsOpen => _actionPanel != null && _actionPanel.activeSelf;
    public BookShelfZoomButton SelectedShelf => _selectedShelfButton;
    private BookShelfZoomButton _selectedShelfButton;

    private void Awake()
    {
        Instance = this;
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        _actionPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        Unsubscribe();
        if (Instance == this) Instance = null;
    }

    public void Open(BookShelfZoomButton shelfButton)
    {
        if (shelfButton == null || _gameplayUI.HasModal) return;
        if (_audioSource != null && _shelfButtonClickSound != null)
            _audioSource.PlayOneShot(_shelfButtonClickSound);
        shelfButton.ZoomShelf();
    }

    public void SetShelfContext(BookShelfZoomButton shelfButton)
    {
        Unsubscribe();
        _selectedShelfButton = shelfButton;
        if (shelfButton != null)
        {
            shelfButton.SetSelected(true);
            if (shelfButton.ShelfJudge != null)
                shelfButton.ShelfJudge.StateChanged += RefreshStatus;
        }
        RefreshStatus();
    }

    public void SetVisible(bool visible)
    {
        _actionPanel.SetActive(visible && _selectedShelfButton != null);
    }

    public void JudgeSelectedShelf()
    {
        if (_selectedShelfButton == null) return;
        bool correct = _selectedShelfButton.JudgeShelf();
        if (_selectedShelfButton != null && _shelfStatusLabel != null)
            _shelfStatusLabel.text = correct ? "この棚は完成です！" : "まだ並びが違うようです";
    }

    public void ShowSelectedShelfAnswer()
    {
        if (_selectedShelfButton != null) _selectedShelfButton.ShowCompletedImage();
    }

    public void Close()
    {
        Unsubscribe();
        _selectedShelfButton = null;
        _actionPanel.SetActive(false);
        _gameplayUI.RefreshVisibility();
    }

    private void Unsubscribe()
    {
        if (_selectedShelfButton == null) return;
        _selectedShelfButton.SetSelected(false);
        if (_selectedShelfButton.ShelfJudge != null)
            _selectedShelfButton.ShelfJudge.StateChanged -= RefreshStatus;
    }

    private void RefreshStatus()
    {
        if (_selectedShelfButton == null) return;
        if (_shelfTitleLabel != null) _shelfTitleLabel.text = _selectedShelfButton.DisplayName;
        BookShelfJudge shelf = _selectedShelfButton.ShelfJudge;
        if (_shelfStatusLabel != null && shelf != null)
            _shelfStatusLabel.text = shelf.IsCompleted ? "この棚は完成です！" : $"収納 {shelf.PlacedCount} / {shelf.SlotCount} 冊";
    }
}
