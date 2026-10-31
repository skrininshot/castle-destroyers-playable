namespace CastleDestroyers.Destruction
{
    /// <summary>
    /// Связность на грубой сетке (ТЗ §3.6): заливка от нижнего ряда (дно под водой), всё недостигнутое — острова.
    /// Буферы выделяются в Init, Solve не аллоцирует.
    /// При loadTexels больше нуля связность считается по видимому материалу: нагрузку держат только ячейки, где видимых
    /// текселей не меньше loadTexels. Тонкие ячейки — бахрома и перемычки: они не соединяют части, а достаются соседней
    /// несущей ячейке. Иначе кусок через видимую щель или на нитке оставался бы висеть в воздухе.
    /// </summary>
    public sealed class ConnectivitySolver
    {
        private const byte CellEmpty = 0;
        private const byte CellThin = 1;
        private const byte CellLoaded = 2;

        private DestructibleMask mask;
        private int[] visitMark;
        private int[] stack;
        private byte[] cellKind;
        private int[] supported;
        private int supportedCount;
        private int loadTexels;
        private bool followThin;
        private int generation;

        /// <summary>Ячейки островов подряд; остров i занимает Cells[IslandStart[i]..IslandStart[i] + IslandSize[i]).</summary>
        public int[] Cells { get; private set; }
        public int[] IslandStart { get; private set; }
        public int[] IslandSize { get; private set; }
        public int IslandCount { get; private set; }

        public void Init(DestructibleMask destructibleMask, int maxIslands, int visibleLoadTexels = 0)
        {
            mask = destructibleMask;
            loadTexels = visibleLoadTexels;
            int cells = mask.CellsX * mask.CellsY;
            visitMark = new int[cells];
            stack = new int[cells];
            cellKind = new byte[cells];
            supported = new int[cells];
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
            ClassifyCells();
            FloodFromSupport();

            int used = 0;
            followThin = false;
            bool overflow = !CollectIslands(CellLoaded, ref used);
            if (!overflow && loadTexels > 0)
            {
                // Тонкие ячейки у опоры остаются, всё остальное тонкое — висящие крошки.
                FloodThinFromSupported();
                followThin = true;
                overflow = !CollectIslands(CellThin, ref used);
            }
            mask.ConnectivityDirty = overflow;
        }

        /// <summary>Острова, начиная с непосещённых ячеек вида kind. False, если не хватило лимита островов.</summary>
        private bool CollectIslands(byte kind, ref int used)
        {
            int total = cellKind.Length;
            for (int cell = 0; cell < total; cell++)
            {
                if (cellKind[cell] != kind || visitMark[cell] == generation)
                {
                    continue;
                }
                if (IslandCount == IslandStart.Length)
                {
                    return false;
                }
                IslandStart[IslandCount] = used;
                used = CollectIsland(cell, used);
                IslandSize[IslandCount] = used - IslandStart[IslandCount];
                IslandCount++;
            }
            return true;
        }

        private void ClassifyCells()
        {
            int[] counts = mask.CellCounts;
            int total = counts.Length;
            if (loadTexels <= 0)
            {
                for (int cell = 0; cell < total; cell++)
                {
                    cellKind[cell] = counts[cell] != 0 ? CellLoaded : CellEmpty;
                }
                return;
            }
            for (int cell = 0; cell < total; cell++)
            {
                int visible = counts[cell] != 0 ? CountVisible(cell) : 0;
                cellKind[cell] = visible >= loadTexels ? CellLoaded : (visible > 0 ? CellThin : CellEmpty);
            }
        }

        private int CountVisible(int cell)
        {
            int size = mask.CellSize;
            int width = mask.Width;
            int x0 = cell % mask.CellsX * size;
            int y0 = cell / mask.CellsX * size;
            int x1 = x0 + size < width ? x0 + size : width;
            int y1 = y0 + size < mask.Height ? y0 + size : mask.Height;
            byte[] solid = mask.Solid;
            int visible = 0;
            for (int y = y0; y < y1; y++)
            {
                int row = y * width;
                for (int x = x0; x < x1; x++)
                {
                    if (solid[row + x] >= DestructibleMask.SolidThreshold)
                    {
                        visible++;
                    }
                }
            }
            return visible;
        }

        private void FloodFromSupport()
        {
            int top = 0;
            supportedCount = 0;
            for (int cell = 0; cell < mask.CellsX; cell++)
            {
                if (cellKind[cell] == CellLoaded)
                {
                    visitMark[cell] = generation;
                    stack[top] = cell;
                    top++;
                }
            }
            while (top > 0)
            {
                top--;
                int cell = stack[top];
                supported[supportedCount] = cell;
                supportedCount++;
                top = PushNeighbours(cell, top, CellLoaded);
            }
        }

        private void FloodThinFromSupported()
        {
            int top = supportedCount;
            System.Array.Copy(supported, stack, top);
            while (top > 0)
            {
                top--;
                top = PushNeighbours(stack[top], top, CellThin);
            }
        }

        private int PushNeighbours(int cell, int top, byte kind)
        {
            int cellsX = mask.CellsX;
            int column = cell % cellsX;
            if (column > 0)
            {
                top = Push(cell - 1, top, kind);
            }
            if (column < cellsX - 1)
            {
                top = Push(cell + 1, top, kind);
            }
            if (cell >= cellsX)
            {
                top = Push(cell - cellsX, top, kind);
            }
            if (cell + cellsX < cellKind.Length)
            {
                top = Push(cell + cellsX, top, kind);
            }
            return top;
        }

        private int Push(int cell, int top, byte kind)
        {
            if (cellKind[cell] != kind || visitMark[cell] == generation)
            {
                return top;
            }
            visitMark[cell] = generation;
            stack[top] = cell;
            return top + 1;
        }

        /// <summary>Обход в ширину: Cells служит очередью и результатом. От несущей ячейки берётся и её бахрома, дальше по тонким не идём.</summary>
        private int CollectIsland(int startCell, int used)
        {
            int cellsX = mask.CellsX;
            int total = cellKind.Length;
            int head = used;
            int tail = used;
            visitMark[startCell] = generation;
            Cells[tail] = startCell;
            tail++;
            while (head < tail)
            {
                int cell = Cells[head];
                head++;
                bool takeThin = followThin || cellKind[cell] == CellLoaded;
                int column = cell % cellsX;
                if (column > 0)
                {
                    tail = Enqueue(cell - 1, tail, takeThin);
                }
                if (column < cellsX - 1)
                {
                    tail = Enqueue(cell + 1, tail, takeThin);
                }
                if (cell >= cellsX)
                {
                    tail = Enqueue(cell - cellsX, tail, takeThin);
                }
                if (cell + cellsX < total)
                {
                    tail = Enqueue(cell + cellsX, tail, takeThin);
                }
            }
            return tail;
        }

        private int Enqueue(int cell, int tail, bool takeThin)
        {
            byte kind = cellKind[cell];
            if (kind == CellEmpty || (kind == CellThin && !takeThin) || visitMark[cell] == generation)
            {
                return tail;
            }
            visitMark[cell] = generation;
            Cells[tail] = cell;
            return tail + 1;
        }
    }
}
