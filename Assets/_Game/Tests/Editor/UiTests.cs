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
            Assert.AreEqual(20, guide.HighlightMultiplier);
            Assert.IsFalse(guide.HandVisible);
            Run(rig.Config.hintPanDuration + Step * 2f);
            Assert.IsFalse(director.ShowMountain);
            Assert.AreEqual(20, guide.HighlightMultiplier);
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
        private const int PlayerCannons = 3;
        private const int EnemyCannons = 12;

        private GameConfig config;
        private MatchState match;
        private HudView hud;
        private GameObject canvasObject;
        private GameObject meshObject;
        private GameObject cameraObject;
        private Camera camera;
        private ScreenShake shake;
        private PaletteSet palette;

        private Color32 Segment(Side side, int index)
        {
            return hud.SegmentColor(side, index);
        }

        private static void AssertColor(Color32 expected, Color32 actual)
        {
            Assert.AreEqual((Color)expected, (Color)actual);
        }

        private static void AssertNotColor(Color32 expected, Color32 actual)
        {
            Assert.AreNotEqual((Color)expected, (Color)actual);
        }

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            match = new MatchState();
            match.Init(PlayerCannons, EnemyCannons);
            canvasObject = new GameObject("Canvas", typeof(RectTransform));
            canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(720f, 1280f);
            meshObject = new GameObject("Hud");
            cameraObject = new GameObject("Camera");
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 10f;
            shake = new ScreenShake();
            shake.Init(config);
            hud = new HudView();
            hud.Init(config, match, camera, shake, canvasObject.GetComponent<RectTransform>(), meshObject.transform);
            palette = ThemePalette.CreateNight();
            hud.ApplyPalette(palette);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(hud.Mesh);
            Object.DestroyImmediate(canvasObject);
            Object.DestroyImmediate(meshObject);
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void SegmentPerCannon()
        {
            Assert.AreEqual(PlayerCannons, hud.SegmentCount(Side.Player));
            Assert.AreEqual(EnemyCannons, hud.SegmentCount(Side.Enemy));
        }

        [Test]
        public void SegmentsFillTheBarWithoutOverlap()
        {
            float previousMax = float.MinValue;
            for (int i = 0; i < EnemyCannons; i++)
            {
                float minX;
                float maxX;
                HudView.SegmentBounds(Side.Enemy, i, EnemyCannons, out minX, out maxX);
                Assert.Less(minX, maxX);
                Assert.Greater(minX, previousMax);
                previousMax = maxX;
            }
        }

        [Test]
        public void AimPanelStaysBelowHudInBothOrientations()
        {
            const float PanelHeight = 110f;
            const float Gap = 12f;
            float[] widths = { 720f, 1280f };
            float[] heights = { 1280f, 720f };
            for (int i = 0; i < widths.Length; i++)
            {
                float panelTop = AimPanelView.TopAnchor(widths[i], heights[i], PanelHeight, config.hudLandscapeScale, Gap) + PanelHeight / heights[i];
                Assert.Less(panelTop, HudView.BottomViewport(widths[i], heights[i], config.hudLandscapeScale));
                Assert.Greater(panelTop, 0.4f);
            }
        }

        [Test]
        public void EverythingIsOneMeshAboveTheSafeTopBand()
        {
            hud.Tick(0f);
            Assert.AreEqual(1, hud.Mesh.subMeshCount);
            Assert.Greater(hud.Mesh.vertexCount, 0);
            Assert.AreEqual(camera.orthographicSize * 0.8f, meshObject.transform.position.y, 0.001f);
        }

        [Test]
        public void HudDoesNotFollowCameraShake()
        {
            hud.Tick(0f);
            float restY = meshObject.transform.position.y;
            shake.Add(1f);
            shake.Tick(0.05f);
            cameraObject.transform.position += new Vector3(shake.OffsetX, shake.OffsetY, 0f);
            hud.Tick(0f);
            Assert.AreEqual(restY, meshObject.transform.position.y, 0.001f);
            Assert.AreEqual(0f, meshObject.transform.position.x, 0.001f);
        }

        [Test]
        public void LostCannonFlashesThenGoesDark()
        {
            match.Destroy(Side.Enemy, 2, 0);
            hud.Tick(0.01f);
            AssertColor(palette.hudText, Segment(Side.Enemy, 0));
            AssertColor(palette.hudText, Segment(Side.Enemy, 1));
            AssertColor(palette.hudHealth, Segment(Side.Enemy, 2));
            hud.Tick(config.hudFlashTime + 0.01f);
            AssertNotColor(palette.hudText, Segment(Side.Enemy, 0));
            AssertNotColor(palette.hudHealth, Segment(Side.Enemy, 0));
        }

        [Test]
        public void PlayerBarGoesOutFromTheCenterSide()
        {
            match.Destroy(Side.Player, 1, 0);
            hud.Tick(config.hudFlashTime + 0.01f);
            hud.Tick(0.01f);
            AssertColor(palette.hudHealth, Segment(Side.Player, 0));
            AssertColor(palette.hudHealth, Segment(Side.Player, 1));
            AssertNotColor(palette.hudHealth, Segment(Side.Player, 2));
        }

        [Test]
        public void LossShakesItsBarAndItSettles()
        {
            match.Destroy(Side.Enemy, 1, 0);
            hud.Tick(0.01f);
            hud.Tick(0.01f);
            Assert.AreNotEqual(0f, hud.BarShift(Side.Enemy));
            Assert.AreEqual(0f, hud.BarShift(Side.Player));
            hud.Tick(config.hudShakeTime + 0.01f);
            Assert.AreEqual(0f, hud.BarShift(Side.Enemy));
        }

        [Test]
        public void RestoringMatchDoesNotFlash()
        {
            match.Destroy(Side.Enemy, 3, 0);
            hud.Tick(config.hudFlashTime + 0.01f);
            match.Reset();
            hud.Tick(0.01f);
            AssertColor(palette.hudHealth, Segment(Side.Enemy, 0));
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
