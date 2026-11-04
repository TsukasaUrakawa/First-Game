using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Suspends input without clearing the view or its transient selections.</summary>
public sealed class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _returnButton;
    [SerializeField] private TMP_Text _status;
    [SerializeField] private GameplayUIController _ui;
    [SerializeField] private GameProgressController _progress;
    [SerializeField] private SettingsPanelController _settings;
    [SerializeField] private SceneAudioSettings _audio;
    [SerializeField] private SceneButtonController _scenes;
    private readonly List<(Selectable control, bool interactable)> _controls = new List<(Selectable, bool)>();
    private readonly List<(ScrollRect scroll, bool enabled)> _scrolls = new List<(ScrollRect, bool)>();
    private GameObject _previousSelection;
    private float _previousTimeScale;
    private bool _holdsPause, _leaving;
    public bool IsOpen => _panel != null && _panel.activeSelf;

    private void Awake() => _panel.SetActive(false);

    public void Open()
    {
        if (IsOpen || _ui.IsGameCleared || _ui.IsSettingsOpen) return;
        _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        _previousTimeScale = Time.timeScale;
        _holdsPause = true;
        _leaving = false;
        _controls.Clear();
        foreach (var control in FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (control.gameObject.scene != gameObject.scene || control.transform.IsChildOf(_panel.transform)) continue;
            _controls.Add((control, control.interactable));
            control.interactable = false;
        }
        _scrolls.Clear();
        foreach (var scroll in FindObjectsByType<ScrollRect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (scroll.gameObject.scene != gameObject.scene) continue;
            _scrolls.Add((scroll, scroll.enabled));
            scroll.enabled = false;
        }
        Time.timeScale = 0;
        _status.text = "";
        _resumeButton.interactable = _returnButton.interactable = true;
        _panel.SetActive(true);
        _panel.transform.SetAsLastSibling();
        _ui.RefreshPauseVisibility();
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
        _audio.PlayClick();
    }

    public void Resume()
    {
        if (!IsOpen || _leaving) return;
        Close();
        _audio.PlayClick();
    }

    private void Close()
    {
        _panel.SetActive(false);
        ReleasePause();
        _ui.RefreshPauseVisibility();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_previousSelection != null && _previousSelection.activeInHierarchy ? _previousSelection : null);
    }

    public void SaveAndReturnToTitle()
    {
        if (!IsOpen || _leaving) return;
        _audio.PlayClick();
        if (!_progress.SaveNow() || !_settings.SaveNow())
        {
            _status.text = "保存できませんでした。\nもう一度試すか、「遊びに戻る」を選んでください。";
            return;
        }
        _leaving = true;
        _resumeButton.interactable = _returnButton.interactable = false;
        _status.text = "保存しました。タイトルへ戻ります。";
        StartCoroutine(ReturnAfterClick());
    }

    private IEnumerator ReturnAfterClick()
    {
        yield return new WaitForSecondsRealtime(_audio.ClickDuration + .05f);
        // Resume the clock before entering Title; never carry the pause across scenes.
        ReleasePause();
        _scenes.LoadTitleScene();
    }

    public void CloseForClear()
    {
        if (!IsOpen) return;
        StopAllCoroutines();
        _leaving = false;
        Close();
    }

    private void ReleasePause()
    {
        if (!_holdsPause) return;
        Time.timeScale = _previousTimeScale;
        _holdsPause = false;
        foreach (var state in _controls)
            if (state.control != null) state.control.interactable = state.interactable;
        _controls.Clear();
        foreach (var state in _scrolls)
            if (state.scroll != null) state.scroll.enabled = state.enabled;
        _scrolls.Clear();
    }

    private void OnDisable() => ReleasePause();
}
