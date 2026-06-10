using System.IO;
using Bordy;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bordy.EditorTools
{
    public static class BordyTutorialSceneBuilder
    {
        public const string ScenePath = "Assets/Bordy/Scenes/Tutorial.unity";

        [MenuItem("Bordy/Rebuild Tutorial Scene")]
        public static void BuildAndSave()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            BordyGameplaySceneBuilder.BuildHierarchy(BordyLevelCatalog.Get(BordyLevelCatalog.TutorialId), tutorialMode: true);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[BordyTutorialSceneBuilder] Saved → {ScenePath} (ok={saved})");
            BordyHomeSceneBuilder.RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
