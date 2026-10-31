using CastleDestroyers.Combat;
using CastleDestroyers.Core;
using CastleDestroyers.Destruction;
using CastleDestroyers.Flow;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.CastleRig
{
    public enum CastleState
    {
        Standing,
        Sinking,
        Gone
    }

    /// <summary>
    /// Один замок (ТЗ §3.7): маска, поза платформы, гусеницы, оседание и покачивание. Локальные координаты: начало в центре
    /// низа платформы, y вверх. Мир = Pivot + поворот на Angle. Гусеницы стоят на ландшафте сами по себе.
    /// </summary>
    public sealed class CastleBody
    {
        public const int TrackCount = 2;

        private const int Left = 0;
        private const int Right = 1;
        private const float SettleEpsilon = 0.0001f;

        private GameConfig config;
        private DestructibleMask terrain;
        private float trackHalfLength;
        private float restGroundY;
        private float reachX;

        private readonly float[] trackY = new float[TrackCount];
        private readonly float[] targetY = new float[TrackCount];
        private readonly float[] trackAngle = new float[TrackCount];
        private readonly float[] targetAngle = new float[TrackCount];
        private float sway;
        private float swayVelocity;
        private float sinkVelocity;

        public Side Side { get; private set; }
        public DestructibleMask Mask { get; private set; }
        public float CenterX { get; private set; }
        public float HalfWidth { get; private set; }
        public float PlatformHalf { get; private set; }
        public float TrackCenter { get; private set; }
        /// <summary>Высота замка над низом платформы, метры.</summary>
        public float Height { get; private set; }

        public CastleState State { get; private set; }
        public float PivotX { get; private set; }
        public float PivotY { get; private set; }
        public float Angle { get; private set; }
        public float Cos { get; private set; }
        public float Sin { get; private set; }
        /// <summary>Покачивание от урона, радианы.</summary>
        public float Sway { get { return sway; } }
        /// <summary>Поза менялась в этом кадре.</summary>
        public bool PoseChanged { get; private set; }
        /// <summary>Замок только что начал тонуть: все пушки потеряны.</summary>
        public bool JustSank { get; private set; }
        /// <summary>Замок только что коснулся воды.</summary>
        public bool JustHitWater { get; private set; }
        public int PlatformHits { get; set; }
        /// <summary>Последний FirstHit пришёлся в платформу или гусеницы, а не в камень.</summary>
        public bool HitPlatform { get; private set; }

        public void Init(GameConfig gameConfig, DestructibleMask terrainMask, Side side, float centerX, float groundY, float halfWidth, DestructibleMask castleMask)
        {
            config = gameConfig;
            terrain = terrainMask;
            Side = side;
            CenterX = centerX;
            HalfWidth = halfWidth;
            restGroundY = groundY;
            Mask = castleMask;
            PlatformHalf = CastleShapes.PlatformHalfWidth(halfWidth);
            TrackCenter = CastleShapes.TrackCenter(halfWidth);
            trackHalfLength = CastleShapes.TrackHalfLength();
            Height = Mask.WorldY(Mask.Height - 1);
            // Запас по X на наклон: верх замка уходит вбок.
            float tilt = (config.castleMaxTiltDeg + config.swayMaxDeg) * Mathf.Deg2Rad;
            reachX = PlatformHalf + Height * Mathf.Sin(tilt);
            Reset();
        }

        public void Reset()
        {
            State = CastleState.Standing;
            sway = 0f;
            swayVelocity = 0f;
            sinkVelocity = 0f;
            JustSank = false;
            JustHitWater = false;
            PlatformHits = 0;
            for (int t = 0; t < TrackCount; t++)
            {
                trackY[t] = restGroundY;
                targetY[t] = restGroundY;
                trackAngle[t] = 0f;
                targetAngle[t] = 0f;
            }
            UpdatePose();
            PoseChanged = true;
        }

        public float TrackX(int track)
        {
            return CenterX + (track == Left ? -TrackCenter : TrackCenter);
        }

        public float TrackY(int track)
        {
            return trackY[track];
        }

        public float TrackAngle(int track)
        {
            return trackAngle[track];
        }

        public float LocalX(float worldX, float worldY)
        {
            return (worldX - PivotX) * Cos + (worldY - PivotY) * Sin;
        }

        public float LocalY(float worldX, float worldY)
        {
            return (worldY - PivotY) * Cos - (worldX - PivotX) * Sin;
        }

        public float WorldX(float localX, float localY)
        {
            return PivotX + localX * Cos - localY * Sin;
        }

        public float WorldY(float localX, float localY)
        {
            return PivotY + localX * Sin + localY * Cos;
        }

        /// <summary>Угловой импульс от взрыва: верх замка уходит от него. Импульсы складываются и насыщаются (ТЗ §3.7).</summary>
        public void AddSway(float blastX)
        {
            float direction = blastX < PivotX ? -1f : 1f;
            float limit = config.swayMaxSpeedDeg * Mathf.Deg2Rad;
            swayVelocity = Mathf.Clamp(swayVelocity + direction * config.swayImpulseDeg * Mathf.Deg2Rad, -limit, limit);
        }

        /// <summary>Первое попадание на отрезке мира: камень, платформа или гусеницы. Доля пути 0..1 или -1.</summary>
        public float FirstHit(float fromX, float fromY, float toX, float toY, float step)
        {
            HitPlatform = false;
            if (State != CastleState.Standing || Mathf.Max(fromX, toX) < CenterX - reachX || Mathf.Min(fromX, toX) > CenterX + reachX)
            {
                return -1f;
            }
            float localFromX = LocalX(fromX, fromY);
            float localFromY = LocalY(fromX, fromY);
            float localToX = LocalX(toX, toY);
            float localToY = LocalY(toX, toY);
            float wall = Mask.FirstSolid(localFromX, localFromY, localToX, localToY, step);
            float platform = BoosterSystem.SegmentEntry(localFromX, localFromY, localToX, localToY,
                -PlatformHalf, -config.trackHeight, PlatformHalf, config.platformHeight);
            if (platform >= 0f && (wall < 0f || platform < wall))
            {
                HitPlatform = true;
                return platform;
            }
            return wall;
        }

        /// <summary>terrainChanged — маска земли менялась: пора заново посадить гусеницы.</summary>
        public void Tick(float dt, bool terrainChanged)
        {
            PoseChanged = false;
            JustSank = false;
            JustHitWater = false;
            if (State == CastleState.Gone)
            {
                return;
            }
            if (State == CastleState.Sinking)
            {
                TickSinking(dt);
                return;
            }
            if (terrainChanged)
            {
                SampleGround();
            }
            if (State == CastleState.Sinking)
            {
                JustSank = true;
                return;
            }
            float moved = 0f;
            moved += SettleTrack(Left, dt);
            moved += SettleTrack(Right, dt);
            moved += TickSway(dt);
            if (moved > SettleEpsilon)
            {
                UpdatePose();
                PoseChanged = true;
            }
        }

        /// <summary>Гусеница стоит на высшей из своих крайних точек, наклон — по их разнице (ТЗ §3.7).</summary>
        private void SampleGround()
        {
            float water = config.waterLevel;
            bool leftEmpty = SampleTrack(Left);
            bool rightEmpty = SampleTrack(Right);
            // Director не отдаёт замок игрока воде: матч решают пушки, а не случайный подкоп (ТЗ §3.1).
            bool spared = config.directorEnabled && Side == Side.Player;
            if (leftEmpty && rightEmpty && !spared)
            {
                State = CastleState.Sinking;
                return;
            }
            // Платформа не наклоняется сильнее предела: висит на оставшейся гусенице, как на кадре 12.
            float drop = TrackCenter * 2f * Mathf.Tan(config.castleMaxTiltDeg * Mathf.Deg2Rad);
            targetY[Left] = Mathf.Max(targetY[Left], targetY[Right] - drop);
            targetY[Right] = Mathf.Max(targetY[Right], targetY[Left] - drop);
            targetY[Left] = Mathf.Max(targetY[Left], water);
            targetY[Right] = Mathf.Max(targetY[Right], water);
        }

        /// <summary>True, если под гусеницей пусто до воды.</summary>
        private bool SampleTrack(int track)
        {
            float x = TrackX(track);
            float near = terrain.SurfaceHeight(x - trackHalfLength);
            float far = terrain.SurfaceHeight(x + trackHalfLength);
            targetY[track] = Mathf.Max(near, far);
            float limit = config.trackMaxTiltDeg * Mathf.Deg2Rad;
            targetAngle[track] = Mathf.Clamp(Mathf.Atan2(far - near, trackHalfLength * 2f), -limit, limit);
            return targetY[track] <= config.waterLevel;
        }

        private float SettleTrack(int track, float dt)
        {
            float y = Mathf.MoveTowards(trackY[track], targetY[track], config.castleSettleSpeed * dt);
            float angle = Mathf.MoveTowards(trackAngle[track], targetAngle[track], config.trackTiltSpeed * Mathf.Deg2Rad * dt);
            float moved = Mathf.Abs(y - trackY[track]) + Mathf.Abs(angle - trackAngle[track]);
            trackY[track] = y;
            trackAngle[track] = angle;
            return moved;
        }

        /// <summary>Затухающая пружина; угол ограничен swayMaxDeg.</summary>
        private float TickSway(float dt)
        {
            if (sway == 0f && swayVelocity == 0f)
            {
                return 0f;
            }
            float frequency = config.swayFrequency;
            float acceleration = -frequency * frequency * sway - 2f * config.swayDamping * frequency * swayVelocity;
            swayVelocity += acceleration * dt;
            float next = sway + swayVelocity * dt;
            float limit = config.swayMaxDeg * Mathf.Deg2Rad;
            if (next > limit || next < -limit)
            {
                next = Mathf.Clamp(next, -limit, limit);
                swayVelocity = 0f;
            }
            if (Mathf.Abs(next) < SettleEpsilon && Mathf.Abs(swayVelocity) < SettleEpsilon)
            {
                next = 0f;
                swayVelocity = 0f;
            }
            float moved = Mathf.Abs(next - sway);
            sway = next;
            // Даже у самой вершины колебания пружина не должна считаться остановившейся.
            return moved + SettleEpsilon * 2f;
        }

        private void TickSinking(float dt)
        {
            float wasBottom = PivotY - config.trackHeight;
            sinkVelocity += config.chunkGravity * dt;
            float fall = sinkVelocity * dt;
            trackY[Left] -= fall;
            trackY[Right] -= fall;
            TickSway(dt);
            UpdatePose();
            PoseChanged = true;
            float bottom = PivotY - config.trackHeight;
            if (wasBottom >= config.waterLevel && bottom < config.waterLevel)
            {
                JustHitWater = true;
            }
            if (PivotY + Height * Cos < config.waterLevel)
            {
                State = CastleState.Gone;
            }
        }

        private void UpdatePose()
        {
            float tilt = Mathf.Atan2(trackY[Right] - trackY[Left], TrackCenter * 2f);
            PivotX = CenterX;
            PivotY = (trackY[Left] + trackY[Right]) * 0.5f + config.trackHeight;
            Angle = tilt + sway;
            Cos = Mathf.Cos(Angle);
            Sin = Mathf.Sin(Angle);
        }
    }
}
