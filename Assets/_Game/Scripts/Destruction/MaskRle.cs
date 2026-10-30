namespace CastleDestroyers.Destruction
{
    /// <summary>RLE байтовых каналов маски: один int на серию, count * 256 + value. Ландшафт плоский, серии длинные.</summary>
    public static class MaskRle
    {
        private const int ValueRange = 256;

        /// <summary>Вызывается при запекании в редакторе.</summary>
        public static int[] Encode(byte[] data)
        {
            int runs = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 0 || data[i] != data[i - 1])
                {
                    runs++;
                }
            }

            int[] encoded = new int[runs];
            int run = -1;
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 0 || data[i] != data[i - 1])
                {
                    run++;
                    encoded[run] = data[i];
                }
                encoded[run] += ValueRange;
            }
            return encoded;
        }

        /// <summary>Распаковывает серии в target. Вызывается при Init.</summary>
        public static void Decode(int[] runs, byte[] target)
        {
            int index = 0;
            for (int i = 0; i < runs.Length; i++)
            {
                int count = runs[i] / ValueRange;
                byte value = (byte)(runs[i] % ValueRange);
                for (int k = 0; k < count; k++)
                {
                    target[index] = value;
                    index++;
                }
            }
        }
    }
}
