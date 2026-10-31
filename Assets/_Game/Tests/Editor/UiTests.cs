using CastleDestroyers.CameraRig;
using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.InputRig;
using CastleDestroyers.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CastleDestroyers.Tests
{
    public sealed class UiEaseTests
    {
        [Test]
        public void OutBackStartsAtZeroOvershootsAndEndsAtOne()
        {
            Assert.AreEqual(0f, UiEase.OutBack(0f), 1e-4f);
            Assert.AreEqual(1f, UiEase.OutBack(1f), 1e-4f);
            Assert.Greater(UiEase.OutBack(0.7f), 1f);
            Assert.AreEqual(1f, UiEase.OutBack(5f), 1e-4f);
        }
    }

    public sealed class TurnGuideTests
    {
        private const float Step = 0.02f;
        private const float ScreenHeight = 1000f;

        private CombatRig rig;
        private CameraDirector director;
        private AimInput aim;
        private TurnGuide guide;
        private GameObject cameraObject;

        [SetUp]
        public void SetUp()
        {
            rig = new CombatRig();
            cameraObject = new GameObject("GuideCamera", typeof(Camera));
            Camera camera = cameraObject.GetComponent<Camera>();
            director = new CameraDirector();
            director.Init(camera, rig.Config, rig.Level, rig.Flow, rig.Shake);
            aim = new AimInput();
            aim.Init(rig.Config, rig.Flow, rig.Cannons, camera);
            aim.Scripted = true;
            guide = new TurnGuide();
            guide.Init(rig.Config, rig.Flow, aim, rig.Cannons, director);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(cameraObject);
            rig.Destroy();
        }

        private void ToAim(int turn)
        {
            for (int i = 0; i < 64 && !(rig.Flow.State == SessionState.Aim && rig.Flow.ActiveSide == Side.Player && rig.Flow.PlayerTurn == turn); i++)
            {
                rig.Flow.Skip();
            }
            Assert.AreEqual(turn, rig.Flow.PlayerTurn);
        }

        private void Run(float seconds)
        {
            int steps = Mathf.RoundToInt(seconds / Step);
            for (int i = 0; i < steps; i++)
            {
                guide.Tick(Step);
            }
        }

        [Test]
        public void HandAppearsAfterDelayAndDragsTowardFullPower()
        {
            ToAim(1);
            Assert.IsFalse(guide.HandVisible);
            Run(rig.Config.handStartDelay + rig.Config.handDemoDuration * 0.9f);
            Assert.IsTrue(guide.HandVisible);
            Assert.AreEqual(1f, guide.HandDrag, 1e-3f);
            Assert.IsTrue(aim.Showing);
            Assert.AreEqual(rig.Config.GetHint(0).handPower01, aim.ShownPower, 1e-3f);
            Assert.AreEqual(rig.Config.GetHint(0).handAngleDeg, aim.ShownAngle, 1e-3f);
        }

        [Test]
        public void DemoNeverBecomesAnAim()
        {
            ToAim(1);
            Run(rig.Config.handStartDelay + rig.Config.handDemoDuration * 0.5f);
            Assert.IsFalse(rig.Cannons.HasAim(Side.Player));
        }

        [Test]
        public void HandStopsAfterRepeatsAndReturnsOnIdle()
        {
            ToAim(1);
            Run(rig.Config.handStartDelay + rig.Config.handDemoDuration * rig.Config.handDemoRepeats + Step * 2f);
            Assert.IsFalse(guide.HandVisible);
            Assert.IsFalse(aim.Showing);
            Run(rig.Config.handRepeatDelay + Step * 3f);
            Assert.IsTrue(guide.HandVisible);
        }

        [Test]
        public void FreeTurnShowsHandOnlyAfterIdle()
        {
            ToAim(3);
            Assert.IsFalse(rig.Config.GetHint(2).showHand);
            Run(rig.Config.handRepeatDelay - 0.2f);
            Assert.IsFalse(guide.HandVisible);
            Run(0.4f);
            Assert.IsTrue(guide.HandVisible);
        }

        [Test]
        public void GestureRemovesHandAndMountainPreview()
        {
            rig.Config.turnHints[1].panToBooster = true;
            ToAim(2);
            Assert.IsTrue(director.ShowMountain);
            aim.Feed(true, 100f, 500f, ScreenHeight);
            Run(Step);
            Assert.IsFalse(director.ShowMountain);
            Assert.IsFalse(guide.HandVisible);
        }

        [Test]
        public void HintWithPanShowsMountainThenReturns()
        {
            rig.Config.turnHints[1].panToBooster = true;
            ToAim(2);
            Assert.IsTrue(director.ShowMountain);
            Assert.AreEqual(50, guide.HighlightMultiplier);
            Assert.IsFalse(guide.HandVisible);
            Run(rig.Config.hintPanDuration + Step * 2f);
            Assert.IsFalse(director.ShowMountain);
            Assert.AreEqual(50, guide.HighlightMultiplier);
        }

        [Test]
        public void ReturningTurnGoesStraightToOwnCastle()
        {
            ToAim(2);
            Assert.IsFalse(director.ShowMountain);
            ToAim(3);
            Assert.IsFalse(director.ShowMountain);
        }

        [Test]
        public void LeavingAimClearsEverything()
        {
            ToAim(1);
            Run(rig.Config.handStartDelay + rig.Config.handDemoDuration * 0.5f);
            rig.Flow.Skip();
            Assert.AreEqual(SessionState.Fire, rig.Flow.State);
            Assert.AreEqual(0, guide.HighlightMultiplier);
            Assert.IsFalse(guide.HandVisible);
            Assert.IsFalse(aim.Showing);
        }

        [Test]
        public void GuideDoesNotAllocateInATick()
        {
            ToAim(1);
            Run(rig.Config.handStartDelay + 0.1f);
            long before = System.GC.GetAllocatedBytesForCurrentThread();
            guide.Tick(Step);
            Assert.AreEqual(before, System.GC.GetAllocatedBytesForCurrentThread());
        }
    }

    public sealed class HudViewTests
    {
        private const int Segments = 14;

        private GameConfig config;
        private MatchState match;
        private HudView hud;
        private GameObject root;
        private Image[] player;
        private Image[] enemy;
        private RectTransform[] bars;
        private PaletteSet palette;

        private Image Make(Transform parent, string objectName)
        {
            GameObject item = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            return item.GetComponent<Image>();
        }

        private Image[] MakeRow(Transform parent, string prefix)
        {
            Image[] row = new Image[Segments];
            for (int i = 0; i < Segments; i++)
            {
                row[i] = Make(parent, prefix + i);
            }
            return row;
        }

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            match = new MatchState();
            match.Init(3, 12);
            root = new GameObject("Canvas", typeof(RectTransform));
            RectTransform hudRoot = Make(root.transform, "Hud").rectTransform;
            bars = new[] { Make(hudRoot, "BarL").rectTransform, Make(hudRoot, "BarR").rectTransform };
            player = MakeRow(bars[0], "p");
            enemy = MakeRow(bars[1], "e");
            hud = new HudView();
            hud.Init(config, match, hudRoot, bars, player, enemy, new[] { Make(hudRoot, "Panel") }, new[] { Make(hudRoot, "Frame") },
                new[] { Make(hudRoot, "Inner") }, new[] { Make(hudRoot, "IconL"), Make(hudRoot, "IconR") }, Make(hudRoot, "Crossed"));
            palette = ThemePalette.CreateNight();
            hud.ApplyPalette(palette);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SegmentPerCannonAndExtrasAreHidden()
        {
            Assert.IsTrue(player[2].gameObject.activeSelf);
            Assert.IsFalse(player[3].gameObject.activeSelf);
            Assert.IsTrue(enemy[11].gameObject.activeSelf);
            Assert.IsFalse(enemy[12].gameObject.activeSelf);
        }

        [Test]
        public void LostCannonFlashesThenGoesDark()
        {
            match.Destroy(Side.Enemy, 2, 0);
            hud.Tick(0.01f);
            Assert.AreEqual((Color)palette.hudText, enemy[0].color);
            Assert.AreEqual((Color)palette.hudText, enemy[1].color);
            Assert.AreEqual((Color)palette.hudHealth, enemy[2].color);
            hud.Tick(config.hudFlashTime + 0.01f);
            Assert.AreNotEqual((Color)palette.hudText, enemy[0].color);
            Assert.AreNotEqual((Color)palette.hudHealth, enemy[0].color);
        }

        [Test]
        public void PlayerBarGoesOutFromTheCenterSide()
        {
            match.Destroy(Side.Player, 1, 0);
            hud.Tick(config.hudFlashTime + 0.01f);
            hud.Tick(0.01f);
            Assert.AreEqual((Color)palette.hudHealth, player[0].color);
            Assert.AreEqual((Color)palette.hudHealth, player[1].color);
            Assert.AreNotEqual((Color)palette.hudHealth, player[2].color);
        }

        [Test]
        public void LossShakesItsBarAndItSettles()
        {
            match.Destroy(Side.Enemy, 1, 0);
            hud.Tick(0.01f);
            hud.Tick(0.01f);
            Assert.AreNotEqual(0f, bars[1].localPosition.x);
            Assert.AreEqual(0f, bars[0].localPosition.x);
            hud.Tick(config.hudShakeTime + 0.01f);
            Assert.AreEqual(0f, bars[1].localPosition.x);
        }

        [Test]
        public void RestoringMatchDoesNotFlash()
        {
            match.Destroy(Side.Enemy, 3, 0);
            hud.Tick(config.hudFlashTime + 0.01f);
            match.Reset();
            hud.Tick(0.01f);
            Assert.AreEqual((Color)palette.hudHealth, enemy[0].color);
        }
    }

    public sealed class BannerAndEndCardTests
    {
        private const float ScreenHeight = 1000f;
        private const float UpperScreen = 700f;

        private GameConfig config;
        private MatchState match;
        private SessionFlow flow;
        private TurnBannerView banner;
        private EndCardView endCard;
        private Image bannerImage;
        private Text bannerText;
        private GameObject root;
        private GameObject endRoot;
        private Text caption;

        private GameObject MakeObject(string objectName, Transform parent)
        {
            GameObject item = new GameObject(objectName, typeof(RectTransform));
            item.transform.SetParent(parent, false);
            return item;
        }

        private Image MakeImage(Transform parent)
        {
            return MakeObject("Image", parent).AddComponent<Image>();
        }

        private Text MakeText(Transform parent)
        {
            return MakeObject("Text", parent).AddComponent<Text>();
        }

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            match = new MatchState();
            match.Init(config.playerCannons, config.enemyCannons);
            flow = new SessionFlow();
            flow.Init(config, match);
            root = new GameObject("Canvas", typeof(RectTransform));

            bannerImage = MakeImage(root.transform);
            bannerText = MakeText(bannerImage.transform);
            banner = new TurnBannerView();
            banner.Init(config, flow, bannerImage.rectTransform, bannerImage, bannerText);

            endRoot = MakeObject("End", root.transform);
            caption = MakeText(endRoot.transform);
            endCard = new EndCardView();
            endCard.Init(config, flow, endRoot, MakeImage(endRoot.transform), MakeObject("Content", endRoot.transform).GetComponent<RectTransform>(),
                MakeObject("Button", endRoot.transform).GetComponent<RectTransform>(), MakeImage(endRoot.transform), caption,
                MakeImage(endRoot.transform), MakeImage(endRoot.transform), MakeText(endRoot.transform), MakeImage(endRoot.transform), null, null, new RectTransform[0]);
            endCard.Scripted = true;
            PaletteSet palette = ThemePalette.CreateNight();
            banner.ApplyPalette(palette);
            endCard.ApplyPalette(palette);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(config);
        }

        private void To(SessionState state, Side side)
        {
            for (int i = 0; i < 64 && !(flow.State == state && flow.ActiveSide == side); i++)
            {
                flow.Skip();
            }
            Assert.AreEqual(state, flow.State);
        }

        private void ToEndCard()
        {
            for (int i = 0; i < 64 && !flow.IsFinished; i++)
            {
                flow.Skip();
            }
            Assert.IsTrue(flow.IsFinished);
        }

        [Test]
        public void BannerAnnouncesEachTurnAndHides()
        {
            To(SessionState.Aim, Side.Player);
            Assert.AreEqual(TurnBannerView.YourTurn, bannerText.text);
            Assert.IsTrue(bannerImage.gameObject.activeSelf);
            banner.Tick(config.bannerDuration + 0.01f);
            Assert.IsFalse(bannerImage.gameObject.activeSelf);
            To(SessionState.Aim, Side.Enemy);
            Assert.AreEqual(TurnBannerView.EnemyTurn, bannerText.text);
        }

        [Test]
        public void VictoryBannerWaitsForTheCameraToReachTheWinner()
        {
            To(SessionState.Resolve, Side.Player);
            banner.Tick(config.bannerDuration + 0.01f);
            match.Destroy(Side.Enemy, config.enemyCannons, 0);
            To(SessionState.Victory, Side.Player);
            banner.Tick(0.01f);
            Assert.IsFalse(bannerImage.gameObject.activeSelf);
            flow.Tick(config.victoryLoserHold + 0.01f);
            banner.Tick(0.01f);
            Assert.IsTrue(bannerImage.gameObject.activeSelf);
            Assert.AreEqual(TurnBannerView.Victory, bannerText.text);
        }

        [Test]
        public void EndCardOpensOnlyAtTheEnd()
        {
            To(SessionState.Fire, Side.Player);
            Assert.IsFalse(endRoot.activeSelf);
            ToEndCard();
            Assert.IsTrue(endRoot.activeSelf);
            flow.Restart();
            Assert.IsFalse(endRoot.activeSelf);
        }

        [Test]
        public void EarlyTapAndLeftoverFingerDoNotOpenStore()
        {
            ToEndCard();
            endCard.Tick(0.01f);
            endCard.Feed(true, UpperScreen, ScreenHeight);
            Assert.AreEqual(0, endCard.CtaCount);
            endCard.Tick(config.endCardTapDelay + 0.1f);
            endCard.Feed(true, UpperScreen, ScreenHeight);
            Assert.AreEqual(0, endCard.CtaCount, "finger was already down");
            endCard.Feed(false, UpperScreen, ScreenHeight);
            endCard.Feed(true, UpperScreen, ScreenHeight);
            Assert.AreEqual(1, endCard.CtaCount);
            endCard.Feed(true, UpperScreen, ScreenHeight);
            Assert.AreEqual(1, endCard.CtaCount, "one press is one click");
        }

        [Test]
        public void DefeatCaptionInvitesToTheFullGame()
        {
            config.directorEnabled = false;
            To(SessionState.Resolve, Side.Enemy);
            match.Destroy(Side.Player, config.playerCannons, 0);
            ToEndCard();
            Assert.AreEqual(EndReason.Defeat, flow.EndReason);
            Assert.AreEqual(EndCardView.TryAgain, caption.text);
        }

        [Test]
        public void VictoryHasNoCaption()
        {
            ToEndCard();
            Assert.AreEqual(EndReason.Victory, flow.EndReason);
            Assert.AreEqual("", caption.text);
        }
    }
}
