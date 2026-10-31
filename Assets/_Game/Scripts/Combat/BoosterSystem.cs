using CastleDestroyers.Core;
using CastleDestroyers.Flow;
using CastleDestroyers.Fx;
using CastleDestroyers.Level;
using UnityEngine;

namespace CastleDestroyers.Combat
{
    /// <summary>
    /// Бустеры (ТЗ §3.5): ракета, вошедшая в бустер, заменяется на N копий на выходной стороне. Копии идут рядами
    /// по фиксированным потокам веера, случайности нет (D-031). Выпуск идёт из Tick, а не из цикла RocketSystem.
    /// Сверх бюджета копии не создаются, их урон достаётся оставшимся через вес (damageWeight).
    /// </summary>
    public sealed class BoosterSystem : ITickable
    {
        private const float Epsilon = 0.0001f;
        private const float FullTurn = Mathf.PI * 2f;
        private const float ScatterFloor = 0.4f;
        private const float AccentChance = 0.5f;
        // Новый сквош ниже этого уровня: иначе под потоком ракет бустер застынет сжатым.
        private const float SquashRetrigger = 0.4f;
        public const int MaxBoosters = 8;

        private GameConfig config;
        private RocketSystem rockets;
        private FlatParticles particles;
        private Rng rng;

        private int count;
        private int[] multiplier;
        private float[] minX;
        private float[] minY;
        private float[] maxX;
        private float[] maxY;
        private float[] squash;
        private float[] puffCooldown;
        private float[] popupValue;
        private float[] popupAge;
        private Color32[] puffColor;
        private Color32[] puffAccent;
        private float[] streamCos;
        private float[] streamSin;

        private IndexPool emitters;
        private float[] emitX;
        private float[] emitY;
        private float[] emitVelX;
        private float[] emitVelY;
        private float[] emitWeight;
        private float[] emitTimer;
        private int[] emitLeft;
        private byte[] emitBits;
        private byte[] emitSide;
        private int reserved;

        public int Count { get { return count; } }
        /// <summary>Копии, которые ещё не вышли из бустеров: ход не закончен, пока они не вылетят.</summary>
        public int PendingCopies { get { return reserved; } }
        /// <summary>Доля отрезка до входа в бустер из последнего FindEntry.</summary>
        public float EntryFraction { get; private set; }
        /// <summary>«Честное» число ракет из бустеров с начала сессии, без учёта бюджета.</summary>
        public float HonestTotal { get; private set; }
        /// <summary>Сколько копий реально вылетело с начала сессии.</summary>
        public int SpawnedTotal { get; private set; }

        public void Init(GameConfig gameConfig, RocketSystem rocketSystem, FlatParticles flatParticles, Rng random)
        {
            config = gameConfig;
            rockets = rocketSystem;
            particles = flatParticles;
            rng = random;

            count = Mathf.Min(config.boosters.Length, MaxBoosters);
            multiplier = new int[count];
            minX = new float[count];
            minY = new float[count];
            maxX = new float[count];
            maxY = new float[count];
            squash = new float[count];
            puffCooldown = new float[count];
            popupValue = new float[count];
            popupAge = new float[count];
            puffColor = new Color32[count];
            puffAccent = new Color32[count];
            float halfWidth = config.boosterWidth * 0.5f;
            float halfHeight = config.boosterHeight * 0.5f;
            for (int k = 0; k < count; k++)
            {
                BoosterSpec spec = config.boosters[k];
                multiplier[k] = spec.multiplier;
                minX[k] = spec.position.x - halfWidth;
                maxX[k] = spec.position.x + halfWidth;
                minY[k] = spec.position.y - halfHeight;
                maxY[k] = spec.position.y + halfHeight;
            }

            int streams = Mathf.Max(config.boosterStreams, 1);
            streamCos = new float[streams];
            streamSin = new float[streams];
            for (int s = 0; s < streams; s++)
            {
                float radians = StreamAngleDeg(s, streams, config.boosterSpreadDeg) * Mathf.Deg2Rad;
                streamCos[s] = Mathf.Cos(radians);
                streamSin[s] = Mathf.Sin(radians);
            }

            int capacity = rockets.Capacity;
            emitters = new IndexPool(capacity);
            emitX = new float[capacity];
            emitY = new float[capacity];
            emitVelX = new float[capacity];
            emitVelY = new float[capacity];
            emitWeight = new float[capacity];
            emitTimer = new float[capacity];
            emitLeft = new int[capacity];
            emitBits = new byte[capacity];
            emitSide = new byte[capacity];
        }

