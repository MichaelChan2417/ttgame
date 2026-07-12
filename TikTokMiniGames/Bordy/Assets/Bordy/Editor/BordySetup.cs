using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// One-click PlayerSettings + scene + build-target bootstrap for the demo.
    /// 一键完成 PlayerSettings、场景生成与 WebGL 平台切换的 demo 引导工具。
    /// </summary>
    public static class BordySetup
    {
        [MenuItem("Bordy/Fix Build Output Paths")]
        public static void FixBuildOutputPathsMenu() => BordyStarkBuilderPaths.EnsureLocalPaths();

        /// <summary>
        /// Editor menu entry — runs the whole setup pipeline.
        /// 编辑器菜单入口——一次性把整套配置跑完。
        /// </summary>
        [MenuItem("Bordy/Run Full Setup")]
        public static void RunAll()
        {
            try
            {
                ConfigurePlayerSettings();
                BordyStarkBuilderPaths.EnsureLocalPaths();
                if (Resources.Load<Font>("Bordy/BordyUI") == null && Application.platform == RuntimePlatform.OSXEditor)
                    Debug.LogWarning("[BordySetup] No CJK UI font — run Bordy → Import UI Font (macOS) for 简体中文.");
                BordyHomeSceneBuilder.BuildAndSave();
                BordyLevelSelectSceneBuilder.BuildAndSave();
                BordyCampaignLevelSelectSceneBuilder.BuildAndSave();
                BordyTutorialSceneBuilder.BuildAndSave();
                BordyPlaySceneBuilder.BuildAndSave();
                BordySceneBuilder.BuildAndSave();
                SwitchToWebGL();
                Debug.Log("[BordySetup] Done.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[BordySetup] failed: {e}");
                if (Application.isBatchMode)
                    EditorApplication.Exit(2);
                else
                    EditorUtility.DisplayDialog("Bordy Setup Failed", e.Message, "OK");
                return;
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Configure PlayerSettings the way the TikTok minigame container expects.
        /// 按 TikTok 小游戏容器的预期来配置 PlayerSettings。
        /// </summary>
        public static void ConfigurePlayerSettings()
        {
            // EN: companyName / productName are baked into the WebGL output and surfaced in the container UI.
            // ZH: companyName / productName 会写进 WebGL 产物，并在容器上下文中显示。
            PlayerSettings.companyName = "Bordy";
            PlayerSettings.productName = "Bordy";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, "com.tiktok.minigame.demo");

            // EN: TT minigame requires IL2CPP for WebGL — Mono fallback is not supported.
            // ZH: TT 小游戏 WebGL 必须用 IL2CPP，Mono 不被支持。
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);

            // Release-friendly WebGL: smaller wasm, faster first compile in TikTok container.
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 });

            Debug.Log("[BordySetup] Player settings configured.");
        }

        /// <summary>
        /// Switch active build target to WebGL if not already.
        /// 若当前 build target 不是 WebGL 就切换过去。
        /// </summary>
        public static void SwitchToWebGL()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                Debug.Log("[BordySetup] Already WebGL.");
                return;
            }
            bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            Debug.Log($"[BordySetup] Switched to WebGL: {ok}");
        }
    }
}
