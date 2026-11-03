using UnityEngine;
using UnityEngine.UI;

/// <summary>Darkens the screen around a transparent, softly edged target.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public class TutorialSpotlightGraphic : MaskableGraphic
{
    [SerializeField, Min(0)] private float _feather = 16f;
    private Rect _opening;

    public void SetOpening(Rect opening)
    {
        if (_opening == opening) return;
        _opening = opening;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect screen = rectTransform.rect;
        Rect inner = Rect.MinMaxRect(
            Mathf.Clamp(_opening.xMin, screen.xMin, screen.xMax),
            Mathf.Clamp(_opening.yMin, screen.yMin, screen.yMax),
            Mathf.Clamp(_opening.xMax, screen.xMin, screen.xMax),
            Mathf.Clamp(_opening.yMax, screen.yMin, screen.yMax));
        Rect outer = Rect.MinMaxRect(
            Mathf.Max(screen.xMin, inner.xMin - _feather),
            Mathf.Max(screen.yMin, inner.yMin - _feather),
            Mathf.Min(screen.xMax, inner.xMax + _feather),
            Mathf.Min(screen.yMax, inner.yMax + _feather));

        Quad(mesh, new Vector2(screen.xMin, screen.yMin), new Vector2(screen.xMax, screen.yMin),
            new Vector2(screen.xMax, outer.yMin), new Vector2(screen.xMin, outer.yMin), color, color);
        Quad(mesh, new Vector2(screen.xMin, outer.yMax), new Vector2(screen.xMax, outer.yMax),
            new Vector2(screen.xMax, screen.yMax), new Vector2(screen.xMin, screen.yMax), color, color);
        Quad(mesh, new Vector2(screen.xMin, outer.yMin), new Vector2(outer.xMin, outer.yMin),
            new Vector2(outer.xMin, outer.yMax), new Vector2(screen.xMin, outer.yMax), color, color);
        Quad(mesh, new Vector2(outer.xMax, outer.yMin), new Vector2(screen.xMax, outer.yMin),
            new Vector2(screen.xMax, outer.yMax), new Vector2(outer.xMax, outer.yMax), color, color);

        Color clear = new Color(color.r, color.g, color.b, 0);
        Quad(mesh, inner.min, new Vector2(inner.xMax, inner.yMin),
            new Vector2(outer.xMax, outer.yMin), outer.min, clear, color);
        Quad(mesh, new Vector2(inner.xMax, inner.yMin), inner.max,
            outer.max, new Vector2(outer.xMax, outer.yMin), clear, color);
        Quad(mesh, inner.max, new Vector2(inner.xMin, inner.yMax),
            new Vector2(outer.xMin, outer.yMax), outer.max, clear, color);
        Quad(mesh, new Vector2(inner.xMin, inner.yMax), inner.min,
            outer.min, new Vector2(outer.xMin, outer.yMax), clear, color);
    }

    private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color near, Color far)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(a, near, Vector2.zero);
        mesh.AddVert(b, near, Vector2.zero);
        mesh.AddVert(c, far, Vector2.zero);
        mesh.AddVert(d, far, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }
}
