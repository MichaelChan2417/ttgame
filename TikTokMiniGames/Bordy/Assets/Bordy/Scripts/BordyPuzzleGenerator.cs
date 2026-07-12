using System;
using System.Collections.Generic;

namespace Bordy
{
    /// <summary>
    /// Procedural puzzle generator: solution → edges → givens (unique-solution peel).
    /// 程序化谜题生成：完整解 → 边约束 → 给定格（揭开法保唯一解）。
    /// </summary>
    public static class BordyPuzzleGenerator
    {
        public sealed class GenConfig
        {
            public int Size = 6;
            public double GivenRatio = 0.42;
            public double EdgeDensity = 0.20;
            public int MaxAttempts = 80;
            public int Seed;
        }

        public sealed class GenResult
        {
            public int[,] Solution;
            public bool[,] Givens;
            public EdgeConstraint[] Edges;
            public double DifficultyScore;
        }

        /// <summary>Difficulty ramps from easy (low) to hard (high). Used to order campaign levels.</summary>
        public static GenConfig ConfigForDifficulty(double t)
        {
            t = Clamp01(t);
            var cfg = new GenConfig { Seed = (int)(t * 1_000_000) };

            if (t < 0.40)
            {
                cfg.Size = 6;
                cfg.GivenRatio = Lerp(0.50, 0.38, t / 0.40);
                cfg.EdgeDensity = Lerp(0.12, 0.22, t / 0.40);
            }
            else if (t < 0.75)
            {
                cfg.Size = 8;
                cfg.GivenRatio = Lerp(0.42, 0.30, (t - 0.40) / 0.35);
                cfg.EdgeDensity = Lerp(0.16, 0.26, (t - 0.40) / 0.35);
            }
            else
            {
                // 10×10 peel + uniqueness is very slow in Editor; cap at 8×8 for responsive generation.
                cfg.Size = 8;
                cfg.GivenRatio = Lerp(0.36, 0.30, (t - 0.75) / 0.25);
                cfg.EdgeDensity = Lerp(0.20, 0.28, (t - 0.75) / 0.25);
            }

            if (cfg.Size >= 8)
                cfg.GivenRatio = Math.Max(cfg.GivenRatio, 0.34);

            return cfg;
        }

        public static bool TryGenerate(GenConfig cfg, out GenResult result)
        {
            result = null;
            var rng = new System.Random(cfg.Seed);

            for (int attempt = 0; attempt < cfg.MaxAttempts; attempt++)
            {
                if (!TryGenerateSolution(cfg.Size, rng, out var solution))
                    continue;

                var edges = GenerateEdges(solution, cfg.EdgeDensity, rng);
                if (!BordyPuzzleSolver.IsCompleteSolutionValid(cfg.Size, solution, edges))
                    continue;

                if (!TryGenerateGivens(cfg.Size, solution, edges, cfg.GivenRatio, rng, out var givens))
                    continue;

                result = new GenResult
                {
                    Solution = solution,
                    Givens = givens,
                    Edges = edges,
                    DifficultyScore = ScoreDifficulty(cfg.Size, givens, edges),
                };
                return true;
            }

            return false;
        }

        /// <summary>Generate a batch sorted by difficulty (easy → hard).</summary>
        public static List<GenResult> GenerateCampaignBatch(int count, int baseSeed = 20260709,
            Action<int, int, string> onProgress = null)
        {
            var raw = new List<GenResult>(count);
            for (int i = 0; i < count; i++)
            {
                double t = count <= 1 ? 0.5 : (double)i / (count - 1);
                var cfg = ConfigForDifficulty(t);
                cfg.Seed = baseSeed + i * 9973;
                onProgress?.Invoke(i, count, $"Level {i + 1}/{count} ({cfg.Size}×{cfg.Size})…");

                if (!TryGenerate(cfg, out var puzzle))
                {
                    cfg.GivenRatio = Math.Min(0.58, cfg.GivenRatio + 0.08);
                    cfg.Seed += 17;
                    if (!TryGenerate(cfg, out puzzle))
                        throw new InvalidOperationException($"Failed to generate level {i + 1} (size={cfg.Size}).");
                }

                raw.Add(puzzle);
            }

            onProgress?.Invoke(count, count, "Sorting by difficulty…");
            raw.Sort((a, b) => a.DifficultyScore.CompareTo(b.DifficultyScore));
            return raw;
        }

        private static bool TryGenerateSolution(int size, System.Random rng, out int[,] solution)
        {
            solution = new int[size, size];
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                    solution[r, c] = BordyPuzzleData.Empty;
            }

            var order = new List<(int r, int c)>(size * size);
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    order.Add((r, c));
            Shuffle(order, rng);

