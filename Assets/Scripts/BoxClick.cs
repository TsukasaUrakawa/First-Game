using UnityEngine;

public class BoxClick : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        isOpen = !isOpen;
        animator.SetBool("IsOpen", isOpen);
    }
}