using UnityEngine;

/// <summary>A rigid cloth binding and page block, with the original puzzle image on its spine.</summary>
public sealed class BookVisual3D : MonoBehaviour
{
    [SerializeField] private BookVisualLibrary _library;
    [SerializeField] private MeshFilter _bodyMesh;
    [SerializeField] private MeshRenderer _body;
    [SerializeField] private MeshFilter _spineMesh;
    [SerializeField] private MeshRenderer _spine;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private bool _ghost;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private MaterialPropertyBlock _tint;
    private Color[] _bodyColors;
    private Color _spineColor;
    public Sprite Sprite => _sprite;
    public float HighlightAmount { get; private set; }
    public MeshRenderer SpineRenderer => _spine;
    private void OnEnable() { if (_sprite != null) SetSprite(_sprite); }

    public void SetSprite(Sprite sprite)
    {
        if (_sprite == sprite && _bodyColors != null && _bodyMesh.sharedMesh != null && _spineMesh.sharedMesh != null) return;
        if (!_library.TryGet(sprite, out var entry))
        {
            Debug.LogError("3Dの背表紙が見つかりません: " + (sprite == null ? "null" : sprite.name), this);
            return;
        }
        _sprite = sprite;
        _bodyMesh.sharedMesh = _library.BodyMesh;
        _spineMesh.sharedMesh = entry.Mesh;
        var materials = _library.BodyMaterials(entry.ColorIndex, _ghost);
        _body.sharedMaterials = materials;
        _spine.sharedMaterial = _library.SpineMaterial(entry.ColorIndex, _ghost);
        if (_bodyColors == null) _bodyColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++) _bodyColors[i] = materials[i].GetColor(BaseColor);
        _spineColor = _spine.sharedMaterial.GetColor(BaseColor);
        SetHighlight(0);
    }

    public void SetHighlight(float amount)
    {
        HighlightAmount = Mathf.Clamp01(amount);
        if (HighlightAmount == 0)
        {
            _spine.SetPropertyBlock(null);
            if (_bodyColors != null) for (int i = 0; i < _bodyColors.Length; i++) _body.SetPropertyBlock(null, i);
            return;
        }
        if (_tint == null) _tint = new MaterialPropertyBlock();
        Color Green(Color original) => Color.Lerp(original, new Color(.35f, .95f, .46f, original.a), HighlightAmount);
        _tint.SetColor(BaseColor, Green(_spineColor)); _spine.SetPropertyBlock(_tint);
        for (int i = 0; i < _bodyColors.Length; i++)
        {
            _tint.SetColor(BaseColor, Green(_bodyColors[i]));
            _body.SetPropertyBlock(_tint, i);
        }
    }
}
