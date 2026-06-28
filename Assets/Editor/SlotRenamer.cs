using UnityEditor;
using UnityEngine;

public class SlotRenamer
{
    [MenuItem("Tools/Rename All BookSlots")]
    private static void RenameAllBookSlots()
    {
        BookSlot[] slots = Object.FindObjectsByType<BookSlot>(FindObjectsSortMode.None);

        if (slots.Length == 0)
        {
            Debug.LogWarning("BookSlot が見つかりません");
            return;
        }

        System.Array.Sort(slots, (a, b) =>
        {
            int parentCompare = string.Compare(
                a.transform.parent != null ? a.transform.parent.name : "",
                b.transform.parent != null ? b.transform.parent.name : ""
            );

            if (parentCompare != 0)
            {
                return parentCompare;
            }

            return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
        });

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.name = $"Slot{(i + 1):00}";
        }

        Debug.Log($"{slots.Length} 個の BookSlot をリネームしました");
    }
}