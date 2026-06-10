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
            WireButton("Level1Button", BordyLevelCatalog.Level1Id, BordyProgress.IsLevelUnlocked(BordyLevelCatalog.Level1Id));

            if (_hintLabel != null)
            {
                _hintLabel.text = BordyProgress.TutorialCompleted
                    ? "选择一个关卡开始挑战"
                    : "请先完成新手引导，解锁正式关卡";
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
