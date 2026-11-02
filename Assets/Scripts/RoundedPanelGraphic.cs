using UnityEngine;
using UnityEngine.UI;

/// <summary>Resolution-independent rounded UI panels without a texture asset.</summary>
[AddComponentMenu("UI/Shelf Care/Rounded Panel")]
[RequireComponent(typeof(CanvasRenderer))]
public class RoundedPanelGraphic : MaskableGraphic
{
    [SerializeField, Min(0)] private float _cornerRadius = 18f;

    public float CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Mathf.Max(0, value); SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float radius = Mathf.Min(_cornerRadius, Mathf.Min(rect.width, rect.height) * .5f);
        mesh.AddVert(rect.center, color, new Vector2(.5f, .5f));
        const int segments = 8;
        for (int corner = 0; corner < 4; corner++)
        {
            var center = new Vector2(
                corner == 0 || corner == 3 ? rect.xMax - radius : rect.xMin + radius,
                corner < 2 ? rect.yMax - radius : rect.yMin + radius);
            for (int step = 0; step <= segments; step++)
            {
                float angle = (corner * 90 + step * 90f / segments) * Mathf.Deg2Rad;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                mesh.AddVert(point, color, Vector2.zero);
            }
        }
        int perimeter = 4 * (segments + 1);
        for (int i = 1; i <= perimeter; i++)
            mesh.AddTriangle(0, i, i == perimeter ? 1 : i + 1);
    }
}
