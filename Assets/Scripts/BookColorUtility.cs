public static class BookColorUtility
{
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
