using System;
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
        [Header("Фон: ступени светлоты, дальше = светлее")]
        public Color32 sky;
        public Color32 backgroundFar;
        public Color32 backgroundMid;
        public Color32 backgroundNear;
        public Color32 skyStar;
        public Color32 moon;
        public Color32 moonShade;

        [Header("Ландшафт")]
        public Color32 ground;
        public Color32 groundShade;
        public Color32 groundRim;
        public Color32 treeLight;
        public Color32 treeShade;
        public Color32 water;
        public Color32 waterHighlight;
        public Color32 waterRim;

        [Header("Замки и пушки")]
        public Color32 castleStone;
        public Color32 castleStoneShade;
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
        public Color32 rocketPlayer;
        public Color32 rocketEnemy;
        public Color32 rocketTail;
        public Color32 smoke;

        [Header("HUD (общий для тем)")]
        public Color32 hudPanel;
        public Color32 hudHealth;
        public Color32 hudGold;
        public Color32 hudText;
    }

    /// <summary>Палитра с пресетами Night / Sunset (ТЗ §4.2). HEX уточняются на этапе 7 по скриншотам.</summary>
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

        public PaletteSet Current
        {
            get { return Get(preset); }
        }

        public PaletteSet Get(ThemePreset requested)
        {
            return requested == ThemePreset.Night ? night : sunset;
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
            set.sky = Hex(0x5C6FC2);
            set.backgroundFar = Hex(0x4C5CAE);
            set.backgroundMid = Hex(0x373A8A);
            set.backgroundNear = Hex(0x1E1F5C);
            set.skyStar = Hex(0xDCE3FF);
            set.moon = Hex(0xF2EFD2);
            set.moonShade = Hex(0xD6D2B0);
            set.ground = Hex(0x1F6E4C);
            set.groundShade = Hex(0x1A5E41);
            set.groundRim = Hex(0x3FE07A);
            set.treeLight = Hex(0x2C9C6B);
            set.treeShade = Hex(0x1F7A55);
            set.water = Hex(0x2AA7A8);
            set.waterHighlight = Hex(0x45BFBD);
            set.waterRim = Hex(0x8CEDE4);
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
            set.sky = Hex(0xFFC4A8);
            set.backgroundFar = Hex(0xFFA597);
            set.backgroundMid = Hex(0xE8727F);
            set.backgroundNear = Hex(0xA33A5E);
            set.skyStar = Hex(0xFFE3D3);
            set.moon = Hex(0xFFF1C9);
            set.moonShade = Hex(0xFFD79A);
            set.ground = Hex(0xB8303A);
            set.groundShade = Hex(0xA32832);
            set.groundRim = Hex(0xFF6B57);
            set.treeLight = Hex(0x8FB04E);
            set.treeShade = Hex(0x6C8C3A);
            set.water = Hex(0x2E70D8);
            set.waterHighlight = Hex(0x4B8BE6);
            set.waterRim = Hex(0x93C4FF);
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
            set.castleStone = Hex(0xD7D3E6);
            set.castleStoneShade = Hex(0xA9A4C4);
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
            set.rocketPlayer = Hex(0x7FE6FF);
            set.rocketEnemy = Hex(0xFF4D5E);
            set.rocketTail = Hex(0xFFB22E);
            set.hudPanel = Hex(0x4B2463);
            set.hudHealth = Hex(0xF0335A);
            set.hudGold = Hex(0xF5C443);
            set.hudText = Hex(0xFFFFFF);
        }
    }
}
