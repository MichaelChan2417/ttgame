using System;

namespace Bordy
{
    /// <summary>
    /// Backtracking solver for sun/moon puzzles. Used by the generator (uniqueness) and QA.
    /// 太阳/月亮谜题回溯求解器，供生成器验证唯一解与 QA 使用。
    /// </summary>
    public static class BordyPuzzleSolver
    {
        public const int Empty = BordyPuzzleData.Empty;

        /// <summary>Count solutions up to <paramref name="limit"/> (stop early at limit). / 数解个数，达到 limit 提前停止。</summary>
        public static int CountSolutions(int size, int[,] values, bool[,] fixedCells, EdgeConstraint[] edges, int limit = 2)
        {
            if (limit < 1)
                limit = 1;

            EnsureScratch(size);
            CopyToScratch(size, values);
            int count = 0;
            Solve(size, _scratch, fixedCells, edges, ref count, limit);
            return count;
        }

        public static bool HasUniqueSolution(int size, int[,] values, bool[,] fixedCells, EdgeConstraint[] edges)
            => CountSolutions(size, values, fixedCells, edges, 2) == 1;

        /// <summary>Build partial state from solution+givens into scratch and test uniqueness (no alloc).</summary>
        public static bool HasUniqueFromGivens(int size, int[,] solution, bool[,] givens, EdgeConstraint[] edges)
        {
            EnsureScratch(size);
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                    _scratch[r, c] = givens[r, c] ? solution[r, c] : Empty;
            }

            int count = 0;
            Solve(size, _scratch, givens, edges, ref count, 2);
            return count == 1;
        }

        private static int[,] _scratch;

        private static void EnsureScratch(int size)
        {
            if (_scratch == null || _scratch.GetLength(0) < size)
                _scratch = new int[size, size];
        }

        private static void CopyToScratch(int size, int[,] values)
        {
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    _scratch[r, c] = values[r, c];
        }

