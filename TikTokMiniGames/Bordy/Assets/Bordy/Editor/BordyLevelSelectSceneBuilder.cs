using System.IO;
using Bordy;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy.EditorTools
{
    public static class BordyLevelSelectSceneBuilder
    {
        public const string ScenePath = "Assets/Bordy/Scenes/LevelSelect.unity";

        private static readonly Color ColPageBg = new Color(0.96f, 0.95f, 0.92f);
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);
        private static readonly Color ColDaily = new Color(0.22f, 0.48f, 0.92f);

        [MenuItem("Bordy/Rebuild Level Select Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHierarchy();
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[BordyLevelSelectSceneBuilder] Saved → {ScenePath} (ok={saved})");
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
            canvasGo.AddComponent<BordyLevelSelectController>();

            var bg = CreatePanel("Background", canvasGo.transform, ColPageBg);
            bg.raycastTarget = false;
            Stretch(bg.rectTransform);

            BordyEditorUi.CreateBackButton(canvasGo.transform, nav.BackToHome);

            var title = CreateText("Title", canvasGo.transform, "Select Level", 72, FontStyle.Bold);
            Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(900, 100);
            title.rectTransform.anchoredPosition = new Vector2(0, -280);
            title.alignment = TextAnchor.MiddleCenter;

            var hint = CreateText("HintBanner", canvasGo.transform, "Finish the tutorial to unlock the main levels", 30, FontStyle.Normal);
            Anchor(hint.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hint.rectTransform.sizeDelta = new Vector2(900, 60);
            hint.rectTransform.anchoredPosition = new Vector2(0, -400);
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = ColMuted;

            BuildLevelCards(canvasGo.transform);
        }

        // One row per level button. Add / remove entries here and the cards auto-distribute.
        // 每个关卡按钮一行。在这里增删条目，卡片会自动均匀分布。
        private static readonly (string Name, string Title, string Subtitle, bool Accent)[] Cards =
        {
            ("TutorialButton", "Tutorial",        "4×4 lesson",                 true),
            ("DailyButton",    "Daily Challenge", "One puzzle a day · Same for all", true),
            ("Level1Button",   "Level 1",         "6×6 challenge",              false),
        };

        /// <summary>
        /// Lay the level cards out evenly down the screen so any number of cards (3, 4, …) stays
        /// balanced with comfortable spacing — no hand-tuned positions.
        /// 把关卡卡片在屏幕上均匀竖向分布，无论几张（3、4…）都保持平衡、间距舒适，无需手调坐标。
        /// </summary>
        private static void BuildLevelCards(Transform parent)
        {
            const float bandTop = -470f;     // just under the hint banner / 提示语下方
            const float bandBottom = -1770f; // leave room above the bottom edge / 给底部留白
            const float cardW = 840f;
            const float cardH = 200f;

            int n = Cards.Length;
            float available = bandTop - bandBottom;            // total vertical span / 总竖向跨度
            float gap = (available - n * cardH) / (n + 1);     // equal gaps incl. both ends / 含两端的等间距

            for (int i = 0; i < n; i++)
            {
                float topY = bandTop - gap * (i + 1) - cardH * i;
                var def = Cards[i];
                Color fill = def.Accent ? (def.Name == "DailyButton" ? ColDaily : ColAccent) : ColPill;
                Color text = def.Accent ? Color.white : ColInk;
                var card = CreateLevelButton(def.Name, parent, def.Title, def.Subtitle, fill, text, topY, cardW, cardH);
                card.gameObject.AddComponent<Button>().targetGraphic = card;
            }
        }

        private static Image CreateLevelButton(string name, Transform parent, string title, string subtitle, Color fill, Color textColor, float y, float width, float height)
        {
            var card = CreatePanel(name, parent, fill);
            card.sprite = BordyUi.Rounded();
            card.type = Image.Type.Sliced;
            Anchor(card.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            card.rectTransform.sizeDelta = new Vector2(width, height);
            card.rectTransform.anchoredPosition = new Vector2(0, y);

            var titleText = CreateText("Title", card.transform, title, 44, FontStyle.Bold);
            Anchor(titleText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
            titleText.rectTransform.sizeDelta = new Vector2(-48, 80);
            titleText.rectTransform.anchoredPosition = new Vector2(32, -24);
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.color = textColor;

            var subText = CreateText("Subtitle", card.transform, subtitle, 28, FontStyle.Normal);
            Anchor(subText.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0));
            subText.rectTransform.sizeDelta = new Vector2(-48, 60);
            subText.rectTransform.anchoredPosition = new Vector2(32, 24);
            subText.alignment = TextAnchor.MiddleLeft;
            subText.color = textColor == Color.white ? new Color(1f, 1f, 1f, 0.9f) : ColMuted;
            return card;
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<Image>();
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
