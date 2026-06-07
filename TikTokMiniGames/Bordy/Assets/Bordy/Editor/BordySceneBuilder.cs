using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Rebuilds the <c>MainMenu.unity</c> scene entirely from code so the layout never
    /// drifts. The scene reproduces a "Bordy" puzzle board: a 6×6 grid of
    /// sun / moon cells plus the "=" / "×" edge constraints, a header (timer, difficulty,
    /// reset), an undo / hint row, and the rules panel.
    /// Triggered via <c>Bordy → Rebuild MainMenu Scene</c>, or <see cref="BordySetup.RunAll"/>.
    ///
    /// 通过纯代码重建 <c>MainMenu.unity</c> 场景，避免布局飘移。场景还原 "Bordy"
    /// 谜题面板：6×6 的太阳 / 月亮格子，外加 "=" / "×" 边约束、头部（计时、难度、重置）、
    /// 撤销 / 提示按钮行与规则面板。
    /// 注意：太阳 / 月亮目前用纯色圆形占位（图案暂时跳过）。
    /// </summary>
    public static class BordySceneBuilder
    {
        private const string ScenePath = "Assets/Bordy/Scenes/MainMenu.unity";

        // -----------------------------------------------------------------
        // Board model. 0 = Sun (orange), 1 = Moon (blue). Read off the screenshot.
        // 面板数据。0 = 太阳（橙），1 = 月亮（蓝）。取自截图。
        // -----------------------------------------------------------------
        private const int Sun = 0;
        private const int Moon = 1;

        private static readonly int[,] Board =
        {
            // c0   c1    c2    c3    c4    c5
            { Moon, Moon, Sun,  Moon, Sun,  Sun  }, // r0
            { Sun,  Moon, Moon, Sun,  Moon, Sun  }, // r1
            { Moon, Sun,  Sun,  Moon, Sun,  Moon }, // r2
            { Sun,  Sun,  Moon, Sun,  Moon, Moon }, // r3
            { Sun,  Moon, Moon, Sun,  Moon, Sun  }, // r4
            { Moon, Sun,  Sun,  Moon, Sun,  Moon }, // r5
        };

        /// <summary>
        /// Edge constraint between two adjacent cells.
        /// Horizontal: between (Row,Col) and (Row,Col+1). Vertical: between (Row,Col) and (Row+1,Col).
        /// 相邻两格之间的约束。水平：(Row,Col) 与 (Row,Col+1) 之间；垂直：(Row,Col) 与 (Row+1,Col) 之间。
        /// </summary>
        private struct Edge
        {
            public int Row, Col;
            public bool Horizontal; // true = between left/right neighbours
            public string Symbol;   // "=" or "×"
            public Edge(int row, int col, bool horizontal, string symbol)
            { Row = row; Col = col; Horizontal = horizontal; Symbol = symbol; }
        }

        private static readonly Edge[] Edges =
        {
            new Edge(0, 3, true,  "×"), // r0: between c3 and c4
            new Edge(0, 4, false, "×"), // c4: between r0 and r1
            new Edge(1, 4, false, "×"), // c4: between r1 and r2
            new Edge(3, 1, false, "×"), // c1: between r3 and r4
            new Edge(4, 1, false, "×"), // c1: between r4 and r5
            new Edge(5, 1, true,  "="), // r5: between c1 and c2
        };

        // Layout constants (reference canvas 1080×1920). / 布局常量（参考画布 1080×1920）。
        private const float CellPitch = 160f;   // distance between cell centres / 格心间距
        private const float CellSize = 152f;    // visible white square / 可见白格
        private const float TokenSize = 116f;   // sun / moon circle diameter / 太阳月亮圆直径
        private const float BoardSize = CellPitch * 6f; // 960

        // Vertical flow, all measured downward from the canvas top edge (pivots are top).
        // 垂直布局，全部从画布顶边向下度量（pivot 均在顶部）。
        private const float BoardTopY  = 380f;                       // board top edge / 面板上边
        private const float BoardBotY  = BoardTopY + BoardSize + 8f;  // 1288
        private const float ActionTopY = BoardBotY + 36f;            // undo / hint row top / 撤销提示行上边
        private const float ActionH    = 90f;
        private const float RulesTopY  = ActionTopY + ActionH + 30f; // rules card top / 规则卡上边
        private const float RulesH     = 360f;

        // Palette. / 调色板。
        private static readonly Color ColPageBg    = new Color(0.96f, 0.95f, 0.92f);
        private static readonly Color ColBoardLine = new Color(0.78f, 0.75f, 0.68f);
        private static readonly Color ColCell      = Color.white;
        private static readonly Color ColSun       = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColMoon      = new Color(0.22f, 0.48f, 0.92f);
        private static readonly Color ColInk       = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted     = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColPill      = new Color(0.92f, 0.91f, 0.88f);

        [MenuItem("Bordy/Rebuild MainMenu Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHierarchy();

            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[BordySceneBuilder] Saved Bordy scene → {ScenePath} (ok={saved})");

            // Keep Home as scene 0 and this gameplay scene as scene 1.
            // 保持主页为 0 号场景，本游戏场景为 1 号场景。
            BordyHomeSceneBuilder.RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Build the full Bordy UI under a fresh Canvas.
        /// 在全新 Canvas 下构建完整 Bordy UI。
        /// </summary>
        public static void BuildHierarchy()
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Navigation controller lives on the Canvas; the back button points at it.
            // 导航控制器挂在 Canvas 上；返回按钮指向它。
            var nav = canvasGo.AddComponent<BordyNav>();

            var bg = CreatePanel("Background", canvasGo.transform, ColPageBg);
            Stretch(bg.rectTransform);

            BuildHeader(canvasGo.transform, nav);
            BuildBoard(canvasGo.transform);
            BuildActionRow(canvasGo.transform);
            BuildRules(canvasGo.transform);
        }

        // -----------------------------------------------------------------
        // Header: back / title / help / gear, then timer · difficulty · reset.
        // 头部：返回 / 标题 / 帮助 / 设置，下面是计时 · 难度 · 重置。
        // -----------------------------------------------------------------
        private static void BuildHeader(Transform parent, BordyNav nav)
        {
            var back = CreateText("Back", parent, "←", 56, FontStyle.Normal);
            Anchor(back.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            back.rectTransform.sizeDelta = new Vector2(90, 80);
            back.rectTransform.anchoredPosition = new Vector2(40, -120);
            back.alignment = TextAnchor.MiddleCenter;
            back.color = ColInk;

            // Turn the "←" label into a real button → return to the home scene.
            // The label is the Button's target graphic, so it must receive raycasts.
            // 把 "←" 文字变成真正的按钮 → 返回主页场景。该文字是按钮的目标图形，必须能接收射线。
            back.raycastTarget = true;
            var backBtn = back.gameObject.AddComponent<Button>();
            backBtn.targetGraphic = back;
            UnityEventTools.AddPersistentListener(backBtn.onClick, nav.BackToHome);

            var title = CreateText("Title", parent, "Bordy", 52, FontStyle.Bold);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(0, 80);
            title.rectTransform.anchoredPosition = new Vector2(0, -120);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = ColInk;

            var help = CreateText("Help", parent, "?", 44, FontStyle.Bold);
            Anchor(help.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            help.rectTransform.sizeDelta = new Vector2(80, 80);
            help.rectTransform.anchoredPosition = new Vector2(-150, -120);
            help.alignment = TextAnchor.MiddleCenter;
            help.color = ColMuted;

            var gear = CreateText("Settings", parent, "⚙", 44, FontStyle.Normal);
            Anchor(gear.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            gear.rectTransform.sizeDelta = new Vector2(80, 80);
            gear.rectTransform.anchoredPosition = new Vector2(-60, -120);
            gear.alignment = TextAnchor.MiddleCenter;
            gear.color = ColMuted;

            // Status row: centred timer + reset pill (difficulty tag removed).
            // 状态行：居中计时 + 重置药丸（难度标签已删除）。
            var timer = CreateText("Timer", parent, "◷ 0:00", 36, FontStyle.Normal);
            Anchor(timer.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            timer.rectTransform.sizeDelta = new Vector2(260, 64);
            timer.rectTransform.anchoredPosition = new Vector2(0, -230);
            timer.alignment = TextAnchor.MiddleCenter;
            timer.color = ColInk;
            // Live, persistent clock — starts on first entry, resumes (not reset) on return.
            // 实时持久计时——首次进入开始，返回再进时从原值继续（不清零）。
            timer.gameObject.AddComponent<BordyTimer>();

            var reset = CreatePill("ResetPill", parent, "重置", ColPill, ColMuted);
            Anchor(reset.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            reset.rectTransform.sizeDelta = new Vector2(180, 76);
            reset.rectTransform.anchoredPosition = new Vector2(-60, -228);
        }

        // -----------------------------------------------------------------
        // The 6×6 board with cells, tokens and edge constraints.
        // 6×6 面板：格子、棋子与边约束。
        // -----------------------------------------------------------------
        private static void BuildBoard(Transform parent)
        {
            // Board frame: line-coloured panel; white cells leave thin grid lines.
            // 面板框：线条色底；白格之间留出细网格线。
            var board = CreatePanel("Board", parent, ColBoardLine);
            var boardRT = board.rectTransform;
            Anchor(boardRT, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            boardRT.sizeDelta = new Vector2(BoardSize + 8, BoardSize + 8);
            // Pivot is top-centre, so anchoredPosition.y is the board's TOP edge.
            // pivot 在顶部中点，所以 anchoredPosition.y 即面板上边的位置。
            boardRT.anchoredPosition = new Vector2(0, -BoardTopY);

            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    Vector2 center = CellCenter(r, c);

                    var cell = CreatePanel($"Cell_{r}_{c}", board.transform, ColCell);
                    var cellRT = cell.rectTransform;
                    Anchor(cellRT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                    cellRT.sizeDelta = new Vector2(CellSize, CellSize);
                    cellRT.anchoredPosition = center;

                    // Token: coloured circle placeholder (sun / moon art skipped for now).
                    // 棋子：纯色圆形占位（太阳 / 月亮图案暂时跳过）。
                    var token = CreateCircle($"Token_{r}_{c}", cell.transform,
                        Board[r, c] == Sun ? ColSun : ColMoon);
                    token.rectTransform.sizeDelta = new Vector2(TokenSize, TokenSize);
                    token.rectTransform.anchoredPosition = Vector2.zero;
                }
            }

            // Edge constraint symbols, drawn on top of the grid lines.
            // 边约束符号，画在网格线上方。
            foreach (var e in Edges)
            {
                Vector2 a = CellCenter(e.Row, e.Col);
                Vector2 b = e.Horizontal
                    ? CellCenter(e.Row, e.Col + 1)
                    : CellCenter(e.Row + 1, e.Col);
                Vector2 mid = (a + b) * 0.5f;

                var sym = CreateText($"Edge_{e.Row}_{e.Col}_{(e.Horizontal ? "H" : "V")}",
                    board.transform, e.Symbol, e.Symbol == "=" ? 44 : 40, FontStyle.Bold);
                Anchor(sym.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                sym.rectTransform.sizeDelta = new Vector2(48, 48);
                sym.rectTransform.anchoredPosition = mid;
                sym.alignment = TextAnchor.MiddleCenter;
                sym.color = ColInk;
            }
        }

        /// <summary>Centre of cell (row, col) in board-local space (origin = board centre, y up). / 格 (row,col) 在面板局部坐标的中心（原点=面板中心，y 向上）。</summary>
        private static Vector2 CellCenter(int row, int col)
        {
            float x = -BoardSize / 2f + CellPitch / 2f + col * CellPitch;
            float y = BoardSize / 2f - CellPitch / 2f - row * CellPitch;
            return new Vector2(x, y);
        }

        // -----------------------------------------------------------------
        // Undo / Hint row, just below the board.
        // 撤销 / 提示按钮行，紧贴面板下方。
        // -----------------------------------------------------------------
        private static void BuildActionRow(Transform parent)
        {
            var undo = CreatePill("UndoButton", parent, "撤销", ColPill, ColMuted);
            Anchor(undo.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            undo.rectTransform.sizeDelta = new Vector2(440, ActionH);
            undo.rectTransform.anchoredPosition = new Vector2(-235, -ActionTopY);

            var hint = CreatePill("HintButton", parent, "提示", ColPill, ColMuted);
            Anchor(hint.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hint.rectTransform.sizeDelta = new Vector2(440, ActionH);
            hint.rectTransform.anchoredPosition = new Vector2(235, -ActionTopY);
        }

        // -----------------------------------------------------------------
        // Rules panel ("游戏玩法"), pinned just below the action row.
        // 规则面板（"游戏玩法"），固定在按钮行下方。
        // -----------------------------------------------------------------
        private static void BuildRules(Transform parent)
        {
            var card = CreatePanel("RulesCard", parent, Color.white);
            var cardRT = card.rectTransform;
            // Stretched horizontally, top-anchored just under the action row.
            // 横向拉伸，顶部锚定在按钮行下方。
            Anchor(cardRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            cardRT.sizeDelta = new Vector2(-48, RulesH);
            cardRT.anchoredPosition = new Vector2(0, -RulesTopY);

            var heading = CreateText("RulesHeading", card.transform, "游戏玩法", 40, FontStyle.Bold);
            Anchor(heading.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
            heading.rectTransform.sizeDelta = new Vector2(-48, 70);
            heading.rectTransform.anchoredPosition = new Vector2(28, -24);
            heading.alignment = TextAnchor.MiddleLeft;
            heading.color = ColInk;

            const string rules =
                "•  填充网格，使每个格子都有一个太阳或一个月亮。\n" +
                "•  每行（和每列）最多 2 个相同图案相邻，且太阳与月亮数量相等。\n" +
                "•  由 = 分隔的格子必须相同；由 × 分隔的格子必须相反。";

            var body = CreateText("RulesBody", card.transform, rules, 28, FontStyle.Normal);
            Anchor(body.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            body.rectTransform.offsetMin = new Vector2(28, 28);
            body.rectTransform.offsetMax = new Vector2(-28, -100);
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.color = ColMuted;
        }

        // -----------------------------------------------------------------
        // Helpers. / 辅助方法。
        // -----------------------------------------------------------------
        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        /// <summary>Circle via the built-in UGUI "Knob" sprite. / 用 UGUI 内置 "Knob" 圆形 sprite 画圆。</summary>
        private static Image CreateCircle(string name, Transform parent, Color color)
        {
            var img = CreatePanel(name, parent, color);
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            Anchor(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            return img;
        }

        /// <summary>Rounded "pill" with a centred label. / 带居中文字的圆角药丸。</summary>
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
            // Decorative labels must not eat clicks. The full-width Title used to sit on top
            // of the "←" button and swallow its raycast. Interactive labels re-enable this.
            // 装饰性文字不能吞点击：全宽标题原本盖在 "←" 上把射线吃掉了；可交互文字会重新开启。
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