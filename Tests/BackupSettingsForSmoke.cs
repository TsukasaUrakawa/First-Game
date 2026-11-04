if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Stop Play first.");
const string path=".utmp/settings-smoke-original.txt";
if(System.IO.File.Exists(path))throw new System.Exception("An earlier settings backup exists; restore it first.");
System.IO.Directory.CreateDirectory(".utmp");
System.IO.File.WriteAllLines(path,new[]{PlayerPrefs.HasKey(GamePreferences.PreferenceKey)?"1":"0",System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PlayerPrefs.GetString(GamePreferences.PreferenceKey,"")))});
return "Original settings backed up.";
