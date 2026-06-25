using UnityEngine;

public class BookShelfJudge : MonoBehaviour
{
    [SerializeField] private GameObject _correctEffectPrefab;
    [SerializeField] private GameObject _wrongEffectPrefab;
    [SerializeField] private Transform _effectPoint;

    public bool CheckShelf()
    {
        BookSlot[] slots = GetComponentsInChildren<BookSlot>();

        foreach (BookSlot slot in slots)
        {
            if (!slot.IsFilled || !slot.IsCorrect())
            {
                if (_wrongEffectPrefab != null)
                {
                    Instantiate(_wrongEffectPrefab, _effectPoint.position, Quaternion.identity);
                }

                return false;
            }
        }

        if (_correctEffectPrefab != null)
        {
            Instantiate(_correctEffectPrefab, _effectPoint.position, Quaternion.identity);
        }

        return true;
    }
}