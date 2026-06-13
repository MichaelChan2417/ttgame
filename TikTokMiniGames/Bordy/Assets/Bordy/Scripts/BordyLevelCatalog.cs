using System.Collections.Generic;

namespace Bordy
{
    /// <summary>All playable levels and the tutorial puzzle. / 所有可玩关卡与新手谜题。</summary>
    public static class BordyLevelCatalog
    {
        public const string TutorialId = "tutorial";
        public const string Level1Id = "level1";

        public const string TutorialScene = "Tutorial";
        public const string Level1Scene = "MainMenu";
        public const string LevelSelectScene = "LevelSelect";
        public const string HomeScene = "Home";

        private static readonly Dictionary<string, BordyPuzzleData> Levels = new Dictionary<string, BordyPuzzleData>
        {
            { TutorialId, BuildTutorial() },
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
    }
}
