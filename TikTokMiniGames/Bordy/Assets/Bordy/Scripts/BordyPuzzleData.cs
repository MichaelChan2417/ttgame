namespace Bordy
{
    /// <summary>Immutable puzzle definition (size, solution, givens, edge constraints). / 谜题数据。</summary>
    public sealed class BordyPuzzleData
    {
        public const int Sun = 0;
        public const int Moon = 1;
        public const int Empty = -1;

        public readonly string Id;
        public readonly string Title;
        public readonly int Size;
        public readonly int[,] Solution;
        public readonly bool[,] Givens;
        public readonly EdgeConstraint[] Edges;

        public BordyPuzzleData(string id, string title, int[,] solution, bool[,] givens, EdgeConstraint[] edges)
        {
            Id = id;
            Title = title;
            Size = solution.GetLength(0);
            Solution = solution;
            Givens = givens;
            Edges = edges;
        }

        public bool IsGiven(int row, int col) => Givens[row, col];
        public int GivenValue(int row, int col) => Solution[row, col];
        public int TargetPerLine => Size / 2;
    }

    /// <summary>"=" → <see cref="MustMatch"/> true; "×" → false.</summary>
    public readonly struct EdgeConstraint
    {
        public readonly int Row;
        public readonly int Col;
        public readonly bool Horizontal;
        public readonly bool MustMatch;

        public EdgeConstraint(int row, int col, bool horizontal, bool mustMatch)
        {
            Row = row;
            Col = col;
            Horizontal = horizontal;
            MustMatch = mustMatch;
        }
    }
}
