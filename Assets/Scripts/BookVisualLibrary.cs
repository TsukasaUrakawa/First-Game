using System;
using UnityEngine;

/// <summary>Baked 3D spine UVs reference the existing 140 sprites and eight source textures.</summary>
public sealed class BookVisualLibrary : ScriptableObject
{
    [Serializable]
    public sealed class Spine
    {
        public Sprite Sprite;
        public Mesh Mesh;
        public int ColorIndex;
    }
    [SerializeField] private Mesh _bodyMesh;
    [SerializeField] private Spine[] _spines;
    [SerializeField] private Material[] _coverMaterials;
    [SerializeField] private Material[] _spineMaterials;
    [SerializeField] private Material[] _ghostSpineMaterials;
    [SerializeField] private Material _paperMaterial;
    [SerializeField] private Material _ghostBodyMaterial;
    private Material[][] _bodyMaterials;
    private Material[] _ghostMaterials;
    public Mesh BodyMesh => _bodyMesh;

    public bool TryGet(Sprite sprite, out Spine spine)
    {
        spine = null;
        if (sprite == null || !BookColorUtility.TryGetBookIndex(sprite.name, out int index) ||
            _spines == null || index < 0 || index >= _spines.Length) return false;
        spine = _spines[index];
        return spine != null && spine.Sprite == sprite && spine.Mesh != null;
    }

    public Material[] BodyMaterials(int color, bool ghost)
    {
        if (ghost) return _ghostMaterials ?? (_ghostMaterials = new[] { _ghostBodyMaterial, _ghostBodyMaterial });
        if (_bodyMaterials == null)
        {
            _bodyMaterials = new Material[_coverMaterials.Length][];
            for (int i = 0; i < _bodyMaterials.Length; i++) _bodyMaterials[i] = new[] { _coverMaterials[i], _paperMaterial };
        }
        return _bodyMaterials[color];
    }
    public Material SpineMaterial(int color, bool ghost) => ghost ? _ghostSpineMaterials[color] : _spineMaterials[color];
}
