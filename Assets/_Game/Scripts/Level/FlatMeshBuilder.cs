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
        private const int CircleSegments = 40;
        private const int ArchSegments = 10;
        private const int CornerSegments = 5;
        private const float FullTurn = Mathf.PI * 2f;
        private const float HalfTurn = Mathf.PI;
        private const float QuarterTurn = Mathf.PI * 0.5f;

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

        public void AddCircle(float cx, float cy, float radius, Color32 color)
        {
            AddPie(cx, cy, radius, radius, 0f, FullTurn, CircleSegments, color);
        }

        public void AddEllipse(float cx, float cy, float radiusX, float radiusY, Color32 color)
        {
            AddPie(cx, cy, radiusX, radiusY, 0f, FullTurn, CircleSegments, color);
        }

        /// <summary>Прямоугольник со скруглёнными углами; радиус ограничен половиной меньшей стороны.</summary>
        public void AddRoundedRect(float minX, float minY, float maxX, float maxY, float radius, Color32 color)
        {
            float halfSide = Mathf.Min(maxX - minX, maxY - minY) * 0.5f;
            float r = Mathf.Min(radius, halfSide);
            AddRect(minX + r, minY, maxX - r, maxY, color);
            AddRect(minX, minY + r, minX + r, maxY - r, color);
            AddRect(maxX - r, minY + r, maxX, maxY - r, color);
            AddPie(maxX - r, maxY - r, r, r, 0f, QuarterTurn, CornerSegments, color);
            AddPie(minX + r, maxY - r, r, r, QuarterTurn, QuarterTurn, CornerSegments, color);
            AddPie(minX + r, minY + r, r, r, HalfTurn, QuarterTurn, CornerSegments, color);
            AddPie(maxX - r, minY + r, r, r, HalfTurn + QuarterTurn, QuarterTurn, CornerSegments, color);
        }

        /// <summary>Арка: прямоугольник с полукруглым верхом во всю ширину (окна, ворота).</summary>
        public void AddArch(float minX, float minY, float maxX, float maxY, Color32 color)
        {
            float r = (maxX - minX) * 0.5f;
            float shoulder = maxY - r;
            AddRect(minX, minY, maxX, shoulder, color);
            AddPie(minX + r, shoulder, r, r, 0f, HalfTurn, ArchSegments, color);
        }

        /// <summary>Веер от центра: сектор эллипса от startAngle на sweep радиан.</summary>
        private void AddPie(float cx, float cy, float radiusX, float radiusY, float startAngle, float sweep, int segments, Color32 color)
        {
            EnsureCapacity(segments + 2, segments * 3);
            int center = vertexCount;
            SetVertex(center, cx, cy, color);
            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + sweep * i / segments;
                SetVertex(center + 1 + i, cx + Mathf.Cos(angle) * radiusX, cy + Mathf.Sin(angle) * radiusY, color);
            }
            for (int i = 0; i < segments; i++)
            {
                triangles[triangleIndexCount++] = center;
                triangles[triangleIndexCount++] = center + 1 + i;
                triangles[triangleIndexCount++] = center + 2 + i;
            }
            vertexCount += segments + 2;
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

        /// <summary>
        /// Копирует накопленные вершины в массив-приёмник (для мешей, которые двигаются в кадре). Поля пишутся по одному:
        /// Array.Copy в Luna может оставить в двух массивах один и тот же struct (D-011).
        /// </summary>
        public void CopyVertices(Vector3[] target)
        {
            for (int i = 0; i < vertexCount; i++)
            {
                target[i].x = vertices[i].x;
                target[i].y = vertices[i].y;
            }
        }

        private void SetVertex(int index, float x, float y, Color32 color)
        {
            vertices[index] = new Vector3(x, y, 0f);
            colors[index] = color;
        }

        private void EnsureCapacity(int addVertices, int addIndices)
        {
            int size = vertices.Length;
            while (vertexCount + addVertices > size)
            {
                size *= GrowFactor;
            }
            if (size != vertices.Length)
            {
                System.Array.Resize(ref vertices, size);
                System.Array.Resize(ref colors, size);
            }
            int indexSize = triangles.Length;
            while (triangleIndexCount + addIndices > indexSize)
            {
                indexSize *= GrowFactor;
            }
            if (indexSize != triangles.Length)
            {
                System.Array.Resize(ref triangles, indexSize);
            }
        }
    }
}
