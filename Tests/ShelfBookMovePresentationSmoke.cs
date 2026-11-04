// Select a shelf book and let at least one frame render before running this check.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Play.");
GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
int checks=0;void Check(bool ok,string message){if(!ok)throw new System.Exception("FAIL: "+message);checks++;}
var move=ShelfBookMoveController.Instance;Check(move.HasSelection,"A shelf book is selected");
var hand=UnityEngine.Object.FindFirstObjectByType<HandItemController>();
Check(Named("ShelfMoveActions").activeInHierarchy&&!Named("ShelfNormalActions").activeInHierarchy,"Context actions replace the usual shelf actions");
Check(Named("SelectedShelfBookImage").GetComponent<UnityEngine.UI.Image>().sprite==move.SelectedSprite,"Preview shows the selected spine");
var canvas=Named("Canvas").GetComponent<RectTransform>();
var toolbar=Named("ShelfActionPanel").GetComponent<RectTransform>();
var corners=new Vector3[4];toolbar.GetWorldCorners(corners);Check(corners.All(p=>canvas.rect.Contains(canvas.InverseTransformPoint(p))),"Toolbar fits on screen");
foreach(var label in Named("ShelfMoveActions").GetComponentsInChildren<TMPro.TMP_Text>()){label.ForceMeshUpdate();Check(!label.isTextOverflowing,label.name+" fits");Check(label.text.Where(c=>!char.IsControl(c)).All(c=>label.font.HasCharacter(c)),label.name+" glyphs");}
foreach(string name in new[]{"ReturnShelfBookButton","CancelShelfMoveButton"}){
    var button=Named(name).GetComponent<UnityEngine.UI.Button>();var input=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};
    var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(input,hits);Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>()==button,name+" receives pointer input");
}
Check(Named("ReturnShelfBookButton").GetComponent<UnityEngine.UI.Button>().interactable==!hand.IsFull,"Return availability follows hand capacity");
Check(Named("ReturnShelfBookButtonLabel").GetComponent<TMPro.TMP_Text>().text==(hand.IsFull?"手持ちがいっぱい":"手持ちに戻す"),"Full-hand explanation is visible");
Check(move.SelectedSlot.PlacedBook.GetComponent<SpriteRenderer>().color!=Color.white,"Selected book itself has a visible tint");
var camera=UnityEngine.Object.FindFirstObjectByType<CameraController>().GetComponent<Camera>();
foreach(var slot in move.SelectedSlot.ShelfJudge.GetComponentsInChildren<BookSlot>()){
    var point=(Vector2)camera.WorldToScreenPoint(slot.transform.position);var input=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point};var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(input,hits);Check(hits.Count==0,"Shelf position "+slot.SlotIndex+" remains reachable beneath UI");
}
return new{passed=checks,width=Screen.width,height=Screen.height,fullHand=hand.IsFull};
