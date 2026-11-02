using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BookShelfSlotAligner : MonoBehaviour
{
    [SerializeField] private Vector3 _startLocalPosition = new Vector3(-5.6f, -0.24f, 0f);
    [SerializeField, Min(0.01f)] private float _slotSpacing = 0.68f;

    public int SlotCount => GetSlots().Count;

    private void Awake()
    {
        AlignChildSlots();
    }

    public void AlignChildSlots()
    {
        List<BookSlot> slots = GetSlots();
        slots.Sort((a, b) => a.SlotIndex.CompareTo(b.SlotIndex));

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].transform.localPosition =
                _startLocalPosition + Vector3.right * (_slotSpacing * i);
        }
    }

    private List<BookSlot> GetSlots()
    {
        var slots = new List<BookSlot>();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BookSlot slot))
            {
                slots.Add(slot);
            }
        }

        return slots;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 first = transform.TransformPoint(_startLocalPosition);
        Vector3 last = transform.TransformPoint(
            _startLocalPosition + Vector3.right * (_slotSpacing * Mathf.Max(0, SlotCount - 1)));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(first, last);
        Gizmos.DrawWireSphere(first, 0.04f);
        Gizmos.DrawWireSphere(last, 0.04f);
    }
}
