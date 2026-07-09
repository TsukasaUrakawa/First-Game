using UnityEngine;

public class BookShelfHighlight : MonoBehaviour
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private int _shelfColorIndex = -1;
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private SpriteRenderer _highlightRenderer;
    [SerializeField] private Color _highlightColor = new Color(1f, 0.9f, 0.25f, 0.35f);

    private void Awake()
    {
        if (_shelfColorIndex < 0)
        {
            _shelfColorIndex =
                BookColorUtility.GetShelfColorIndexFromObjectName(gameObject.name);
        }

        if (_highlightObject == null && _highlightRenderer != null)
        {
            _highlightObject = _highlightRenderer.gameObject;
        }

        if (_highlightRenderer != null)
        {
            _highlightRenderer.color = _highlightColor;
        }

        SetHighlightVisible(false);
    }

    private void OnEnable()
    {
        if (_handItemController == null)
        {
            _handItemController = FindFirstObjectByType<HandItemController>();
        }

        if (_handItemController != null)
        {
            _handItemController.SelectedBookChanged += OnSelectedBookChanged;
            RefreshHighlight(_handItemController.SelectedBook);
        }
    }

    private void OnDisable()
    {
        if (_handItemController != null)
        {
            _handItemController.SelectedBookChanged -= OnSelectedBookChanged;
        }
    }

    private void OnSelectedBookChanged(HandBookData selectedBook)
    {
        RefreshHighlight(selectedBook);
    }

    private void RefreshHighlight(HandBookData selectedBook)
    {
        bool shouldHighlight = false;

        if (selectedBook?.Sprite != null)
        {
            int selectedColorIndex =
                BookColorUtility.GetColorIndexFromSpriteName(
                    selectedBook.Sprite.name
                );

            shouldHighlight =
                selectedColorIndex >= 0 &&
                selectedColorIndex == _shelfColorIndex;
        }

        SetHighlightVisible(shouldHighlight);
    }

    private void SetHighlightVisible(bool visible)
    {
        if (_highlightObject != null)
        {
            _highlightObject.SetActive(visible);
        }
        else if (_highlightRenderer != null)
        {
            _highlightRenderer.enabled = visible;
        }
    }
}
