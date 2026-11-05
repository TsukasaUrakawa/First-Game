// Room view, no menus. Tests the visible box mesh and actual collider hit points.
if (!UnityEditor.EditorApplication.isPlaying || GameplayUIController.Instance.HasModal || UnityEngine.Object.FindFirstObjectByType<CameraController>().IsZoomed) throw new System.Exception("Show the room with menus closed.");
int checks=0; void Check(bool ok,string label){if(!ok)throw new System.Exception("FAIL: "+label);checks++;}
var box=UnityEngine.Object.FindFirstObjectByType<BoxOpenAndClose>();var camera=Camera.main;
Rect Project(Bounds b){var points=new System.Collections.Generic.List<Vector3>();for(int x=0;x<2;x++)for(int y=0;y<2;y++)for(int z=0;z<2;z++)points.Add(camera.WorldToScreenPoint(new Vector3(x==0?b.min.x:b.max.x,y==0?b.min.y:b.max.y,z==0?b.min.z:b.max.z)));return Rect.MinMaxRect(points.Min(p=>p.x),points.Min(p=>p.y),points.Max(p=>p.x),points.Max(p=>p.y));}
Rect Clip(Rect r)=>Rect.MinMaxRect(Mathf.Max(0,r.xMin),Mathf.Max(0,r.yMin),Mathf.Min(Screen.width,r.xMax),Mathf.Min(Screen.height,r.yMax));
var renderers=box.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
var meshBounds=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))meshBounds.Encapsulate(renderer.bounds);
var visible=Clip(Project(meshBounds));
var panel=UnityEngine.Object.FindObjectsByType<RectTransform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name=="ShelfButtonPanel");var corners=new Vector3[4];panel.GetWorldCorners(corners);
var navigation=Rect.MinMaxRect(corners.Min(p=>p.x),corners.Min(p=>p.y),corners.Max(p=>p.x),corners.Max(p=>p.y));
Check(visible.width>0&&visible.height>0,"Box is visible");
Check(!visible.Overlaps(navigation),"Shelf navigation does not cover the box mesh");
float scale=panel.GetComponentInParent<Canvas>().scaleFactor;
Check(navigation.yMin-visible.yMax>=24*scale,"Box and navigation have a clear vertical gap");
var collider=box.GetComponent<Collider>();var hitArea=Clip(Project(collider.bounds));int hitPoints=0;
foreach(float x in new[]{.1f,.3f,.5f,.7f,.9f})foreach(float y in new[]{.1f,.3f,.5f,.7f,.9f})
{
    var point=new Vector2(Mathf.Lerp(hitArea.xMin,hitArea.xMax,x),Mathf.Lerp(hitArea.yMin,hitArea.yMax,y));
    if(!collider.Raycast(camera.ScreenPointToRay(point),out var hit,1000))continue;
    hitPoints++;var uiHits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
    UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point},uiHits);
    Check(uiHits.Count==0,"Box collider point is not covered by UI: "+point);
}
Check(hitPoints>=3,"Several visible parts of the collider are reachable");
var body=box.GetComponentInChildren<SkinnedMeshRenderer>();
var bodyRect=Project(body.bounds);
Check(bodyRect.xMin>=6*scale&&bodyRect.xMax<=Screen.width-6*scale&&bodyRect.yMin>=6*scale&&bodyRect.yMax<=Screen.height-6*scale,"Authored box size fits the screen without clipping");
for(int x=0;x<2;x++)for(int y=0;y<2;y++)for(int z=0;z<2;z++)
    Check(camera.WorldToViewportPoint(new Vector3(x==0?body.bounds.min.x:body.bounds.max.x,y==0?body.bounds.min.y:body.bounds.max.y,z==0?body.bounds.min.z:body.bounds.max.z)).z>camera.nearClipPlane,"The camera near plane cannot cut into the cardboard faces");
float floorY=GameObject.Find("GameplayRoom3D/Floor").GetComponent<Renderer>().bounds.max.y;
Check(Mathf.Abs(body.bounds.min.y-floorY)<.002f,"The visible cardboard bottom touches the actual floor, independently of its larger click collider");
var contact=GameObject.Find("GameplayRoom3D/FloorContactShadow").GetComponent<MeshRenderer>();
Check(contact.enabled&&contact.bounds.center.y>floorY&&contact.bounds.center.y<floorY+.02f,"Soft contact shadow stays just above the floor");
Check(contact.GetComponentsInChildren<Collider>().Length==0,"Floor contact shadow cannot block input");
Check(contact.bounds.size.y<.001f,"Changing the box rotation or scale cannot tilt its floor shadow");
return new{passed=checks,width=Screen.width,height=Screen.height,hitPoints,gap=navigation.yMin-visible.yMax};
