using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Keeps UI input alive across scene loads. After Home → MainMenu, Unity can leave
    /// <see cref="EventSystem.current"/> pointing at a destroyed object, which makes every
    /// button appear dead.
    ///
    /// 保证场景切换后 UI 输入仍可用。Home → MainMenu 后 Unity 有时会让
    /// <see cref="EventSystem.current"/> 指向已销毁对象，导致所有按钮失效。
    /// </summary>
    public static class BordyUiBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            BordyLocale.Changed -= OnLocaleChanged;
            BordyLocale.Changed += OnLocaleChanged;
        }

        private static void OnLocaleChanged()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
                return;

            BordyLocalization.ApplyScene(scene);

            var levelSelect = Object.FindObjectOfType<BordyLevelSelectController>();
            if (levelSelect != null)
                levelSelect.Refresh();

            var campaignSelect = Object.FindObjectOfType<BordyCampaignLevelSelectController>();
            if (campaignSelect != null)
                campaignSelect.Refresh();

            var tutorial = Object.FindObjectOfType<BordyTutorialGuide>();
            if (tutorial != null)
                tutorial.RefreshLocale();

            var board = Object.FindObjectOfType<BordyBoardController>();
            if (board != null)
                board.RefreshLocale();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureEventSystem();
            DisableDecorativeRaycasts();
            BordyLocalization.ApplyScene(scene);

            if (scene.name == BordyLevelCatalog.HomeScene)
            {
                var homeCanvas = Object.FindObjectOfType<Canvas>();
                if (homeCanvas != null)
                {
                    if (BordyDebugReset.Enabled && homeCanvas.GetComponent<BordyDebugReset>() == null)
                        homeCanvas.gameObject.AddComponent<BordyDebugReset>();
                    BordyHomeGate.EnsureOn(homeCanvas.transform);
                }
                return;
            }

            if (scene.name == BordyLevelCatalog.LevelSelectScene)
            {
                var canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null && canvas.GetComponent<BordyLevelSelectController>() == null)
                    canvas.gameObject.AddComponent<BordyLevelSelectController>();
                return;
            }

            if (scene.name == BordyLevelCatalog.CampaignSelectScene)
            {
                var canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null && canvas.GetComponent<BordyCampaignLevelSelectController>() == null)
                    canvas.gameObject.AddComponent<BordyCampaignLevelSelectController>();
                FixCampaignScrollMask(canvas);
                return;
            }

            if (scene.name != BordyLevelCatalog.Level1Scene
                && scene.name != BordyLevelCatalog.TutorialScene
                && scene.name != BordyLevelCatalog.PlayScene)
                return;

            var gameCanvas = Object.FindObjectOfType<Canvas>();
            if (gameCanvas == null)
                return;

            if (gameCanvas.GetComponent<BordyBoardController>() == null)
                gameCanvas.gameObject.AddComponent<BordyBoardController>();

            if (scene.name == BordyLevelCatalog.TutorialScene && gameCanvas.GetComponent<BordyTutorialGuide>() == null)
                gameCanvas.gameObject.AddComponent<BordyTutorialGuide>();
        }

        private static void EnsureEventSystem()
        {
            var stale = Object.FindObjectsOfType<EventSystem>(true);
            foreach (var system in stale)
            {
                if (!system.gameObject.scene.isLoaded)
                    Object.Destroy(system.gameObject);
            }

            var active = Object.FindObjectOfType<EventSystem>();
            if (active == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                return;
            }

            if (active.GetComponent<StandaloneInputModule>() == null)
                active.gameObject.AddComponent<StandaloneInputModule>();

            // Re-activate so EventSystem.current binds to the loaded scene instance.
            // 重新激活，让 EventSystem.current 绑定到当前场景实例。
            active.enabled = false;
            active.enabled = true;

            var module = active.currentInputModule;
            if (module != null)
            {
                module.DeactivateModule();
                module.ActivateModule();
            }
        }

        private static void DisableDecorativeRaycasts()
        {
            DisableImageRaycast("Background");
            DisableImageRaycast("RulesCard");
            DisableImageRaycast("Board");

            var board = GameObject.Find("Board");
            if (board == null)
                return;

            foreach (var image in board.GetComponentsInChildren<Image>(true))
            {
                if (image.gameObject.name.StartsWith("Token_") ||
                    image.gameObject.name.StartsWith("Edge_"))
                    image.raycastTarget = false;
            }

            var countOverlay = board.transform.Find("CountOverlay");
            if (countOverlay != null)
                Object.Destroy(countOverlay.gameObject);

            var totalCount = GameObject.Find("TotalCount");
            if (totalCount != null)
                Object.Destroy(totalCount);
        }

        private static void DisableImageRaycast(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go == null)
                return;

            var image = go.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;
        }

        private static void FixCampaignScrollMask(Canvas canvas)
        {
            if (canvas == null)
                return;

            var viewport = canvas.transform.Find("ScrollViewport");
            if (viewport == null)
                return;

            var legacyMask = viewport.GetComponent<Mask>();
            if (legacyMask != null)
                Object.Destroy(legacyMask);

            if (viewport.GetComponent<RectMask2D>() == null)
                viewport.gameObject.AddComponent<RectMask2D>();

            // Migrate old vertical list container to grid.
            var legacyList = viewport.Find("LevelList");
            if (legacyList != null)
                Object.Destroy(legacyList.gameObject);

            if (viewport.Find("LevelGrid") == null)
            {
                var gridGo = new GameObject("LevelGrid", typeof(RectTransform));
                gridGo.transform.SetParent(viewport, false);
                var gridRt = gridGo.GetComponent<RectTransform>();
                gridRt.anchorMin = new Vector2(0, 1);
                gridRt.anchorMax = new Vector2(1, 1);
                gridRt.pivot = new Vector2(0.5f, 1);
                gridRt.anchoredPosition = Vector2.zero;
                gridRt.sizeDelta = new Vector2(0, 400);

                var grid = gridGo.AddComponent<GridLayoutGroup>();
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 4;
                grid.cellSize = new Vector2(160, 160);
                grid.spacing = new Vector2(16, 16);
                grid.padding = new RectOffset(12, 12, 8, 24);
                grid.childAlignment = TextAnchor.UpperCenter;

                var fitter = gridGo.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var scroll = viewport.GetComponent<ScrollRect>();
                if (scroll != null)
                    scroll.content = gridRt;
            }
        }
    }
}
