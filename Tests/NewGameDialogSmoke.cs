// Open New Game confirmation, let a frame render, then run this check.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Play mode.");
UnityEngine.GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
if(!Named("NewGameDialog").activeInHierarchy)throw new System.Exception("Open confirmation first.");
int checks=0;
void Check(bool ok,string label){if(!ok)throw new System.Exception("FAIL: "+label);checks++;}
foreach(string name in new[]{"CancelNewGameButton","ConfirmNewGameButton"}){
    var button=Named(name).GetComponent<UnityEngine.UI.Button>();
    var input=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};
    var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(input,hits);
    Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>()==button,name+" receives pointer input");
}
foreach(string name in new[]{"StartButton","NewGameButton"}){
    var input=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,Named(name).transform.position)};
    var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(input,hits);
    Check(hits.Count>0&&hits[0].gameObject.transform.IsChildOf(Named("NewGameDialog").transform),"Confirmation blocks "+name);
}
return new{passed=checks,width=Screen.width,height=Screen.height};
