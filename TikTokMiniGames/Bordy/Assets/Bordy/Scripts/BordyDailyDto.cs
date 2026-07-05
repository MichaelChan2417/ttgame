using System;

namespace Bordy
{
    /// <summary>
    /// Wire format for a daily puzzle served as JSON (one file per UTC day). Everything the puzzle
    /// needs — solution, given cells, and the = / × edges — lives here, so every player who fetches
    /// the same file gets an identical board. Arrays are flattened row-major.
    ///
    /// 每日题目的 JSON 传输格式（每个 UTC 日一份）。题目所需的一切——解、给定格、= / × 边——都在
    /// 这里，因此拿到同一文件的所有玩家看到完全相同的盘。数组按行主序展平。
    ///
    /// Example / 示例:
    /// {
    ///   "date": "2026-06-13",
    ///   "size": 6,
    ///   "solution": [1,1,0,1,0,0, 0,1,1,0,1,0, ...36 ints, 0=sun 1=moon...],
    ///   "givens":   [true,false,true,... 36 bools ...],
    ///   "edges": [ {"row":0,"col":3,"horizontal":true,"mustMatch":false}, ... ]
    /// }
    /// </summary>
    [Serializable]
    public class BordyDailyDto
    {
        public string date;
        public int size = 6;
        public int[] solution;          // row-major, 0=sun 1=moon / 行主序，0太阳 1月亮
        public bool[] givens;           // row-major / 行主序
        public BordyEdgeDto[] edges;

        public bool IsValid()
        {
            int n = size;
            return n > 0
                && solution != null && solution.Length == n * n
                && givens != null && givens.Length == n * n;
        }

        /// <summary>Convert the wire DTO into the in-game puzzle model. / 把传输 DTO 转成游戏内谜题模型。</summary>
        public BordyPuzzleData ToPuzzle()
        {
            int n = size;
            var sol = new int[n, n];
            var giv = new bool[n, n];
            for (int i = 0; i < n * n; i++)
            {
                sol[i / n, i % n] = solution[i];
                giv[i / n, i % n] = givens[i];
            }

            int m = edges?.Length ?? 0;
            var e = new EdgeConstraint[m];
            for (int k = 0; k < m; k++)
                e[k] = new EdgeConstraint(edges[k].row, edges[k].col, edges[k].horizontal, edges[k].mustMatch);

            return new BordyPuzzleData(BordyLevelCatalog.DailyId, "Daily Challenge", sol, giv, e);
        }
    }

    [Serializable]
    public class BordyEdgeDto
    {
        public int row;
        public int col;
        public bool horizontal;  // true = between (row,col) and (row,col+1); false = vertical
        public bool mustMatch;   // true = "=" (same); false = "×" (differ)
    }
}
