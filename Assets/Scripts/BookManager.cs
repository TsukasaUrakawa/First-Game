using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全ての本画像
/// </summary>
public class BookManager : MonoBehaviour
{
    public Sprite[] _allBookSprites;

    private void Awake()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Books");
        List<Sprite> bookSprites = new List<Sprite>();

        foreach (Sprite sprite in loadedSprites)
        {
            if (sprite == null)
            {
                continue;
            }

            if (BookColorUtility.GetColorIndexFromSpriteName(sprite.name) >= 0)
            {
                bookSprites.Add(sprite);
            }
        }

        _allBookSprites = bookSprites.ToArray();
    }
}
