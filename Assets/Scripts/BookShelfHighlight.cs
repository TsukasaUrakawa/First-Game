using System.Collections;
using UnityEngine;

public class BookShelfHighlight : MonoBehaviour
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private int _shelfColorIndex = -1;
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private SpriteRenderer _highlightRenderer;
    [SerializeField] private Color _highlightColor = new Color(1f, 0.9f, 0.25f, 0.45f);
    [SerializeField, Min(1)] private int _blinkCount = 2;
    [SerializeField, Min(0.01f)] private float _blinkOnTime = 0.18f;
    [SerializeField, Min(0.01f)] private float _blinkOffTime = 0.12f;

    private Coroutine _blinkCoroutine;

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

        StopBlink();
    }

    private void OnSelectedBookChanged(HandBookData selectedBook)
    {
        RefreshHighlight(selectedBook);
    }

    private void RefreshHighlight(HandBookData selectedBook)
    {
        bool shouldBlink = false;

        if (selectedBook?.Sprite != null)
        {
            int selectedColorIndex =
                BookColorUtility.GetColorIndexFromSpriteName(
                    selectedBook.Sprite.name
                );

            shouldBlink =
                selectedColorIndex >= 0 &&
                selectedColorIndex == _shelfColorIndex;
        }

        if (shouldBlink)
        {
            StartBlink();
        }
        else
        {
            StopBlink();
        }
    }

    private void StartBlink()
    {
        StopBlink();
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private void StopBlink()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        SetHighlightVisible(false);
    }

    private IEnumerator BlinkRoutine()
    {
        if (_highlightRenderer != null)
        {
            _highlightRenderer.color = _highlightColor;
        }

        for (int i = 0; i < _blinkCount; i++)
        {
            SetHighlightVisible(true);
            yield return new WaitForSeconds(_blinkOnTime);

            SetHighlightVisible(false);

            if (i < _blinkCount - 1)
            {
                yield return new WaitForSeconds(_blinkOffTime);
            }
        }

        _blinkCoroutine = null;
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