        public void ApplyPalette(PaletteSet palette)
        {
            for (int k = 0; k < count; k++)
            {
                puffColor[k] = GreyboxBoosters.GlassColor(palette, multiplier[k]);
                puffAccent[k] = palette.hudText;
            }
        }

        /// <summary>Сквош бустера 0..1.</summary>
        public float Squash(int booster)
        {
            return squash[booster];
        }

        /// <summary>Число на поп-апе «+N»; 0 — поп-апа нет.</summary>
        public int PopupValue(int booster)
        {
            return Mathf.RoundToInt(popupValue[booster]);
        }

        /// <summary>Время с последнего срабатывания, секунды.</summary>
        public float PopupAge(int booster)
        {
            return popupAge[booster];
        }

        /// <summary>Первый бустер на отрезке носа, которым ракета ещё не умножена; -1, если такого нет.</summary>
        public int FindEntry(float fromX, float fromY, float toX, float toY, byte usedBits)
        {
            int found = -1;
            float nearest = 2f;
            for (int k = 0; k < count; k++)
            {
                if ((usedBits & (1 << k)) != 0)
                {
                    continue;
                }
                float fraction = SegmentEntry(fromX, fromY, toX, toY, minX[k], minY[k], maxX[k], maxY[k]);
                if (fraction >= 0f && fraction < nearest)
                {
                    nearest = fraction;
                    found = k;
                }
            }
            EntryFraction = found >= 0 ? nearest : 0f;
            return found;
        }

        /// <summary>Заменяет ракету на копии: считает их число по бюджету и ставит выпуск в очередь. Ракету удаляет вызывающий.</summary>
        public void Multiply(int booster, float x, float y, float speedX, float speedY, Side owner, byte usedBits, float weight)
        {
            int factor = multiplier[booster];
            // Заменяемая ракета ещё в ActiveCount, её место свободно.
            int free = rockets.Budget - rockets.ActiveCount + 1 - reserved;
            int copies = CopyCount(factor, free);
            float honest = factor * weight;
            HonestTotal += honest;
            popupValue[booster] += honest;
            popupAge[booster] = 0f;
            if (squash[booster] < SquashRetrigger)
            {
                squash[booster] = 1f;
            }
            Puff(booster);

            int e = emitters.Alloc();
            if (e < 0)
            {
                return;
            }
            BoosterSpec spec = config.boosters[booster];
            if (spec.fixedExit)
            {
                float radians = spec.exitAngleDeg * Mathf.Deg2Rad;
                float speed = Mathf.Sqrt(speedX * speedX + speedY * speedY);
                speedX = (speedX >= 0f ? speed : -speed) * Mathf.Cos(radians);
                speedY = speed * Mathf.Sin(radians);
            }
            float exit = ExitDistance(x, y, speedX, speedY, minX[booster], minY[booster], maxX[booster], maxY[booster]);
            float exitX = x + speedX * exit;
            float exitY = y + speedY * exit;
            // Выход стягивается к оси бустера: иначе дуги разных пушек размывают веер в сплошную ленту.
            float focus = config.boosterExitFocus;
            float centerX = (minX[booster] + maxX[booster]) * 0.5f;
            float centerY = (minY[booster] + maxY[booster]) * 0.5f;
            if (exitX <= minX[booster] + Epsilon || exitX >= maxX[booster] - Epsilon)
            {
                exitY += (centerY - exitY) * focus;
            }
            else
            {
                exitX += (centerX - exitX) * focus;
            }
            emitX[e] = exitX;
            emitY[e] = exitY;
            emitVelX[e] = speedX;
            emitVelY[e] = speedY;
            emitWeight[e] = CopyWeight(factor, copies, weight);
            emitTimer[e] = 0f;
            emitLeft[e] = copies;
            emitBits[e] = (byte)(usedBits | (1 << booster));
            emitSide[e] = (byte)owner;
            reserved += copies;
        }

