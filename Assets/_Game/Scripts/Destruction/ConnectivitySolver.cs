namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Связность на грубой сетке (ТЗ §3.6): заливка от нижнего ряда (дно под водой), всё недостигнутое — острова.
    /// Буферы выделяются в Init, Solve не аллоцирует.
    /// </summary>
    public sealed class ConnectivitySolver
    {
        private DestructibleMask mask;
        private int[] visitMark;
        private int[] stack;
        private int generation;

        /// <summary>Ячейки островов подряд; остров i занимает Cells[IslandStart[i]..IslandStart[i] + IslandSize[i]).</summary>
        public int[] Cells { get; private set; }
        public int[] IslandStart { get; private set; }
        public int[] IslandSize { get; private set; }
        public int IslandCount { get; private set; }

        public void Init(DestructibleMask destructibleMask, int maxIslands)
        {
            mask = destructibleMask;
            int cells = mask.CellsX * mask.CellsY;
            visitMark = new int[cells];
            stack = new int[cells];
            Cells = new int[cells];
            IslandStart = new int[maxIslands];
            IslandSize = new int[maxIslands];
        }

        /// <summary>
        /// Находит острова. Если их больше лимита, ConnectivityDirty остаётся поднятым и остаток найдётся
        /// при следующем запуске, после вырезания этих.
        /// </summary>
        public void Solve()
        {
            generation++;
            IslandCount = 0;
            FloodFromSupport();

            int[] counts = mask.CellCounts;
            int total = counts.Length;
            int used = 0;
            bool overflow = false;
            for (int cell = 0; cell < total; cell++)
            {
                if (counts[cell] == 0 || visitMark[cell] == generation)
                {
                    continue;
                }
                if (IslandCount == IslandStart.Length)
                {
                    overflow = true;
                    break;
                }
                IslandStart[IslandCount] = used;
                used = CollectIsland(cell, used);
                IslandSize[IslandCount] = used - IslandStart[IslandCount];
                IslandCount++;
            }
            mask.ConnectivityDirty = overflow;
        }

        private void FloodFromSupport()
        {
            int[] counts = mask.CellCounts;
            int top = 0;
            for (int cell = 0; cell < mask.CellsX; cell++)
            {
                if (counts[cell] != 0)
                {
                    visitMark[cell] = generation;
                    stack[top] = cell;
                    top++;
                }
            }
            int cellsX = mask.CellsX;
            int total = counts.Length;
            while (top > 0)
            {
                top--;
                int cell = stack[top];
                int column = cell % cellsX;
                if (column > 0)
                {
                    top = Push(cell - 1, top);
                }
                if (column < cellsX - 1)
                {
                    top = Push(cell + 1, top);
                }
                if (cell >= cellsX)
                {
                    top = Push(cell - cellsX, top);
                }
                if (cell + cellsX < total)
                {
                    top = Push(cell + cellsX, top);
                }
            }
        }

        private int Push(int cell, int top)
        {
            if (mask.CellCounts[cell] == 0 || visitMark[cell] == generation)
            {
                return top;
            }
            visitMark[cell] = generation;
            stack[top] = cell;
            return top + 1;
        }

        /// <summary>Обход в ширину: Cells служит очередью и результатом.</summary>
        private int CollectIsland(int startCell, int used)
        {
            int cellsX = mask.CellsX;
            int total = mask.CellCounts.Length;
            int head = used;
            int tail = used;
            visitMark[startCell] = generation;
            Cells[tail] = startCell;
            tail++;
            while (head < tail)
            {
                int cell = Cells[head];
                head++;
                int column = cell % cellsX;
                if (column > 0)
                {
                    tail = Enqueue(cell - 1, tail);
                }
                if (column < cellsX - 1)
                {
                    tail = Enqueue(cell + 1, tail);
                }
                if (cell >= cellsX)
                {
                    tail = Enqueue(cell - cellsX, tail);
                }
                if (cell + cellsX < total)
                {
                    tail = Enqueue(cell + cellsX, tail);
                }
            }
            return tail;
        }

        private int Enqueue(int cell, int tail)
        {
            if (mask.CellCounts[cell] == 0 || visitMark[cell] == generation)
            {
                return tail;
            }
            visitMark[cell] = generation;
            Cells[tail] = cell;
            return tail + 1;
        }
    }
}
