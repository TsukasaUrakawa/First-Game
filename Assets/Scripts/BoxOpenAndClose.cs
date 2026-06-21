using UnityEngine;

public class BoxOpenAndClose : MonoBehaviour
{
    [SerializeField] GameObject _bookListPanel;
    private Animator _animator;

    //Animationの遷移
    private void OnMouseDown()
    {
        _animator.SetBool("IsOpen", true);
    }
    private void Start()
    {
        _animator = GetComponent<Animator>();
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
    
}
