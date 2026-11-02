using UnityEngine;
using UnityEngine.EventSystems;

public class BoxOpenAndClose : MonoBehaviour
{
    [SerializeField] private GameplayUIController _gameplayUI;

    [SerializeField] private AudioClip _openSE;
    [SerializeField] private AudioClip _closeSE;

    private Animator _animator;
    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        if (GameplayUIController.IsWorldInputBlocked ||
            (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            return;
        }

        _animator.SetBool("IsOpen", true);
    }

    // 箱を開くAnimation Eventから呼ばれる
    private void ShowBookList()
    {
        _gameplayUI.OpenBookList();
    }

    public void CloseBookList()
    {
        _gameplayUI.CloseBookList();
    }

    public void CloseLid()
    {
        _animator.SetBool("IsOpen", false);
    }

    private void PlayOpenSE()
    {
        if (_audioSource != null && _openSE != null)
        {
            _audioSource.PlayOneShot(_openSE);
        }
    }

    private void PlayCloseSE()
    {
        if (_audioSource != null && _closeSE != null)
        {
            _audioSource.PlayOneShot(_closeSE);
        }
    }
}
