using System;
using CastleDestroyers.Destruction;
using UnityEngine;

namespace CastleDestroyers.Core
{
    public enum ThemePreset
    {
        Night,
        Sunset
    }

    /// <summary>Набор цветов одной темы. Все view берут цвета отсюда, в коде и шейдерах цветов нет.</summary>
    [Serializable]
    public sealed class PaletteSet
    {
        [Header("Фон: ступени светлоты, дальше = светлее (небо Night тёмное, D-023)")]
        public Color32 sky;
        public Color32 backgroundFar;
        public Color32 backgroundFarShade;
        public Color32 backgroundMid;
        public Color32 backgroundNear;
        public Color32 backgroundNearLight;
        public Color32 skyStar;
        public Color32 moon;
        public Color32 moonShade;

        [Header("Ландшафт")]
        public Color32 ground;
        public Color32 groundShade;
        public Color32 groundBuried;
        public Color32 groundBand;
        public Color32 groundRim;
        public Color32 groundRimLight;
        public Color32 groundCrater;
        public Color32 bush;
        public Color32 bushLight;
        public Color32 treeLight;
        public Color32 treeShade;
        public Color32 water;
        public Color32 waterHighlight;
        public Color32 waterRim;

        [Header("Замки и пушки")]
        public Color32 castleStone;
        public Color32 castleStoneShade;
        public Color32 castleBrick;
        public Color32 castleCrack;
        public Color32 castleWindow;
        public Color32 castleWindowLit;
        public Color32 roofPlayer;
        public Color32 roofPlayerShade;
        public Color32 roofEnemy;
        public Color32 roofEnemyShade;
        public Color32 cannonPlayer;
        public Color32 cannonPlayerShade;
        public Color32 cannonEnemy;
        public Color32 cannonEnemyShade;
        public Color32 platform;
        public Color32 platformShade;
        public Color32 track;
        public Color32 trackWheel;

        [Header("Бустеры")]
        public Color32 booster10;
        public Color32 booster10Cap;
        public Color32 booster20;
        public Color32 booster20Cap;
        public Color32 booster50;
        public Color32 booster50Cap;

        [Header("Ракеты и дым")]
        public Color32 rocketBody;
        public Color32 rocketShade;
        public Color32 rocketPlayer;
        public Color32 rocketEnemy;
        public Color32 rocketTail;
        public Color32 smoke;
        public Color32 blastCore;
        public Color32 blastMid;
        public Color32 blastEdge;

        [Header("HUD (общий для тем)")]
        public Color32 hudPanel;
        public Color32 hudTrack;
        public Color32 hudHealth;
        public Color32 hudHealthShade;
        public Color32 hudGold;
        public Color32 hudGoldShade;
        public Color32 hudText;
    }

    /// <summary>Текстуры темы, их собирает Tools/build_art.py. Раскладка атласа общая (ArtSprites).</summary>
    [Serializable]
    public sealed class ThemeArt
    {
        public Texture2D atlas;
        public Texture2D castleBlue;
        public Texture2D castleRed;
        public Texture2D backFar;
        public Texture2D backMid;
        public Texture2D backNear;
        [Tooltip("Кромка и декоры земли этой темы; форма уровня общая.")]
        public TerrainMaskData terrain;
    }

    /// <summary>Палитра с пресетами Night / Sunset (ТЗ §4.2): HEX сняты пипеткой со скриншотов Bowmasters (D-054).</summary>
    [CreateAssetMenu(fileName = "ThemePalette", menuName = "Castle Destroyers/Theme Palette")]
    public sealed class ThemePalette : ScriptableObject
    {
        private const int ByteMask = 0xFF;
        private const int RedShift = 16;
        private const int GreenShift = 8;

        [Tooltip("Активный пресет. Переключается и в рантайме из debug-панели.")]
        public ThemePreset preset = ThemePreset.Night;
        public PaletteSet night = CreateNight();
        public PaletteSet sunset = CreateSunset();
        public ThemeArt nightArt = new ThemeArt();
        public ThemeArt sunsetArt = new ThemeArt();

        public PaletteSet Current
        {
            get { return Get(preset); }
        }

        public PaletteSet Get(ThemePreset requested)
        {
            return requested == ThemePreset.Night ? night : sunset;
        }

        public ThemeArt ArtOf(ThemePreset requested)
        {
            return requested == ThemePreset.Night ? nightArt : sunsetArt;
        }

        /// <summary>Возвращает пресеты к значениям по умолчанию.</summary>
        public void ResetPresets()
        {
            night = CreateNight();
            sunset = CreateSunset();
        }

        public static Color32 Hex(int rgb)
        {
            return new Color32(
                (byte)((rgb >> RedShift) & ByteMask),
                (byte)((rgb >> GreenShift) & ByteMask),
                (byte)(rgb & ByteMask),
                255);
        }

