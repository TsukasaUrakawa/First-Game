if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Stop Play first.");
const string path=".utmp/settings-smoke-original.txt";var lines=System.IO.File.ReadAllLines(path);
if(lines.Length!=2||(lines[0]!="0"&&lines[0]!="1"))throw new System.Exception("Invalid settings backup.");
string original=System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(lines[1]));
if(lines[0]=="1")PlayerPrefs.SetString(GamePreferences.PreferenceKey,original);else PlayerPrefs.DeleteKey(GamePreferences.PreferenceKey);
PlayerPrefs.Save();GamePreferences.ReloadFromStorage();System.IO.File.Delete(path);
return "Original settings restored.";
