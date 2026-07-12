using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bordy
{
    /// <summary>Scene navigation helpers used by UI buttons. / 场景跳转。</summary>
    public class BordyNav : MonoBehaviour
    {
        /// <summary>Backup level id if <see cref="BordyBoardController.RequestedLevelId"/> is lost on load.</summary>
        public static string PendingPlayLevelId;

        [SerializeField] private string homeSceneName = BordyLevelCatalog.HomeScene;
        [SerializeField] private string levelSelectSceneName = BordyLevelCatalog.LevelSelectScene;
        [SerializeField] private string tutorialSceneName = BordyLevelCatalog.TutorialScene;
        [SerializeField] private string campaignSelectSceneName = BordyLevelCatalog.CampaignSelectScene;
        [SerializeField] private string playSceneName = BordyLevelCatalog.PlayScene;

        public void StartGame()
        {
            if (BordyUserService.CloudEnabled && !BordyUserService.IsReady)
            {
                Debug.LogWarning("[BordyNav] StartGame blocked — cloud login not ready.");
                return;
            }

            if (BordyUserService.CloudEnabled && BordyUserService.CloudLoginFailed)
            {
                Debug.LogWarning("[BordyNav] StartGame blocked — cloud login failed.");
                return;
            }

            BordyUserService.NoteGameEntered();
            bool firstTime = BordyUserService.IsFirstTimePlayer;
            string target = firstTime ? tutorialSceneName : levelSelectSceneName;
            Debug.Log($"[BordyNav] StartGame firstTime={firstTime} → {target}");
            SceneManager.LoadScene(target);
        }

        public void BackToHome() => SceneManager.LoadScene(homeSceneName);

        public void BackToLevelSelect() => SceneManager.LoadScene(levelSelectSceneName);

        public void BackToCampaignSelect() => SceneManager.LoadScene(campaignSelectSceneName);

        public void OpenTutorial() => SceneManager.LoadScene(tutorialSceneName);

        public void OpenCampaign() => SceneManager.LoadScene(campaignSelectSceneName);

        public void OpenCampaignLevel(string levelId)
        {
            PendingPlayLevelId = levelId;
            BordyBoardController.RequestedLevelId = levelId;
            SceneManager.LoadScene(playSceneName);
        }

        /// <summary>Daily challenge — uses Play scene with runtime board. / 每日挑战，Play 场景运行时建盘。</summary>
        public void OpenDaily()
        {
            PendingPlayLevelId = BordyLevelCatalog.DailyId;
            BordyBoardController.RequestedLevelId = BordyLevelCatalog.DailyId;
            SceneManager.LoadScene(playSceneName);
        }

        // Legacy Level 1 entry (dev only). / 旧第一关入口（仅开发兼容）。
        public void OpenLevel1()
        {
            BordyBoardController.RequestedLevelId = BordyLevelCatalog.Level1Id;
            SceneManager.LoadScene(BordyLevelCatalog.Level1Scene);
        }
    }
}