        public static PaletteSet CreateNight()
        {
            PaletteSet set = new PaletteSet();
            set.sky = Hex(0x192DA0);
            set.backgroundFar = Hex(0x5974C3);
            set.backgroundFarShade = Hex(0x404AA1);
            set.backgroundMid = Hex(0x043987);
            set.backgroundNear = Hex(0x513D77);
            set.backgroundNearLight = Hex(0x795586);
            set.skyStar = Hex(0xDCE3FF);
            set.moon = Hex(0xF5EBD2);
            set.moonShade = Hex(0xDCCFB4);
            set.ground = Hex(0x00A988);
            set.groundShade = Hex(0x00997C);
            set.groundBuried = Hex(0x008F75);
            set.groundBand = Hex(0x31E18C);
            set.groundRim = Hex(0x31E18C);
            set.groundRimLight = Hex(0x77E9A6);
            set.groundCrater = Hex(0x007F6C);
            set.bush = Hex(0x52E48F);
            set.bushLight = Hex(0x77E9A6);
            set.treeLight = Hex(0x14B98C);
            set.treeShade = Hex(0x069B80);
            set.water = Hex(0x1FB6E8);
            set.waterHighlight = Hex(0x4CCDF0);
            set.waterRim = Hex(0xB5F1FA);
            FillShared(set);
            set.roofPlayer = Hex(0x3CC7E0);
            set.roofPlayerShade = Hex(0x2AA0BC);
            set.roofEnemy = Hex(0xE23B52);
            set.roofEnemyShade = Hex(0xB82A40);
            set.platform = Hex(0x8B5E3C);
            set.platformShade = Hex(0x6E4529);
            set.smoke = Hex(0xB9BCE0);
            return set;
        }

        public static PaletteSet CreateSunset()
        {
            PaletteSet set = new PaletteSet();
            set.sky = Hex(0xFDB649);
            set.backgroundFar = Hex(0xFC9076);
            set.backgroundFarShade = Hex(0xFF7C80);
            set.backgroundMid = Hex(0xF17A58);
            set.backgroundNear = Hex(0xE96344);
            set.backgroundNearLight = Hex(0xEA483B);
            set.skyStar = Hex(0xFFE3D3);
            set.moon = Hex(0xFFE36E);
            set.moonShade = Hex(0xFECC13);
            set.ground = Hex(0xF65157);
            set.groundShade = Hex(0xF14448);
            set.groundBuried = Hex(0xFF656B);
            set.groundBand = Hex(0xF14448);
            set.groundRim = Hex(0xC81A1D);
            set.groundRimLight = Hex(0xDE2E2C);
            set.groundCrater = Hex(0xC92F3A);
            set.bush = Hex(0x8FB04E);
            set.bushLight = Hex(0xDBE0AF);
            set.treeLight = Hex(0x8FB04E);
            set.treeShade = Hex(0x6C8C3A);
            set.water = Hex(0x3A86E8);
            set.waterHighlight = Hex(0x69C1E8);
            set.waterRim = Hex(0xC4E6FF);
            FillShared(set);
            set.roofPlayer = Hex(0x3CA8E8);
            set.roofPlayerShade = Hex(0x2A86C2);
            set.roofEnemy = Hex(0x8E1B3A);
            set.roofEnemyShade = Hex(0x6E1330);
            set.platform = Hex(0x6B3F2A);
            set.platformShade = Hex(0x52301F);
            set.smoke = Hex(0xF5D0C0);
            return set;
        }

        /// <summary>Цвета, одинаковые в обеих темах: камень, пушки, бустеры, ракеты, HUD.</summary>
        private static void FillShared(PaletteSet set)
        {
            set.castleStone = Hex(0xEDE6D6);
            set.castleStoneShade = Hex(0xCFC6BE);
            set.castleBrick = Hex(0xDCD3C5);
            set.castleCrack = Hex(0x6B5F78);
            set.castleWindow = Hex(0x3A3358);
            set.castleWindowLit = Hex(0xFFD27A);
            set.cannonPlayer = Hex(0x2E7BEA);
            set.cannonPlayerShade = Hex(0x1B56AD);
            set.cannonEnemy = Hex(0xD8262F);
            set.cannonEnemyShade = Hex(0x9E1620);
            set.track = Hex(0x2B2640);
            set.trackWheel = Hex(0x8E89A8);
            set.booster10 = Hex(0x3C82F6);
            set.booster10Cap = Hex(0x2460C8);
            set.booster20 = Hex(0xFFB02E);
            set.booster20Cap = Hex(0xE8860F);
            set.booster50 = Hex(0x3FD072);
            set.booster50Cap = Hex(0x27A854);
            set.rocketBody = Hex(0xF2F0F8);
            set.rocketShade = Hex(0xB9B6D2);
            set.rocketPlayer = Hex(0x2F8CF0);
            set.rocketEnemy = Hex(0xFF4D5E);
            set.rocketTail = Hex(0xFFB22E);
            set.blastCore = Hex(0xFFFFFF);
            set.blastMid = Hex(0xFFD84A);
            set.blastEdge = Hex(0xFF8A2B);
            set.hudPanel = Hex(0x813A86);
            set.hudTrack = Hex(0x540D59);
            set.hudHealth = Hex(0xFC2E5C);
            set.hudHealthShade = Hex(0xCE214C);
            set.hudGold = Hex(0xFDDA4E);
            set.hudGoldShade = Hex(0xE8A92C);
            set.hudText = Hex(0xFFFFFF);
        }
    }
}
