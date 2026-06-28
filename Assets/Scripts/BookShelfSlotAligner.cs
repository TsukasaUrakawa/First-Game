using UnityEngine;
using UnityEngine.Splines;

public class BookShelfSlotAligner : MonoBehaviour
{
    [SerializeField] private SplineContainer _spline;

    [ContextMenu("Align Child Slots")]
    private void AlignChildSlots()
    {
        if (_spline == null)
        {
            Debug.LogWarning("Spline が設定されていません");
            return;
        }

        BookSlot[] slots = GetComponentsInChildren<BookSlot>();

        if (slots.Length == 0)
        {
            Debug.LogWarning("子オブジェクトに BookSlot がありません");
            return;
        }

        System.Array.Sort(slots, (a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex())
        );

        for (int i = 0; i < slots.Length; i++)
        {
            float t = slots.Length == 1 ? 0f : (float)i / (slots.Length - 1);
            Vector3 position = _spline.EvaluatePosition(t);

            slots[i].transform.position = position;
        }

        Debug.Log($"{slots.Length} 個のSlotをSpline上に配置しました");
    }
}