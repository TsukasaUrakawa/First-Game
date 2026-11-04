// Run in Edit mode after the tests, including after a failed test.
if(UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Stop Play before restoring progress.");
const string path=".utmp/progress-smoke-original.txt";
var lines=System.IO.File.ReadAllLines(path);
if(lines.Length!=6)throw new System.Exception("Unexpected backup format; no preferences changed.");
var values=new[]{System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(lines[1])),System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(lines[3]))};
int tutorial=int.Parse(lines[5]);
if(new[]{lines[0],lines[2],lines[4]}.Any(s=>s!="0"&&s!="1"))throw new System.Exception("Invalid presence flags; no preferences changed.");
var keys=new[]{GameProgressStore.PreferenceKey,GameProgressStore.BackupKey};
for(int i=0;i<keys.Length;i++){if(lines[i*2]=="1")PlayerPrefs.SetString(keys[i],values[i]);else PlayerPrefs.DeleteKey(keys[i]);}
if(lines[4]=="1")PlayerPrefs.SetInt(BookPlacementTutorial.PreferenceKey,tutorial);else PlayerPrefs.DeleteKey(BookPlacementTutorial.PreferenceKey);
PlayerPrefs.Save();System.IO.File.Delete(path);
return "Original progress and tutorial preferences restored.";
