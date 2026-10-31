using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Пушки (ТЗ §3.3): синхронный поворот стороны, залп, чередование стволов, отдача и вспышка, отрыв и падение в воду.
    /// Пушки индексируются сквозным номером: сначала все синие, затем красные. Крепления живут в координатах замка,
    /// мировые положения пересчитывает SetBodyPose. MatchState — счёт: снижение счёта снаружи отрывает пушки с конца.
    /// </summary>
    public sealed class CannonSystem : ITickable
    {
        public const int BarrelCount = 3;
        public const float BarrelLength = 1.25f;
        public const float BarrelPitch = 0.21f;

        private const int SideCount = 2;
        private const byte Alive = 0;
        private const byte Falling = 1;
        private const byte Gone = 2;

        // Порядок стволов: верхний, нижний, средний. Значение — смещение от оси в шагах BarrelPitch.
        private static readonly int[] BarrelOrder = { 1, -1, 0 };

        private GameConfig config;
        private MatchState match;
        private RocketSystem rockets;
        private FlatParticles particles;
        private LevelFx fx;
        private Rng rng;
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

        private float[] localX;
        private float[] localY;
        private float[] restLocalY;
        private float[] worldX;
        private float[] worldY;
        private float[] fallVelX;
        private float[] fallVelY;
        private float[] fallAngle;
        private float[] fallSpin;
        private byte[] state;
        private bool[] onPlatform;
        private int[] living;
        private int fallingCount;
        private int playerLossesLeft;
        private float[] salvoWeight;
        private float[] poseX;
        private float[] poseY;
        private float[] poseCos;
        private float[] poseSin;
        private float[] jumpY;
        private int droppingCount;

        public int TotalCannons { get { return playerCount + enemyCount; } }
        /// <summary>Положение стволов менялось в этом кадре: виду пора перестроить меш.</summary>
        public bool Moved { get; private set; }
        public bool IsFiring { get { return firing[0] || firing[1]; } }
        public int FallingCount { get { return fallingCount; } }
        /// <summary>Сколько пушек врага нельзя оторвать сейчас: Director бережёт ход до последнего залпа игрока.</summary>
        public int SparedEnemyCannons { get; set; }

        public void Init(GameConfig gameConfig, GreyboxLevel level, MatchState matchState, RocketSystem rocketSystem, FlatParticles flatParticles, LevelFx levelFx, Rng random)
        {
            config = gameConfig;
            match = matchState;
            rockets = rocketSystem;
            particles = flatParticles;
            fx = levelFx;
            rng = random;
            playerCount = level.PlayerMountX.Length;
            enemyCount = level.EnemyMountX.Length;

            angleDeg = new float[SideCount];
            targetDeg = new float[SideCount];
            power = new float[SideCount];
            hasAim = new bool[SideCount];
            firing = new bool[SideCount];
            salvoTime = new float[SideCount];
            salvoWeight = new float[SideCount];
            poseX = new float[SideCount];
            poseY = new float[SideCount];
            poseCos = new float[SideCount];
            poseSin = new float[SideCount];
            int total = TotalCannons;
            jumpY = new float[total];
            shots = new int[total];
            recoil = new float[total * BarrelCount];
            localX = new float[total];
            localY = new float[total];
            restLocalY = new float[total];
            worldX = new float[total];
            worldY = new float[total];
            fallVelX = new float[total];
            fallVelY = new float[total];
            fallAngle = new float[total];
            fallSpin = new float[total];
            state = new byte[total];
            onPlatform = new bool[total];
            living = new int[SideCount];

            float playerPivotY = level.CastleGround(Side.Player) + config.trackHeight;
            float enemyPivotY = level.CastleGround(Side.Enemy) + config.trackHeight;
            for (int i = 0; i < playerCount; i++)
            {
                localX[i] = level.PlayerMountX[i] - config.playerCastleX;
                restLocalY[i] = level.PlayerMountY[i] - playerPivotY;
            }
            for (int i = 0; i < enemyCount; i++)
            {
                localX[playerCount + i] = level.EnemyMountX[i] - config.enemyCastleX;
                restLocalY[playerCount + i] = level.EnemyMountY[i] - enemyPivotY;
            }
            salvoWeight[(int)Side.Player] = 1f;
            salvoWeight[(int)Side.Enemy] = 1f;
            ResetCannons();
            SetBodyPose(Side.Player, config.playerCastleX, playerPivotY, 1f, 0f);
            SetBodyPose(Side.Enemy, config.enemyCastleX, enemyPivotY, 1f, 0f);
            match.Changed += SyncWithMatch;
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

        public int FirstCannon(Side side)
        {
            return side == Side.Player ? 0 : playerCount;
        }

        public int CountOf(Side side)
        {
            return side == Side.Player ? playerCount : enemyCount;
        }

        public bool IsAlive(int cannon)
        {
            return state[cannon] == Alive;
        }

        public bool IsFalling(int cannon)
        {
            return state[cannon] == Falling;
        }

        public bool IsOnPlatform(int cannon)
        {
            return onPlatform[cannon];
        }

        /// <summary>Крепление в координатах замка, метры.</summary>
        public float LocalX(int cannon)
        {
            return localX[cannon];
        }

        public float LocalY(int cannon)
        {
            return localY[cannon];
        }

        /// <summary>Положение пушки в мире: на креплении или в падении.</summary>
        public float WorldX(int cannon)
        {
            return worldX[cannon];
        }

        public float WorldY(int cannon)
        {
            return worldY[cannon];
        }

        /// <summary>Подскок победителя над креплением, метры (ТЗ §3.11).</summary>
        public float JumpY(int cannon)
        {
            return jumpY[cannon];
        }

        /// <summary>Угол кувыркающейся пушки поверх угла стороны, радианы.</summary>
        public float FallAngle(int cannon)
        {
            return fallAngle[cannon];
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

        /// <summary>Поворачивает пушки, не задавая прицел: подсказка не должна становиться выстрелом.</summary>
        public void PointAt(Side side, float degrees)
        {
            targetDeg[(int)side] = degrees;
        }

        /// <summary>Вес урона ракет следующих залпов стороны: добивающий выстрел Director (ТЗ §3.1.1).</summary>
        public void SetSalvoWeight(Side side, float weight)
        {
            salvoWeight[(int)side] = weight;
        }

        /// <summary>Подскок живых пушек победителя со сдвигом фаз. time — секунды от начала; 0 ставит пушки на место.</summary>
        public void Celebrate(Side side, float time)
        {
            int first = FirstCannon(side);
            int count = CountOf(side);
            for (int i = 0; i < count; i++)
            {
                int c = first + i;
                jumpY[c] = time > 0f ? Mathf.Abs(Mathf.Sin(time * config.victoryJumpSpeed + i * config.victoryJumpPhase)) * config.victoryJumpHeight : 0f;
            }
            Moved = true;
        }

        /// <summary>Начинает залп живых пушек по текущему прицелу.</summary>
        public void BeginSalvo(Side side)
        {
            int s = (int)side;
            firing[s] = true;
            salvoTime[s] = 0f;
            if (side == Side.Enemy)
            {
                playerLossesLeft = config.directorMaxLossPerAiTurn;
            }
            int first = FirstCannon(side);
            int count = CountOf(side);
            for (int cannon = first; cannon < first + count; cannon++)
            {
                shots[cannon] = 0;
            }
        }

        /// <summary>Прекращает залп стороны: цель уже разбита (ТЗ §3.11).</summary>
        public void StopSalvo(Side side)
        {
            firing[(int)side] = false;
        }

        public void Clear()
        {
            for (int s = 0; s < SideCount; s++)
            {
                angleDeg[s] = 0f;
                targetDeg[s] = 0f;
                hasAim[s] = false;
                firing[s] = false;
                salvoWeight[s] = 1f;
            }
            SparedEnemyCannons = 0;
            System.Array.Clear(recoil, 0, recoil.Length);
            System.Array.Clear(jumpY, 0, jumpY.Length);
            ResetCannons();
            Moved = true;
        }

        /// <summary>Поза замка стороны: пересчитывает крепления живых пушек в мир.</summary>
        public void SetBodyPose(Side side, float pivotX, float pivotY, float cos, float sin)
        {
            int s = (int)side;
            poseX[s] = pivotX;
            poseY[s] = pivotY;
            poseCos[s] = cos;
            poseSin[s] = sin;
            int first = FirstCannon(side);
            int end = first + CountOf(side);
            for (int c = first; c < end; c++)
            {
                if (state[c] == Alive)
                {
                    worldX[c] = pivotX + localX[c] * cos - localY[c] * sin;
                    worldY[c] = pivotY + localX[c] * sin + localY[c] * cos;
                }
            }
            Moved = true;
        }

        /// <summary>
        /// Взрыв задел пушку: она отлетает от него (ТЗ §3.3, условие 1). Хитбокс — капсула вдоль ствола, поэтому взрыв
        /// рядом с дулом считается так же, как у крепления. radius — радиус взрыва, метры.
        /// </summary>
        public void ApplyBlast(Side side, float x, float y, float radius)
        {
            float reach = config.cannonHitRadius + radius;
            float reachSq = reach * reach;
            float radians = angleDeg[(int)side] * Mathf.Deg2Rad;
            float barrelX = Facing(side) * Mathf.Cos(radians);
            float barrelY = Mathf.Sin(radians);
            int first = FirstCannon(side);
            int end = first + CountOf(side);
            for (int c = first; c < end; c++)
            {
                if (state[c] != Alive)
                {
                    continue;
                }
                float toBlastX = x - worldX[c];
                float toBlastY = y - worldY[c];
                float along = Mathf.Clamp(toBlastX * barrelX + toBlastY * barrelY, 0f, BarrelLength);
                float dx = worldX[c] + barrelX * along - x;
                float dy = worldY[c] + barrelY * along - y;
                float distanceSq = dx * dx + dy * dy;
                if (distanceSq < reachSq)
                {
                    float direction = dx >= 0f ? 1f : -1f;
                    float spin = rng.Range(-config.cannonDetachSpinDeg, config.cannonDetachSpinDeg) * Mathf.Deg2Rad;
                    Detach(c, direction * config.cannonDetachSpeed, config.cannonDetachLift, spin);
                }
            }
        }

        /// <summary>
        /// Отрывает пушку: дальше она падает без коллизий. False, если Director бережёт последнюю пушку игрока (ТЗ §3.1);
        /// тогда пушка остаётся на месте.
        /// </summary>
        public bool Detach(int cannon, float speedX, float speedY, float spin)
        {
            if (state[cannon] != Alive)
            {
                return false;
            }
            Side side = cannon < playerCount ? Side.Player : Side.Enemy;
            if (config.directorEnabled && side == Side.Player && (living[(int)side] <= 1 || playerLossesLeft <= 0))
            {
                return false;
            }
            if (side == Side.Enemy && living[(int)side] <= SparedEnemyCannons)
            {
                return false;
            }
            if (side == Side.Player)
            {
                playerLossesLeft--;
            }
            Release(cannon, speedX, speedY, spin);
            living[(int)side]--;
            match.Destroy(side, 1, 0);
            return true;
        }

        /// <summary>Замок ушёл под воду: все его пушки потеряны, Director здесь бессилен (ТЗ §3.7).</summary>
        public void LoseAll(Side side)
        {
            int first = FirstCannon(side);
            int end = first + CountOf(side);
            for (int c = first; c < end; c++)
            {
                if (state[c] == Alive)
                {
                    Release(c, 0f, 0f, rng.Range(-config.chunkMaxSpinDeg, config.chunkMaxSpinDeg) * Mathf.Deg2Rad);
                }
            }
            living[(int)side] = 0;
            match.Destroy(side, match.Alive(side), 0);
        }

        /// <summary>Спасённая Director-ом пушка без опоры съезжает на платформу и больше не проверяет материал.</summary>
        public void DropToPlatform(int cannon)
        {
            onPlatform[cannon] = true;
            droppingCount++;
        }

        /// <summary>X средней точки вылета живых пушек при текущем прицеле: начало пунктира.</summary>
        public float MuzzleCenterX(Side side)
        {
            float direction = Facing(side) * Mathf.Cos(targetDeg[(int)side] * Mathf.Deg2Rad);
            return Mean(worldX, side) + direction * BarrelLength;
        }

        /// <summary>Высшая точка дуги стороны при текущем прицеле: по ней камера заранее выбирает кадр.</summary>
        public float SalvoApexY(Side side)
        {
            int s = (int)side;
            float speed = Ballistics.Speed(power[s], config.launchSpeedMin, config.launchSpeedMax);
            float speedY = speed * Mathf.Sin(targetDeg[s] * Mathf.Deg2Rad);
            return MuzzleCenterY(side) + Ballistics.ApexHeight(speedY, config.gravity);
        }

        /// <summary>Y средней точки вылета живых пушек при текущем прицеле.</summary>
        public float MuzzleCenterY(Side side)
        {
            return Mean(worldY, side) + Mathf.Sin(targetDeg[(int)side] * Mathf.Deg2Rad) * BarrelLength;
        }

        public void Tick(float dt)
        {
            Moved = false;
            Turn((int)Side.Player, dt);
            Turn((int)Side.Enemy, dt);
            DecayRecoil(dt);
            if (fallingCount > 0)
            {
                TickFalling(dt);
            }
            if (droppingCount > 0)
            {
                TickDropping(dt);
            }
            if (firing[(int)Side.Player])
            {
                TickSalvo(Side.Player, dt);
            }
            if (firing[(int)Side.Enemy])
            {
                TickSalvo(Side.Enemy, dt);
            }
        }

        private void ResetCannons()
        {
            for (int c = 0; c < state.Length; c++)
            {
                state[c] = Alive;
                onPlatform[c] = false;
                localY[c] = restLocalY[c];
                fallAngle[c] = 0f;
            }
            living[(int)Side.Player] = playerCount;
            living[(int)Side.Enemy] = enemyCount;
            fallingCount = 0;
            droppingCount = 0;
            playerLossesLeft = int.MaxValue;
        }

        /// <summary>Счёт снизили снаружи (Director, тесты): лишние пушки отрываются с конца, остальное уже совпадает.</summary>
        private void SyncWithMatch()
        {
            SyncSide(Side.Player);
            SyncSide(Side.Enemy);
        }

        private void SyncSide(Side side)
        {
            int s = (int)side;
            int first = FirstCannon(side);
            for (int c = first + CountOf(side) - 1; c >= first && living[s] > match.Alive(side); c--)
            {
                if (state[c] == Alive)
                {
                    float direction = rng.Range(-1f, 1f);
                    float spin = rng.Range(-config.cannonDetachSpinDeg, config.cannonDetachSpinDeg) * Mathf.Deg2Rad;
                    Release(c, direction * config.cannonDetachSpeed, config.cannonDetachLift, spin);
                    living[s]--;
                }
            }
        }

        private void Release(int cannon, float speedX, float speedY, float spin)
        {
            state[cannon] = Falling;
            fallVelX[cannon] = speedX;
            fallVelY[cannon] = speedY;
            fallSpin[cannon] = spin;
            fallingCount++;
            Moved = true;
        }

        private void TickFalling(float dt)
        {
            float gravity = config.chunkGravity;
            float water = config.waterLevel;
            for (int c = 0; c < state.Length; c++)
            {
                if (state[c] != Falling)
                {
                    continue;
                }
                fallVelY[c] -= gravity * dt;
                worldX[c] += fallVelX[c] * dt;
                worldY[c] += fallVelY[c] * dt;
                fallAngle[c] += fallSpin[c] * dt;
                if (worldY[c] < water)
                {
                    fx.Splash(worldX[c], water, config.cannonSplashWidth);
                    state[c] = Gone;
                    fallingCount--;
                }
            }
            Moved = true;
        }

        /// <summary>Спасённые пушки плавно съезжают по замку до платформы.</summary>
        private void TickDropping(float dt)
        {
            float floor = config.platformHeight;
            float step = config.cannonDropSpeed * dt;
            for (int c = 0; c < state.Length; c++)
            {
                if (!onPlatform[c] || state[c] != Alive || localY[c] <= floor)
                {
                    continue;
                }
                localY[c] = Mathf.MoveTowards(localY[c], floor, step);
                int s = c < playerCount ? (int)Side.Player : (int)Side.Enemy;
                worldX[c] = poseX[s] + localX[c] * poseCos[s] - localY[c] * poseSin[s];
                worldY[c] = poseY[s] + localX[c] * poseSin[s] + localY[c] * poseCos[s];
                if (localY[c] <= floor)
                {
                    droppingCount--;
                }
                Moved = true;
            }
        }

        private void Turn(int s, float dt)
        {
            float current = angleDeg[s];
            float speed = s == (int)Side.Enemy ? config.aiTurnSpeed : config.aimTurnSpeed;
            float next = Mathf.MoveTowards(current, targetDeg[s], speed * dt);
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
            float weight = salvoWeight[s];

            int first = FirstCannon(side);
            int count = CountOf(side);
            int aliveCount = living[s];
            int limit = config.rocketsPerCannon;
            int order = 0;
            bool unfinished = false;
            for (int cannon = first; cannon < first + count; cannon++)
            {
                if (state[cannon] != Alive)
                {
                    continue;
                }
                float stagger = (float)order / aliveCount;
                order++;
                while (shots[cannon] < limit)
                {
                    float due = (shots[cannon] + stagger) * interval;
                    if (due > time)
                    {
                        break;
                    }
                    int offset = BarrelOrder[shots[cannon] % BarrelCount];
                    float x = worldX[cannon] + dirX * BarrelLength + perpX * offset * BarrelPitch;
                    float y = worldY[cannon] + dirY * BarrelLength + perpY * offset * BarrelPitch;
                    rockets.SpawnCopy(x, y, dirX * speed, dirY * speed, side, time - due, 0, weight);
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

        /// <summary>Среднее значение по живым пушкам стороны; если живых нет, первая пушка.</summary>
        private float Mean(float[] values, Side side)
        {
            int first = FirstCannon(side);
            int end = first + CountOf(side);
            float sum = 0f;
            int used = 0;
            for (int c = first; c < end; c++)
            {
                if (state[c] == Alive)
                {
                    sum += values[c];
                    used++;
                }
            }
            return used > 0 ? sum / used : values[first];
        }
    }
}
