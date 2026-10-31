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
        private const int FireworkStarStep = 2;
        private const float FireworkStarSize = 2.2f;

        private GameConfig config;
        private FlatParticles particles;
        private Rng rng;
        private Color32 dustColor;
        private Color32 dustAccent;
        private Color32 stoneColor;
        private Color32 stoneAccent;
        private Color32 splashColor;
        private Color32 splashAccent;
        private Color32[] sparkColors;

        /// <summary>Звуки эффектов. В тестах не задан.</summary>
        public Platform.GameSounds Sounds { get; set; }

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
            sparkColors = new[] { palette.hudGold, palette.hudHealth, palette.hudText };
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

        /// <summary>Залп фейерверка: плоские искры кольцом, каждый залп своего цвета с белыми вкраплениями (ТЗ §3.11).</summary>
        public void Firework(float x, float y)
        {
            Color32 main = sparkColors[rng.Range(0, sparkColors.Length)];
            int count = config.fireworkSparks;
            for (int i = 0; i < count; i++)
            {
                float angle = FullTurn * i / count;
                float speed = config.fireworkSpeed * rng.Range(SplashScatterFloor, 1f);
                float life = config.fireworkLife * rng.Range(SplashLifeFloor, 1f);
                Color32 color = rng.Chance(AccentChance) ? main : sparkColors[sparkColors.Length - 1];
                if (i % FireworkStarStep == 0)
                {
                    particles.SpawnStar(x, y, Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, config.fireworkSparkSize * FireworkStarSize, life, config.fireworkGravity, color);
                }
                else
                {
                    particles.Spawn(x, y, Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, config.fireworkSparkSize, life, config.fireworkGravity, color);
                }
            }
        }

        public void Play(Platform.SoundId sound)
        {
            if (Sounds != null)
            {
                Sounds.Play(sound);
            }
        }

        /// <summary>Всплеск на воде шириной width: капли летят вверх и падают обратно.</summary>
        public void Splash(float x, float waterY, float width)
        {
            Play(Platform.SoundId.Splash);
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
