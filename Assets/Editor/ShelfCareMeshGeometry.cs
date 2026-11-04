using System.Collections.Generic;
using UnityEngine;

/// <summary>Small editor-only box/bevel builder used to bake the gameplay room and books.</summary>
internal sealed class ShelfCareMeshGeometry
{
    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<Vector3> _normals = new List<Vector3>();
    private readonly List<Vector2> _uv = new List<Vector2>();
    private readonly List<int>[] _triangles;
    public ShelfCareMeshGeometry(int materials)
    {
        _triangles = new List<int>[materials];
        for (int i = 0; i < materials; i++) _triangles[i] = new List<int>();
    }

    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, int material, Vector2[] uv = null)
    {
        int start = _vertices.Count;
        _vertices.AddRange(new[] { a, b, c, d });
        for (int i = 0; i < 4; i++) _normals.Add(normal);
        _uv.AddRange(uv ?? new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) > 0)
            _triangles[material].AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        else _triangles[material].AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
    }

    public void Box(Vector3 center, Vector3 size, int material, float bevel = 0)
    {
        var half = size / 2;
        float radius = Mathf.Min(bevel, Mathf.Min(half.x, Mathf.Min(half.y, half.z)) * .8f);
        var axes = new[] { Vector3.right, Vector3.up, Vector3.forward };
        for (int axis = 0; axis < 3; axis++) for (int sign = -1; sign <= 1; sign += 2)
        {
            int u = (axis + 1) % 3, v = (axis + 2) % 3;
            var origin = center + axes[axis] * (half[axis] * sign);
            var du = axes[u] * (half[u] - radius); var dv = axes[v] * (half[v] - radius);
            Quad(origin - du - dv, origin - du + dv, origin + du + dv, origin + du - dv, axes[axis] * sign, material);
        }
        if (radius == 0) return;
        for (int edge = 0; edge < 3; edge++) for (int a = -1; a <= 1; a += 2) for (int b = -1; b <= 1; b += 2)
        {
            int u = (edge + 1) % 3, v = (edge + 2) % 3;
            var first = center + axes[u] * (a * half[u]) + axes[v] * (b * (half[v] - radius));
            var second = center + axes[u] * (a * (half[u] - radius)) + axes[v] * (b * half[v]);
            var length = axes[edge] * (half[edge] - radius);
            Quad(first - length, first + length, second + length, second - length, (axes[u] * a + axes[v] * b).normalized, material);
        }
        for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
        {
            var inset = new Vector3(x * (half.x - radius), y * (half.y - radius), z * (half.z - radius));
            var a = center + inset + Vector3.right * (x * radius);
            var b = center + inset + Vector3.up * (y * radius);
            var c = center + inset + Vector3.forward * (z * radius);
            Triangle(a, b, c, new Vector3(x, y, z).normalized, material);
        }
    }

    private void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, int material)
    {
        int start = _vertices.Count;
        _vertices.AddRange(new[] { a, b, c });
        _normals.AddRange(new[] { normal, normal, normal });
        _uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one });
        _triangles[material].AddRange(Vector3.Dot(Vector3.Cross(b - a, c - a), normal) > 0
            ? new[] { start, start + 1, start + 2 } : new[] { start, start + 2, start + 1 });
    }

    public Mesh Mesh(string name)
    {
        var mesh = new Mesh { name = name };
        mesh.SetVertices(_vertices); mesh.SetNormals(_normals); mesh.SetUVs(0, _uv);
        mesh.subMeshCount = _triangles.Length;
        for (int i = 0; i < _triangles.Length; i++) mesh.SetTriangles(_triangles[i], i);
        mesh.RecalculateBounds(); return mesh;
    }
}
