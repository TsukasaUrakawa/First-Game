using UnityEngine;
using UnityEngine.UI;

/// <summary>A quiet dark reading area over the live 3D title, with no background bitmap.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class TitleBackdropGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect r = rectTransform.rect;
        float[] stops = { 0, .26f, .42f, .56f, 1 };
        float[] opacity = { .96f, .88f, .30f, 0, 0 };
        for (int i = 0; i < stops.Length - 1; i++)
        {
            float left = Mathf.Lerp(r.xMin, r.xMax, stops[i]);
            float right = Mathf.Lerp(r.xMin, r.xMax, stops[i + 1]);
            Color a = color, b = color;
            a.a *= opacity[i]; b.a *= opacity[i + 1];
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector2(left, r.yMin), a, Vector2.zero);
            mesh.AddVert(new Vector2(left, r.yMax), a, Vector2.zero);
            mesh.AddVert(new Vector2(right, r.yMax), b, Vector2.zero);
            mesh.AddVert(new Vector2(right, r.yMin), b, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