        public void Tick(float dt)
        {
            float decay = config.boosterSquashDecay * dt;
            float popupEnd = config.boosterPopupHold + config.boosterPopupFade;
            for (int k = 0; k < count; k++)
            {
                squash[k] = squash[k] > decay ? squash[k] - decay : 0f;
                puffCooldown[k] -= dt;
                if (popupValue[k] > 0f)
                {
                    popupAge[k] += dt;
                    if (popupAge[k] >= popupEnd)
                    {
                        popupValue[k] = 0f;
                        popupAge[k] = 0f;
                    }
                }
            }
            if (emitters.ActiveCount > 0)
            {
                EmitCopies(dt);
            }
        }

        public void Clear()
        {
            emitters.Clear();
            reserved = 0;
            HonestTotal = 0f;
            SpawnedTotal = 0;
            for (int k = 0; k < count; k++)
            {
                squash[k] = 0f;
                puffCooldown[k] = 0f;
                popupValue[k] = 0f;
                popupAge[k] = 0f;
            }
        }

        /// <summary>Сколько копий выпустить: не меньше одной, ракета заменяет сама себя.</summary>
        public static int CopyCount(int factor, int free)
        {
            return Mathf.Clamp(free, 1, Mathf.Max(factor, 1));
        }

        /// <summary>Вес копии: урон несозданных копий делится на созданные.</summary>
        public static float CopyWeight(int factor, int copies, float weight)
        {
            return weight * factor / copies;
        }

        /// <summary>Во сколько раз больше кратер ракеты с весом: площадь растёт как вес, радиус ограничен.</summary>
        public static float CraterScale(float weight, float maxScale)
        {
            return weight <= 1f ? 1f : Mathf.Min(Mathf.Sqrt(weight), maxScale);
        }

        /// <summary>Угол потока от оси: 0, +шаг, −шаг, +2 шага… Малое число копий идёт по центру.</summary>
        public static float StreamAngleDeg(int stream, int streams, float spreadDeg)
        {
            if (streams <= 1 || stream == 0)
            {
                return 0f;
            }
            float step = spreadDeg / (streams - 1);
            int ring = (stream + 1) / 2;
            return stream % 2 == 1 ? ring * step : -ring * step;
        }

        /// <summary>Вход отрезка в прямоугольник (метод плит): доля отрезка 0..1 или -1, если не пересекает.</summary>
        public static float SegmentEntry(float fromX, float fromY, float toX, float toY, float rectMinX, float rectMinY, float rectMaxX, float rectMaxY)
        {
            float enter = 0f;
            float leave = 1f;
            float deltaX = toX - fromX;
            if (Mathf.Abs(deltaX) < Epsilon)
            {
                if (fromX < rectMinX || fromX > rectMaxX)
                {
                    return -1f;
                }
            }
            else
            {
                float nearX = ((deltaX > 0f ? rectMinX : rectMaxX) - fromX) / deltaX;
                float farX = ((deltaX > 0f ? rectMaxX : rectMinX) - fromX) / deltaX;
                enter = nearX > enter ? nearX : enter;
                leave = farX < leave ? farX : leave;
            }
            float deltaY = toY - fromY;
            if (Mathf.Abs(deltaY) < Epsilon)
            {
                if (fromY < rectMinY || fromY > rectMaxY)
                {
                    return -1f;
                }
            }
            else
            {
                float nearY = ((deltaY > 0f ? rectMinY : rectMaxY) - fromY) / deltaY;
                float farY = ((deltaY > 0f ? rectMaxY : rectMinY) - fromY) / deltaY;
                enter = nearY > enter ? nearY : enter;
                leave = farY < leave ? farY : leave;
            }
            return enter <= leave ? enter : -1f;
        }

