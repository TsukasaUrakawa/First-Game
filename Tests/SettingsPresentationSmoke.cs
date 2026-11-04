// Open settings, let a frame render, then check at both desktop resolutions.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Play.");
GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
int checks=0;void Check(bool ok,string label){if(!ok)throw new System.Exception("FAIL: "+label);checks++;}
var controller=UnityEngine.Object.FindFirstObjectByType<SettingsPanelController>();Check(controller.IsOpen,"Settings visible");var panel=Named("SettingsPanel").transform;var card=Named("SettingsCard").GetComponent<RectTransform>();var canvas=Named("Canvas").GetComponent<RectTransform>();var corners=new Vector3[4];card.GetWorldCorners(corners);Check(corners.All(p=>canvas.rect.Contains(canvas.InverseTransformPoint(p))),"Settings card fits screen");
foreach(var label in card.GetComponentsInChildren<TMPro.TMP_Text>()){label.ForceMeshUpdate();Check(!label.isTextOverflowing,label.name+" fits");Check(label.text.Where(c=>!char.IsControl(c)).All(c=>label.font.HasCharacter(c)),label.name+" glyphs");}
foreach(string name in new[]{"MusicVolumeSlider","EffectsVolumeSlider","MuteAudioToggle","ReducedMotionToggle","PreviewEffectsButton","CloseSettingsButton"}){var control=Named(name);var point=RectTransformUtility.WorldToScreenPoint(null,control.GetComponent<RectTransform>().TransformPoint(control.GetComponent<RectTransform>().rect.center));var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point},hits);Check(hits.Count>0&&hits[0].gameObject.transform.IsChildOf(control.transform),name+" receives pointer input");}
Rect Bounds(RectTransform rect){var points=new Vector3[4];rect.GetWorldCorners(points);var local=points.Select(p=>card.InverseTransformPoint(p)).ToArray();return Rect.MinMaxRect(local.Min(p=>p.x),local.Min(p=>p.y),local.Max(p=>p.x),local.Max(p=>p.y));}
var preview=Bounds(Named("PreviewEffectsButton").GetComponent<RectTransform>());
foreach(string name in new[]{"EffectsVolumeSlider","MusicVolumeSlider"})
{
    var slider=Named(name).GetComponent<UnityEngine.UI.Slider>();float original=slider.value;
    try
    {
        foreach(float value in new[]{0f,.25f,.5f,.75f,1f})
        {
            slider.SetValueWithoutNotify(value);Canvas.ForceUpdateCanvases();
            var thumb=Bounds(slider.handleRect);var row=Bounds(slider.GetComponent<RectTransform>());
            Check(thumb.yMin>=row.yMin-.1f&&thumb.yMax<=row.yMax+.1f,name+" thumb stays inside its hit area at "+value);
            Check(!thumb.Overlaps(preview),name+" thumb does not overlap Preview at "+value);
            var point=RectTransformUtility.WorldToScreenPoint(null,slider.handleRect.TransformPoint(new Vector3(slider.handleRect.rect.center.x,slider.handleRect.rect.yMin+1)));
            var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point},hits);
            Check(hits.Count>0&&hits[0].gameObject.transform.IsChildOf(slider.transform),name+" thumb lower edge receives slider input at "+value);
        }
    }
    finally{slider.SetValueWithoutNotify(original);}
}
var effects=Bounds(Named("EffectsVolumeSlider").GetComponent<RectTransform>());var mute=Bounds(Named("MuteAudioToggle").GetComponent<RectTransform>());
Check(effects.yMin-preview.yMax>=24,"Preview has space below the effects row");
Check(preview.yMin-mute.yMax>=24,"Preview has space above the mute row");
var backdropHits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=new Vector2(12,Screen.height-12)},backdropHits);Check(backdropHits.Count>0&&backdropHits[0].gameObject.transform.IsChildOf(panel),"Dimmer blocks input outside the card");
return new{passed=checks,width=Screen.width,height=Screen.height,scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name};
