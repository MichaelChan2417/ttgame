using System.IO;
using Bordy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy.EditorTools
{
    public static class BordyCampaignLevelSelectSceneBuilder
    {
        public const string ScenePath = "Assets/Bordy/Scenes/CampaignSelect.unity";

        [MenuItem("Bordy/Rebuild Campaign Select Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BuildHierarchy();
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[BordyCampaignLevelSelectSceneBuilder] Saved → {ScenePath} (ok={saved})");
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
            canvasGo.AddComponent<BordyCampaignLevelSelectController>();

            var bg = CreatePanel("Background", canvasGo.transform, new Color(0.96f, 0.95f, 0.92f));
            bg.raycastTarget = false;
            Stretch(bg.rectTransform);

            BordyEditorUi.CreateBackButton(canvasGo.transform, nav.BackToLevelSelect);

            var title = CreateText("Title", canvasGo.transform, "Campaign", 72, FontStyle.Bold);
            Anchor(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(900, 100);
            title.rectTransform.anchoredPosition = new Vector2(0, -280);
            title.alignment = TextAnchor.MiddleCenter;

            var hint = CreateText("HintBanner", canvasGo.transform, "Complete levels in order to unlock the next", 30, FontStyle.Normal);
            Anchor(hint.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            hint.rectTransform.sizeDelta = new Vector2(900, 60);
            hint.rectTransform.anchoredPosition = new Vector2(0, -400);
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = new Color(0.45f, 0.45f, 0.48f);

            BuildScrollArea(canvasGo.transform);
        }

        private static void BuildScrollArea(Transform parent)
        {
            var viewport = CreatePanel("ScrollViewport", parent, new Color(0, 0, 0, 0));
            viewport.raycastTarget = true;
            var vpRT = viewport.rectTransform;
            Anchor(vpRT, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 1));
            vpRT.offsetMin = new Vector2(36, 120);
            vpRT.offsetMax = new Vector2(-36, -470);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("LevelGrid", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 400);

            var grid = content.AddComponent<GridLayoutGroup>();
            grid.spacing = new Vector2(16, 16);
            grid.padding = new RectOffset(12, 12, 8, 24);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.cellSize = new Vector2(160, 160);

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = vpRT;
            scroll.content = contentRT;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
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
