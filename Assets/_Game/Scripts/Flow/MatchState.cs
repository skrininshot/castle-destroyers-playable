using System;

namespace CastleDestroyers.Flow
{
    /// <summary>Счёт живых пушек обеих сторон. «Здоровье» стороны = живые пушки / начальные (ТЗ §3.12).</summary>
    public sealed class MatchState
    {
        public int PlayerStart { get; private set; }
        public int EnemyStart { get; private set; }
        public int PlayerAlive { get; private set; }
        public int EnemyAlive { get; private set; }

        /// <summary>Срабатывает после любого изменения числа пушек.</summary>
        public event Action Changed;

        public void Init(int playerCannons, int enemyCannons)
        {
            PlayerStart = playerCannons;
            EnemyStart = enemyCannons;
            Reset();
        }

        public void Reset()
        {
            PlayerAlive = PlayerStart;
            EnemyAlive = EnemyStart;
            RaiseChanged();
        }

        public int Alive(Side side)
        {
            return side == Side.Player ? PlayerAlive : EnemyAlive;
        }

        public int Start(Side side)
        {
            return side == Side.Player ? PlayerStart : EnemyStart;
        }

        public bool IsEliminated(Side side)
        {
            return Alive(side) <= 0;
        }

        /// <summary>Доля живых пушек стороны, 0..1.</summary>
        public float Health01(Side side)
        {
            int start = Start(side);
            return start <= 0 ? 0f : (float)Alive(side) / start;
        }

        /// <summary>
        /// Уничтожает до count пушек стороны, оставляя не меньше minRemaining.
        /// Возвращает, сколько реально уничтожено.
        /// </summary>
        public int Destroy(Side side, int count, int minRemaining)
        {
            int alive = Alive(side);
            int floor = minRemaining < 0 ? 0 : minRemaining;
            int destroyed = Math.Min(count, alive - floor);
            if (destroyed <= 0)
            {
                return 0;
            }
            if (side == Side.Player)
            {
                PlayerAlive -= destroyed;
            }
            else
            {
                EnemyAlive -= destroyed;
            }
            RaiseChanged();
            return destroyed;
        }

        private void RaiseChanged()
        {
            if (Changed != null)
            {
                Changed();
            }
        }
    }
}
