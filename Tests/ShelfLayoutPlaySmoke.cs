// Run in a fresh ShelfCare Play session, with the book list closed and empty hands.
if (!UnityEditor.EditorApplication.isPlaying || GameplayUIController.IsWorldInputBlocked)
    throw new System.Exception("Run in a fresh Play session in the room.");
int checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new System.Exception("FAIL: " + label);
    checks++;
}
var hand = UnityEngine.Object.FindFirstObjectByType<HandItemController>(UnityEngine.FindObjectsInactive.Include);
var placement = UnityEngine.Object.FindFirstObjectByType<HandBookPlacementController>(UnityEngine.FindObjectsInactive.Include);
var shelves = UnityEngine.Object.FindObjectsByType<BookShelfJudge>(UnityEngine.FindObjectsSortMode.None).OrderBy(s => s.name).ToArray();
var sprites = UnityEngine.Object.FindFirstObjectByType<BookManager>()._allBookSprites;
Check(hand.BookCount == 0 && shelves.Length == 8, "Fresh room has eight shelves and an empty hand");
// Keep the bulk placement/judgement test silent. Stop Play mode to restore audio.
foreach (var audio in UnityEngine.Object.FindObjectsByType<UnityEngine.AudioSource>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) audio.volume = 0;
var sound = new UnityEditor.SerializedObject(placement);
foreach (var property in new[] { "_placeSoundVolume", "_returnSoundVolume", "_wrongShelfSoundVolume" }) sound.FindProperty(property).floatValue = 0;
sound.ApplyModifiedPropertiesWithoutUndo();
int placed = 0;
for (int color = 0; color < shelves.Length; color++)
{
    var row = shelves[color].GetComponentsInChildren<BookSlot>(true).OrderBy(s => s.SlotIndex).ToArray();
    var books = sprites.Where(s => BookColorUtility.GetColorIndexFromSpriteName(s.name) == color).OrderBy(s => int.Parse(s.name.Substring(s.name.Length - 2))).ToArray();
    Check(row.Length == books.Length && row.All(s => s.ShelfColorIndex == color), shelves[color].name + " keeps its color and book catalogue");
    for (int i = 0; i < row.Length; i++)
    {
        Check(hand.TryAddBook(books[i], row[i].SlotIndex), "Add " + books[i].name);
        hand.SelectBook(0);
        if (color == 0 && i == 0)
        {
            var wrongColor = shelves[1].GetComponentsInChildren<BookSlot>()[0];
            Check(!placement.TryPlaceBook(wrongColor) && hand.BookCount == 1 && !wrongColor.IsFilled, "A book still cannot enter a different color shelf");
            Check(placement.TryPlaceBook(row[1]) && !row[1].IsCorrect() && !shelves[0].IsShelfCorrect(), "Wrong order is still judged incorrect");
            Check(placement.TryReturnBookToHand(row[1]) && !row[1].IsFilled, "A wrongly placed book can be returned");
            hand.SelectBook(0);
        }
        Check(placement.CanPlaceSelectedBookOnSlot(row[i], out _), "Placement preview available for " + books[i].name);
        Check(placement.TryPlaceBook(row[i]) && row[i].IsCorrect() && hand.BookCount == 0, "Place correct book in " + row[i].name);
        Check(UnityEngine.Vector3.Distance(row[i].PlacedBook.transform.position, row[i].transform.position) < .00002f && row[i].PlacedBook.GetComponent<UnityEngine.SpriteRenderer>().sprite == books[i], "Book image and click position follow " + row[i].name);
        Check(!row[i].GetComponent<UnityEngine.Collider2D>().enabled && row[i].PlacedBook.GetComponent<UnityEngine.Collider2D>().enabled, "Filled slot transfers click handling to the book");
        placed++;
    }
    foreach (var slot in new[] { row.First(), row.Last() })
    {
        Check(placement.TryReturnBookToHand(slot) && !slot.IsFilled && slot.GetComponent<UnityEngine.Collider2D>().enabled, "Return endpoint book " + slot.name);
        hand.SelectBook(0);
        Check(placement.TryPlaceBook(slot) && slot.IsCorrect(), "Restore endpoint book " + slot.name);
    }
    Check(shelves[color].IsShelfCorrect(), shelves[color].name + " judges the complete order correctly");
    Check(shelves[color].CheckShelf() && shelves[color].IsCompleted, shelves[color].name + " completes successfully");
    Check(row.All(s => s.IsLocked && !s.PlacedBook.GetComponent<UnityEngine.Collider2D>().enabled), shelves[color].name + " locks the completed books");
}
Check(placed == 140 && GameplayUIController.Instance.IsGameCleared && GameplayUIController.IsWorldInputBlocked, "All 140 books still lead to game clear");
return new { passed = checks, placed, completedShelves = shelves.Count(s => s.IsCompleted) };
