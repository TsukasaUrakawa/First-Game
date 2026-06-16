using UnityEngine;
using UnityEngine.InputSystem;

public class BoxClick : MonoBehaviour
{
    private Animator _animator;

    // 本一覧UI
    public GameObject _bookListPanel;

    bool _isOpen = false;

    void Start()
    {
        _animator = GetComponent<Animator>();

        // 念のためゲーム開始時は非表示
        if (_bookListPanel != null)
        {
            _bookListPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    _isOpen = !_isOpen;
                    _animator.SetBool("IsOpen", _isOpen);

                    // 閉じるときはすぐUIを消す
                    if (!_isOpen)
                    {
                        _bookListPanel.SetActive(false);
                    }
                }
            }
        }
    }

    // Openアニメーションの最後にAnimation Eventから呼ぶ
    public void ShowBookList()
    {
        _bookListPanel.SetActive(true);
    }
}