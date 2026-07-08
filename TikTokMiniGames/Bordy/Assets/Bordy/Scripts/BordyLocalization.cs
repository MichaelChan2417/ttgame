using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>Applies localized text to baked scene UI by object name. / 按对象名刷新场景 UI 文案。</summary>
    public static class BordyLocalization
    {
        public static void ApplyScene(Scene scene)
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
                return;

            BordySettingsUi.EnsureOn(canvas);
            BordyUiChrome.EnsureBackButton(canvas.transform);
            BordyFonts.ApplyAllUnder(canvas.transform);
            BordyUi.FixMissingSprites(canvas.gameObject);

            switch (scene.name)
            {
                case BordyLevelCatalog.HomeScene:
                    ApplyHome(canvas.transform);
                    break;
                case BordyLevelCatalog.LevelSelectScene:
                    ApplyLevelSelect(canvas.transform);
                    break;
                case BordyLevelCatalog.TutorialScene:
                    ApplyGameplay(canvas.transform, tutorial: true);
                    break;
                case BordyLevelCatalog.Level1Scene:
                    ApplyGameplay(canvas.transform, tutorial: false);
                    break;
            }
        }

        private static void ApplyHome(Transform root)
        {
            SetText(root, "Subtitle", BordyStrings.Keys.HomeSubtitle);
            SetPillText(root, "StartButton", BordyStrings.Keys.HomeStart);
            SetText(root, "Footer", BordyStrings.Keys.HomeFooter);
        }

        private static void ApplyLevelSelect(Transform root)
        {
            BordyUiChrome.RefreshBackLabel(root);
            SetText(root, "Title", BordyStrings.Keys.LevelSelectTitle);
            SetCardTitle(root, "TutorialButton", BordyStrings.Keys.LevelTutorialTitle);
            SetCardSubtitle(root, "TutorialButton", BordyStrings.Keys.LevelTutorialSubtitle);
            SetCardTitle(root, "DailyButton", BordyStrings.Keys.LevelDailyTitle);
            SetCardTitle(root, "Level1Button", BordyStrings.Keys.Level1Title);
            SetCardSubtitle(root, "Level1Button", BordyStrings.Keys.Level1Subtitle);

            var controller = root.GetComponent<BordyLevelSelectController>();
            controller?.Refresh();
        }

        public static void ApplyGameplay(Transform root, bool tutorial)
        {
            BordyUiChrome.RefreshBackLabel(root);
            SetPillText(root, "ResetPill", BordyStrings.Keys.GameplayReset);
            SetPillText(root, "UndoButton", BordyStrings.Keys.GameplayUndo);
            SetPillText(root, "HintButton", BordyStrings.Keys.GameplayHint);

            SetText(root, "RulesCard/RulesHeading",
                tutorial ? BordyStrings.Keys.GameplayRulesTutorialHeading : BordyStrings.Keys.GameplayRulesHeading);
            SetText(root, "RulesCard/RulesBody",
                tutorial ? BordyStrings.Keys.GameplayRulesTutorialBody : BordyStrings.Keys.GameplayRulesBody);
        }

        public static void SetPillText(Transform root, string pillName, string key)
        {
            var pill = root.Find(pillName);
            if (pill == null)
                return;
            var text = pill.Find("Text")?.GetComponent<Text>();
            if (text != null)
            {
                text.text = BordyStrings.Get(key);
                BordyFonts.Apply(text);
            }
        }

        private static void SetCardTitle(Transform root, string cardName, string key)
            => SetText(root, $"{cardName}/Title", key);

        private static void SetCardSubtitle(Transform root, string cardName, string key)
            => SetText(root, $"{cardName}/Subtitle", key);

        public static void SetText(Transform root, string path, string key)
        {
            var t = root.Find(path)?.GetComponent<Text>();
            if (t != null)
            {
                t.text = BordyStrings.Get(key);
                BordyFonts.Apply(t);
            }
        }
    }
}
