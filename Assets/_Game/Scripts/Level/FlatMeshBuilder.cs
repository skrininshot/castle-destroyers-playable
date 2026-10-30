using UnityEngine;

namespace CastleDestroyers.Level
{
    /// <summary>
    /// Собирает плоские фигуры одного цвета в один меш (вершинный цвет). Используется при Init и при смене темы,
    /// не в кадре: буферы растут по необходимости и переиспользуются между сборками.
    /// </summary>
    public sealed class FlatMeshBuilder
    {
        private const int InitialVertices = 512;
        private const int GrowFactor = 2;

        private Vector3[] vertices = new Vector3[InitialVertices];
        private Color32[] colors = new Color32[InitialVertices];
        private int[] triangles = new int[InitialVertices * 3];
        private int vertexCount;
        private int triangleIndexCount;

        public int VertexCount { get { return vertexCount; } }

        public void Clear()
        {
            vertexCount = 0;
            triangleIndexCount = 0;
        }

        public void AddTriangle(float x0, float y0, float x1, float y1, float x2, float y2, Color32 color)
        {
            EnsureCapacity(3, 3);
            int v = vertexCount;
            SetVertex(v, x0, y0, color);
            SetVertex(v + 1, x1, y1, color);
            SetVertex(v + 2, x2, y2, color);
            triangles[triangleIndexCount++] = v;
            triangles[triangleIndexCount++] = v + 1;
            triangles[triangleIndexCount++] = v + 2;
            vertexCount += 3;
        }

        /// <summary>Выпуклый четырёхугольник с обходом вершин по кругу.</summary>
        public void AddQuad(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3, Color32 color)
        {
            EnsureCapacity(4, 6);
            int v = vertexCount;
            SetVertex(v, x0, y0, color);
            SetVertex(v + 1, x1, y1, color);
            SetVertex(v + 2, x2, y2, color);
            SetVertex(v + 3, x3, y3, color);
            triangles[triangleIndexCount++] = v;
            triangles[triangleIndexCount++] = v + 1;
            triangles[triangleIndexCount++] = v + 2;
            triangles[triangleIndexCount++] = v;
            triangles[triangleIndexCount++] = v + 2;
            triangles[triangleIndexCount++] = v + 3;
            vertexCount += 4;
        }

        public void AddRect(float minX, float minY, float maxX, float maxY, Color32 color)
        {
            AddQuad(minX, minY, maxX, minY, maxX, maxY, minX, maxY, color);
        }

        /// <summary>Переносит накопленное в меш. Лишние элементы буферов отсекаются.</summary>
        public void Apply(Mesh mesh)
        {
            Vector3[] outVertices = new Vector3[vertexCount];
            Color32[] outColors = new Color32[vertexCount];
            int[] outTriangles = new int[triangleIndexCount];
            System.Array.Copy(vertices, outVertices, vertexCount);
            System.Array.Copy(colors, outColors, vertexCount);
            System.Array.Copy(triangles, outTriangles, triangleIndexCount);
            mesh.Clear();
            mesh.vertices = outVertices;
            mesh.colors32 = outColors;
            mesh.triangles = outTriangles;
            mesh.RecalculateBounds();
        }

        private void SetVertex(int index, float x, float y, Color32 color)
        {
            vertices[index] = new Vector3(x, y, 0f);
            colors[index] = color;
        }

        private void EnsureCapacity(int addVertices, int addIndices)
        {
            if (vertexCount + addVertices > vertices.Length)
            {
                int size = vertices.Length * GrowFactor;
                System.Array.Resize(ref vertices, size);
                System.Array.Resize(ref colors, size);
            }
            if (triangleIndexCount + addIndices > triangles.Length)
            {
                System.Array.Resize(ref triangles, triangles.Length * GrowFactor);
            }
        }
    }
}
