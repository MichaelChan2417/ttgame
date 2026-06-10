using System.IO;
using Bordy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>Rebuilds the level-1 gameplay scene (6×6). / 重建第一关 6×6 场景。</summary>
    public static class BordySceneBuilder
    {
        private const string ScenePath = "Assets/Bordy/Scenes/MainMenu.unity";

        [MenuItem("Bordy/Rebuild MainMenu Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BordyGameplaySceneBuilder.BuildHierarchy(BordyLevelCatalog.Get(BordyLevelCatalog.Level1Id), tutorialMode: false);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[BordySceneBuilder] Saved Bordy scene → {ScenePath} (ok={saved})");
            BordyHomeSceneBuilder.RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
