using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _effectsSlider;
    [SerializeField] private TMP_Text _musicPercent;
    [SerializeField] private TMP_Text _effectsPercent;
    [SerializeField] private Toggle _mutedToggle;
    [SerializeField] private Toggle _motionToggle;
    [SerializeField] private TMP_Text _saveStatus;
    [SerializeField] private TitleSceneController _title;
    [SerializeField] private GameplayUIController _gameplay;
    [SerializeField] private AudioSource _effectsSource;
    [SerializeField] private AudioClip _previewSound;
    private GameObject _previousSelection;
    private bool _savePending;
    private float _saveAfter;
    public bool IsOpen => _panel != null && _panel.activeSelf;

    private void Awake() => _panel.SetActive(false);
    private void OnEnable()
    {
        GamePreferences.Changed += OnPreferencesChanged;
        _musicSlider.onValueChanged.AddListener(GamePreferences.SetMusicVolume);
        _effectsSlider.onValueChanged.AddListener(GamePreferences.SetEffectsVolume);
        _mutedToggle.onValueChanged.AddListener(GamePreferences.SetMuted);
        _motionToggle.onValueChanged.AddListener(GamePreferences.SetReducedMotion);
        Refresh();
    }

    private void OnDisable()
    {
        SaveNow();
        GamePreferences.Changed -= OnPreferencesChanged;
        _musicSlider.onValueChanged.RemoveListener(GamePreferences.SetMusicVolume);
        _effectsSlider.onValueChanged.RemoveListener(GamePreferences.SetEffectsVolume);
        _mutedToggle.onValueChanged.RemoveListener(GamePreferences.SetMuted);
        _motionToggle.onValueChanged.RemoveListener(GamePreferences.SetReducedMotion);
    }

    public void Open()
    {
        if (IsOpen || (_title != null && !_title.CanUseTitleActions) || (_gameplay != null && _gameplay.HasModal)) return;
        _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        _panel.SetActive(true);
        _panel.transform.SetAsLastSibling();
        Refresh();
        _title?.RefreshInput();
        _gameplay?.OnSettingsOpened();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_closeButton.gameObject);
    }

    public void Close()
    {
        if (!IsOpen) return;
        SaveNow();
        _panel.SetActive(false);
        _title?.RefreshInput();
        _gameplay?.RefreshVisibility();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(
            _previousSelection != null && _previousSelection.activeInHierarchy ? _previousSelection : null);
    }

    public void PreviewSound()
    {
        if (IsOpen && _effectsSource != null && _previewSound != null) _effectsSource.PlayOneShot(_previewSound, .6f);
    }

    private void OnPreferencesChanged()
    {
        Refresh();
        _savePending = GamePreferences.IsDirty;
        _saveAfter = Time.unscaledTime + .35f;
        if (_savePending) _saveStatus.text = "設定を保存中…";
    }

    private void Refresh()
    {
        _musicSlider.SetValueWithoutNotify(GamePreferences.MusicVolume);
        _effectsSlider.SetValueWithoutNotify(GamePreferences.EffectsVolume);
        _mutedToggle.SetIsOnWithoutNotify(GamePreferences.Muted);
        _motionToggle.SetIsOnWithoutNotify(GamePreferences.ReducedMotion);
        _musicPercent.text = Mathf.RoundToInt(GamePreferences.MusicVolume * 100) + "%";
        _effectsPercent.text = Mathf.RoundToInt(GamePreferences.EffectsVolume * 100) + "%";
    }

    private void LateUpdate() { if (_savePending && Time.unscaledTime >= _saveAfter) SaveNow(); }
    public bool SaveNow()
    {
        _savePending = false;
        if (!GamePreferences.IsDirty) return true;
        bool saved = GamePreferences.Save();
        if (_saveStatus != null) _saveStatus.text = saved ? "設定を保存しました" : "設定を保存できませんでした。";
        return saved;
    }
    private void OnApplicationFocus(bool focused) { if (!focused) SaveNow(); }
    private void OnApplicationPause(bool paused) { if (paused) SaveNow(); }
    private void OnApplicationQuit() => SaveNow();
}