        /// <summary>Через сколько единиц направления точка внутри прямоугольника выйдет из него.</summary>
        public static float ExitDistance(float x, float y, float directionX, float directionY, float rectMinX, float rectMinY, float rectMaxX, float rectMaxY)
        {
            float exit = float.PositiveInfinity;
            if (Mathf.Abs(directionX) >= Epsilon)
            {
                exit = ((directionX > 0f ? rectMaxX : rectMinX) - x) / directionX;
            }
            if (Mathf.Abs(directionY) >= Epsilon)
            {
                float exitY = ((directionY > 0f ? rectMaxY : rectMinY) - y) / directionY;
                exit = exitY < exit ? exitY : exit;
            }
            return float.IsInfinity(exit) || exit < 0f ? 0f : exit;
        }

        /// <summary>Выпуск рядами: в ряду по копии на поток, следующий ряд через boosterSpawnStagger.</summary>
        private void EmitCopies(float dt)
        {
            int streams = streamCos.Length;
            float stagger = config.boosterSpawnStagger;
            for (int e = 0; e < emitters.Capacity; e++)
            {
                if (!emitters.IsUsed(e))
                {
                    continue;
                }
                float timer = emitTimer[e] - dt;
                int left = emitLeft[e];
                while (timer <= 0f && left > 0)
                {
                    int row = left < streams ? left : streams;
                    for (int s = 0; s < row; s++)
                    {
                        float speedX = emitVelX[e] * streamCos[s] - emitVelY[e] * streamSin[s];
                        float speedY = emitVelX[e] * streamSin[s] + emitVelY[e] * streamCos[s];
                        // Сдвиг фазы между потоками: иначе ряд читается вертикальной полосой поперёк веера.
                        float startAge = stagger * s / streams - timer;
                        if (rockets.SpawnCopy(emitX[e], emitY[e], speedX, speedY, (Side)emitSide[e], startAge, emitBits[e], emitWeight[e]))
                        {
                            SpawnedTotal++;
                        }
                    }
                    left -= row;
                    reserved -= row;
                    timer += stagger;
                }
                emitTimer[e] = timer;
                emitLeft[e] = left;
                if (left == 0)
                {
                    emitters.Free(e);
                }
            }
        }

        /// <summary>«Пых»: плоские пуфы разлетаются от корпуса. Под потоком ракет — не чаще boosterPuffInterval.</summary>
        private void Puff(int booster)
        {
            if (puffCooldown[booster] > 0f)
            {
                return;
            }
            puffCooldown[booster] = config.boosterPuffInterval;
            float centerX = (minX[booster] + maxX[booster]) * 0.5f;
            float centerY = (minY[booster] + maxY[booster]) * 0.5f;
            float halfWidth = (maxX[booster] - minX[booster]) * 0.5f;
            float halfHeight = (maxY[booster] - minY[booster]) * 0.5f;
            for (int n = 0; n < config.boosterPuffCount; n++)
            {
                float angle = rng.Range(0f, FullTurn);
                float directionX = Mathf.Cos(angle);
                float directionY = Mathf.Sin(angle);
                float speed = config.boosterPuffSpeed * rng.Range(ScatterFloor, 1f);
                particles.Spawn(centerX + directionX * halfWidth, centerY + directionY * halfHeight, directionX * speed, directionY * speed,
                    config.boosterPuffSize * rng.Range(ScatterFloor, 1f), config.boosterPuffLife * rng.Range(ScatterFloor, 1f), 0f,
                    rng.Chance(AccentChance) ? puffColor[booster] : puffAccent[booster]);
            }
        }
    }
}
