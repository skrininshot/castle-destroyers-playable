namespace CastleDestroyers.Core
{
    /// <summary>Xorshift32 без аллокаций. Фиксированный seed даёт детерминированную сессию.</summary>
    public sealed class Rng
    {
        private const float MantissaToUnit = 1f / 16777216f;
        private const int MantissaShift = 8;
        private const uint FallbackSeed = 2463534242u;

        private uint state;

        public Rng(uint seed)
        {
            Reset(seed);
        }

        public void Reset(uint seed)
        {
            state = seed == 0u ? FallbackSeed : seed;
        }

        public uint NextUInt()
        {
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }

        /// <summary>Число в [0, 1). Берём 24 старших бита, чтобы float не округлялся до 1.</summary>
        public float NextFloat()
        {
            return (NextUInt() >> MantissaShift) * MantissaToUnit;
        }

        public float Range(float min, float max)
        {
            return min + (max - min) * NextFloat();
        }

        /// <summary>Целое в [min, max).</summary>
        public int Range(int min, int max)
        {
            if (max <= min)
            {
                return min;
            }
            return min + (int)(NextUInt() % (uint)(max - min));
        }

        public bool Chance(float probability)
        {
            return NextFloat() < probability;
        }
    }
}
