using System;

namespace Bordy
{
    /// <summary>JSON bundle for campaign levels (StreamingAssets or Resources). / 闯关关卡 JSON 包。</summary>
    [Serializable]
    public class BordyCampaignBundleDto
    {
        public int version = 1;
        public BordyCampaignLevelDto[] levels;
    }

    [Serializable]
    public class BordyCampaignLevelDto
    {
        public string id;
        public int index;
        public string tier;
        public int size = 6;
        public float difficulty;
        public int[] solution;
        public bool[] givens;
        public BordyEdgeDto[] edges;

        public bool IsValid()
        {
            int n = size;
            return !string.IsNullOrEmpty(id)
                && index > 0
                && solution != null && solution.Length == n * n
                && givens != null && givens.Length == n * n;
        }

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

            string title = $"Level {index}";
            return new BordyPuzzleData(id, title, sol, giv, e);
        }

        public static BordyCampaignLevelDto FromPuzzle(string id, int index, float difficulty, int[,] solution, bool[,] givens,
            EdgeConstraint[] edges)
        {
            int n = solution.GetLength(0);
            var dto = new BordyCampaignLevelDto
            {
                id = id,
                index = index,
                size = n,
                difficulty = difficulty,
                solution = new int[n * n],
                givens = new bool[n * n],
            };

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    int i = r * n + c;
                    dto.solution[i] = solution[r, c];
                    dto.givens[i] = givens[r, c];
                }
            }

            int m = edges?.Length ?? 0;
            dto.edges = new BordyEdgeDto[m];
            for (int k = 0; k < m; k++)
            {
                dto.edges[k] = new BordyEdgeDto
                {
                    row = edges[k].Row,
                    col = edges[k].Col,
                    horizontal = edges[k].Horizontal,
                    mustMatch = edges[k].MustMatch,
                };
            }

            return dto;
        }
    }
}
