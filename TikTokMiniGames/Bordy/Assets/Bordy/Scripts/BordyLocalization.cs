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
                case BordyLevelCatalog.CampaignSelectScene:
                    ApplyCampaignSelect(canvas.transform);
                    break;
                case BordyLevelCatalog.TutorialScene:
                    ApplyGameplay(canvas.transform, tutorial: true);
                    break;
                case BordyLevelCatalog.PlayScene:
                case BordyLevelCatalog.Level1Scene:
                    ApplyGameplay(canvas.transform, tutorial: false);
                    break;
            }
        }

        private static void ApplyHome(Transform root)
        {
            var title = root.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = BordyLevelCatalog.GameTitle;
                BordyFonts.Apply(title);
            }

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
            SetCardTitle(root, "CampaignButton", BordyStrings.Keys.CampaignHubTitle);
            SetCardSubtitle(root, "CampaignButton", BordyStrings.Keys.CampaignHubSubtitle);

            var controller = root.GetComponent<BordyLevelSelectController>();
            controller?.Refresh();
        }

        private static void ApplyCampaignSelect(Transform root)
        {
            BordyUiChrome.RefreshBackLabel(root);
            SetText(root, "Title", BordyStrings.Keys.CampaignTitle);
            var controller = root.GetComponent<BordyCampaignLevelSelectController>();
            controller?.Refresh();
        }

        public static void ApplyGameplay(Transform root, bool tutorial)
        {
            BordyUiChrome.RefreshBackLabel(root);
            SetPillText(root, "ResetPill", BordyStrings.Keys.GameplayReset);
            SetPillText(root, "UndoButton", BordyStrings.Keys.GameplayCheck);
            SetPillText(root, "CheckButton", BordyStrings.Keys.GameplayCheck);
            SetPillText(root, "HintButton", BordyStrings.Keys.GameplayHint);

            SetText(root, "RulesCard/RulesHeading",
                tutorial ? BordyStrings.Keys.GameplayRulesTutorialHeading : BordyStrings.Keys.GameplayRulesHeading);
            SetText(root, "RulesCard/RulesBody",
                tutorial ? BordyStrings.Keys.GameplayRulesTutorialBody : BordyStrings.Keys.GameplayRulesBody);

            EnsureRulesLegend(root);
        }

        /// <summary>
        /// Inline icon legend under the rules heading: "Fill each cell with [sun] or [moon]",
        /// using classic sun / moon during the tutorial (ForceSkinId), otherwise the equipped skin.
        /// 规则标题下的图案图例：「每格填入 [日] 或 [月]」。新手引导固定经典皮肤，其它关跟装备皮肤。
        /// </summary>
        private static void EnsureRulesLegend(Transform root)
        {
            var card = root.Find("RulesCard");
            if (card == null)
                return;

            // Make room under the heading for the legend row.
            var body = card.Find("RulesBody")?.GetComponent<RectTransform>();
            if (body != null)
                body.offsetMax = new Vector2(-28f, -156f);

            var row = card.Find("RulesIcons") as RectTransform;
            if (row == null)
            {
                var go = new GameObject("RulesIcons", typeof(RectTransform));
                go.transform.SetParent(card, false);
                row = go.GetComponent<RectTransform>();
                row.anchorMin = row.anchorMax = new Vector2(0f, 1f);
                row.pivot = new Vector2(0f, 1f);
                row.anchoredPosition = new Vector2(28f, -100f);
                row.sizeDelta = new Vector2(600f, 52f);

                var h = go.AddComponent<HorizontalLayoutGroup>();
                h.spacing = 10f;
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childForceExpandWidth = false;
                h.childForceExpandHeight = false;
                h.childControlWidth = true;
                h.childControlHeight = true;

                MakeLegendText(go.transform, "Fill");
                MakeLegendIcon(go.transform, "Sun");
                MakeLegendText(go.transform, "Or");
                MakeLegendIcon(go.transform, "Moon");
            }

            var fill = row.Find("Fill")?.GetComponent<Text>();
            if (fill != null) { fill.text = BordyStrings.Get(BordyStrings.Keys.RulesIconsFill); BordyFonts.Apply(fill); }
            var or = row.Find("Or")?.GetComponent<Text>();
            if (or != null) { or.text = BordyStrings.Get(BordyStrings.Keys.RulesIconsOr); BordyFonts.Apply(or); }
            var sun = row.Find("Sun")?.GetComponent<Image>();
            if (sun != null) sun.sprite = BordyTokenSprites.Sun;
            var moon = row.Find("Moon")?.GetComponent<Image>();
            if (moon != null) moon.sprite = BordyTokenSprites.Moon;
        }

        private static void MakeLegendText(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = BordyFonts.Ui;
            t.fontSize = 28;
            t.color = new Color(0.45f, 0.45f, 0.48f); // match RulesBody (ColMuted) / 与正文同色
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
        }

        private static void MakeLegendIcon(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 44f;
            le.preferredHeight = 44f;
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
