// ShelfCare Play, with at least one hand book and an empty place of that color.
if (!UnityEditor.EditorApplication.isPlaying || GameplayUIController.Instance.HasModal) throw new System.Exception("Run in the room/shelf with no modal.");
int checks = 0;
void Check(bool ok, string message) { if (!ok) throw new System.Exception("FAIL: " + message); checks++; }
var hand = UnityEngine.Object.FindFirstObjectByType<HandItemController>();
if (hand.BookCount == 0) throw new System.Exception("Take a book first.");
if (!hand.HasSelectedBook) hand.SelectBook(0);
hand.TryGetSelectedBook(out var selected);
int color = BookColorUtility.GetColorIndexFromSpriteName(selected.Sprite.name);
var slot = UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).First(s => s.ShelfColorIndex == color && !s.IsFilled && !s.IsLocked);
UnityEngine.Object.FindObjectsByType<BookShelfZoomButton>(UnityEngine.FindObjectsSortMode.None).Single(b => b.ShelfJudge == slot.ShelfJudge).ZoomShelf();
slot.SendMessage("OnMouseEnter");
var ghost = slot.GhostPreview;
Check(ghost != null && ghost.gameObject.activeSelf && ghost.Sprite == selected.Sprite, "Hover shows the chosen book as a 3D preview");
Check(ghost.GetComponentsInChildren<Collider>().Length == 0, "Preview has no input collider");
Check(ghost.GetComponentsInChildren<MeshRenderer>().All(r => r.enabled && r.sharedMaterials.All(m => m.renderQueue >= 3000 && m.GetFloat("_ZWrite") == 0)), "Preview materials are genuinely transparent");
Physics.SyncTransforms();
var ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(slot.transform.position));
Check(Physics.Raycast(ray, out var hit, 200) && hit.collider.GetComponentInParent<BookSlot>() == slot, "Pointer passes through the preview to the intended empty place");
slot.SendMessage("OnMouseExit"); Check(!ghost.gameObject.activeSelf, "Leaving hides the preview");
slot.SendMessage("OnMouseEnter"); Check(slot.GhostPreview == ghost && ghost.gameObject.activeSelf, "Next hover reuses its cached preview");
GameplayUIController.Instance.OpenBookList(); slot.SendMessage("OnMouseOver");
Check(!ghost.gameObject.activeSelf, "Opening a modal removes the world preview");
GameplayUIController.Instance.CloseBookList();
return new { passed = checks, book = selected.Sprite.name, slot = slot.SlotIndex };
