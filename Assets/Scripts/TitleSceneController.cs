using System.Collections;
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
    }

    public void StartGame()
    {
        if (_isProcessing)
        {
            return;
        }

        StartCoroutine(StartGameRoutine());
    }

    public void QuitGame()
    {
        if (_isProcessing)
        {
            return;
        }

        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        _isProcessing = true;

        PlaySound(_startButtonSound);

        yield return new WaitForSeconds(_sceneChangeDelay);

        SceneManager.LoadScene(_gameSceneName);
    }

    private IEnumerator QuitGameRoutine()
    {
        _isProcessing = true;

        PlaySound(_quitButtonSound);

        yield return new WaitForSeconds(_sceneChangeDelay);

#if UNITY_EDITOR
        Debug.Log("ゲーム終了");
#else
        Application.Quit();
#endif

        _isProcessing = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip, _buttonSoundVolume);
        }
    }
}