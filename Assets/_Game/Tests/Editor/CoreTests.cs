using CastleDestroyers.Core;
using NUnit.Framework;
using UnityEngine;

namespace CastleDestroyers.Tests
{
    public sealed class RngTests
    {
        [Test]
        public void SameSeedGivesSameSequence()
        {
            Rng a = new Rng(777u);
            Rng b = new Rng(777u);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextUInt(), b.NextUInt());
            }
        }

        [Test]
        public void DifferentSeedsDiverge()
        {
            Rng a = new Rng(1u);
            Rng b = new Rng(2u);
            bool differs = false;
            for (int i = 0; i < 10; i++)
            {
                differs |= a.NextUInt() != b.NextUInt();
            }
            Assert.IsTrue(differs);
        }

        [Test]
        public void ZeroSeedDoesNotStick()
        {
            Rng rng = new Rng(0u);
            Assert.AreNotEqual(rng.NextUInt(), rng.NextUInt());
        }

        [Test]
        public void NextFloatStaysInUnitInterval()
        {
            Rng rng = new Rng(12345u);
            for (int i = 0; i < 20000; i++)
            {
                float value = rng.NextFloat();
                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        [Test]
        public void FloatRangeRespectsBounds()
        {
            Rng rng = new Rng(99u);
            for (int i = 0; i < 5000; i++)
            {
                float value = rng.Range(-2f, 3f);
                Assert.GreaterOrEqual(value, -2f);
                Assert.Less(value, 3f);
            }
        }

        [Test]
        public void IntRangeIsExclusiveAndCoversAllValues()
        {
            Rng rng = new Rng(5u);
            bool[] seen = new bool[4];
            for (int i = 0; i < 1000; i++)
            {
                int value = rng.Range(2, 6);
                Assert.GreaterOrEqual(value, 2);
                Assert.Less(value, 6);
                seen[value - 2] = true;
            }
            for (int i = 0; i < seen.Length; i++)
            {
                Assert.IsTrue(seen[i], "value " + (i + 2) + " never produced");
            }
        }

        [Test]
        public void EmptyIntRangeReturnsMin()
        {
            Rng rng = new Rng(5u);
            Assert.AreEqual(4, rng.Range(4, 4));
        }
    }

    public sealed class PoolTests
    {
        [Test]
        public void IndexPoolHandsOutEachSlotOnceThenReportsExhausted()
        {
            IndexPool pool = new IndexPool(5);
            bool[] seen = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                int index = pool.Alloc();
                Assert.GreaterOrEqual(index, 0);
                Assert.IsFalse(seen[index]);
                seen[index] = true;
            }
            Assert.AreEqual(-1, pool.Alloc());
            Assert.AreEqual(5, pool.ActiveCount);
        }

        [Test]
        public void IndexPoolReusesFreedSlot()
        {
            IndexPool pool = new IndexPool(3);
            pool.Alloc();
            int middle = pool.Alloc();
            pool.Alloc();
            Assert.IsTrue(pool.Free(middle));
            Assert.AreEqual(2, pool.ActiveCount);
            Assert.AreEqual(middle, pool.Alloc());
        }

        [Test]
        public void IndexPoolRejectsDoubleFreeAndForeignIndex()
        {
            IndexPool pool = new IndexPool(2);
            int index = pool.Alloc();
            Assert.IsTrue(pool.Free(index));
            Assert.IsFalse(pool.Free(index));
            Assert.IsFalse(pool.Free(-1));
            Assert.IsFalse(pool.Free(2));
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void IndexPoolClearFreesEverythingAndStartsFromZero()
        {
            IndexPool pool = new IndexPool(4);
            pool.Alloc();
            pool.Alloc();
            pool.Clear();
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.IsFalse(pool.IsUsed(0));
            Assert.AreEqual(0, pool.Alloc());
        }

        [Test]
        public void IndexPoolTracksUsage()
        {
            IndexPool pool = new IndexPool(2);
            int index = pool.Alloc();
            Assert.IsTrue(pool.IsUsed(index));
            pool.Free(index);
            Assert.IsFalse(pool.IsUsed(index));
        }

        [Test]
        public void ZeroCapacityPoolIsAlwaysExhausted()
        {
            IndexPool pool = new IndexPool(0);
            Assert.AreEqual(-1, pool.Alloc());
        }

        [Test]
        public void ObjectPoolReturnsItemsThenNull()
        {
            string[] items = { "a", "b" };
            ObjectPool<string> pool = new ObjectPool<string>(items);
            Assert.AreEqual(2, pool.FreeCount);
            string first = pool.Get();
            string second = pool.Get();
            Assert.AreNotEqual(first, second);
            Assert.IsNull(pool.Get());
            Assert.AreEqual(0, pool.FreeCount);
        }

        [Test]
        public void ObjectPoolAcceptsReleasedItemButNotOverflow()
        {
            ObjectPool<string> pool = new ObjectPool<string>(new[] { "a", "b" });
            string item = pool.Get();
            Assert.IsTrue(pool.Release(item));
            Assert.IsFalse(pool.Release("extra"));
            Assert.IsFalse(pool.Release(null));
            Assert.AreEqual(2, pool.FreeCount);
        }
    }

    public sealed class GameLoopTests
    {
        private sealed class Recorder : ITickable
        {
            public readonly System.Collections.Generic.List<string> log;
            private readonly string name;
            public float lastDt;

            public Recorder(string name, System.Collections.Generic.List<string> log)
            {
                this.name = name;
                this.log = log;
            }

            public void Tick(float dt)
            {
                lastDt = dt;
                log.Add(name);
            }
        }

        [Test]
        public void SystemsTickInDeclaredOrder()
        {
            System.Collections.Generic.List<string> log = new System.Collections.Generic.List<string>();
            GameLoop loop = new GameLoop();
            loop.Init(new ITickable[] { new Recorder("a", log), new Recorder("b", log), new Recorder("c", log) }, 0.05f);
            loop.Tick(0.016f);
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, log);
        }

        [Test]
        public void DeltaIsClampedAndScaled()
        {
            System.Collections.Generic.List<string> log = new System.Collections.Generic.List<string>();
            Recorder recorder = new Recorder("a", log);
            GameLoop loop = new GameLoop();
            loop.Init(new ITickable[] { recorder }, 0.05f);
            loop.Tick(1f);
            Assert.AreEqual(0.05f, recorder.lastDt, 1e-6f);
            loop.TimeScale = 0.25f;
            loop.Tick(0.04f);
            Assert.AreEqual(0.01f, recorder.lastDt, 1e-6f);
            Assert.AreEqual(0.06f, loop.GameTime, 1e-6f);
        }

        [Test]
        public void PausedLoopDoesNotTick()
        {
            System.Collections.Generic.List<string> log = new System.Collections.Generic.List<string>();
            GameLoop loop = new GameLoop();
            loop.Init(new ITickable[] { new Recorder("a", log) }, 0.05f);
            loop.Paused = true;
            loop.Tick(0.016f);
            Assert.AreEqual(0, log.Count);
            Assert.AreEqual(0f, loop.GameTime);
        }
    }

    public sealed class ThemePaletteTests
    {
        private static float Brightness(Color32 c)
        {
            return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
        }

        [Test]
        public void PresetsAreSelectable()
        {
            ThemePalette theme = ScriptableObject.CreateInstance<ThemePalette>();
            Assert.AreSame(theme.night, theme.Get(ThemePreset.Night));
            Assert.AreSame(theme.sunset, theme.Get(ThemePreset.Sunset));
            theme.preset = ThemePreset.Sunset;
            Assert.AreSame(theme.sunset, theme.Current);
            Object.DestroyImmediate(theme);
        }

        [Test]
        public void NightFarLayerIsLightestOverDarkSky()
        {
            PaletteSet night = ThemePalette.CreateNight();
            Assert.Greater(Brightness(night.backgroundFar), Brightness(night.backgroundMid));
            Assert.Greater(Brightness(night.backgroundMid), Brightness(night.backgroundNear), "D-061: aerial perspective");
            Assert.Less(Brightness(night.sky), Brightness(night.backgroundFar), "D-023: night sky is dark");
        }

        [Test]
        public void SunsetBackgroundGetsLighterWithDistance()
        {
            PaletteSet sunset = ThemePalette.CreateSunset();
            Assert.Greater(Brightness(sunset.skyHorizon), Brightness(sunset.backgroundFar));
            Assert.Greater(Brightness(sunset.backgroundFar), Brightness(sunset.backgroundMid));
            Assert.Greater(Brightness(sunset.backgroundMid), Brightness(sunset.backgroundNear));
        }

        [Test]
        public void GameplayObjectsContrastWithNearBackground()
        {
            PaletteSet[] sets = { ThemePalette.CreateNight(), ThemePalette.CreateSunset() };
            for (int i = 0; i < sets.Length; i++)
            {
                Color32 near = sets[i].backgroundNear;
                Assert.Greater(Mathf.Abs(Brightness(sets[i].castleStone) - Brightness(near)), 60f, "castle stone vs background, set " + i);
                Assert.Greater(Mathf.Abs(Brightness(sets[i].booster20) - Brightness(near)), 40f, "booster x20 vs background, set " + i);
            }
        }

        [Test]
        public void HudAndBoosterColorsAreSharedBetweenPresets()
        {
            PaletteSet night = ThemePalette.CreateNight();
            PaletteSet sunset = ThemePalette.CreateSunset();
            Assert.AreEqual(night.hudPanel, sunset.hudPanel);
            Assert.AreEqual(night.hudHealth, sunset.hudHealth);
            Assert.AreEqual(night.booster10, sunset.booster10);
            Assert.AreEqual(night.booster50, sunset.booster50);
        }

        [Test]
        public void PresetsDifferInSkyAndGround()
        {
            PaletteSet night = ThemePalette.CreateNight();
            PaletteSet sunset = ThemePalette.CreateSunset();
            Assert.AreNotEqual(night.sky, sunset.sky);
            Assert.AreNotEqual(night.ground, sunset.ground);
        }

        [Test]
        public void HexParsesChannels()
        {
            Color32 c = ThemePalette.Hex(0x3FE07A);
            Assert.AreEqual(0x3F, c.r);
            Assert.AreEqual(0xE0, c.g);
            Assert.AreEqual(0x7A, c.b);
            Assert.AreEqual(255, c.a);
        }
    }

    public sealed class GameConfigTests
    {
        private GameConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DefaultsFollowSpec()
        {
            Assert.AreEqual(3, config.maxPlayerTurns);
            Assert.AreEqual(75f, config.maxSessionSeconds);
            Assert.AreEqual(3, config.playerCannons);
            Assert.AreEqual(12, config.enemyCannons);
            Assert.AreEqual(10, config.rocketsPerCannon);
            Assert.AreEqual(600, config.maxActiveRockets);
            Assert.IsTrue(config.directorEnabled);
            Assert.IsFalse(config.bonusPickEnabled);
            Assert.IsFalse(config.rainEnabled);
        }

        [Test]
        public void HasHintForEveryDefaultTurn()
        {
            Assert.GreaterOrEqual(config.turnHints.Length, config.maxPlayerTurns);
            Assert.AreEqual(20, config.GetHint(0).highlightBoosterMultiplier);
            Assert.AreEqual(20, config.GetHint(1).highlightBoosterMultiplier);
            Assert.IsFalse(config.GetHint(2).showHand);
        }

        [Test]
        public void HintIndexIsClamped()
        {
            Assert.AreSame(config.turnHints[0], config.GetHint(-3));
            Assert.AreSame(config.turnHints[config.turnHints.Length - 1], config.GetHint(99));
        }

        [Test]
        public void MissingHintsGiveNull()
        {
            config.turnHints = new TurnHint[0];
            Assert.IsNull(config.GetHint(0));
        }

        [Test]
        public void BoostersMatchLevelSpec()
        {
            bool has10 = false;
            bool has20 = false;
            bool has50 = false;
            for (int i = 0; i < config.boosters.Length; i++)
            {
                has10 |= config.boosters[i].multiplier == 10;
                has20 |= config.boosters[i].multiplier == 20;
                has50 |= config.boosters[i].multiplier == 50;
            }
            Assert.IsTrue(has10 && has20 && has50);
        }

        [Test]
        public void FrameWidthsFollowCameraSpec()
        {
            Assert.Greater(config.centerFrameWidth, config.targetFrameWidth);
            Assert.Greater(config.targetFrameWidth, config.castleFrameWidth);
        }
    }
}
