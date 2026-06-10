using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Forces the Editor Play button to always start from Home.unity instead of whatever
    /// untitled / empty scene happens to be open. Also re-registers build scenes on load.
    ///
    /// 让编辑器点 Play 时始终从 Home.unity 启动，而不是当前打开的空白 Untitled 场景。
    /// 同时在编辑器加载时重新登记 Build Settings 场景列表。
    /// </summary>
    [InitializeOnLoad]
    public static class BordyPlayModeEntry
    {
        static BordyPlayModeEntry()
        {
            BordyHomeSceneBuilder.RegisterBuildScenes();
            ApplyPlayModeStartScene();
        }

        [MenuItem("Bordy/Open Home Scene")]
        public static void OpenHomeScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(BordyHomeSceneBuilder.HomeScenePath);
            ApplyPlayModeStartScene();
        }

        private static void ApplyPlayModeStartScene()
        {
            var home = AssetDatabase.LoadAssetAtPath<SceneAsset>(BordyHomeSceneBuilder.HomeScenePath);
            if (home == null)
            {
                Debug.LogWarning($"[Bordy] Home scene missing at {BordyHomeSceneBuilder.HomeScenePath}. Run Bordy → Run Full Setup.");
                return;
            }

            EditorSceneManager.playModeStartScene = home;
        }
    }
}
