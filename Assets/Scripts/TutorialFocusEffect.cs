using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>Shared spotlight/pulse presentation, independent of tutorial progress.</summary>
public class TutorialFocusEffect : MonoBehaviour
{
    [SerializeField] private TutorialSpotlightGraphic _spotlight;
    [SerializeField] private Renderer _boxRenderer;
    [SerializeField, Min(.5f)] private float _pulseSeconds = 1.8f;
    [SerializeField, ColorUsage(false, true)] private Color _boxTint = new Color(.3f, 1.6f, .45f, 1);

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private MaterialPropertyBlock[] _originalProperties;
    private MaterialPropertyBlock[] _pulseProperties;
    private Color[] _baseColors;
    private bool _highlightBox;
    private RectTransform _uiTarget;
    private readonly List<Graphic> _graphics = new List<Graphic>();
    private readonly List<Color> _graphicColors = new List<Color>();

    public void SetTarget(Rect opening, bool highlightBox, RectTransform uiTarget)
    {
        _spotlight.SetOpening(opening);
        _spotlight.gameObject.SetActive(true);
        if (_uiTarget != uiTarget)
        {
            RestoreGraphics();
            _uiTarget = uiTarget;
            if (uiTarget != null)
            {
                // Tint the actual books in a list/hand, or the control itself.
                foreach (var image in uiTarget.GetComponentsInChildren<Image>(true))
                    if (image.sprite != null && BookColorUtility.GetColorIndexFromSpriteName(image.sprite.name) >= 0)
                        RememberGraphic(image);
                if (_graphics.Count == 0 && uiTarget.TryGetComponent<Graphic>(out var graphic))
                    RememberGraphic(graphic);
            }
        }
        if (_highlightBox != highlightBox)
        {
            if (highlightBox) CaptureBoxColors();
            else RestoreBoxColors();
            _highlightBox = highlightBox;
        }
        UpdatePulse();
    }

    private void UpdatePulse()
    {
        // A gentle fade, including when timeScale is zero. The object never disappears.
        float wave = GamePreferences.ReducedMotion ? .5f : .5f + .5f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2 / _pulseSeconds));
        for (int i = 0; i < _graphics.Count; i++)
        {
            Color original = _graphicColors[i];
            _graphics[i].color = Color.Lerp(original, new Color(.2f, 1f, .4f, original.a), wave * .65f);
        }
        if (!_highlightBox) return;
        for (int i = 0; i < _pulseProperties.Length; i++)
        {
            Color tint = Color.Lerp(_baseColors[i], _boxTint, Mathf.Lerp(.25f, .9f, wave));
            tint.a = _baseColors[i].a;
            _pulseProperties[i].SetColor(BaseColor, tint);
            _boxRenderer.SetPropertyBlock(_pulseProperties[i], i);
        }
    }

    private void RememberGraphic(Graphic graphic)
    {
        _graphics.Add(graphic);
        _graphicColors.Add(graphic.color);
    }

    private void RestoreGraphics()
    {
        for (int i = 0; i < _graphics.Count; i++)
            if (_graphics[i] != null) _graphics[i].color = _graphicColors[i];
        _graphics.Clear();
        _graphicColors.Clear();
        _uiTarget = null;
    }

    private void CaptureBoxColors()
    {
        var materials = _boxRenderer.sharedMaterials;
        if (_originalProperties == null)
        {
            _originalProperties = new MaterialPropertyBlock[materials.Length];
            _pulseProperties = new MaterialPropertyBlock[materials.Length];
            _baseColors = new Color[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                _originalProperties[i] = new MaterialPropertyBlock();
                _pulseProperties[i] = new MaterialPropertyBlock();
            }
        }
        for (int i = 0; i < _originalProperties.Length; i++)
        {
            _boxRenderer.GetPropertyBlock(_originalProperties[i], i);
            // Preserve unrelated overrides as well as the original tint.
            _boxRenderer.GetPropertyBlock(_pulseProperties[i], i);
            if (_pulseProperties[i].isEmpty) _boxRenderer.GetPropertyBlock(_pulseProperties[i]);
            _baseColors[i] = _pulseProperties[i].HasColor(BaseColor)
                ? _pulseProperties[i].GetColor(BaseColor) : materials[i].GetColor(BaseColor);
        }
    }

    private void RestoreBoxColors()
    {
        if (!_highlightBox || _boxRenderer == null) return;
        for (int i = 0; i < _originalProperties.Length; i++)
            _boxRenderer.SetPropertyBlock(_originalProperties[i].isEmpty ? null : _originalProperties[i], i);
        _highlightBox = false;
    }

    private void OnDisable()
    {
        RestoreBoxColors();
        RestoreGraphics();
        if (_spotlight != null) _spotlight.gameObject.SetActive(false);
    }
}
