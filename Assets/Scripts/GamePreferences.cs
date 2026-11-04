using System;
using UnityEngine;

[Serializable]
public sealed class GamePreferencesData
{
    public int version;
    public float music;
    public float effects;
    public bool muted;
    public bool reducedMotion;
    public static GamePreferencesData Defaults() => new GamePreferencesData { version = 1, music = 1, effects = 1 };
    public bool IsValid() => version == 1 && ValidVolume(music) && ValidVolume(effects);
    private static bool ValidVolume(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0 && value <= 1;
}

/// <summary>Independent from progress. Changes apply live; the dialog batches disk writes.</summary>
public static class GamePreferences
{
    public const string PreferenceKey = "ShelfCare.Settings.v1";
    private static GamePreferencesData _data;
    public static event Action Changed;
    public static bool IsDirty { get; private set; }
    public static float MusicVolume { get { EnsureLoaded(); return _data.music; } }
    public static float EffectsVolume { get { EnsureLoaded(); return _data.effects; } }
    public static bool Muted { get { EnsureLoaded(); return _data.muted; } }
    public static bool ReducedMotion { get { EnsureLoaded(); return _data.reducedMotion; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() { _data = null; IsDirty = false; Changed = null; }

    private static void EnsureLoaded()
    {
        if (_data != null) return;
        try { TryParse(PlayerPrefs.GetString(PreferenceKey, ""), out _data); }
        catch (Exception) { _data = null; }
        if (_data == null) _data = GamePreferencesData.Defaults();
    }

    public static bool TryParse(string json, out GamePreferencesData data)
    {
        data = null;
        if (string.IsNullOrEmpty(json) || json.Length > 2048) return false;
        try
        {
            var parsed = JsonUtility.FromJson<GamePreferencesData>(json);
            if (parsed == null || !parsed.IsValid()) return false;
            data = parsed;
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    private static float Normalize(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 1 : Mathf.Clamp01(value);
    private static void Publish() { IsDirty = true; Changed?.Invoke(); }
    public static void SetMusicVolume(float value) { EnsureLoaded(); value = Normalize(value); if (_data.music == value) return; _data.music = value; Publish(); }
    public static void SetEffectsVolume(float value) { EnsureLoaded(); value = Normalize(value); if (_data.effects == value) return; _data.effects = value; Publish(); }
    public static void SetMuted(bool value) { EnsureLoaded(); if (_data.muted == value) return; _data.muted = value; Publish(); }
    public static void SetReducedMotion(bool value) { EnsureLoaded(); if (_data.reducedMotion == value) return; _data.reducedMotion = value; Publish(); }

    public static void ReloadFromStorage() { _data = null; IsDirty = false; EnsureLoaded(); Changed?.Invoke(); }

    public static bool Save()
    {
        EnsureLoaded();
        if (!IsDirty) return true;
        string previous = ""; bool existed = false, captured = false;
        try
        {
            existed = PlayerPrefs.HasKey(PreferenceKey);
            previous = PlayerPrefs.GetString(PreferenceKey, ""); captured = true;
            PlayerPrefs.SetString(PreferenceKey, JsonUtility.ToJson(_data)); PlayerPrefs.Save();
            IsDirty = false;
            return true;
        }
        catch (Exception)
        {
            if (captured)
            {
                try { if (existed) PlayerPrefs.SetString(PreferenceKey, previous); else PlayerPrefs.DeleteKey(PreferenceKey); PlayerPrefs.Save(); }
                catch (Exception) { /* Keep live values; a subsequent change or close retries. */ }
            }
            return false;
        }
    }
}
