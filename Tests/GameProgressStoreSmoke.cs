// Edit mode. Only the two progress preferences are changed, then restored in finally.
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Edit mode.");
int checks=0;
void Check(bool ok,string label) { if(!ok)throw new System.Exception("FAIL: "+label);checks++; }
string key=GameProgressStore.PreferenceKey, backup=GameProgressStore.BackupKey;
bool had=PlayerPrefs.HasKey(key),hadBackup=PlayerPrefs.HasKey(backup);
string old=PlayerPrefs.GetString(key,""),oldBackup=PlayerPrefs.GetString(backup,"");
GameProgressData Clone(GameProgressData d)=>JsonUtility.FromJson<GameProgressData>(JsonUtility.ToJson(d));
try {
    PlayerPrefs.DeleteKey(key);PlayerPrefs.DeleteKey(backup);
    Check(GameProgressStore.Read(out _)==ProgressReadResult.None,"No save is distinct from corrupt data");
    var empty=GameProgressData.Empty();
    Check(empty.IsValid()&&!empty.HasProgress&&GameProgressStore.Write(empty,true),"An empty new game is valid");
    Check(GameProgressStore.Read(out var read)==ProgressReadResult.Loaded&&!read.HasProgress,"Empty progress is not offered as Continue");
    var data=GameProgressData.Empty();data.hand=new[]{"Green03","Blue02"};data.placed=new[]{new SavedBookPlacement{book="Green01",slot=1},new SavedBookPlacement{book="Green02",slot=0},new SavedBookPlacement{book="Blue01",slot=18}};
    Check(data.IsValid()&&GameProgressStore.Write(data,true),"Wrong ordering within the same shelf is saved");
    Check(GameProgressStore.Read(out read)==ProgressReadResult.Loaded&&JsonUtility.ToJson(read)==JsonUtility.ToJson(data),"Round trip preserves hand order and exact positions");
    var next=Clone(data);next.hand=new[]{"Green03","Blue02","Green04"};
    Check(GameProgressStore.Write(next),"A subsequent operation saves");
    PlayerPrefs.SetString(key,"{broken");
    Check(GameProgressStore.Read(out read)==ProgressReadResult.Recovered&&JsonUtility.ToJson(read)==JsonUtility.ToJson(data),"A broken primary recovers the previous valid snapshot");
    Check(PlayerPrefs.GetString(key)=="{broken","Reading does not overwrite the damaged primary");
    PlayerPrefs.SetString(backup,"null");
    Check(GameProgressStore.Read(out read)==ProgressReadResult.Invalid&&read==null,"Both broken copies are reported without fabricating progress");
    Check(GameProgressStore.Write(empty,true)&&PlayerPrefs.GetString(key)==PlayerPrefs.GetString(backup),"Explicit New Game replaces both generations");
    var bad=Clone(data);bad.hand=new[]{"Green01"};Check(!bad.IsValid(),"A book cannot exist in hand and on a shelf");
    bad=Clone(data);bad.hand=new[]{"Green03","Green03"};Check(!bad.IsValid(),"Duplicate hand books rejected");
    bad=Clone(data);bad.placed[1].slot=1;Check(!bad.IsValid(),"Duplicate shelf positions rejected");
    bad=Clone(data);bad.placed[0].slot=18;Check(!bad.IsValid(),"Cross-color placement rejected");
    bad=Clone(data);bad.hand=Enumerable.Range(1,10).Select(i=>"Purple"+i.ToString("D2")).ToArray();Check(!bad.IsValid(),"Inventory overflow rejected");
    bad=Clone(data);bad.version=2;Check(!GameProgressStore.TryParse(JsonUtility.ToJson(bad),out _),"Unsupported version rejected");
    foreach(string invalid in new[]{"Green00","Green19","Blue18","Green1","Other01","xGreen01","Green+1"}) { bad=Clone(data);bad.hand=new[]{invalid};Check(!bad.IsValid(),"Unknown identity rejected: "+invalid); }
    bad=Clone(data);bad.placed[0]=null;Check(!bad.IsValid(),"Null placement rejected");
    bad=Clone(data);bad.hand=null;Check(!bad.IsValid(),"Missing arrays rejected");
    bad=Clone(data);bad.placed[0].slot=140;Check(!bad.IsValid(),"Out-of-range position rejected");
    bad=Clone(data);bad.completedShelves=new[]{0};Check(!bad.IsValid(),"Incomplete or misordered shelf cannot be locked");
    var full=GameProgressData.Empty();var sprites=Resources.LoadAll<Sprite>("Books").Where(s=>BookColorUtility.TryGetBookIndex(s.name,out _)).ToArray();
    Check(sprites.Length==140,"All current identities are supported");
    full.placed=sprites.Select(s=>{BookColorUtility.TryGetBookIndex(s.name,out int i);return new SavedBookPlacement{book=s.name,slot=i};}).ToArray();
    full.completedShelves=Enumerable.Range(0,8).ToArray();full.cleared=true;
    Check(full.IsValid()&&GameProgressStore.Write(full,true),"All eight completed shelves and game clear are persisted");
    bad=Clone(full);bad.placed=bad.placed.Skip(1).ToArray();Check(!bad.IsValid(),"A partial game cannot claim completion");
    bad=Clone(full);bad.completedShelves=new[]{0,0};Check(!bad.IsValid(),"Repeated completion flags rejected");
    Check(!GameProgressStore.TryParse("{}",out _)&&!GameProgressStore.TryParse("null",out _)&&!GameProgressStore.TryParse(new string('x',32769),out _),"Malformed or excessive data rejected");
    string good=PlayerPrefs.GetString(key);Check(!GameProgressStore.Write(bad)&&PlayerPrefs.GetString(key)==good,"Invalid snapshots never overwrite a valid save");
    return new { passed=checks, maxSnapshotBytes=System.Text.Encoding.UTF8.GetByteCount(JsonUtility.ToJson(full)) };
} finally {
    if(had)PlayerPrefs.SetString(key,old);else PlayerPrefs.DeleteKey(key);
    if(hadBackup)PlayerPrefs.SetString(backup,oldBackup);else PlayerPrefs.DeleteKey(backup);
    PlayerPrefs.Save();
}
