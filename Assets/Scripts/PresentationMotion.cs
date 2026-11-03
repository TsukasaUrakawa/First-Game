using TMPro;
using UnityEngine;

/// <summary>Unscaled, restrained motion for the title and completed-book display.</summary>
public class PresentationMotion : MonoBehaviour
{
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _brand;
    [SerializeField] private TMP_Text _headline;
    [SerializeField] private CanvasGroup _intro;
    [SerializeField] private Vector2 _parallax = new Vector2(10, 6);
    [SerializeField, Min(0)] private float _floatHeight = 5f;
    [SerializeField, Min(.1f)] private float _introDuration = .8f;
    private Vector2 _backgroundOrigin, _brandOrigin, _pointer;
    private float _elapsed;

    private void Awake()
    {
        _backgroundOrigin = _background.anchoredPosition;
        _brandOrigin = _brand.anchoredPosition;
    }

    private void OnEnable()
    {
        _elapsed = 0;
        _pointer = Vector2.zero;
        if (_intro != null) _intro.alpha = 0;
    }

    private void LateUpdate()
    {
        _elapsed += Time.unscaledDeltaTime;
        float t = _elapsed;
        Vector2 mouse = Input.mousePosition;
        Vector2 target = new Vector2(
            Mathf.Clamp(mouse.x / Mathf.Max(1, Screen.width), 0, 1) * 2 - 1,
            Mathf.Clamp(mouse.y / Mathf.Max(1, Screen.height), 0, 1) * 2 - 1);
        _pointer = Vector2.Lerp(_pointer, target, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 2));
        _background.anchoredPosition = _backgroundOrigin + new Vector2(
            Mathf.Sin(t * .16f) * 6 + _pointer.x * _parallax.x,
            Mathf.Cos(t * .13f) * 4 + _pointer.y * _parallax.y);
        _brand.anchoredPosition = _brandOrigin + Vector2.up * (Mathf.Sin(t * .7f) * _floatHeight);
        if (_intro != null) _intro.alpha = Mathf.SmoothStep(0, 1, t / _introDuration);

        // Rebuild the base glyph positions before applying this frame's offsets.
        _headline.ForceMeshUpdate();
        var text = _headline.textInfo;
        for (int i = 0; i < text.characterCount; i++)
        {
            var ch = text.characterInfo[i];
            if (!ch.isVisible) continue;
            var vertices = text.meshInfo[ch.materialReferenceIndex].vertices;
            Vector3 offset = Vector3.up * (Mathf.Sin(t * 1.1f + i * .45f) * 1.8f);
            for (int v = 0; v < 4; v++) vertices[ch.vertexIndex + v] += offset;
        }
        _headline.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }

    private void OnDisable()
    {
        if (_background != null) _background.anchoredPosition = _backgroundOrigin;
        if (_brand != null) _brand.anchoredPosition = _brandOrigin;
        if (_headline != null) _headline.ForceMeshUpdate();
        if (_intro != null) _intro.alpha = 1;
    }
}
