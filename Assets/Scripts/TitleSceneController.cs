using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "ShelfCare";
    [SerializeField] private GameObject _quitButton;
    [Header("Saved Progress")]
    [SerializeField] private TMP_Text _startButtonLabel;
    [SerializeField] private GameObject _newGameButton;
    [SerializeField] private TMP_Text _saveSummary;
    [SerializeField] private GameObject _newGameDialog;
    [SerializeField] private Button _cancelNewGameButton;
    [SerializeField] private CanvasGroup _titleContent;
    [SerializeField] private SettingsPanelController _settings;
    [SerializeField] private Button _settingsButton;
    public bool CanUseTitleActions => !_isProcessing && !(_newGameDialog != null && _newGameDialog.activeSelf) && !(_settings != null && _settings.IsOpen);

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _startButtonSound;
    [SerializeField] private AudioClip _quitButtonSound;
    [SerializeField, Range(0f, 1f)] private float _buttonSoundVolume = 1f;
    [SerializeField, Min(0f)] private float _sceneChangeDelay = 0.15f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField, Min(0f)] private float _fadeDuration = 0.6f;
    [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

    private bool _isProcessing;
    private bool _hasProgress;
    private GameObject _previousSelection;

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (_quitButton != null) _quitButton.SetActive(false);
#endif
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        if (_fadeCanvasGroup != null)
        {
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;
        }
        SetNewGameDialog(false);
        RefreshSavedProgress();
    }

    private void RefreshSavedProgress()
    {
        var result = GameProgressStore.Read(out var data);
        _hasProgress = data != null && data.HasProgress;
        if (_startButtonLabel != null) _startButtonLabel.text = _hasProgress ? "続きから" : "はじめる";
        if (_newGameButton != null) _newGameButton.SetActive(_hasProgress);
        if (_quitButton != null && _quitButton.transform is RectTransform quit)
            quit.anchorMin = quit.anchorMax = new Vector2(.38f, _hasProgress ? .155f : .231f);
        if (_saveSummary == null) return;
        _saveSummary.text = result == ProgressReadResult.Invalid
            ? "保存データを読み込めませんでした。\n「はじめる」で新しく遊べます。"
            : _hasProgress
                ? (result == ProgressReadResult.Recovered ? "前回の保存データから再開できます。\n" : "") +
                    (data.cleared ? "すべての本棚が完成しました" : $"完成した棚 {data.completedShelves.Length} / 8・収納 {data.placed.Length} / 140 冊")
                : "進行は自動で保存されます。";
    }

    private void Update()
    {
        if (_settings != null && _settings.IsOpen && Input.GetKeyDown(KeyCode.Escape)) { _settings.Close(); return; }
        if (_newGameDialog != null && _newGameDialog.activeSelf && Input.GetKeyDown(KeyCode.Escape)) CancelNewGame();
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }

    public void StartGame()
    {
        if (!CanUseTitleActions) return;
        BeginGame(!_hasProgress);
    }

    public void StartNewGame()
    {
        if (!CanUseTitleActions) return;
        if (!_hasProgress) { BeginGame(true); return; }
        if (_newGameDialog == null) return;
        PlaySound(_startButtonSound);
        _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        SetNewGameDialog(true);
        if (EventSystem.current != null && _cancelNewGameButton != null)
            EventSystem.current.SetSelectedGameObject(_cancelNewGameButton.gameObject);
    }

    public void CancelNewGame()
    {
        if (_isProcessing || _newGameDialog == null || !_newGameDialog.activeSelf) return;
        SetNewGameDialog(false);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_previousSelection);
    }

    public void ConfirmNewGame()
    {
        if (_isProcessing || _newGameDialog == null || !_newGameDialog.activeSelf) return;
        SetNewGameDialog(false);
        BeginGame(true);
    }

    private void SetNewGameDialog(bool visible)
    {
        if (_newGameDialog != null) _newGameDialog.SetActive(visible);
        RefreshInput();
    }

    public void RefreshInput()
    {
        bool available = CanUseTitleActions;
        if (_titleContent != null)
        {
            _titleContent.interactable = available;
            _titleContent.blocksRaycasts = available;
        }
        if (_settingsButton != null) _settingsButton.interactable = available;
    }

    private void BeginGame(bool newGame)
    {
        _isProcessing = true;
        RefreshInput();
        PlaySound(_startButtonSound);

        DOTween.Sequence()
            .SetId(this)
            .SetUpdate(true)
            .AppendInterval(_sceneChangeDelay)
            .Append(FadeOut())
            .OnComplete(() =>
            {
                if (newGame) GameProgressStore.RequestNewGame();
                SceneManager.LoadScene(_gameSceneName);
            });
    }

    public void QuitGame()
    {
        if (!CanUseTitleActions)
        {
            return;
        }

        _isProcessing = true;
        RefreshInput();
        PlaySound(_quitButtonSound);

        DOTween.Sequence()
            .SetId(this)
            .SetUpdate(true)
            .AppendInterval(_sceneChangeDelay)
            .Append(FadeOut())
            .OnComplete(() =>
            {
#if UNITY_EDITOR
                Debug.Log("ゲーム終了");
                if (_fadeCanvasGroup != null)
                {
                    _fadeCanvasGroup.alpha = 0;
                    _fadeCanvasGroup.blocksRaycasts = false;
                }
                _isProcessing = false;
                RefreshInput();
#else
                Application.Quit();
#endif
            });
    }

    private Tween FadeOut()
    {
        if (_fadeCanvasGroup == null || _fadeDuration <= 0f)
        {
            return DOVirtual.DelayedCall(0f, () => { });
        }

        _fadeCanvasGroup.blocksRaycasts = true;

        return _fadeCanvasGroup
            .DOFade(1f, _fadeDuration)
            .SetEase(_fadeEase);
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip, _buttonSoundVolume);
        }
    }
}
