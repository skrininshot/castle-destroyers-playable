namespace CastleDestroyers.Core
{
    /// <summary>
    /// Пул индексов для SoA-массивов: выдаёт свободные слоты без аллокаций.
    /// Сами данные лежат в параллельных массивах системы-владельца.
    /// </summary>
    public sealed class IndexPool
    {
        private readonly int[] freeStack;
        private readonly bool[] used;
        private int freeCount;

        public int Capacity { get; private set; }
        public int ActiveCount { get { return Capacity - freeCount; } }

        public IndexPool(int capacity)
        {
            Capacity = capacity < 0 ? 0 : capacity;
            freeStack = new int[Capacity];
            used = new bool[Capacity];
            Clear();
        }

        /// <summary>Выдаёт свободный индекс или -1, если лимит исчерпан.</summary>
        public int Alloc()
        {
            if (freeCount == 0)
            {
                return -1;
            }
            freeCount--;
            int index = freeStack[freeCount];
            used[index] = true;
            return index;
        }

        /// <summary>Возвращает индекс в пул. False, если индекс чужой или уже свободен.</summary>
        public bool Free(int index)
        {
            if (index < 0 || index >= Capacity || !used[index])
            {
                return false;
            }
            used[index] = false;
            freeStack[freeCount] = index;
            freeCount++;
            return true;
        }

        public bool IsUsed(int index)
        {
            return index >= 0 && index < Capacity && used[index];
        }

        /// <summary>Освобождает все слоты. Первым снова выдаётся индекс 0.</summary>
        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
            {
                used[i] = false;
                freeStack[i] = Capacity - 1 - i;
            }
            freeCount = Capacity;
        }
    }
}
