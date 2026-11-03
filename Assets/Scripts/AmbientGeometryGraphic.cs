using UnityEngine;
using UnityEngine.UI;

/// <summary>Lightweight moving gold motes and geometric accents drawn as UI geometry.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public class AmbientGeometryGraphic : MaskableGraphic
{
    [SerializeField, Range(0, 40)] private int _moteCount = 22;
    [SerializeField, Range(0, 1)] private float _ornamentAlpha = .18f;
    private void Update() => SetVerticesDirty();

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect r = rectTransform.rect;
        float t = Time.unscaledTime;
        for (int i = 0; i < _moteCount; i++)
        {
            float phase = i * 2.39996f;
            float x = Mathf.Repeat(i * .618034f + Mathf.Sin(t * .1f + phase) * .008f, 1);
            float y = Mathf.Repeat(i * .381966f + t * (.009f + i % 3 * .002f), 1);
            Vector2 point = r.min + Vector2.Scale(r.size, new Vector2(x, y));
            Color tint = color; tint.a *= .16f + .34f * (.5f + .5f * Mathf.Sin(t * .8f + phase));
            float size = 1.5f + i % 3 * .55f;
            Line(mesh, point + Vector2.left * size, point + Vector2.right * size, 1.5f, tint);
            Line(mesh, point + Vector2.down * size, point + Vector2.up * size, 1.5f, tint);
        }
        for (int i = 0; i < 3; i++)
        {
            Vector2 center = r.min + Vector2.Scale(r.size, new Vector2(.14f + i * .33f, i == 1 ? .88f : .26f));
            float radius = 32 + i * 13;
            Color tint = color; tint.a *= _ornamentAlpha;
            float angle = t * (i % 2 == 0 ? .035f : -.025f) + i;
            for (int j = 0; j < 40; j++)
            {
                float a = angle + j * Mathf.PI * 1.6f / 40;
                float b = angle + (j + 1) * Mathf.PI * 1.6f / 40;
                Line(mesh, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, 1.2f, tint);
            }
            Vector2 previous = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * .68f);
            for (int j = 1; j <= 4; j++)
            {
                float a = angle + j * Mathf.PI * .5f;
                Vector2 next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (radius * .68f);
                Line(mesh, previous, next, 1.2f, tint); previous = next;
            }
        }
    }

    private static void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width, Color tint)
    {
        Vector2 normal = new Vector2(-(b - a).y, (b - a).x).normalized * (width * .5f);
        int start = mesh.currentVertCount;
        mesh.AddVert(a - normal, tint, Vector2.zero); mesh.AddVert(a + normal, tint, Vector2.zero);
        mesh.AddVert(b + normal, tint, Vector2.zero); mesh.AddVert(b - normal, tint, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
    }
}
