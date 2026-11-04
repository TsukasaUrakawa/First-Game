using System;
using UnityEngine;

public enum ProgressReadResult { None, Loaded, Recovered, Invalid }

/// <summary>Small versioned snapshots, stored in PlayerPrefs (IndexedDB on Web).</summary>
public static class GameProgressStore
{
    public const string PreferenceKey = "ShelfCare.Progress.v1";
    public const string BackupKey = "ShelfCare.Progress.v1.backup";
    private static bool _newGameRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() => _newGameRequested = false;

    public static void RequestNewGame() => _newGameRequested = true;

    internal static bool ConsumeNewGameRequest()
    {
        bool requested = _newGameRequested;
        _newGameRequested = false;
        return requested;
    }

    public static ProgressReadResult Read(out GameProgressData data)
    {
        data = null;
        try
        {
            if (TryParse(PlayerPrefs.GetString(PreferenceKey, ""), out data)) return ProgressReadResult.Loaded;
            if (TryParse(PlayerPrefs.GetString(BackupKey, ""), out data)) return ProgressReadResult.Recovered;
            return PlayerPrefs.HasKey(PreferenceKey) || PlayerPrefs.HasKey(BackupKey)
                ? ProgressReadResult.Invalid : ProgressReadResult.None;
        }
        catch (Exception) { return ProgressReadResult.Invalid; }
    }

    public static bool TryParse(string json, out GameProgressData data)
    {
        data = null;
        if (string.IsNullOrEmpty(json) || json.Length > 32768) return false;
        try
        {
            var parsed = JsonUtility.FromJson<GameProgressData>(json);
            if (parsed == null || !parsed.IsValid()) return false;
            data = parsed;
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    public static bool Write(GameProgressData data, bool resetHistory = false)
    {
        if (data == null || !data.IsValid()) return false;
        string previous = null, previousBackup = null;
        bool hadPrimary = false, hadBackup = false, captured = false;
        try
        {
            hadPrimary = PlayerPrefs.HasKey(PreferenceKey);
            hadBackup = PlayerPrefs.HasKey(BackupKey);
            previous = PlayerPrefs.GetString(PreferenceKey, "");
            previousBackup = PlayerPrefs.GetString(BackupKey, "");
            captured = true;
            string json = JsonUtility.ToJson(data);
            string backup = resetHistory ? json : TryParse(previous, out _) ? previous :
                TryParse(previousBackup, out _) ? previousBackup : json;
            PlayerPrefs.SetString(BackupKey, backup);
            PlayerPrefs.SetString(PreferenceKey, json);
            PlayerPrefs.Save();
            return true;
        }
        catch (Exception)
        {
            // Keep the last readable snapshot in memory as well if a write fails.
            if (captured)
            {
                try
                {
                    if (hadPrimary) PlayerPrefs.SetString(PreferenceKey, previous); else PlayerPrefs.DeleteKey(PreferenceKey);
                    if (hadBackup) PlayerPrefs.SetString(BackupKey, previousBackup); else PlayerPrefs.DeleteKey(BackupKey);
                    PlayerPrefs.Save();
                }
                catch (Exception) { /* Storage remains unavailable; the UI reports failure. */ }
            }
            return false;
        }
    }
}
