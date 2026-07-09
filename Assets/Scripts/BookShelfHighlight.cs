using UnityEngine;

public class BookShelfHighlight : MonoBehaviour
{
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private SpriteRenderer _highlightRenderer;

    private void Awake()
    {
        HideHighlight();
    }

    private void OnEnable()
    {
        HideHighlight();
    }

    private void HideHighlight()
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(false);
        }

        if (_highlightRenderer != null)
        {
            _highlightRenderer.enabled = false;
        }
    }
}
