using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy.EditorTools
{
    /// <summary>Shared gameplay scene layout for tutorial and standard levels. / 共用游戏场景布局。</summary>
    public static class BordyGameplaySceneBuilder
    {
        private static readonly Color ColPageBg = new Color(0.96f, 0.95f, 0.92f);
        private static readonly Color ColBoardLine = new Color(0.78f, 0.75f, 0.68f);
        private static readonly Color ColCell = Color.white;
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);

        public static void BuildHierarchy(BordyPuzzleData puzzle, bool tutorialMode)
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            var nav = canvasGo.AddComponent<BordyNav>();
            var board = canvasGo.AddComponent<BordyBoardController>();
            var boardSo = new SerializedObject(board);
            boardSo.FindProperty("_levelId").stringValue = puzzle.Id;
            boardSo.ApplyModifiedPropertiesWithoutUndo();

            if (tutorialMode)
                canvasGo.AddComponent<BordyTutorialGuide>();

            var bg = CreatePanel("Background", canvasGo.transform, ColPageBg);
            bg.raycastTarget = false;
            Stretch(bg.rectTransform);

            float cellPitch = puzzle.Size == 4 ? 200f : 160f;
            float cellSize = puzzle.Size == 4 ? 188f : 152f;
            float tokenSize = puzzle.Size == 4 ? 140f : 116f;
            float boardSize = cellPitch * puzzle.Size;
            float boardTopY = puzzle.Size == 4 ? 420f : 380f;
            float boardBotY = boardTopY + boardSize + 8f;
            float actionTopY = boardBotY + 36f;
            float actionH = 90f;
            float rulesTopY = actionTopY + actionH + 30f;
            float rulesH = puzzle.Size == 4 ? 300f : 360f;

            BuildHeader(canvasGo.transform, nav, board, puzzle.Title);
            BuildBoard(canvasGo.transform, puzzle, cellPitch, cellSize, tokenSize, boardSize, boardTopY);
            BuildActionRow(canvasGo.transform, board, actionTopY, actionH);
            BuildRules(canvasGo.transform, rulesTopY, rulesH, tutorialMode);
        }

        private static void BuildHeader(Transform parent, BordyNav nav, BordyBoardController board, string title)
        {
            var back = CreateText("Back", parent, "←", 56, FontStyle.Normal);
            Anchor(back.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            back.rectTransform.sizeDelta = new Vector2(90, 80);
            back.rectTransform.anchoredPosition = new Vector2(40, -120);
            back.alignment = TextAnchor.MiddleCenter;
            back.color = ColInk;
            back.raycastTarget = true;
            var backBtn = back.gameObject.AddComponent<Button>();
            backBtn.targetGraphic = back;
            UnityEventTools.AddPersistentListener(backBtn.onClick, nav.BackToLevelSelect);

            var titleLabel = CreateText("Title", parent, title, 52, FontStyle.Bold);
            Anchor(titleLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            titleLabel.rectTransform.sizeDelta = new Vector2(0, 80);
            titleLabel.rectTransform.anchoredPosition = new Vector2(0, -120);
            titleLabel.alignment = TextAnchor.MiddleCenter;
            titleLabel.color = ColInk;

            var timer = CreateText("Timer", parent, "◷ 0:00", 36, FontStyle.Normal);
            Anchor(timer.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            timer.rectTransform.sizeDelta = new Vector2(260, 64);
            timer.rectTransform.anchoredPosition = new Vector2(0, -230);
            timer.alignment = TextAnchor.MiddleCenter;
            timer.color = ColInk;
            timer.gameObject.AddComponent<BordyTimer>();

            var reset = CreateClickablePill("ResetPill", parent, "重置", ColPill, ColMuted);
            Anchor(reset.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            reset.rectTransform.sizeDelta = new Vector2(180, 76);
            reset.rectTransform.anchoredPosition = new Vector2(-60, -228);
            UnityEventTools.AddPersistentListener(reset.GetComponent<Button>().onClick, board.ResetPuzzle);
        }

        private static void BuildBoard(Transform parent, BordyPuzzleData puzzle, float cellPitch, float cellSize, float tokenSize, float boardSize, float boardTopY)
        {
            var board = CreatePanel("Board", parent, ColBoardLine);
            board.raycastTarget = false;
            var boardRT = board.rectTransform;
            Anchor(boardRT, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            boardRT.sizeDelta = new Vector2(boardSize + 8, boardSize + 8);
            boardRT.anchoredPosition = new Vector2(0, -boardTopY);

            for (int r = 0; r < puzzle.Size; r++)
            {
                for (int c = 0; c < puzzle.Size; c++)
                {
                    Vector2 center = CellCenter(r, c, puzzle.Size, cellPitch, boardSize);
                    var cell = CreatePanel($"Cell_{r}_{c}", board.transform, ColCell);
                    cell.raycastTarget = true;
                    var cellRT = cell.rectTransform;
                    Anchor(cellRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                    cellRT.sizeDelta = new Vector2(cellSize, cellSize);
                    cellRT.anchoredPosition = center;
                    if (puzzle.IsGiven(r, c))
                        cell.color = new Color(0.94f, 0.93f, 0.90f);

                    var token = CreateToken($"Token_{r}_{c}", cell.transform);
                    token.rectTransform.sizeDelta = new Vector2(tokenSize, tokenSize);
                    token.rectTransform.anchoredPosition = Vector2.zero;
                    token.enabled = false;
                }
            }

            foreach (var e in puzzle.Edges)
            {
                Vector2 a = CellCenter(e.Row, e.Col, puzzle.Size, cellPitch, boardSize);
                Vector2 b = e.Horizontal
                    ? CellCenter(e.Row, e.Col + 1, puzzle.Size, cellPitch, boardSize)
                    : CellCenter(e.Row + 1, e.Col, puzzle.Size, cellPitch, boardSize);
                Vector2 mid = (a + b) * 0.5f;
                string symbol = e.MustMatch ? "=" : "×";
                var sym = CreateText($"Edge_{e.Row}_{e.Col}_{(e.Horizontal ? "H" : "V")}",
                    board.transform, symbol, symbol == "=" ? 44 : 40, FontStyle.Bold);
                Anchor(sym.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                sym.rectTransform.sizeDelta = new Vector2(48, 48);
                sym.rectTransform.anchoredPosition = mid;
                sym.alignment = TextAnchor.MiddleCenter;
                sym.color = ColInk;
                sym.raycastTarget = false;
            }
        }

        private static Vector2 CellCenter(int row, int col, int size, float cellPitch, float boardSize)
        {
            float x = -boardSize / 2f + cellPitch / 2f + col * cellPitch;
            float y = boardSize / 2f - cellPitch / 2f - row * cellPitch;
            return new Vector2(x, y);
        }

        private static void BuildActionRow(Transform parent, BordyBoardController board, float actionTopY, float actionH)
        {
            var undo = CreateClickablePill("UndoButton", parent, "撤销", ColPill, ColMuted);
            Anchor(undo.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            undo.rectTransform.sizeDelta = new Vector2(440, actionH);
            undo.rectTransform.anchoredPosition = new Vector2(-235, -actionTopY);
            UnityEventTools.AddPersistentListener(undo.GetComponent<Button>().onClick, board.Undo);

            var hint = CreateClickablePill("HintButton", parent, "提示", ColPill, ColMuted);
            Anchor(hint.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hint.rectTransform.sizeDelta = new Vector2(440, actionH);
            hint.rectTransform.anchoredPosition = new Vector2(235, -actionTopY);
            UnityEventTools.AddPersistentListener(hint.GetComponent<Button>().onClick, board.Hint);
        }

        private static void BuildRules(Transform parent, float rulesTopY, float rulesH, bool tutorialMode)
        {
            var card = CreatePanel("RulesCard", parent, Color.white);
            card.raycastTarget = false;
            var cardRT = card.rectTransform;
            Anchor(cardRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            cardRT.sizeDelta = new Vector2(-48, rulesH);
            cardRT.anchoredPosition = new Vector2(0, -rulesTopY);

            var heading = CreateText("RulesHeading", card.transform, tutorialMode ? "引导提示" : "游戏玩法", 40, FontStyle.Bold);
            Anchor(heading.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
            heading.rectTransform.sizeDelta = new Vector2(-48, 70);
            heading.rectTransform.anchoredPosition = new Vector2(28, -24);
            heading.alignment = TextAnchor.MiddleLeft;
            heading.color = ColInk;

            string rules = tutorialMode
                ? "•  跟随底部卡片完成教学步骤。\n•  4×4 棋盘每行/列各 2 个太阳、2 个月亮。\n•  × 要相反，= 要相同，不能连出 3 个一样。"
                : "•  填充网格，使每个格子都有一个太阳或一个月亮。\n•  每行（和每列）最多 2 个相同图案相邻，且太阳与月亮数量相等。\n•  由 = 分隔的格子必须相同；由 × 分隔的格子必须相反。";

            var body = CreateText("RulesBody", card.transform, rules, 28, FontStyle.Normal);
            Anchor(body.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            body.rectTransform.offsetMin = new Vector2(28, 28);
            body.rectTransform.offsetMax = new Vector2(-28, -100);
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.color = ColMuted;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
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

        private static Image CreatePill(string name, Transform parent, string label, Color fill, Color textColor)
        {
            var img = CreatePanel(name, parent, fill);
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;
            var t = CreateText("Text", img.transform, label, 30, FontStyle.Normal);
            t.alignment = TextAnchor.MiddleCenter;
            t.color = textColor;
            Stretch(t.rectTransform);
            return img;
        }

        private static Image CreateClickablePill(string name, Transform parent, string label, Color fill, Color textColor)
        {
            var img = CreatePill(name, parent, label, fill, textColor);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            return img;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.color = ColInk;
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }
    }
}
