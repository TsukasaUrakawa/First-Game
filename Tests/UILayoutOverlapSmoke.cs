// Let the current view finish rendering first. Audit all visible UI, not just button centers.
// Intentional modal backdrops/covered views are excluded; masks clip scroll content.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play.");
Canvas.ForceUpdateCanvases();
GameObject Named(string n) => UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).SingleOrDefault(t => t.name == n)?.gameObject;
bool Active(string n) => Named(n) != null && Named(n).activeInHierarchy;
var ui = GameplayUIController.Instance;
string view = "room"; Transform scope = null;
if (Active("PausePanel")) { view = "pause"; scope = Named("PausePanel").transform; }
else if (Active("SettingsPanel")) { view = "settings"; scope = Named("SettingsPanel").transform; }
else if (Active("NewGameDialog")) { view = "new-game"; scope = Named("NewGameDialog").transform; }
else if (Active("ClearPanel")) { view = "clear"; scope = Named("ClearPanel").transform; }
else if (ui != null && UnityEngine.Object.FindFirstObjectByType<BookPopUpController>().IsOpen) { view = "confirmation"; scope = Named("BookPopUp").transform; }
else if (ui != null && ShelfAnswerPanelController.Instance.IsOpen) { view = "answer"; scope = Named("ShelAnswerPanel").transform; }
else if (ui != null && ui.IsBookListOpen) { view = "list"; scope = Named("BookListPanel").transform; }
else if (ui != null && UnityEngine.Object.FindFirstObjectByType<CameraController>().IsZoomed) view = "shelf";
else if (ui == null) view = "title";
bool InScope(Transform t) => scope == null || t.IsChildOf(scope) ||
    ((view == "list" || view == "confirmation" || view == "answer") && t.IsChildOf(Named("OpenPauseButton").transform)) ||
    ((view == "list" || view == "confirmation") && Active("TutorialPanel") && t.IsChildOf(Named("TutorialPanel").transform));