        public static bool IsCompleteSolutionValid(int size, int[,] solution, EdgeConstraint[] edges)
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (solution[r, c] != BordyPuzzleData.Sun && solution[r, c] != BordyPuzzleData.Moon)
                        return false;
                }
            }

            for (int r = 0; r < size; r++)
            {
                if (!LineCompleteValid(solution, r, true, size))
                    return false;
            }

            for (int c = 0; c < size; c++)
            {
                if (!LineCompleteValid(solution, c, false, size))
                    return false;
            }

            foreach (var edge in edges)
            {
                int aRow = edge.Row;
                int aCol = edge.Col;
                int bRow = edge.Horizontal ? edge.Row : edge.Row + 1;
                int bCol = edge.Horizontal ? edge.Col + 1 : edge.Col;
                int a = solution[aRow, aCol];
                int b = solution[bRow, bCol];
                if (edge.MustMatch && a != b)
                    return false;
                if (!edge.MustMatch && a == b)
                    return false;
            }

            return true;
        }

        private static void Solve(int size, int[,] state, bool[,] fixedCells, EdgeConstraint[] edges, ref int count, int limit)
        {
            if (count >= limit)
                return;

            if (!FindEmpty(size, state, out int row, out int col))
            {
                if (IsCompleteValid(size, state, edges))
                    count++;
                return;
            }

            if (!CellDomainValid(size, state, row, col, edges))
                return;

            for (int v = BordyPuzzleData.Sun; v <= BordyPuzzleData.Moon; v++)
            {
                state[row, col] = v;
                if (!CellDomainValid(size, state, row, col, edges))
                    continue;

                Solve(size, state, fixedCells, edges, ref count, limit);
                if (count >= limit)
                    return;
            }

            state[row, col] = Empty;
        }

        private static bool FindEmpty(int size, int[,] state, out int row, out int col)
        {
            int bestRow = -1;
            int bestCol = -1;
            int bestScore = int.MaxValue;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (state[r, c] != Empty)
                        continue;

                    int score = DomainSize(size, state, r, c);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestRow = r;
                        bestCol = c;
                    }
                }
            }

            row = bestRow;
            col = bestCol;
            return bestRow >= 0;
        }

        private static int DomainSize(int size, int[,] state, int row, int col)
        {
            int n = 0;
            for (int v = BordyPuzzleData.Sun; v <= BordyPuzzleData.Moon; v++)
            {
                state[row, col] = v;
                if (CellDomainValid(size, state, row, col, null))
                    n++;
            }

            state[row, col] = Empty;
            return n == 0 ? 999 : n;
        }

        private static bool CellDomainValid(int size, int[,] state, int row, int col, EdgeConstraint[] edges)
        {
            if (!LinePartialValid(state, row, true, size))
                return false;
            if (!LinePartialValid(state, col, false, size))
                return false;

            if (edges == null)
                return true;

            foreach (var edge in edges)
            {
                int aRow = edge.Row;
                int aCol = edge.Col;
                int bRow = edge.Horizontal ? edge.Row : edge.Row + 1;
                int bCol = edge.Horizontal ? edge.Col + 1 : edge.Col;

                bool touches = (aRow == row && aCol == col) || (bRow == row && bCol == col);
                if (!touches)
                    continue;

                int a = state[aRow, aCol];
                int b = state[bRow, bCol];
                if (a == Empty || b == Empty)
                    continue;

                if (edge.MustMatch && a != b)
                    return false;
                if (!edge.MustMatch && a == b)
                    return false;
            }

            return true;
        }

        private static bool IsCompleteValid(int size, int[,] state, EdgeConstraint[] edges)
        {
            for (int r = 0; r < size; r++)
            {
                if (!LineCompleteValid(state, r, true, size))
                    return false;
            }

            for (int c = 0; c < size; c++)
            {
                if (!LineCompleteValid(state, c, false, size))
                    return false;
            }

            foreach (var edge in edges)
            {
                int aRow = edge.Row;
                int aCol = edge.Col;
                int bRow = edge.Horizontal ? edge.Row : edge.Row + 1;
                int bCol = edge.Horizontal ? edge.Col + 1 : edge.Col;
                int a = state[aRow, aCol];
                int b = state[bRow, bCol];
                if (edge.MustMatch && a != b)
                    return false;
                if (!edge.MustMatch && a == b)
                    return false;
            }

            return true;
        }

        private static bool LineCompleteValid(int[,] state, int index, bool horizontal, int size)
        {
            int sun = 0;
            int moon = 0;
            for (int i = 0; i < size; i++)
            {
                int v = Read(state, index, i, horizontal);
                if (v == BordyPuzzleData.Sun) sun++;
                else if (v == BordyPuzzleData.Moon) moon++;
            }

            if (sun != moon)
                return false;

            for (int i = 0; i <= size - 3; i++)
            {
                int a = Read(state, index, i, horizontal);
                int b = Read(state, index, i + 1, horizontal);
                int d = Read(state, index, i + 2, horizontal);
                if (a == b && b == d)
                    return false;
            }

            return true;
        }

        private static bool LinePartialValid(int[,] state, int index, bool horizontal, int size)
        {
            int sun = 0;
            int moon = 0;
            int empty = 0;
            for (int i = 0; i < size; i++)
            {
                int v = Read(state, index, i, horizontal);
                if (v == BordyPuzzleData.Sun) sun++;
                else if (v == BordyPuzzleData.Moon) moon++;
                else empty++;
            }

            int target = size / 2;
            if (sun > target || moon > target)
                return false;
            if (sun + empty < target || moon + empty < target)
                return false;

            for (int i = 0; i <= size - 3; i++)
            {
                int a = Read(state, index, i, horizontal);
                int b = Read(state, index, i + 1, horizontal);
                int d = Read(state, index, i + 2, horizontal);
                if (a != Empty && a == b && b == d)
                    return false;
            }

            return true;
        }

        private static int Read(int[,] state, int index, int offset, bool horizontal)
            => horizontal ? state[index, offset] : state[offset, index];
    }
}
