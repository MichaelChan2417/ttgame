using System.Collections.Generic;

namespace Bordy
{
    /// <summary>All playable levels and the tutorial puzzle. / 所有可玩关卡与新手谜题。</summary>
    public static class BordyLevelCatalog
    {
        public const string TutorialId = "tutorial";
        public const string DailyId = "daily";
        public const string Level1Id = "level1";

        public const string TutorialScene = "Tutorial";
        public const string Level1Scene = "MainMenu";
        // Daily reuses the 6×6 gameplay scene (same baked board + edge symbols as Level 1).
        // 每日挑战复用 6×6 游戏场景（与第一关相同的烘焙棋盘与边符号）。
        public const string DailyScene = "MainMenu";
        public const string LevelSelectScene = "LevelSelect";
        public const string HomeScene = "Home";

        private static readonly Dictionary<string, BordyPuzzleData> Levels = new Dictionary<string, BordyPuzzleData>
        {
            { TutorialId, BuildTutorial() },
            { DailyId, BuildDaily() },
            { Level1Id, BuildLevel1() },
        };

        public static BordyPuzzleData Get(string id) => Levels[id];

        public static bool TryGet(string id, out BordyPuzzleData puzzle) => Levels.TryGetValue(id, out puzzle);

        private static BordyPuzzleData BuildTutorial()
        {
            const int s = BordyPuzzleData.Sun;
            const int m = BordyPuzzleData.Moon;

            // Valid solution: each row/col has 2 suns + 2 moons and every edge constraint
            // (E3 below requires (1,1) == (1,2)) is satisfied. The first row reads m s s m,
            // so the tutorial guides (0,2)→Sun then (0,3)→Moon.
            // 合法解：每行/列各 2 太阳 2 月亮，且满足所有边约束（E3 要求 (1,1)==(1,2)）。
            // 第一行是 月 日 日 月，因此引导先把 (0,2) 点成太阳，再把 (0,3) 点成月亮。
            var solution = new[,]
            {
                { m, s, s, m },
                { s, m, m, s },
                { s, m, s, m },
                { m, s, m, s },
            };

            var givens = new[,]
            {
                { true,  true,  false, false },
                { true,  false, false, false },
                { false, false, false, true  },
                { false, false, false, false },
            };

            var edges = new[]
            {
                new EdgeConstraint(0, 0, true,  false),
                new EdgeConstraint(0, 1, false, false),
                new EdgeConstraint(1, 1, true,  true),
                new EdgeConstraint(2, 0, false, false),
            };

            return new BordyPuzzleData(TutorialId, "新手引导", solution, givens, edges);
        }

        private static BordyPuzzleData BuildLevel1()
        {
            const int s = BordyPuzzleData.Sun;
            const int m = BordyPuzzleData.Moon;

            var solution = new[,]
            {
                { m, m, s, m, s, s },
                { s, m, m, s, m, s },
                { m, s, s, m, s, m },
                { s, s, m, s, m, m },
                { s, m, m, s, m, s },
                { m, s, s, m, s, m },
            };

            var givens = new[,]
            {
                { true,  false, true,  false, false, true  },
                { true,  false, false, true,  false, false },
                { false, true,  false, false, true,  false },
                { true,  false, false, true,  true,  false },
                { false, false, true,  false, false, true  },
                { false, true,  true,  false, false, true  },
            };

            var edges = new[]
            {
                new EdgeConstraint(0, 3, true,  false),
                new EdgeConstraint(0, 4, false, false),
                new EdgeConstraint(1, 4, false, false),
                new EdgeConstraint(3, 1, false, false),
                new EdgeConstraint(4, 1, false, false),
                new EdgeConstraint(5, 1, true,  true),
            };

            return new BordyPuzzleData(Level1Id, "第一关", solution, givens, edges);
        }

        // -----------------------------------------------------------------
        // Daily challenge. Shared fixed template for ALL players (no backend yet).
        // Solution is the colour-complement of Level 1, so it satisfies the SAME edge
        // constraints — letting it reuse the 6×6 scene whose = / × symbols are baked at
        // Level 1's edge positions. Different givens make it feel distinct.
        //
        // 每日挑战：所有玩家共用的固定模板（暂无后端）。解是第一关解的反色，因此满足
        // 完全相同的边约束，可复用 6×6 场景（其 = / × 符号按第一关边位置烘焙）；给定格
        // 不同，使其手感有别。
        // -----------------------------------------------------------------
        private static BordyPuzzleData BuildDaily()
        {
            const int s = BordyPuzzleData.Sun;
            const int m = BordyPuzzleData.Moon;

            // Colour-complement of Level 1's solution (sun <-> moon).
            // 第一关解的反色（太阳 <-> 月亮）。
            var solution = new[,]
            {
                { s, s, m, s, m, m },
                { m, s, s, m, s, m },
                { s, m, m, s, m, s },
                { m, m, s, m, s, s },
                { m, s, s, m, s, m },
                { s, m, m, s, m, s },
            };

            // Distinct clue layout (positions differ from Level 1).
            // 不同的给定格布局（与第一关不同）。
            var givens = new[,]
            {
                { true,  true,  false, false, false, true  },
                { false, false, true,  false, true,  false },
                { true,  false, false, true,  false, false },
                { false, false, true,  false, false, true  },
                { false, true,  false, false, true,  false },
                { true,  false, false, false, true,  true  },
            };

            // Same edge positions / types as Level 1 (the scene's baked symbols match these).
            // 与第一关相同的边位置/类型（场景烘焙的符号与之一致）。
            var edges = new[]
            {
                new EdgeConstraint(0, 3, true,  false),
                new EdgeConstraint(0, 4, false, false),
                new EdgeConstraint(1, 4, false, false),
                new EdgeConstraint(3, 1, false, false),
                new EdgeConstraint(4, 1, false, false),
                new EdgeConstraint(5, 1, true,  true),
            };

            return new BordyPuzzleData(DailyId, "每日挑战", solution, givens, edges);
        }
    }
}
