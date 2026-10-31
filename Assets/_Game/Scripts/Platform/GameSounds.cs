using CastleDestroyers.Core;
using CastleDestroyers.InputRig;
using UnityEngine;

namespace CastleDestroyers.Platform
{
    public enum SoundId
    {
        Shot,
        ExplosionA,
        ExplosionB,
        Booster,
        Splash,
        Victory,
        Click,
        Blast,
        Star
    }

    /// <summary>
    /// Звуки (ТЗ §3.13): молчат до первого тапа, глушатся при потере видимости. Одинаковые звуки разделены паузой soundCooldown:
    /// сотни взрывов залпа не должны забивать источники. Клипы лежат в порядке SoundId.
    /// </summary>
    public sealed class GameSounds : ITickable
    {
        private readonly PointerReader pointer = new PointerReader();
        private GameConfig config;
        private AudioClip[] clips;
        private AudioSource[] sources;
        private float[] nextTime;
        private float time;
        private int nextSource;

        /// <summary>Игрок уже коснулся экрана.</summary>
        public bool Unlocked { get; private set; }
        public bool Muted { get; private set; }

        public void Init(GameConfig gameConfig, AudioClip[] soundClips, AudioSource[] audioSources)
        {
            config = gameConfig;
            clips = soundClips;
            sources = audioSources;
            nextTime = new float[clips.Length];
            AudioListener.volume = config.soundVolume;
        }

        public void Tick(float dt)
        {
            time += dt;
            if (!Unlocked)
            {
                pointer.Read();
                Unlocked = pointer.Pressed;
            }
        }

        public void Play(SoundId sound)
        {
            int index = (int)sound;
            if (!Unlocked || Muted || time < nextTime[index])
            {
                return;
            }
            nextTime[index] = time + config.soundCooldown;
            sources[nextSource].PlayOneShot(clips[index]);
            nextSource = (nextSource + 1) % sources.Length;
        }

        /// <summary>Потеря и возврат видимости.</summary>
        public void SetMuted(bool muted)
        {
            Muted = muted;
            AudioListener.volume = muted ? 0f : config.soundVolume;
        }
    }
}
