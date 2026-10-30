namespace CastleDestroyers.Core
{
    /// <summary>Пул заранее созданных объектов (view-обёртки). Фабрик и замыканий нет: объекты передаёт владелец.</summary>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly T[] stack;
        private int count;

        public int Capacity { get { return stack.Length; } }
        public int FreeCount { get { return count; } }

        public ObjectPool(T[] items)
        {
            stack = new T[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                stack[i] = items[items.Length - 1 - i];
            }
            count = items.Length;
        }

        /// <summary>Достаёт свободный объект или null, если пул пуст.</summary>
        public T Get()
        {
            if (count == 0)
            {
                return null;
            }
            count--;
            T item = stack[count];
            stack[count] = null;
            return item;
        }

        /// <summary>Возвращает объект. False, если пул уже полон.</summary>
        public bool Release(T item)
        {
            if (item == null || count >= stack.Length)
            {
                return false;
            }
            stack[count] = item;
            count++;
            return true;
        }
    }
}
