using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy.EditorTools
{
    /// <summary>Generic gameplay shell — board is built at runtime. / 通用游戏场景壳，棋盘运行时生成。</summary>
    public static class BordyPlaySceneBuilder
    {
        public const string ScenePath = "Assets/Bordy/Scenes/Play.unity";

        [MenuItem("Bordy/Rebuild Play Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHierarchy();
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[BordyPlaySceneBuilder] Saved → {ScenePath} (ok={saved})");
            BordyHomeSceneBuilder.RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void BuildHierarchy()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

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
            boardSo.FindProperty("_levelId").stringValue = BordyLevelCatalog.DailyId;
            boardSo.ApplyModifiedPropertiesWithoutUndo();

            var bg = CreatePanel("Background", canvasGo.transform, new Color(0.96f, 0.95f, 0.92f));
            bg.raycastTarget = false;
            Stretch(bg.rectTransform);

            BordyEditorUi.CreateBackButton(canvasGo.transform, nav.BackToCampaignSelect);

            var title = CreateText("Title", canvasGo.transform, "Play", 52, FontStyle.Bold);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(0, 80);
            title.rectTransform.anchoredPosition = new Vector2(0, -120);
            title.alignment = TextAnchor.MiddleCenter;

            var timer = CreateText("Timer", canvasGo.transform, "◷ 0:00", 36, FontStyle.Normal);
            Anchor(timer.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            timer.rectTransform.sizeDelta = new Vector2(260, 64);
            timer.rectTransform.anchoredPosition = new Vector2(0, -230);
            timer.alignment = TextAnchor.MiddleCenter;
            timer.gameObject.AddComponent<BordyTimer>();

            var reset = CreateClickablePill("ResetPill", canvasGo.transform, "Reset", new Color(0.92f, 0.91f, 0.88f), new Color(0.45f, 0.45f, 0.48f));
            Anchor(reset.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
            reset.rectTransform.sizeDelta = new Vector2(180, 76);
            reset.rectTransform.anchoredPosition = new Vector2(-60, -228);
            UnityEventTools.AddPersistentListener(reset.GetComponent<Button>().onClick, board.ResetPuzzle);

            var undo = CreateClickablePill("UndoButton", canvasGo.transform, "Undo", new Color(0.92f, 0.91f, 0.88f), new Color(0.45f, 0.45f, 0.48f));
            Anchor(undo.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            undo.rectTransform.sizeDelta = new Vector2(440, 84);
            undo.rectTransform.anchoredPosition = new Vector2(-235, -700);
            UnityEventTools.AddPersistentListener(undo.GetComponent<Button>().onClick, board.Undo);

            var hint = CreateClickablePill("HintButton", canvasGo.transform, "Hint", new Color(0.92f, 0.91f, 0.88f), new Color(0.45f, 0.45f, 0.48f));
            Anchor(hint.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hint.rectTransform.sizeDelta = new Vector2(440, 84);
            hint.rectTransform.anchoredPosition = new Vector2(235, -700);
            UnityEventTools.AddPersistentListener(hint.GetComponent<Button>().onClick, board.Hint);

            var rules = CreatePanel("RulesCard", canvasGo.transform, Color.white);
            rules.raycastTarget = false;
            var rulesRT = rules.rectTransform;
            Anchor(rulesRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            rulesRT.sizeDelta = new Vector2(-48, 300);
            rulesRT.anchoredPosition = new Vector2(0, -820);

            var heading = CreateText("RulesHeading", rules.transform, "How to Play", 40, FontStyle.Bold);
            Anchor(heading.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
            heading.rectTransform.sizeDelta = new Vector2(-48, 70);
            heading.rectTransform.anchoredPosition = new Vector2(28, -24);
            heading.alignment = TextAnchor.MiddleLeft;

            var body = CreateText("RulesBody", rules.transform,
                "•  Fill the grid so every cell holds a sun or a moon.\n•  Each row (and column) has equal suns and moons, with at most 2 identical symbols adjacent.\n•  Cells split by = must match; cells split by × must differ.",
                28, FontStyle.Normal);
            Anchor(body.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1));
            body.rectTransform.offsetMin = new Vector2(28, 28);
            body.rectTransform.offsetMax = new Vector2(-28, -100);
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.color = new Color(0.45f, 0.45f, 0.48f);
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<Image>();
        }

        private static Image CreateClickablePill(string name, Transform parent, string label, Color fill, Color textColor)
        {
            var img = CreatePanel(name, parent, fill);
            img.sprite = BordyUi.Rounded();
            img.type = Image.Type.Sliced;
            var t = CreateText("Text", img.transform, label, 30, FontStyle.Normal);
            t.alignment = TextAnchor.MiddleCenter;
            t.color = textColor;
            Stretch(t.rectTransform);
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
            t.color = new Color(0.16f, 0.16f, 0.18f);
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
