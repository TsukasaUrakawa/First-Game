using UnityEngine;

public class BoxOpenAndClose : MonoBehaviour
{
    [SerializeField] private GameObject _bookListPanel;
    [SerializeField] private CanvasGroup _handItemCanvasGroup;
    [SerializeField] private CanvasGroup _shelfButtonCanvasGroup;

    [SerializeField] private AudioClip _openSE;
    [SerializeField] private AudioClip _closeSE;

    private Animator _animator;
    private AudioSource _audioSource;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        // 本一覧が閉じているなら手持ち欄を表示する
        SetGameplayUIVisible(!_bookListPanel.activeSelf);
    }

    private void OnMouseDown()
    {
        _animator.SetBool("IsOpen", true);
    }

    // 箱を開くAnimation Eventから呼ばれる
    private void ShowBookList()
    {
        _bookListPanel.SetActive(true);
        SetGameplayUIVisible(false);
    }

    public void CloseBookList()
    {
        _bookListPanel.SetActive(false);
        SetGameplayUIVisible(true);

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

    private void SetGameplayUIVisible(bool visible)
    {
        SetCanvasGroupVisible(_handItemCanvasGroup, visible);
        SetCanvasGroupVisible(_shelfButtonCanvasGroup, visible);
    }

    private void SetCanvasGroupVisible(CanvasGroup canvasGroup, bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
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
