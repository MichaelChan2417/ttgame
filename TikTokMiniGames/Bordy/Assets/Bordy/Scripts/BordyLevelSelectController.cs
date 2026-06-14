using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>Level select screen: tutorial + unlocked stages. / 关卡选择界面。</summary>
    public class BordyLevelSelectController : MonoBehaviour
    {
        private Text _hintLabel;

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
                    ? "选择一个关卡开始挑战"
                    : "请先完成新手引导，解锁正式关卡";
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

            // Always enterable once unlocked — after completion it opens a read-only result page.
            // 解锁后始终可进入——完成后进入的是只读结算页。
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            if (unlocked)
                button.onClick.AddListener(GetComponent<BordyNav>().OpenDaily);

            // Update the card's subtitle to reflect today's state.
            // 更新卡片副标题以反映今天的状态。
            var subtitle = tr.Find("Subtitle")?.GetComponent<Text>();
            if (subtitle != null)
            {
                if (!unlocked)
                    subtitle.text = "完成新手引导后开放";
                else if (BordyDaily.CompletedToday)
                    subtitle.text = $"今日已完成 · 用时 {BordyTimer.Format(BordyDaily.CompletedSeconds)} · 点击查看";
                else
                    subtitle.text = "每日一题 · 全球同题 · 今日可挑战";
            }
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
