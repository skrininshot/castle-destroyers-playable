using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.InputRig;
using UnityEngine;

namespace CastleDestroyers.UI
{
    /// <summary>Пунктир прицела (ТЗ §3.2): белые точки вдоль первой части траектории, к концу меньше и прозрачнее.</summary>
    public sealed class AimPreview : ITickable
    {
        private const int VerticesPerQuad = 4;
        private const int IndicesPerQuad = 6;
        private const float BoundsSize = 500f;
        private const byte Opaque = 255;

        private GameConfig config;
        private AimInput aim;
        private CannonSystem cannons;
        private Vector3[] vertices;
        private Color32[] colors;
        private bool shown;

        public Mesh Mesh { get; private set; }

        public void Init(GameConfig gameConfig, AimInput aimInput, CannonSystem cannonSystem)
        {
            config = gameConfig;
            aim = aimInput;
            cannons = cannonSystem;

            int count = config.aimDotCount;
            vertices = new Vector3[count * VerticesPerQuad];
            colors = new Color32[count * VerticesPerQuad];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[count * IndicesPerQuad];
            for (int i = 0; i < count; i++)
            {
                int v = i * VerticesPerQuad;
                uv[v + 1].x = 1f;
                uv[v + 2].y = 1f;
                uv[v + 3].x = 1f;
                uv[v + 3].y = 1f;
                int t = i * IndicesPerQuad;
                triangles[t] = v;
                triangles[t + 1] = v + 2;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 2;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 1;
            }

            Mesh = new Mesh();
            Mesh.name = "AimPreview";
            Mesh.MarkDynamic();
            Mesh.vertices = vertices;
            Mesh.uv = uv;
            Mesh.colors32 = colors;
            Mesh.triangles = triangles;
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(BoundsSize, BoundsSize, 1f));
        }

        public void ApplyPalette(PaletteSet palette)
        {
            int count = config.aimDotCount;
            for (int i = 0; i < count; i++)
            {
                Color32 color = palette.hudText;
                color.a = (byte)Mathf.Lerp(Opaque, config.aimDotAlphaEnd, Fraction(i, count));
                int v = i * VerticesPerQuad;
                colors[v] = color;
                colors[v + 1] = color;
                colors[v + 2] = color;
                colors[v + 3] = color;
            }
            Mesh.colors32 = colors;
        }

        public void Tick(float dt)
        {
            if (!aim.Aiming)
            {
                if (shown)
                {
                    shown = false;
                    System.Array.Clear(vertices, 0, vertices.Length);
                    Mesh.vertices = vertices;
                }
                return;
            }
            shown = true;

            float radians = aim.AngleDeg * Mathf.Deg2Rad;
            float speed = Ballistics.Speed(aim.Power01, config.launchSpeedMin, config.launchSpeedMax);
            float speedX = Mathf.Cos(radians) * speed;
            float speedY = Mathf.Sin(radians) * speed;
            float startX = cannons.MuzzleCenterX(Side.Player);
            float startY = cannons.MuzzleCenterY(Side.Player);
            float gravity = config.gravity;
            float flight = Ballistics.TimeToHeight(startY, speedY, gravity, config.waterLevel);
            float shownTime = flight * config.aimPreviewFraction;

            int count = config.aimDotCount;
            for (int i = 0; i < count; i++)
            {
                float fraction = Fraction(i, count);
                float t = shownTime * (i + 1) / count;
                float x = startX + speedX * t;
                float y = startY + speedY * t - 0.5f * gravity * t * t;
                float half = Mathf.Lerp(config.aimDotSizeStart, config.aimDotSizeEnd, fraction) * 0.5f;
                int v = i * VerticesPerQuad;
                vertices[v].x = x - half;
                vertices[v].y = y - half;
                vertices[v + 1].x = x + half;
                vertices[v + 1].y = y - half;
                vertices[v + 2].x = x - half;
                vertices[v + 2].y = y + half;
                vertices[v + 3].x = x + half;
                vertices[v + 3].y = y + half;
            }
            Mesh.vertices = vertices;
        }

        private static float Fraction(int index, int count)
        {
            return count > 1 ? (float)index / (count - 1) : 0f;
        }
    }
}
