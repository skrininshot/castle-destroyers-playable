using CastleDestroyers.Core;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using CastleDestroyers.Platform;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    public sealed class ArtSpritesTests
    {
        [Test]
        public void TablesDescribeEverySprite()
        {
            Assert.AreEqual(ArtSprites.Uv.Length, ArtSprites.Rect.Length);
            Assert.AreEqual((ArtSprites.GlyphPlus + 1) * 4, ArtSprites.Uv.Length);
        }

        [Test]
        public void UvStayInsideAtlas()
        {
            for (int i = 0; i < ArtSprites.Uv.Length; i += 4)
            {
                Assert.GreaterOrEqual(ArtSprites.Uv[i], 0f);
                Assert.GreaterOrEqual(ArtSprites.Uv[i + 1], 0f);
                Assert.LessOrEqual(ArtSprites.Uv[i + 2], 1f);
                Assert.LessOrEqual(ArtSprites.Uv[i + 3], 1f);
                Assert.Less(ArtSprites.Uv[i], ArtSprites.Uv[i + 2]);
                Assert.Less(ArtSprites.Uv[i + 1], ArtSprites.Uv[i + 3]);
            }
        }

        [Test]
        public void CastleArtCoversTheSilhouette()
        {
            Assert.Greater(ArtSprites.CastleArtWidthMeters * 0.5f, CastleShapes.RedHalfWidth + 0.2f);
            Assert.Greater(ArtSprites.CastleArtHeightMeters, CastleShapes.RedTotalHeight() + 0.6f);
        }

        [Test]
        public void PlatformSpritesMatchPlatformWidth()
        {
            Assert.AreEqual(CastleShapes.PlatformHalfWidth(CastleShapes.BlueHalfWidth), ArtSprites.Rect[ArtSprites.PlatformBlue * 4 + 2], 0.1f);
            Assert.AreEqual(CastleShapes.PlatformHalfWidth(CastleShapes.RedHalfWidth), ArtSprites.Rect[ArtSprites.PlatformRed * 4 + 2], 0.1f);
        }
    }

    public sealed class FlatMeshBuilderSpriteTests
    {
        [Test]
        public void SpriteGoesToItsRectAndUv()
        {
            FlatMeshBuilder builder = new FlatMeshBuilder();
            builder.AddSprite(ArtSprites.Moon, 10f, 20f, 2f, Color.white);
            Mesh mesh = new Mesh();
            builder.Apply(mesh);
            int r = ArtSprites.Moon * 4;
            Assert.AreEqual(10f + ArtSprites.Rect[r] * 2f, mesh.vertices[0].x, 1e-4f);
            Assert.AreEqual(20f + ArtSprites.Rect[r + 3] * 2f, mesh.vertices[2].y, 1e-4f);
            Assert.AreEqual(ArtSprites.Uv[r], mesh.uv[0].x, 1e-5f);
            Assert.AreEqual(ArtSprites.Uv[r + 3], mesh.uv[2].y, 1e-5f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void FlatShapesUseTheWhiteArea()
        {
            FlatMeshBuilder builder = new FlatMeshBuilder();
            builder.AddRect(0f, 0f, 1f, 1f, Color.red);
            Mesh mesh = new Mesh();
            builder.Apply(mesh);
            int r = ArtSprites.White * 4;
            Assert.Greater(mesh.uv[0].x, ArtSprites.Uv[r]);
            Assert.Less(mesh.uv[0].x, ArtSprites.Uv[r + 2]);
            Object.DestroyImmediate(mesh);
        }
    }

    public sealed class FlatParticleShapeTests
    {
        [Test]
        public void SlotReusedAfterStarIsCircleAgain()
        {
            FlatParticles particles = new FlatParticles();
            particles.Init(1);
            particles.SpawnStar(0f, 0f, 0f, 0f, 1f, 0.01f, 0f, Color.white);
            particles.Tick(0.001f);
            float star = particles.Mesh.uv[0].x;
            Assert.AreEqual(ArtSprites.Uv[ArtSprites.Spark * 4], star, 1e-5f);
            particles.Tick(0.1f);
            particles.Spawn(0f, 0f, 0f, 0f, 1f, 1f, 0f, Color.white);
            particles.Tick(0.001f);
            Assert.AreEqual(ArtSprites.Uv[ArtSprites.Circle * 4], particles.Mesh.uv[0].x, 1e-5f);
            Object.DestroyImmediate(particles.Mesh);
        }
    }

    public sealed class GameSoundsTests
    {
        [Test]
        public void SilentUntilFirstTap()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            GameSounds sounds = new GameSounds();
            // Источников нет: любой реальный вызов упал бы здесь.
            sounds.Init(config, new AudioClip[7], new AudioSource[0]);
            sounds.Play(SoundId.ExplosionA);
            sounds.Tick(0.1f);
            Assert.IsFalse(sounds.Unlocked);
            sounds.Play(SoundId.Victory);
            sounds.SetMuted(true);
            Assert.AreEqual(0f, AudioListener.volume);
            sounds.SetMuted(false);
            Assert.AreEqual(config.soundVolume, AudioListener.volume);
            Object.DestroyImmediate(config);
        }
    }
}
