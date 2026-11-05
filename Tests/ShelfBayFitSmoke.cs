// All 140 books placed, in Play. Check actual rendered book bounds against real cabinet edges.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Play.");
int checks=0;void Check(bool ok,string label){if(!ok)throw new System.Exception("FAIL: "+label);checks++;}
var posts=GameObject.Find("GameplayRoom3D/Bookcase").GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Upright").OrderBy(r=>r.bounds.center.x).ToArray();
var shelves=UnityEngine.Object.FindObjectsByType<BookShelfJudge>(UnityEngine.FindObjectsSortMode.None).OrderBy(s=>s.name).ToArray();
Check(posts.Length==3&&shelves.Length==8,"Physical cabinet has two bays and eight book rows");
foreach(var shelf in shelves)
{
    var slots=shelf.GetComponentsInChildren<BookSlot>().OrderBy(s=>s.SlotIndex).ToArray();
    Check(slots.All(s=>s.IsFilled),"Full row: "+shelf.name);
    int col=slots[0].ShelfColorIndex%2;
    float first=slots[0].PlacedBook.transform.Find("BindingAndPages").GetComponent<Renderer>().bounds.min.x;
    float last=slots.Last().PlacedBook.transform.Find("BindingAndPages").GetComponent<Renderer>().bounds.max.x;
    float left=first-posts[col].bounds.max.x,right=posts[col+1].bounds.min.x-last;
    Check(left>=-.0001f&&left<=.025f&&right>=-.0001f&&right<=.025f,"Actual books fill the bay without touching or leaving wide empty ends: "+shelf.name);
    Check(slots.All(s=>s.PlacedBook.transform.lossyScale==Vector3.one),"Original book and artwork proportions are preserved: "+shelf.name);
}
return new{passed=checks,shelves=shelves.Length};
