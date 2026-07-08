using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Keeps TTSDK StarkBuilder output paths on the local machine (never a teammate's Windows path).
    /// StarkBuilderSetting.asset is gitignored; this script creates/updates it from the .example template.
    /// </summary>
    public static class BordyStarkBuilderPaths
    {
        private const string ExampleAssetPath = "Assets/Editor/StarkBuilderSetting.asset.example";
        private const string LocalAssetPath = "Assets/Editor/StarkBuilderSetting.asset";

        [InitializeOnLoadMethod]
        private static void AutoFixOnLoad() => EnsureLocalPaths(log: false);

        /// <summary>Menu + Run Full Setup entry point. / 菜单与 Run Full Setup 调用。</summary>
        public static void EnsureLocalPaths(bool log = true)
        {
            EnsureAssetExists();

            var asset = AssetDatabase.LoadAssetAtPath<Object>(LocalAssetPath);
            if (asset == null)
            {
                Debug.LogWarning("[Bordy] StarkBuilderSetting.asset missing after copy — skip path fix.");
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            string outputDir = Path.Combine(projectRoot, "tt-minigame");
            string webglDir = Path.Combine(outputDir, "webgl");

            Directory.CreateDirectory(webglDir);

            var so = new SerializedObject(asset);
            var outputProp = so.FindProperty("OutputDir");
            var webglProp = so.FindProperty("_webglPackagePath");

            if (outputProp == null || webglProp == null)
            {
                Debug.LogWarning("[Bordy] StarkBuilderSetting fields not found — TTSDK version mismatch?");
                return;
            }

            bool changed = outputProp.stringValue != outputDir || webglProp.stringValue != webglDir;
            if (!changed)
                return;

            outputProp.stringValue = outputDir;
            webglProp.stringValue = webglDir;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            if (log)
                Debug.Log($"[Bordy] StarkBuilder paths →\n  OutputDir: {outputDir}\n  WebGL: {webglDir}");
        }

        private static void EnsureAssetExists()
        {
            string localFull = Path.Combine(Application.dataPath, "Editor/StarkBuilderSetting.asset");
            if (File.Exists(localFull))
                return;

            string exampleFull = Path.Combine(Application.dataPath, "Editor/StarkBuilderSetting.asset.example");

            if (!File.Exists(exampleFull))
            {
                Debug.LogWarning("[Bordy] Missing StarkBuilderSetting.asset.example — cannot create local build settings.");
                return;
            }

            File.Copy(exampleFull, localFull, overwrite: false);
            AssetDatabase.ImportAsset(LocalAssetPath);
        }
    }
}
