// Play in ShelfCare after restoring .utmp/save-smoke-expected.json (see Tests/README.md).
if(!UnityEditor.EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="ShelfCare") throw new System.Exception("Run in ShelfCare Play mode.");
var expected=JsonUtility.FromJson<GameProgressData>(System.IO.File.ReadAllText(".utmp/save-smoke-expected.json"));
if(!expected.IsValid())throw new System.Exception("Expected a valid fixture.");
int checks=0;
void Check(bool ok,string label){if(!ok)throw new System.Exception("FAIL: "+label);checks++;}
var progress=UnityEngine.Object.FindFirstObjectByType<GameProgressController>(UnityEngine.FindObjectsInactive.Include);
var hand=UnityEngine.Object.FindFirstObjectByType<HandItemController>(UnityEngine.FindObjectsInactive.Include);
var slots=UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).OrderBy(s=>s.SlotIndex).ToArray();
Check(progress.IsReady,"Progress initialized");
Check(hand.Books.Select(b=>b.Sprite.name).SequenceEqual(expected.hand),"Exact hand order restored");
Check(!hand.HasSelectedBook,"Transient selection is cleared");
Check(slots.Length==140&&slots.Count(s=>s.IsFilled)==expected.placed.Length,"Exact number of placed books");
foreach(var book in expected.placed){var slot=slots[book.slot];BookColorUtility.TryGetBookIndex(book.book,out int correct);
    Check(slot.IsFilled&&slot.PlacedBook.Sprite.name==book.book&&slot.PlacedBook.CorrectSlotIndex==correct,"Restored "+book.book+" at "+book.slot);
    Check(Vector3.Distance(slot.PlacedBook.transform.position,slot.transform.position)<.00002f,"Restored book follows slot position");
}
foreach(var shelf in UnityEngine.Object.FindObjectsByType<BookShelfJudge>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None)){
    var row=shelf.GetComponentsInChildren<BookSlot>(true);int color=row[0].ShelfColorIndex;bool complete=expected.completedShelves.Contains(color);
    Check(shelf.IsCompleted==complete&&row.All(s=>s.IsLocked==complete),"Completion locks restored for "+shelf.name);
    foreach(var slot in row.Where(s=>s.IsFilled))Check(slot.PlacedBook.GetComponent<Collider>().enabled==!complete,"Placed-book input restored");
    var card=UnityEngine.Object.FindObjectsByType<BookShelfZoomButton>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(s=>s.ShelfJudge==shelf);
    Check(card.transform.Find("ShelfProgressLabel").GetComponent<TMPro.TMP_Text>().text==(complete?"完成":$"{row.Count(s=>s.IsFilled)} / {row.Length} 冊"),"Navigation progress refreshed");
}
var owned=new System.Collections.Generic.HashSet<string>(expected.hand.Concat(expected.placed.Select(p=>p.book)));
var available=UnityEngine.Object.FindObjectsByType<BookButton>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Select(b=>b.BookSprite.name).ToArray();
Check(available.Length+owned.Count==140&&available.Distinct().Count()==available.Length&&!available.Any(owned.Contains),"Every book exists exactly once across box, hand and shelves");
Check(GameplayUIController.Instance.IsGameCleared==expected.cleared,"Game-clear state restored");
if(!expected.cleared)Check(!GameplayUIController.Instance.HasModal&&!UnityEngine.Object.FindFirstObjectByType<CameraController>().IsZoomed,"Resume starts in the room without stale dialogs or zoom");
Check(GameProgressStore.Read(out var saved)==ProgressReadResult.Loaded&&JsonUtility.ToJson(saved)==JsonUtility.ToJson(expected),"Restore does not alter the snapshot");
return new{passed=checks,hand=hand.BookCount,placed=expected.placed.Length,completed=expected.completedShelves.Length,cleared=expected.cleared};
