using UnityEngine;

public class BookShelfJudge : MonoBehaviour
{
    [SerializeField] private GameObject _correctEffectPrefab;
    [SerializeField] private GameObject _wrongEffectPrefab;
    [SerializeField] private Transform _effectPoint;
    [SerializeField] private BookJudgeManager _judgeManager;

    public bool IsCompleted { get; private set; }

    /// <summary>
    /// この棚を判定して、正解/不正解のエフェクトを出します。
    /// ShelfButton の Judge から呼ぶ想定です。
    /// </summary>
    public bool CheckShelf()
    {
        bool isCorrect = IsShelfCorrect();
        IsCompleted = isCorrect;

        SpawnJudgeEffect(isCorrect);

        if (_judgeManager == null)
        {
            _judgeManager = FindFirstObjectByType<BookJudgeManager>();
        }

        if (_judgeManager != null)
        {
            _judgeManager.CheckGameClear();
        }

        return isCorrect;
    }

    /// <summary>
    /// エフェクトを出さずに、この棚が完成しているかだけ調べます。
    /// </summary>
    public bool IsShelfCorrect()
    {
        BookSlot[] slots = GetComponentsInChildren<BookSlot>();

        if (slots.Length == 0)
        {
            return false;
        }

        foreach (BookSlot slot in slots)
        {
            if (!slot.IsFilled || !slot.IsCorrect())
            {
                return false;
            }
        }

        return true;
    }

    public void ResetCompleted()
    {
        IsCompleted = false;
    }

    private void SpawnJudgeEffect(bool isCorrect)
    {
        GameObject effectPrefab = isCorrect ? _correctEffectPrefab : _wrongEffectPrefab;

        if (effectPrefab == null)
        {
            return;
        }

        Vector3 effectPosition = _effectPoint != null ? _effectPoint.position : transform.position;
        Instantiate(effectPrefab, effectPosition, Quaternion.identity);
    }
}
