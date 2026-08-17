using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Command-line / menu entry to run TTSDK "TikTokGame → Build Minigame" without clicking the window.
    /// Unity: -executeMethod Bordy.EditorTools.BordyTikTokBuild.BuildMinigameBatch
    /// </summary>
    public static class BordyTikTokBuild
    {
        private static DateTime _startedUtc;
        private static bool _waiting;

        [MenuItem("Bordy/Build TikTok Minigame")]
        public static void BuildMinigameBatch()
        {
            BordyStarkBuilderPaths.EnsureLocalPaths(log: true);
            _startedUtc = DateTime.UtcNow;
            _waiting = true;

            try
            {
                InvokeSdkBuild();
            }
            catch (Exception e)
            {
                Debug.LogError($"[BordyTikTokBuild] failed to start: {e}");
                if (Application.isBatchMode)
                    EditorApplication.Exit(2);
                return;
            }

            Debug.Log("[BordyTikTokBuild] Native minigame build started. Waiting for tt-minigame/tt-minigame/game.json ...");
            EditorApplication.update += TickWait;
        }

        private static void InvokeSdkBuild()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "ttsdk_tools");
            if (asm == null)
                throw new InvalidOperationException("ttsdk_tools assembly not loaded");

            var windowType = asm.GetType("TTSDK.Tool.StarkSDKToolWindow");
            if (windowType == null)
                throw new InvalidOperationException("TTSDK.Tool.StarkSDKToolWindow not found");

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var trigger = windowType.GetMethod("TriggerBuild", flags);
            if (trigger != null && trigger.GetParameters().Length == 0)
            {
                object target = trigger.IsStatic ? null : EditorWindow.GetWindow(windowType);
                Debug.Log("[BordyTikTokBuild] Invoking StarkSDKToolWindow.TriggerBuild");
                trigger.Invoke(target, null);
                return;
            }

            if (!EditorApplication.ExecuteMenuItem("TikTokGame/Build Minigame"))
                throw new InvalidOperationException("Menu TikTokGame/Build Minigame not found");
            Debug.Log("[BordyTikTokBuild] Executed menu TikTokGame/Build Minigame");
        }

        private static void TickWait()
        {
            if (!_waiting)
                return;

            string root = Directory.GetParent(Application.dataPath)!.FullName;
            string gameJson = Path.Combine(root, "tt-minigame", "tt-minigame", "game.json");
            if (File.Exists(gameJson) && File.GetLastWriteTimeUtc(gameJson) >= _startedUtc.AddSeconds(-5))
            {
                Finish(0, $"[BordyTikTokBuild] Build output ready: {gameJson}");
                return;
            }

            if (DateTime.UtcNow - _startedUtc > TimeSpan.FromMinutes(90))
                Finish(3, "[BordyTikTokBuild] Timed out waiting for Native minigame output.");
        }

        private static void Finish(int code, string message)
        {
            _waiting = false;
            EditorApplication.update -= TickWait;
            if (code == 0) Debug.Log(message);
            else Debug.LogError(message);
            if (Application.isBatchMode)
                EditorApplication.Exit(code);
        }
    }
}
