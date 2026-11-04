public static class BookColorUtility
{
    private static readonly string[] BookPrefixes = { "Green", "Blue", "Beige", "Red", "Purple", "Brown", "White", "Black" };
    private static readonly int[] ShelfOffsets = { 0, 18, 35, 53, 70, 88, 105, 123, 140 };
    public const int TotalBooks = 140;
    public const int ShelfCount = 8;

    // A stable identity shared by selection, placement and saved progress.
    public static bool TryGetBookIndex(string name, out int index)
    {
        index = -1;
        if (string.IsNullOrEmpty(name)) return false;
        for (int color = 0; color < ShelfCount; color++)
        {
            string prefix = BookPrefixes[color];
            if (name.Length != prefix.Length + 2 || !name.StartsWith(prefix, System.StringComparison.Ordinal)) continue;
            string suffix = name.Substring(prefix.Length);
            if (int.TryParse(suffix, out int number) && number > 0 &&
                number <= ShelfOffsets[color + 1] - ShelfOffsets[color] && suffix == number.ToString("D2"))
            {
                index = ShelfOffsets[color] + number - 1;
                return true;
            }
        }
        return false;
    }

    public static int GetColorIndexFromSlotIndex(int index)
    {
        for (int color = 0; color < ShelfCount; color++)
            if (index >= ShelfOffsets[color] && index < ShelfOffsets[color + 1]) return color;
        return -1;
    }

    public static int GetShelfSlotCount(int color) => color >= 0 && color < ShelfCount
        ? ShelfOffsets[color + 1] - ShelfOffsets[color] : 0;

    public static int GetColorIndexFromSpriteName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return -1;
        }

        if (spriteName.Contains("Green")) return 0;
        if (spriteName.Contains("Blue")) return 1;
        if (spriteName.Contains("Beige")) return 2;
        if (spriteName.Contains("Red")) return 3;
        if (spriteName.Contains("Purple")) return 4;
        if (spriteName.Contains("Brown")) return 5;
        if (spriteName.Contains("White")) return 6;
        if (spriteName.Contains("Black")) return 7;

        return -1;
    }

    public static int GetShelfColorIndexFromObjectName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return -1;
        }

        const string shelfNamePrefix = "BookShelf";

        if (!objectName.StartsWith(shelfNamePrefix))
        {
            return -1;
        }

        int startIndex = shelfNamePrefix.Length;
        int endIndex = startIndex;

        while (endIndex < objectName.Length &&
               char.IsDigit(objectName[endIndex]))
        {
            endIndex++;
        }

        if (endIndex == startIndex)
        {
            return -1;
        }

        string numberPart = objectName.Substring(
            startIndex,
            endIndex - startIndex
        );

        if (int.TryParse(numberPart, out int shelfNumber))
        {
            return shelfNumber - 1;
        }

        return -1;
    }
}
