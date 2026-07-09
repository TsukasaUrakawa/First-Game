using UnityEngine;

public class BookShelfJudge : MonoBehaviour
{
    [Header("Effect")]
    [SerializeField] private GameObject _correctEffectPrefab;
    [SerializeField] private GameObject _wrongEffectPrefab;
    [SerializeField] private Transform _effectPoint;

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _correctSound;
    [SerializeField] private AudioClip _wrongSound;
    [SerializeField, Range(0f, 1f)] private float _correctSoundVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _wrongSoundVolume = 1f;

    [Header("Lock")]
    [SerializeField] private bool _lockBooksWhenCompleted = true;

    [SerializeField] private BookJudgeManager _judgeManager;

    public bool IsCompleted { get; private set; }

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null && (_correctSound != null || _wrongSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// この棚を判定して、正解/不正解のエフェクトを出します。
    /// ShelfButton の Judge から呼ぶ想定です。
    /// </summary>
    public bool CheckShelf()
    {
        bool isCorrect = IsShelfCorrect();

        if (isCorrect)
        {
            if (!IsCompleted)
            {
                CompleteShelf();
            }
        }
        else
        {
            IsCompleted = false;
            SetShelfLocked(false);
            SpawnJudgeEffect(false);
            PlayJudgeSound(false);
        }

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
        SetShelfLocked(false);
    }

    private void CompleteShelf()
    {
        IsCompleted = true;

        SpawnJudgeEffect(true);
        PlayJudgeSound(true);

        if (_lockBooksWhenCompleted)
        {
            SetShelfLocked(true);
        }
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

    private void PlayJudgeSound(bool isCorrect)
    {
        if (_audioSource == null)
        {
            return;
        }

        AudioClip sound = isCorrect ? _correctSound : _wrongSound;
        float volume = isCorrect ? _correctSoundVolume : _wrongSoundVolume;

        if (sound != null)
        {
            _audioSource.PlayOneShot(sound, volume);
        }
    }

    private void SetShelfLocked(bool locked)
    {
        BookSlot[] slots = GetComponentsInChildren<BookSlot>();

        foreach (BookSlot slot in slots)
        {
            if (slot != null)
            {
                slot.SetLocked(locked);
            }
        }
    }
}
