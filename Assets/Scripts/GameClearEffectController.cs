using System.Collections;
using UnityEngine;

public class GameClearEffectController : MonoBehaviour
{
    [Header("Clear Panel")]
    [SerializeField] private GameObject _clearPanel;
    [SerializeField] private CanvasGroup _clearCanvasGroup;
    [SerializeField, Min(0f)] private float _panelDelay = 0.5f;
    [SerializeField, Min(0.01f)] private float _fadeDuration = 1f;

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _clearSound;
    [SerializeField, Range(0f, 1f)] private float _clearSoundVolume = 1f;

    [Header("Particle / Effect")]
    [SerializeField] private GameObject _clearEffectPrefab;
    [SerializeField] private Transform _effectPoint;
    [SerializeField, Min(0.1f)] private float _effectLifetime = 5f;

    [Header("UI Control")]
    [SerializeField] private GameObject[] _objectsToHideOnClear;
    [SerializeField] private GameObject[] _objectsToShowAfterFade;

    [Header("Time")]
    [SerializeField] private bool _useUnscaledTime = true;

    private Coroutine _clearRoutine;
    private bool _hasPlayed;

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null && _clearSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        SetupCanvasGroup();
        HideClearPanel();
        SetObjectsActive(_objectsToShowAfterFade, false);
    }

    public void PlayClearEffect()
    {
        if (_hasPlayed)
        {
            return;
        }

        _hasPlayed = true;
        HideClearPanel();

        if (_clearRoutine != null)
        {
            StopCoroutine(_clearRoutine);
        }

        _clearRoutine = StartCoroutine(PlayClearEffectRoutine());
    }

    private IEnumerator PlayClearEffectRoutine()
    {
        SetObjectsActive(_objectsToHideOnClear, false);
        SetObjectsActive(_objectsToShowAfterFade, false);

        PlayClearSound();
        SpawnClearEffect();

        yield return WaitForSeconds(_panelDelay);

        if (_clearPanel != null)
        {
            _clearPanel.SetActive(true);
        }

        if (_clearCanvasGroup != null)
        {
            _clearCanvasGroup.alpha = 0f;
            _clearCanvasGroup.interactable = false;
            _clearCanvasGroup.blocksRaycasts = false;

            yield return FadeCanvasGroup(0f, 1f, _fadeDuration);

            _clearCanvasGroup.interactable = true;
            _clearCanvasGroup.blocksRaycasts = true;
        }

        SetObjectsActive(_objectsToShowAfterFade, true);
        _clearRoutine = null;
    }

    private void PlayClearSound()
    {
        if (_audioSource == null || _clearSound == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_clearSound, _clearSoundVolume);
    }

    private void SpawnClearEffect()
    {
        if (_clearEffectPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = _effectPoint != null
            ? _effectPoint.position
            : transform.position;

        GameObject effect = Instantiate(
            _clearEffectPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Destroy(effect, _effectLifetime);
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / duration);
            _clearCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        _clearCanvasGroup.alpha = to;
    }

    private IEnumerator WaitForSeconds(float duration)
    {
        if (duration <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            yield return null;
        }
    }

    private float GetDeltaTime()
    {
        return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void SetupCanvasGroup()
    {
        if (_clearPanel == null)
        {
            return;
        }

        if (_clearCanvasGroup == null)
        {
            _clearCanvasGroup = _clearPanel.GetComponent<CanvasGroup>();
        }

        if (_clearCanvasGroup == null)
        {
            _clearCanvasGroup = _clearPanel.AddComponent<CanvasGroup>();
        }
    }

    private void HideClearPanel()
    {
        if (_clearCanvasGroup != null)
        {
            _clearCanvasGroup.alpha = 0f;
            _clearCanvasGroup.interactable = false;
            _clearCanvasGroup.blocksRaycasts = false;
        }

        if (_clearPanel != null && _clearPanel != gameObject)
        {
            _clearPanel.SetActive(false);
        }
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
        {
            return;
        }

        foreach (GameObject targetObject in objects)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(active);
            }
        }
    }
}
