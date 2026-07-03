using UnityEngine;

public sealed class HandBookData
{
    public Sprite Sprite { get; }
    public int CorrectSlotIndex { get; }

    public HandBookData(Sprite sprite, int correctSlotIndex)
    {
        Sprite = sprite;
        CorrectSlotIndex = correctSlotIndex;
    }
}
