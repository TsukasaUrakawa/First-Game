using UnityEngine;

public class BoxOpenAndClose : MonoBehaviour
{
    [SerializeField] GameObject _bookListPanel;
    [SerializeField] AudioClip _openSE;
    [SerializeField] AudioClip _closeSE;
    private Animator _animator;
    private AudioSource _audioSource1;

    //Animationの遷移
    private void OnMouseDown()
    {
        _animator.SetBool("IsOpen", true);
    }
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource1 = GetComponent<AudioSource>();
    }

    private void ShowBookList()
    {
        _bookListPanel.SetActive(true);
    }

    public void CloseBookList()
    {
        _bookListPanel.SetActive(false);
        _animator.SetBool("IsOpen", false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && _bookListPanel.activeSelf)
        {
            CloseBookList();
        }
    }

    private void PlayOpenSE()
    {
        _audioSource1.PlayOneShot(_openSE);
    }

    private void PlayCloseSE()
    {
        _audioSource1.PlayOneShot(_closeSE);
    }
}
