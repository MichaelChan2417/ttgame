using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bordy
{
    /// <summary>
    /// Scene navigation for Bordy. The editor scene builders attach this to the Canvas
    /// and wire the "开始游戏" / "←" buttons to <see cref="StartGame"/> / <see cref="BackToHome"/>.
    /// Both target scenes must be registered in Build Settings.
    ///
    /// Bordy 的场景导航。编辑器场景构建脚本会把它挂到 Canvas 上，并把
    /// “开始游戏” / “←” 按钮分别接到 <see cref="StartGame"/> / <see cref="BackToHome"/>。
    /// 两个目标场景都必须在 Build Settings 中登记。
    /// </summary>
    public class BordyNav : MonoBehaviour
    {
        [Tooltip("Gameplay scene name — must be in Build Settings. / 游戏场景名——需在 Build Settings 中。")]
        [SerializeField] private string gameSceneName = "MainMenu";

        [Tooltip("Home scene name — must be in Build Settings. / 主页场景名——需在 Build Settings 中。")]
        [SerializeField] private string homeSceneName = "Home";

        /// <summary>Enter the game from the home page. / 从主页进入游戏。</summary>
        public void StartGame() => SceneManager.LoadScene(gameSceneName);

        /// <summary>Return to the home page from the game. / 从游戏返回主页。</summary>
        public void BackToHome() => SceneManager.LoadScene(homeSceneName);
    }
}
