namespace CastleDestroyers.Spike
{
    /// <summary>Xorshift32 без аллокаций.</summary>
    public sealed class SpikeRng
    {
        private const float UintToUnit = 1f / 4294967296f;
        private uint state;

        public SpikeRng(uint seed)
        {
            state = seed == 0u ? 1u : seed;
        }

        public float NextFloat()
        {
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x * UintToUnit;
        }

        public float Range(float min, float max)
        {
            return min + (max - min) * NextFloat();
        }
    }
}
