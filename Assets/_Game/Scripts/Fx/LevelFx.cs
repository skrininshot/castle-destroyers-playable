using CastleDestroyers.Core;
using UnityEngine;

namespace CastleDestroyers.Fx
{
    /// <summary>Пыль и всплески в плоском стиле (ТЗ §3.6, §3.8) поверх общего пула частиц.</summary>
    public sealed class LevelFx
    {
        private const float FullTurn = Mathf.PI * 2f;
        private const float ScatterFloor = 0.2f;
        private const float AccentChance = 0.5f;
        private const float DustLifeFloor = 0.5f;
        private const float SplashScatterFloor = 0.4f;
        private const float SplashLifeFloor = 0.6f;

        private GameConfig config;
        private FlatParticles particles;
        private Rng rng;
        private Color32 dustColor;
        private Color32 dustAccent;
        private Color32 stoneColor;
        private Color32 stoneAccent;
        private Color32 splashColor;
        private Color32 splashAccent;

        public void Init(GameConfig gameConfig, FlatParticles flatParticles, Rng random)
        {
            config = gameConfig;
            particles = flatParticles;
            rng = random;
        }

        public void ApplyPalette(PaletteSet palette)
        {
            dustColor = palette.groundCrater;
            dustAccent = palette.smoke;
            stoneColor = palette.castleStone;
            stoneAccent = palette.castleStoneShade;
            splashColor = palette.waterRim;
            splashAccent = palette.waterHighlight;
        }

        /// <summary>Облачко пыли и комьев в круге радиуса spread.</summary>
        public void Dust(float x, float y, float spread, int puffs)
        {
            Puffs(x, y, spread, puffs, dustColor, dustAccent);
        }

        /// <summary>То же в цветах камня: разрушение замка.</summary>
        public void StoneDust(float x, float y, float spread, int puffs)
        {
            Puffs(x, y, spread, puffs, stoneColor, stoneAccent);
        }

        private void Puffs(float x, float y, float spread, int puffs, Color32 color, Color32 accentColor)
        {
            for (int i = 0; i < puffs; i++)
            {
                float angle = rng.Range(0f, FullTurn);
                float reach = spread * rng.NextFloat();
                float directionX = Mathf.Cos(angle);
                float directionY = Mathf.Sin(angle);
                float speed = config.dustSpeed * rng.Range(ScatterFloor, 1f);
                float size = config.dustSize * rng.Range(ScatterFloor, 1f);
                bool accent = rng.Chance(AccentChance);
                particles.Spawn(x + directionX * reach, y + directionY * reach, directionX * speed, directionY * speed, size,
                    config.dustLife * rng.Range(DustLifeFloor, 1f), accent ? 0f : config.dustGravity, accent ? accentColor : color);
            }
        }

        /// <summary>Всплеск на воде шириной width: капли летят вверх и падают обратно.</summary>
        public void Splash(float x, float waterY, float width)
        {
            int drops = config.splashDrops + (int)(width * config.splashDropsPerMeter);
            float half = width * 0.5f;
            for (int i = 0; i < drops; i++)
            {
                float offset = rng.Range(-half, half);
                float speedY = config.splashSpeed * rng.Range(SplashScatterFloor, 1f);
                float speedX = rng.Range(-config.splashSpread, config.splashSpread);
                float size = config.splashDropSize * rng.Range(SplashScatterFloor, 1f);
                particles.Spawn(x + offset, waterY, speedX, speedY, size, config.splashLife * rng.Range(SplashLifeFloor, 1f), config.splashGravity,
                    rng.Chance(AccentChance) ? splashColor : splashAccent);
            }
        }
    }
}
