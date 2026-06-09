using UnityEngine;
using UnityEngine.InputSystem;

public class BoxClick : MonoBehaviour
{
    private Animator animator;

    // 本一覧UI
    public GameObject bookListPanel;

    bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 念のためゲーム開始時は非表示
        if (bookListPanel != null)
        {
            bookListPanel.SetActive(false);
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
                    isOpen = !isOpen;
                    animator.SetBool("IsOpen", isOpen);

                    // 閉じるときはすぐUIを消す
                    if (!isOpen)
                    {
                        bookListPanel.SetActive(false);
                    }
                }
            }
        }
    }

    // Openアニメーションの最後にAnimation Eventから呼ぶ
    public void ShowBookList()
    {
        bookListPanel.SetActive(true);
    }
}