using UnityEngine;

public class BoxOpenAndClose : MonoBehaviour
{
    [SerializeField] GameObject _bookListPanel;
    private Animator _animator;

    private void OnMouseDown()
    {
        if (_animator.GetBool("IsOpen"))
        {
            _animator.SetBool("IsOpen", false);
        }
        else
        {
            _animator.SetBool("IsOpen", true);
        }



    }
    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void ShowBookList()
    {
        _bookListPanel.SetActive(true);
    }
    
}
