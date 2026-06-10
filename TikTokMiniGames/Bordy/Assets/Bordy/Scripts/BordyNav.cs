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

        public void StartGame() => SceneManager.LoadScene(levelSelectSceneName);

        public void BackToHome() => SceneManager.LoadScene(homeSceneName);

        public void BackToLevelSelect() => SceneManager.LoadScene(levelSelectSceneName);

        public void OpenTutorial() => SceneManager.LoadScene(tutorialSceneName);

        public void OpenLevel1() => SceneManager.LoadScene(level1SceneName);
    }
}
