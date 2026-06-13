using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bordy
{
    /// <summary>Scene navigation helpers used by UI buttons. / 场景跳转。</summary>
    public class BordyNav : MonoBehaviour
    {
        [SerializeField] private string homeSceneName = BordyLevelCatalog.HomeScene;
        [SerializeField] private string levelSelectSceneName = BordyLevelCatalog.LevelSelectScene;
        [SerializeField] private string tutorialSceneName = BordyLevelCatalog.TutorialScene;
        [SerializeField] private string level1SceneName = BordyLevelCatalog.Level1Scene;

        /// <summary>
        /// Entry from Home. Records the session, then routes: a first-time player (tutorial
        /// not completed) is forced into the tutorial; returning players go to level select.
        /// 从主页进入：记录本次会话后路由——首次玩家（教程未完成）强制进新手教程，
        /// 老玩家进关卡选择。
        /// </summary>
        public void StartGame()
        {
            BordyUserService.NoteGameEntered();
            bool firstTime = BordyUserService.IsFirstTimePlayer;
            string target = firstTime ? tutorialSceneName : levelSelectSceneName;
            Debug.Log($"[BordyNav] StartGame firstTime={firstTime} (tutorialDone={BordyProgress.TutorialCompleted}) → {target}");
            SceneManager.LoadScene(target);
        }

        public void BackToHome() => SceneManager.LoadScene(homeSceneName);

        public void BackToLevelSelect() => SceneManager.LoadScene(levelSelectSceneName);

        public void OpenTutorial() => SceneManager.LoadScene(tutorialSceneName);

        public void OpenLevel1() => SceneManager.LoadScene(level1SceneName);
    }
}
