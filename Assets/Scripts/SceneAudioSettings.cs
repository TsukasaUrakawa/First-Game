using System.Collections.Generic;
using UnityEngine;

/// <summary>Scales authored gains, including PlayOneShot sources, without restarting music.</summary>
[DefaultExecutionOrder(-300)]
public sealed class SceneAudioSettings : MonoBehaviour
{
    [SerializeField] private AudioSource[] _musicSources = new AudioSource[0];
    [SerializeField] private AudioSource _effectsSource;
    [SerializeField] private AudioClip _clickSound;
    [SerializeField, Range(0f, 1f)] private float _clickVolume = .6f;
    public float ClickDuration => _clickSound != null ? _clickSound.length : 0;
    private struct AuthoredLevel { public float volume; public bool muted; }
    private readonly Dictionary<AudioSource, AuthoredLevel> _levels = new Dictionary<AudioSource, AuthoredLevel>();

    private void OnEnable()
    {
        GamePreferences.Changed += Apply;
        BindSources();
    }

    // Include AudioSources created by other components' Awake methods.
    private void Start() => BindSources();

    private void BindSources()
    {
        foreach (var source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (source.gameObject.scene == gameObject.scene && !_levels.ContainsKey(source))
                _levels.Add(source, new AuthoredLevel { volume = source.volume, muted = source.mute });
        Apply();
    }

    private void Apply()
    {
        foreach (var pair in _levels)
        {
            if (pair.Key == null) continue;
            bool music = System.Array.IndexOf(_musicSources, pair.Key) >= 0;
            pair.Key.volume = pair.Value.volume * (music ? GamePreferences.MusicVolume : GamePreferences.EffectsVolume);
            pair.Key.mute = pair.Value.muted || GamePreferences.Muted;
        }
    }

    public void PlayClick()
    {
        if (_effectsSource != null && _clickSound != null)
            _effectsSource.PlayOneShot(_clickSound, _clickVolume);
    }

    private void OnDisable()
    {
        GamePreferences.Changed -= Apply;
        foreach (var pair in _levels)
            if (pair.Key != null) { pair.Key.volume = pair.Value.volume; pair.Key.mute = pair.Value.muted; }
        _levels.Clear();
    }
}
