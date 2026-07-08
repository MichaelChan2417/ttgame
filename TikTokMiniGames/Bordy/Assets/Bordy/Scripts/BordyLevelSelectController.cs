using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>Level select screen: tutorial + unlocked stages. / 关卡选择界面。</summary>
    public class BordyLevelSelectController : MonoBehaviour
    {
        private Text _hintLabel;
        private Text _dailySubtitle;
        private bool _dailyLoading;

        private void OnEnable()
        {
            if (_hintLabel == null)
                _hintLabel = transform.Find("HintBanner")?.GetComponent<Text>();
            Refresh();
        }

        public void Refresh()
        {
            WireButton("TutorialButton", BordyLevelCatalog.TutorialId, true);
            WireDailyButton();
            WireButton("Level1Button", BordyLevelCatalog.Level1Id, BordyProgress.IsLevelUnlocked(BordyLevelCatalog.Level1Id));

            if (_hintLabel != null)
            {
                _hintLabel.text = BordyProgress.TutorialCompleted
                    ? BordyStrings.Get(BordyStrings.Keys.LevelSelectHintUnlocked)
                    : BordyStrings.Get(BordyStrings.Keys.LevelSelectHintLocked);
            }
        }

        private void WireDailyButton()
        {
            var tr = transform.Find("DailyButton");
            if (tr == null)
                return;

            bool unlocked = BordyProgress.IsLevelUnlocked(BordyLevelCatalog.DailyId);

            var button = tr.GetComponent<Button>();
            if (button == null)
                button = tr.gameObject.AddComponent<Button>();
            var image = tr.GetComponent<Image>();
            if (image != null)
                button.targetGraphic = image;

            // Always enterable once unlocked — the tap first fetches today's template, then enters
            // (or opens the read-only result page if already completed).
            // 解锁后始终可点——点击先拉取当天题目再进入（已完成则进只读结算页）。
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            if (unlocked)
                button.onClick.AddListener(OnDailyPressed);

            // Update the card's subtitle to reflect today's state.
            // 更新卡片副标题以反映今天的状态。
            _dailySubtitle = tr.Find("Subtitle")?.GetComponent<Text>();
            if (_dailySubtitle != null && !_dailyLoading)
            {
                if (!unlocked)
                    _dailySubtitle.text = BordyStrings.Get(BordyStrings.Keys.LevelDailySubtitleLocked);
                else if (BordyDaily.CompletedToday)
                    _dailySubtitle.text = BordyStrings.Format(BordyStrings.Keys.LevelDailySubtitleDone,
                        BordyTimer.Format(BordyDaily.CompletedSeconds));
                else
                    _dailySubtitle.text = BordyStrings.Get(BordyStrings.Keys.LevelDailySubtitleOpen);
            }
        }

        private void OnDailyPressed()
        {
            if (_dailyLoading)
                return;

            var nav = GetComponent<BordyNav>();
            _dailyLoading = true;
            if (_dailySubtitle != null)
                _dailySubtitle.text = BordyStrings.Get(BordyStrings.Keys.LevelDailyLoading);

            BordyDailyService.EnsureToday(this,
                onReady: () =>
                {
                    _dailyLoading = false;
                    nav.OpenDaily();
                },
                onError: err =>
                {
                    _dailyLoading = false;
                    if (_dailySubtitle != null)
                        _dailySubtitle.text = BordyStrings.Get(BordyStrings.Keys.LevelDailyLoadError);
                });
        }

        private void WireButton(string name, string levelId, bool unlocked)
        {
            var tr = transform.Find(name);
            if (tr == null)
                return;

            var button = tr.GetComponent<Button>();
            if (button == null)
                button = tr.gameObject.AddComponent<Button>();

            var image = tr.GetComponent<Image>();
            if (image != null)
                button.targetGraphic = image;

            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            if (!unlocked)
                return;

            var nav = GetComponent<BordyNav>();
            if (levelId == BordyLevelCatalog.TutorialId)
                button.onClick.AddListener(nav.OpenTutorial);
            else if (levelId == BordyLevelCatalog.Level1Id)
                button.onClick.AddListener(nav.OpenLevel1);
        }
    }
}
