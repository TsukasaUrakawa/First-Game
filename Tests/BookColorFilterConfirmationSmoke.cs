// Open a mixed-color confirmation and let a frame render before running this check.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Play.");
GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
var popup=UnityEngine.Object.FindFirstObjectByType<BookPopUpController>();if(!popup.IsOpen)throw new System.Exception("Open confirmation first.");
var overlay=Named("BookPopUp").transform;int checks=0;
for(int i=0;i<9;i++){var chip=Named("BookColorFilter"+i);var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,chip.transform.position)},hits);if(hits.Count==0||!hits[0].gameObject.transform.IsChildOf(overlay))throw new System.Exception("Filter "+i+" is above the confirmation.");checks++;}
return new{passed=checks,width=Screen.width,height=Screen.height};
