// Run once in Edit mode before tests that change progress.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Stop Play before backing up progress.");
const string path=".utmp/progress-smoke-original.txt";
if(System.IO.File.Exists(path))throw new System.Exception("An earlier backup exists. Restore it first.");
System.IO.Directory.CreateDirectory(".utmp");
var keys=new[]{GameProgressStore.PreferenceKey,GameProgressStore.BackupKey};
var lines=keys.SelectMany(k=>new[]{PlayerPrefs.HasKey(k)?"1":"0",System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PlayerPrefs.GetString(k,"")))}).Concat(new[]{PlayerPrefs.HasKey(BookPlacementTutorial.PreferenceKey)?"1":"0",PlayerPrefs.GetInt(BookPlacementTutorial.PreferenceKey,0).ToString()}).ToArray();
System.IO.File.WriteAllLines(path,lines);
return "Progress and tutorial preferences backed up without changing them.";