            return FillSolution(size, solution, order, 0, rng);
        }

        private static bool FillSolution(int size, int[,] grid, List<(int r, int c)> order, int idx, System.Random rng)
        {
            if (idx >= order.Count)
                return true;

            var (r, c) = order[idx];
            var values = new[] { BordyPuzzleData.Sun, BordyPuzzleData.Moon };
            if (rng.Next(2) == 1)
                Array.Reverse(values);

            foreach (int v in values)
            {
                grid[r, c] = v;
                if (CellOkForSolution(size, grid, r, c))
                {
                    if (FillSolution(size, grid, order, idx + 1, rng))
                        return true;
                }
            }

            grid[r, c] = BordyPuzzleData.Empty;
            return false;
        }

        private static bool CellOkForSolution(int size, int[,] grid, int row, int col)
        {
            return LinePartialOk(grid, row, true, size)
                && LinePartialOk(grid, col, false, size);
        }

        private static bool LinePartialOk(int[,] grid, int index, bool horizontal, int size)
        {
            int sun = 0;
            int moon = 0;
            int empty = 0;
            for (int i = 0; i < size; i++)
            {
                int v = horizontal ? grid[index, i] : grid[i, index];
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
                int a = horizontal ? grid[index, i] : grid[i, index];
                int b = horizontal ? grid[index, i + 1] : grid[i + 1, index];
                int d = horizontal ? grid[index, i + 2] : grid[i + 2, index];
                if (a != BordyPuzzleData.Empty && a == b && b == d)
                    return false;
            }

            return true;
        }

        private static EdgeConstraint[] GenerateEdges(int[,] solution, double density, System.Random rng)
        {
            int size = solution.GetLength(0);
            var pool = new List<EdgeConstraint>();
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (c + 1 < size)
                    {
                        bool match = solution[r, c] == solution[r, c + 1];
                        pool.Add(new EdgeConstraint(r, c, true, match));
                    }

                    if (r + 1 < size)
                    {
                        bool match = solution[r, c] == solution[r + 1, c];
                        pool.Add(new EdgeConstraint(r, c, false, match));
                    }
                }
            }

            Shuffle(pool, rng);
            int pick = Math.Max(2, (int)Math.Round(pool.Count * Clamp01(density)));
            pick = Math.Min(pick, pool.Count);
            var edges = new EdgeConstraint[pick];
            for (int i = 0; i < pick; i++)
                edges[i] = pool[i];
            return edges;
        }

        private static bool TryGenerateGivens(int size, int[,] solution, EdgeConstraint[] edges, double givenRatio,
            System.Random rng, out bool[,] givens)
        {
            givens = new bool[size, size];
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    givens[r, c] = true;

            var cells = new List<(int r, int c)>(size * size);
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    cells.Add((r, c));
            Shuffle(cells, rng);

            int targetHidden = Math.Max(1, (int)Math.Round(size * size * (1.0 - Clamp01(givenRatio))));
            int peelsSinceCheck = 0;
            int checkInterval = size >= 8 ? 4 : 2;

            foreach (var (r, c) in cells)
            {
                int hidden = CountHidden(givens, size);
                if (hidden >= targetHidden)
                    break;

                givens[r, c] = false;
                peelsSinceCheck++;

                bool shouldCheck = peelsSinceCheck >= checkInterval || hidden + 1 >= targetHidden;
                if (!shouldCheck)
                    continue;

                if (!HasUniqueFromGivens(size, solution, givens, edges))
                {
                    givens[r, c] = true;
                }

                peelsSinceCheck = 0;
            }

            if (CountGiven(givens, size) < 2)
                return false;

            return HasUniqueFromGivens(size, solution, givens, edges);
        }

        private static bool HasUniqueFromGivens(int size, int[,] solution, bool[,] givens, EdgeConstraint[] edges)
            => BordyPuzzleSolver.HasUniqueFromGivens(size, solution, givens, edges);

        private static int CountGiven(bool[,] givens, int size)
        {
            int n = 0;
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                    if (givens[r, c])
                        n++;
            return n;
        }

        private static int CountHidden(bool[,] givens, int size) => size * size - CountGiven(givens, size);

        private static double ScoreDifficulty(int size, bool[,] givens, EdgeConstraint[] edges)
        {
            int given = CountGiven(givens, size);
            double givenRatio = (double)given / (size * size);
            double edgeFactor = edges.Length / (double)(2 * size * (size - 1));
            return size * 2.0 + (1.0 - givenRatio) * 10.0 + edgeFactor * 4.0;
        }

        private static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static double Lerp(double a, double b, double t) => a + (b - a) * Clamp01(t);
        private static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }
}
