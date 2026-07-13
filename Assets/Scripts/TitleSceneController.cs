using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "ShelfCare";

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _startButtonSound;
    [SerializeField] private AudioClip _quitButtonSound;
    [SerializeField, Range(0f, 1f)] private float _buttonSoundVolume = 1f;
    [SerializeField, Min(0f)] private float _sceneChangeDelay = 0.15f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField, Min(0f)] private float _fadeDuration = 0.6f;
    [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

    private bool _isProcessing;

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        if (_fadeCanvasGroup != null)
        {
            _fadeCanvasGroup.alpha = 0f;
            _fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }

    public void StartGame()
    {
        if (_isProcessing)
        {
            return;
        }

        _isProcessing = true;
        PlaySound(_startButtonSound);

        DOTween.Sequence()
            .SetId(this)
            .AppendInterval(_sceneChangeDelay)
            .Append(FadeOut())
            .OnComplete(() =>
            {
                SceneManager.LoadScene(_gameSceneName);
            });
    }

    public void QuitGame()
    {
        if (_isProcessing)
        {
            return;
        }

        _isProcessing = true;
        PlaySound(_quitButtonSound);

        DOTween.Sequence()
            .SetId(this)
            .AppendInterval(_sceneChangeDelay)
            .Append(FadeOut())
            .OnComplete(() =>
            {
#if UNITY_EDITOR
                Debug.Log("ゲーム終了");
                _isProcessing = false;
#else
                Application.Quit();
#endif
            });
    }

    private Tween FadeOut()
    {
        if (_fadeCanvasGroup == null || _fadeDuration <= 0f)
        {
            return DOVirtual.DelayedCall(0f, () => { });
        }

        _fadeCanvasGroup.blocksRaycasts = true;

        return _fadeCanvasGroup
            .DOFade(1f, _fadeDuration)
            .SetEase(_fadeEase);
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip, _buttonSoundVolume);
        }
    }
}