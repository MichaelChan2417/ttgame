using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>Builds puzzle board UI at runtime for arbitrary N×N sizes. / 运行时按尺寸生成棋盘 UI。</summary>
    public static class BordyBoardViewBuilder
    {
        private static readonly Color ColBoardLine = new Color(0.78f, 0.75f, 0.68f);
        private static readonly Color ColCell = Color.white;
        private static readonly Color ColGivenCell = new Color(0.94f, 0.93f, 0.90f);
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);

        public static void EnsureBoard(Transform canvasRoot, BordyPuzzleData puzzle)
        {
            var board = canvasRoot.Find("Board");
            if (board != null)
            {
                // Rebuild if size mismatch
                var probe = board.Find("Cell_0_0");
                if (probe != null)
                {
                    var last = board.Find($"Cell_{puzzle.Size - 1}_{puzzle.Size - 1}");
                    if (last != null)
                        return;
                }

                Object.Destroy(board.gameObject);
            }

            LayoutMetrics metrics = ComputeMetrics(puzzle.Size);
            BuildBoard(canvasRoot, puzzle, metrics);
            RepositionChrome(canvasRoot, metrics);
        }

        public static LayoutMetrics ComputeMetrics(int size)
        {
            float cellPitch = size <= 4 ? 200f : size <= 6 ? 160f : size <= 8 ? 120f : 96f;
            float cellSize = cellPitch - 12f;
            float tokenSize = cellSize * 0.74f;
            float boardSize = cellPitch * size;
            float boardTopY = size <= 4 ? 420f : size <= 6 ? 380f : size <= 8 ? 340f : 320f;
            float boardBotY = boardTopY + boardSize + 8f;
            float actionTopY = boardBotY + 28f;
            float actionH = 84f;
            float rulesTopY = actionTopY + actionH + 24f;
            float rulesH = Mathf.Max(220f, 1080f - rulesTopY - 40f);

            return new LayoutMetrics
            {
                CellPitch = cellPitch,
                CellSize = cellSize,
                TokenSize = tokenSize,
                BoardSize = boardSize,
                BoardTopY = boardTopY,
                ActionTopY = actionTopY,
                ActionH = actionH,
                RulesTopY = rulesTopY,
                RulesH = rulesH,
            };
        }

        private static void BuildBoard(Transform parent, BordyPuzzleData puzzle, LayoutMetrics m)
        {
            var boardGo = CreatePanel("Board", parent, ColBoardLine);
            boardGo.raycastTarget = false;
            var boardRT = boardGo.rectTransform;
            Anchor(boardRT, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            boardRT.sizeDelta = new Vector2(m.BoardSize + 8, m.BoardSize + 8);
            boardRT.anchoredPosition = new Vector2(0, -m.BoardTopY);

            for (int r = 0; r < puzzle.Size; r++)
            {
                for (int c = 0; c < puzzle.Size; c++)
                {
                    Vector2 center = CellCenter(r, c, puzzle.Size, m.CellPitch, m.BoardSize);
                    var cell = CreatePanel($"Cell_{r}_{c}", boardGo.transform, ColCell);
                    cell.raycastTarget = true;
                    var cellRT = cell.rectTransform;
                    Anchor(cellRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                    cellRT.sizeDelta = new Vector2(m.CellSize, m.CellSize);
                    cellRT.anchoredPosition = center;
                    if (puzzle.IsGiven(r, c))
                        cell.color = ColGivenCell;

                    var token = CreateToken($"Token_{r}_{c}", cell.transform);
                    token.rectTransform.sizeDelta = new Vector2(m.TokenSize, m.TokenSize);
                    token.rectTransform.anchoredPosition = Vector2.zero;
                    token.enabled = false;
                }
            }
        }

        private static void RepositionChrome(Transform root, LayoutMetrics m)
        {
            var undo = root.Find("UndoButton")?.GetComponent<RectTransform>();
            if (undo != null)
            {
                undo.sizeDelta = new Vector2(440, m.ActionH);
                undo.anchoredPosition = new Vector2(-235, -m.ActionTopY);
            }

            var hint = root.Find("HintButton")?.GetComponent<RectTransform>();
            if (hint != null)
            {
                hint.sizeDelta = new Vector2(440, m.ActionH);
                hint.anchoredPosition = new Vector2(235, -m.ActionTopY);
            }

            var rules = root.Find("RulesCard")?.GetComponent<RectTransform>();
            if (rules != null)
            {
                rules.sizeDelta = new Vector2(-48, m.RulesH);
                rules.anchoredPosition = new Vector2(0, -m.RulesTopY);
            }
        }

        private static Vector2 CellCenter(int row, int col, int size, float cellPitch, float boardSize)
        {
            float x = -boardSize / 2f + cellPitch / 2f + col * cellPitch;
            float y = boardSize / 2f - cellPitch / 2f - row * cellPitch;
            return new Vector2(x, y);
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static Image CreateToken(string name, Transform parent)
        {
            var img = CreatePanel(name, parent, Color.white);
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.gameObject.AddComponent<BordyTokenView>();
            Anchor(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            return img;
        }

        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }

        public struct LayoutMetrics
        {
            public float CellPitch;
            public float CellSize;
            public float TokenSize;
            public float BoardSize;
            public float BoardTopY;
            public float ActionTopY;
            public float ActionH;
            public float RulesTopY;
            public float RulesH;
        }
    }
}
