using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameProgressData
{
    public int version;
    public string[] hand;
    public SavedBookPlacement[] placed;
    public int[] completedShelves;
    public bool cleared;

    public bool HasProgress => hand.Length > 0 || placed.Length > 0;

    public static GameProgressData Empty() => new GameProgressData
    {
        version = 1, hand = Array.Empty<string>(), placed = Array.Empty<SavedBookPlacement>(),
        completedShelves = Array.Empty<int>()
    };

    // Validate the entire snapshot before changing the scene or its saved copy.
    public bool IsValid()
    {
        if (version != 1 || hand == null || placed == null || completedShelves == null ||
            hand.Length > HandItemController.MaxCapacity || placed.Length > BookColorUtility.TotalBooks ||
            completedShelves.Length > BookColorUtility.ShelfCount) return false;
        var books = new HashSet<string>(StringComparer.Ordinal);
        var positions = new Dictionary<int, int>();
        foreach (string name in hand)
            if (!BookColorUtility.TryGetBookIndex(name, out _) || !books.Add(name)) return false;
        foreach (var book in placed)
        {
            if (book == null || !BookColorUtility.TryGetBookIndex(book.book, out int correctIndex) ||
                !books.Add(book.book) || book.slot < 0 || book.slot >= BookColorUtility.TotalBooks ||
                positions.ContainsKey(book.slot) || BookColorUtility.GetColorIndexFromSlotIndex(book.slot) !=
                BookColorUtility.GetColorIndexFromSlotIndex(correctIndex)) return false;
            positions.Add(book.slot, correctIndex);
        }
        var completed = new HashSet<int>();
        foreach (int color in completedShelves)
        {
            if (color < 0 || color >= BookColorUtility.ShelfCount || !completed.Add(color)) return false;
            for (int slot = 0; slot < BookColorUtility.TotalBooks; slot++)
                if (BookColorUtility.GetColorIndexFromSlotIndex(slot) == color &&
                    (!positions.TryGetValue(slot, out int correct) || correct != slot)) return false;
        }
        if (cleared)
        {
            if (placed.Length != BookColorUtility.TotalBooks || hand.Length != 0) return false;
            foreach (var position in positions) if (position.Key != position.Value) return false;
        }
        return true;
    }
}

[Serializable]
public sealed class SavedBookPlacement
{
    public string book;
    public int slot;
}
