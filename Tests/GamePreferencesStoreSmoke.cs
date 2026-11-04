// Edit-mode store checks. Only the settings key is changed, and restored in finally.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Edit mode.");
int checks=0;void Check(bool ok,string message){if(!ok)throw new System.Exception("FAIL: "+message);checks++;}
string key=GamePreferences.PreferenceKey;bool had=PlayerPrefs.HasKey(key);string original=PlayerPrefs.GetString(key,"");string progress=PlayerPrefs.GetString(GameProgressStore.PreferenceKey,"");int changes=0;System.Action observer=()=>changes++;
try{
    PlayerPrefs.DeleteKey(key);GamePreferences.ReloadFromStorage();GamePreferences.Changed+=observer;
    Check(GamePreferences.MusicVolume==1&&GamePreferences.EffectsVolume==1&&!GamePreferences.Muted&&!GamePreferences.ReducedMotion&&!GamePreferences.IsDirty,"Missing settings use the original mix and motion");
    GamePreferences.SetMusicVolume(.25f);GamePreferences.SetEffectsVolume(.6f);GamePreferences.SetMuted(true);GamePreferences.SetReducedMotion(true);
    Check(changes==4&&GamePreferences.IsDirty,"Changes publish once and mark the settings dirty");GamePreferences.SetMusicVolume(.25f);Check(changes==4,"Identical slider values do not republish");
    Check(GamePreferences.Save()&&!GamePreferences.IsDirty,"Valid settings persist");GamePreferences.ReloadFromStorage();Check(GamePreferences.MusicVolume==.25f&&GamePreferences.EffectsVolume==.6f&&GamePreferences.Muted&&GamePreferences.ReducedMotion,"Every setting survives a reload");
    GamePreferences.SetMuted(false);Check(GamePreferences.MusicVolume==.25f&&GamePreferences.EffectsVolume==.6f,"Unmuting retains both volume levels");
    GamePreferences.SetMusicVolume(-2);GamePreferences.SetEffectsVolume(4);Check(GamePreferences.MusicVolume==0&&GamePreferences.EffectsVolume==1,"Programmatic values are clamped");
    GamePreferences.SetMusicVolume(float.NaN);GamePreferences.SetEffectsVolume(float.PositiveInfinity);Check(GamePreferences.MusicVolume==1&&GamePreferences.EffectsVolume==1,"Nonfinite programmatic values use safe defaults");
    foreach(string json in new[]{"", "{}", "null", "{broken", "{\"version\":2,\"music\":1,\"effects\":1}", "{\"version\":1,\"music\":-1,\"effects\":1}", "{\"version\":1,\"music\":1,\"effects\":2}",new string('x',2049)})Check(!GamePreferences.TryParse(json,out _),"Invalid or unsupported settings rejected");
    PlayerPrefs.SetString(key,"{broken");GamePreferences.ReloadFromStorage();Check(GamePreferences.MusicVolume==1&&GamePreferences.EffectsVolume==1&&!GamePreferences.Muted&&!GamePreferences.ReducedMotion,"Unreadable settings recover to defaults");Check(PlayerPrefs.GetString(key)=="{broken","Reading alone does not overwrite an unreadable key");
    GamePreferences.SetEffectsVolume(.4f);Check(GamePreferences.Save()&&GamePreferences.TryParse(PlayerPrefs.GetString(key),out var saved)&&saved.effects==.4f,"An explicit change replaces unreadable settings");
    Check(PlayerPrefs.GetString(GameProgressStore.PreferenceKey,"")==progress,"Settings do not modify saved progress");
    return new{passed=checks};
}finally{GamePreferences.Changed-=observer;if(had)PlayerPrefs.SetString(key,original);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();GamePreferences.ReloadFromStorage();}
