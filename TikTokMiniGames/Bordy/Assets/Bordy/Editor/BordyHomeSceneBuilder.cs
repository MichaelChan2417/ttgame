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
    /// Builds the Home (start) scene entirely from code: a title and a "开始游戏" button
    /// that loads the gameplay scene. Also registers both scenes in Build Settings with
    /// Home as index 0 (the entry scene).
    /// Triggered via <c>Bordy → Rebuild Home Scene</c>, or <see cref="BordySetup.RunAll"/>.
    ///
    /// 用纯代码构建主页（开始）场景：一个标题 + 一个“开始游戏”按钮，点击进入游戏场景。
    /// 同时把两个场景登记进 Build Settings，主页作为 0 号入口场景。
    /// 通过 <c>Bordy → Rebuild Home Scene</c> 或 <see cref="BordySetup.RunAll"/> 触发。
    /// </summary>
    public static class BordyHomeSceneBuilder
    {
        public const string HomeScenePath = "Assets/Bordy/Scenes/Home.unity";
        public const string LevelSelectScenePath = "Assets/Bordy/Scenes/LevelSelect.unity";
        public const string TutorialScenePath = "Assets/Bordy/Scenes/Tutorial.unity";
        public const string GameScenePath = "Assets/Bordy/Scenes/MainMenu.unity";

        // Palette — shared look with the gameplay scene. / 调色板——与游戏场景统一。
        private static readonly Color ColPageBg = new Color(0.96f, 0.95f, 0.92f);
        private static readonly Color ColInk    = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted  = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);

        [MenuItem("Bordy/Rebuild Home Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(HomeScenePath)!);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHierarchy();

            bool saved = EditorSceneManager.SaveScene(scene, HomeScenePath);
            Debug.Log($"[BordyHomeSceneBuilder] Saved Home scene → {HomeScenePath} (ok={saved})");

            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Register all four scenes in Build Settings (Home → LevelSelect → Tutorial → Level1).
        /// 在 Build Settings 登记四个场景，供 <see cref="BordyNav"/> 按名加载。
        /// </summary>
        public static void RegisterBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(HomeScenePath, true),
                new EditorBuildSettingsScene(LevelSelectScenePath, true),
                new EditorBuildSettingsScene(TutorialScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true),
            };
        }

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

            // Navigation controller lives on the Canvas; the Start button points at it.
            // 导航控制器挂在 Canvas 上；开始按钮指向它。
            var nav = canvasGo.AddComponent<BordyNav>();

            var bg = CreatePanel("Background", canvasGo.transform, ColPageBg);
            bg.raycastTarget = false;
            Stretch(bg.rectTransform);

            // Title. / 标题。
            var title = CreateText("Title", canvasGo.transform, "Bordy", 120, FontStyle.Bold);
            Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(900, 170);
            title.rectTransform.anchoredPosition = new Vector2(0, -560);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = ColInk;

            // Subtitle. / 副标题。
            var sub = CreateText("Subtitle", canvasGo.transform, "Logic Puzzle", 44, FontStyle.Normal);
            Anchor(sub.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            sub.rectTransform.sizeDelta = new Vector2(900, 80);
            sub.rectTransform.anchoredPosition = new Vector2(0, -730);
            sub.alignment = TextAnchor.MiddleCenter;
            sub.color = ColMuted;

            // Start button — loads the gameplay scene. / 开始按钮——加载游戏场景。
            var btnImg = CreatePill("StartButton", canvasGo.transform, "Play", ColAccent, Color.white, 50);
            Anchor(btnImg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            btnImg.rectTransform.sizeDelta = new Vector2(560, 150);
            btnImg.rectTransform.anchoredPosition = new Vector2(0, -80);

            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            // Serialize a persistent onClick → nav.StartGame so it survives into the build.
            // 序列化持久化的 onClick → nav.StartGame，使其保留进构建产物。
            UnityEventTools.AddPersistentListener(btn.onClick, nav.StartGame);

            // Footer hint. / 底部提示。
            var foot = CreateText("Footer", canvasGo.transform, "Tap the button to play", 30, FontStyle.Normal);
            Anchor(foot.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            foot.rectTransform.sizeDelta = new Vector2(800, 60);
            foot.rectTransform.anchoredPosition = new Vector2(0, 160);
            foot.alignment = TextAnchor.MiddleCenter;
            foot.color = ColMuted;
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

        /// <summary>Rounded "pill" with a centred label. / 带居中文字的圆角药丸。</summary>
        private static Image CreatePill(string name, Transform parent, string label, Color fill, Color textColor, int fontSize)
        {
            var img = CreatePanel(name, parent, fill);
            img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            img.type = Image.Type.Sliced;

            var t = CreateText("Text", img.transform, label, fontSize, FontStyle.Bold);
            t.alignment = TextAnchor.MiddleCenter;
            t.color = textColor;
            t.raycastTarget = false;
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
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
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
