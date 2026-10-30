using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Пушки (ТЗ §3.3): синхронный поворот стороны, залп, чередование стволов, отдача и вспышка.
    /// Пушки индексируются сквозным номером: сначала все синие, затем красные. Живы первые alive креплений
    /// стороны, отрыв конкретной пушки появится на этапе 5.
    /// </summary>
    public sealed class CannonSystem : ITickable
    {
        public const int BarrelCount = 3;
        public const float BarrelLength = 1.25f;
        public const float BarrelPitch = 0.21f;

        private const int SideCount = 2;

        // Порядок стволов: верхний, нижний, средний. Значение — смещение от оси в шагах BarrelPitch.
        private static readonly int[] BarrelOrder = { 1, -1, 0 };

        private GameConfig config;
        private GreyboxLevel level;
        private MatchState match;
        private RocketSystem rockets;
        private FlatParticles particles;
        private int playerCount;
        private int enemyCount;

        private float[] angleDeg;
        private float[] targetDeg;
        private float[] power;
        private bool[] hasAim;
        private bool[] firing;
        private float[] salvoTime;
        private int[] shots;
        private float[] recoil;
        private Color32 flashColor;

        public int TotalCannons { get { return playerCount + enemyCount; } }
        /// <summary>Положение стволов менялось в этом кадре: виду пора перестроить меш.</summary>
        public bool Moved { get; private set; }
        public bool IsFiring { get { return firing[0] || firing[1]; } }

        public void Init(GameConfig gameConfig, GreyboxLevel greyboxLevel, MatchState matchState, RocketSystem rocketSystem, FlatParticles flatParticles)
        {
            config = gameConfig;
            level = greyboxLevel;
            match = matchState;
            rockets = rocketSystem;
            particles = flatParticles;
            playerCount = level.PlayerMountX.Length;
            enemyCount = level.EnemyMountX.Length;

            angleDeg = new float[SideCount];
            targetDeg = new float[SideCount];
            power = new float[SideCount];
            hasAim = new bool[SideCount];
            firing = new bool[SideCount];
            salvoTime = new float[SideCount];
            shots = new int[TotalCannons];
            recoil = new float[TotalCannons * BarrelCount];
        }

        public void ApplyPalette(PaletteSet palette)
        {
            flashColor = palette.blastMid;
        }

        public float AngleDeg(Side side)
        {
            return angleDeg[(int)side];
        }

        public bool HasAim(Side side)
        {
            return hasAim[(int)side];
        }

        /// <summary>Откат ствола, метры.</summary>
        public float Recoil(int cannon, int barrel)
        {
            return recoil[cannon * BarrelCount + barrel];
        }

        /// <summary>Смещение ствола от оси пушки в порядке рисования: нижний, средний, верхний.</summary>
        public static float BarrelOffset(int barrel)
        {
            return (barrel - 1) * BarrelPitch;
        }

        /// <summary>Задаёт прицел стороны: угол над горизонтом в сторону врага и сила 0..1.</summary>
        public void SetAim(Side side, float degrees, float power01)
        {
            int s = (int)side;
            targetDeg[s] = degrees;
            power[s] = power01;
            hasAim[s] = true;
        }

        public void ClearAim(Side side)
        {
            hasAim[(int)side] = false;
        }

        /// <summary>Начинает залп живых пушек по текущему прицелу.</summary>
        public void BeginSalvo(Side side)
        {
            int s = (int)side;
            firing[s] = true;
            salvoTime[s] = 0f;
            int first = FirstCannon(side);
            int count = CountOf(side);
            for (int cannon = first; cannon < first + count; cannon++)
            {
                shots[cannon] = 0;
            }
        }

        public void Clear()
        {
            for (int s = 0; s < SideCount; s++)
            {
                angleDeg[s] = 0f;
                targetDeg[s] = 0f;
                hasAim[s] = false;
                firing[s] = false;
            }
            System.Array.Clear(recoil, 0, recoil.Length);
            Moved = true;
        }

        /// <summary>X средней точки вылета живых пушек при текущем прицеле: начало пунктира.</summary>
        public float MuzzleCenterX(Side side)
        {
            float[] mounts = MountsX(side);
            float direction = Facing(side) * Mathf.Cos(targetDeg[(int)side] * Mathf.Deg2Rad);
            return Mean(mounts, match.Alive(side)) + direction * BarrelLength;
        }

        /// <summary>Y средней точки вылета живых пушек при текущем прицеле.</summary>
        public float MuzzleCenterY(Side side)
        {
            float[] mounts = MountsY(side);
            return Mean(mounts, match.Alive(side)) + Mathf.Sin(targetDeg[(int)side] * Mathf.Deg2Rad) * BarrelLength;
        }

        public void Tick(float dt)
        {
            Moved = false;
            Turn((int)Side.Player, dt);
            Turn((int)Side.Enemy, dt);
            DecayRecoil(dt);
            if (firing[(int)Side.Player])
            {
                TickSalvo(Side.Player, dt);
            }
            if (firing[(int)Side.Enemy])
            {
                TickSalvo(Side.Enemy, dt);
            }
        }

        private void Turn(int s, float dt)
        {
            float current = angleDeg[s];
            float next = Mathf.MoveTowards(current, targetDeg[s], config.aimTurnSpeed * dt);
            if (next != current)
            {
                angleDeg[s] = next;
                Moved = true;
            }
        }

        private void DecayRecoil(float dt)
        {
            float back = config.recoilReturnSpeed * dt;
            for (int i = 0; i < recoil.Length; i++)
            {
                float value = recoil[i];
                if (value > 0f)
                {
                    recoil[i] = value > back ? value - back : 0f;
                    Moved = true;
                }
            }
        }

        /// <summary>Пушки стреляют вперемешку: каждая с интервалом rocketInterval, со сдвигом на долю интервала (D-031).</summary>
        private void TickSalvo(Side side, float dt)
        {
            int s = (int)side;
            float time = salvoTime[s] + dt;
            salvoTime[s] = time;

            float degrees = targetDeg[s];
            float radians = degrees * Mathf.Deg2Rad;
            float dirX = Facing(side) * Mathf.Cos(radians);
            float dirY = Mathf.Sin(radians);
            float perpX = -Facing(side) * dirY;
            float perpY = Mathf.Cos(radians);
            float speed = Ballistics.Speed(power[s], config.launchSpeedMin, config.launchSpeedMax);
            float interval = config.rocketInterval;

            float[] mountX = MountsX(side);
            float[] mountY = MountsY(side);
            int first = FirstCannon(side);
            int alive = Mathf.Min(match.Alive(side), CountOf(side));
            int limit = config.rocketsPerCannon;
            bool unfinished = false;
            for (int i = 0; i < alive; i++)
            {
                int cannon = first + i;
                float stagger = (float)i / alive;
                while (shots[cannon] < limit)
                {
                    float due = (shots[cannon] + stagger) * interval;
                    if (due > time)
                    {
                        break;
                    }
                    int offset = BarrelOrder[shots[cannon] % BarrelCount];
                    float x = mountX[i] + dirX * BarrelLength + perpX * offset * BarrelPitch;
                    float y = mountY[i] + dirY * BarrelLength + perpY * offset * BarrelPitch;
                    rockets.Spawn(x, y, dirX * speed, dirY * speed, side, time - due);
                    particles.Spawn(x, y, 0f, 0f, config.muzzleFlashSize, config.muzzleFlashLife, 0f, flashColor);
                    recoil[cannon * BarrelCount + offset + 1] = config.recoilDistance;
                    shots[cannon]++;
                }
                if (shots[cannon] < limit)
                {
                    unfinished = true;
                }
            }
            firing[s] = unfinished;
        }

        private float Facing(Side side)
        {
            return side == Side.Player ? 1f : -1f;
        }

        private float[] MountsX(Side side)
        {
            return side == Side.Player ? level.PlayerMountX : level.EnemyMountX;
        }

        private float[] MountsY(Side side)
        {
            return side == Side.Player ? level.PlayerMountY : level.EnemyMountY;
        }

        private int FirstCannon(Side side)
        {
            return side == Side.Player ? 0 : playerCount;
        }

        private int CountOf(Side side)
        {
            return side == Side.Player ? playerCount : enemyCount;
        }

        private static float Mean(float[] values, int count)
        {
            int used = Mathf.Clamp(count, 1, values.Length);
            float sum = 0f;
            for (int i = 0; i < used; i++)
            {
                sum += values[i];
            }
            return sum / used;
        }
    }
}
