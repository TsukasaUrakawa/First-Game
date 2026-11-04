// Play in TitleScene, after its intro. Saves are preserved by this check.
if(!UnityEditor.EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="TitleScene")throw new System.Exception("Run in TitleScene Play mode.");
UnityEngine.GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
int checks=0;
void Check(bool ok,string label){if(!ok)throw new System.Exception("FAIL: "+label);checks++;}
bool Fits(RectTransform child,RectTransform parent){var corners=new Vector3[4];child.GetWorldCorners(corners);return corners.All(p=>parent.rect.Contains(parent.InverseTransformPoint(p)));}
void TextFits(GameObject root){foreach(var label in root.GetComponentsInChildren<TMPro.TMP_Text>()){label.ForceMeshUpdate();Check(!label.isTextOverflowing,label.name+" fits");Check(label.text.Where(c=>!char.IsControl(c)).All(c=>label.font.HasCharacter(c)),label.name+" glyphs");}}
void Hit(string n){var button=Named(n).GetComponent<UnityEngine.UI.Button>();var point=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);var input=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point};var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(input,hits);Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>()==button,n+" receives pointer input");}
var state=GameProgressStore.Read(out var data);bool has=data!=null&&data.HasProgress;
Check(Named("StartButtonLabel").GetComponent<TMPro.TMP_Text>().text==(has?"続きから":"はじめる"),"Primary action matches saved progress");
Check(Named("NewGameButton").activeSelf==has,"New Game only appears beside an existing save");
Check(!Named("NewGameDialog").activeSelf,"Confirmation starts hidden");
TextFits(Named("TitlePresentation"));Hit("StartButton");
var controller=UnityEngine.Object.FindFirstObjectByType<TitleSceneController>();
if(has){
    string before=PlayerPrefs.GetString(GameProgressStore.PreferenceKey),backup=PlayerPrefs.GetString(GameProgressStore.BackupKey);
    var newButton=Named("NewGameButton");UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(newButton);
    Hit("NewGameButton");newButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Canvas.ForceUpdateCanvases();
    Check(Named("NewGameDialog").activeSelf,"New Game opens confirmation");
    Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==Named("CancelNewGameButton"),"Confirmation initially focuses Cancel");
    Check(!Named("StartButton").GetComponent<UnityEngine.UI.Button>().IsInteractable()&&!newButton.GetComponent<UnityEngine.UI.Button>().IsInteractable(),"Keyboard navigation cannot enter underlying actions");
    Check(Fits(Named("NewGameCard").GetComponent<RectTransform>(),Named("Canvas").GetComponent<RectTransform>()),"Confirmation stays on screen");
    TextFits(Named("NewGameDialog")); // Raycasts are checked after a rendered frame in NewGameDialogSmoke.
    controller.StartGame();controller.QuitGame();
    Check(!DG.Tweening.DOTween.IsTweening(controller),"Underlying actions cannot bypass the modal");
    Named("CancelNewGameButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
    Check(!Named("NewGameDialog").activeSelf&&UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==newButton,"Cancel restores focus");
    Check(Named("StartButton").GetComponent<UnityEngine.UI.Button>().IsInteractable()&&newButton.GetComponent<UnityEngine.UI.Button>().IsInteractable(),"Cancel restores underlying actions");
    Check(PlayerPrefs.GetString(GameProgressStore.PreferenceKey)==before&&PlayerPrefs.GetString(GameProgressStore.BackupKey)==backup,"Opening and cancelling preserves both saved copies");
}
return new{passed=checks,hasProgress=has,readState=state.ToString(),width=Screen.width,height=Screen.height};
