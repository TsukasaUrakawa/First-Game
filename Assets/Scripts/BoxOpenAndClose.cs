using UnityEngine;

public class BoxOpenAndClose : MonoBehaviour
{
    [SerializeField] private GameObject _bookListPanel;
    [SerializeField] private CanvasGroup _handItemCanvasGroup;

    [SerializeField] private AudioClip _openSE;
    [SerializeField] private AudioClip _closeSE;

    private Animator _animator;
    private AudioSource _audioSource;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        // 本一覧が閉じているなら手持ち欄を表示する
        SetHandItemPanelVisible(!_bookListPanel.activeSelf);
    }

    private void OnMouseDown()
    {
        _animator.SetBool("IsOpen", true);
    }

    // 箱を開くAnimation Eventから呼ばれる
    private void ShowBookList()
    {
        _bookListPanel.SetActive(true);
        SetHandItemPanelVisible(false);
    }

    public void CloseBookList()
    {
        _bookListPanel.SetActive(false);
        SetHandItemPanelVisible(true);

        _animator.SetBool("IsOpen", false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            _bookListPanel.activeSelf)
        {
            CloseBookList();
        }
    }

    private void SetHandItemPanelVisible(bool visible)
    {
        if (_handItemCanvasGroup == null)
        {
            return;
        }

        _handItemCanvasGroup.alpha = visible ? 1f : 0f;
        _handItemCanvasGroup.interactable = visible;
        _handItemCanvasGroup.blocksRaycasts = visible;
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