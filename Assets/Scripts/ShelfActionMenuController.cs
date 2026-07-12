using UnityEngine;

public class ShelfActionMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _actionPanel;
    [SerializeField] private AudioClip _shelfButtonClickSound;

    public static ShelfActionMenuController Instance { get; private set; }
    public bool IsOpen => _actionPanel != null && _actionPanel.activeSelf;

    private BookShelfZoomButton _selectedShelfButton;
    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();

        if (_actionPanel != null)
        {
            _actionPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Open(BookShelfZoomButton shelfButton)
    {
        if (shelfButton == null || _actionPanel == null)
        {
            return;
        }

        PlayShelfButtonClickSound();

        if (IsOpen && _selectedShelfButton == shelfButton)
        {
            Close();
            return;
        }

        _selectedShelfButton = shelfButton;
        _actionPanel.SetActive(true);
    }

    public void ZoomSelectedShelf()
    {
        BookShelfZoomButton selectedButton = _selectedShelfButton;
        Close();

        if (selectedButton != null)
        {
            selectedButton.ZoomShelf();
        }
    }

    public void JudgeSelectedShelf()
    {
        BookShelfZoomButton selectedButton = _selectedShelfButton;
        Close();

        if (selectedButton != null)
        {
            bool isCorrect = selectedButton.JudgeShelf();
            Debug.Log(isCorrect ? "この本棚は正解です" : "この本棚は未完成または不正解です");
        }
    }

    public void ShowSelectedShelfAnswer()
    {
        BookShelfZoomButton selectedButton = _selectedShelfButton;
        Close();

        if (selectedButton != null)
        {
            selectedButton.ShowCompletedImage();
        }
    }

    public void Close()
    {
        _selectedShelfButton = null;

        if (_actionPanel != null)
        {
            _actionPanel.SetActive(false);
        }
    }

    private void PlayShelfButtonClickSound()
    {
        if (_audioSource != null && _shelfButtonClickSound != null)
        {
            _audioSource.PlayOneShot(_shelfButtonClickSound);
        }
    }
}