bool Visible(Transform t) => t.gameObject.activeInHierarchy && InScope(t) && t.GetComponentsInParent<CanvasGroup>().All(g => g.alpha > .01f);
Rect Bounds(RectTransform r) { var p = new Vector3[4]; r.GetWorldCorners(p); var q = p.Select(v => RectTransformUtility.WorldToScreenPoint(null, v)).ToArray(); return Rect.MinMaxRect(q.Min(v => v.x), q.Min(v => v.y), q.Max(v => v.x), q.Max(v => v.y)); }
Rect Intersect(Rect a, Rect b) => Rect.MinMaxRect(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.xMax, b.xMax), Mathf.Min(a.yMax, b.yMax));
Rect Clip(Rect r, Transform t)
{
    foreach (var mask in t.GetComponentsInParent<UnityEngine.UI.RectMask2D>().Where(m => m.enabled)) r = Intersect(r, Bounds(mask.GetComponent<RectTransform>()));
    foreach (var mask in t.GetComponentsInParent<UnityEngine.UI.Mask>().Where(m => m.enabled)) r = Intersect(r, Bounds(mask.GetComponent<RectTransform>()));
    return r;
}
bool HasArea(Rect r) => r.width > .5f && r.height > .5f;
int checks = 0;
var issues = new System.Collections.Generic.List<string>();
void Check(bool ok, string label) { checks++; if (!ok) issues.Add(label); }
var controls = new System.Collections.Generic.Dictionary<Transform, Rect>();
foreach (var control in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Selectable>(UnityEngine.FindObjectsSortMode.None).Where(c => Visible(c.transform)))
{ var r = Clip(Bounds(control.GetComponent<RectTransform>()), control.transform); if (HasArea(r)) controls[control.transform] = r; }
foreach (var slot in UnityEngine.Object.FindObjectsByType<HandItemSlotUI>(UnityEngine.FindObjectsSortMode.None).Where(c => Visible(c.transform)))
{
    var image = (UnityEngine.UI.Image)new UnityEditor.SerializedObject(slot).FindProperty("_bookImage").objectReferenceValue;
    if (image == null || image.sprite == null || !image.gameObject.activeInHierarchy) continue;
    var r = Clip(Bounds(slot.GetComponent<RectTransform>()), slot.transform); if (HasArea(r)) controls[slot.transform] = r;
}
var visual = new System.Collections.Generic.Dictionary<Transform, Rect>(controls);
foreach (var label in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(UnityEngine.FindObjectsSortMode.None).Where(t => Visible(t.transform) && !string.IsNullOrWhiteSpace(t.text)))
{
    label.ForceMeshUpdate(); Check(!label.isTextOverflowing, "Text overflow: " + label.name);
    // Use glyph metrics rather than SDF quad padding, which includes transparent space.
    Rect textRect = default; bool hasGlyph = false;
    foreach (var character in label.textInfo.characterInfo.Take(label.textInfo.characterCount).Where(c => c.isVisible && c.textElement != null))
    {
        var m = character.textElement.glyph.metrics; float scale = character.scale;
        float left = character.origin + m.horizontalBearingX * scale, top = character.baseLine + m.horizontalBearingY * scale;
        var min = RectTransformUtility.WorldToScreenPoint(null, label.transform.TransformPoint(new Vector3(left, top-m.height*scale)));
        var max = RectTransformUtility.WorldToScreenPoint(null, label.transform.TransformPoint(new Vector3(left+m.width*scale, top)));
        var glyph = Rect.MinMaxRect(Mathf.Min(min.x,max.x),Mathf.Min(min.y,max.y),Mathf.Max(min.x,max.x),Mathf.Max(min.y,max.y));
        textRect = hasGlyph ? Rect.MinMaxRect(Mathf.Min(textRect.xMin,glyph.xMin),Mathf.Min(textRect.yMin,glyph.yMin),Mathf.Max(textRect.xMax,glyph.xMax),Mathf.Max(textRect.yMax,glyph.yMax)) : glyph;
        hasGlyph = true;
    }
    var r = Clip(textRect, label.transform); if (hasGlyph && HasArea(r)) visual[label.transform] = r;
}
foreach (string n in new[] { "ShelfButtonPanel", "HandItemPanel", "ShelfActionPanel", "TutorialPanel", "BookListFooter", "BookConfirmationCard", "ShelfAnswerCard", "SettingsCard", "PauseCard", "ClearCard" })
{ var g = Named(n); if (g != null && Visible(g.transform)) visual[g.transform] = Bounds(g.GetComponent<RectTransform>()); }
if (view == "room")
{
    var box = UnityEngine.Object.FindFirstObjectByType<BoxOpenAndClose>();
    if (box != null)
    {
        var renderers = box.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
        if (renderers.Length > 0)
        {
            var b = renderers[0].bounds; foreach (var renderer in renderers.Skip(1)) b.Encapsulate(renderer.bounds);
            var points = new System.Collections.Generic.List<Vector3>();
            for (int x=0;x<2;x++) for (int y=0;y<2;y++) for (int z=0;z<2;z++)
                points.Add(Camera.main.WorldToScreenPoint(new Vector3(x==0?b.min.x:b.max.x,y==0?b.min.y:b.max.y,z==0?b.min.z:b.max.z)));
            var projected = Rect.MinMaxRect(points.Min(p=>p.x),points.Min(p=>p.y),points.Max(p=>p.x),points.Max(p=>p.y));
            var visible = Intersect(projected,new Rect(0,0,Screen.width,Screen.height));
            if (HasArea(visible)) visual[box.transform] = visible;
        }
    }
}
foreach (var item in visual)
    Check(item.Value.xMin >= -1 && item.Value.yMin >= -1 && item.Value.xMax <= Screen.width+1 && item.Value.yMax <= Screen.height+1, "Off screen: " + item.Key.name);
var entries = visual.ToArray();
for (int i = 0; i < entries.Length; i++) for (int j = i+1; j < entries.Length; j++)
{
    var a = entries[i]; var b = entries[j]; if (a.Key.IsChildOf(b.Key) || b.Key.IsChildOf(a.Key)) continue;
    var intersection = Intersect(a.Value, b.Value);
    Check(!HasArea(intersection), "Overlap: " + a.Key.name + " / " + b.Key.name);
}
foreach (var item in controls)
{
    var r = item.Value; float x = Mathf.Min(4, r.width*.2f), y = Mathf.Min(4, r.height*.2f);
    foreach (var point in new[] { r.center, new Vector2(r.xMin+x,r.yMin+y), new Vector2(r.xMin+x,r.yMax-y), new Vector2(r.xMax-x,r.yMin+y), new Vector2(r.xMax-x,r.yMax-y) })
    {
        var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = point }, hits);
        Check(hits.Count > 0 && (hits[0].gameObject.transform == item.Key || hits[0].gameObject.transform.IsChildOf(item.Key)), "Covered control: " + item.Key.name + " at " + point);
    }
}
return new { view, width = Screen.width, height = Screen.height, checks, controls = controls.Count, visuals = visual.Count, issueCount = issues.Count, issues = issues.Take(60).ToArray() };
