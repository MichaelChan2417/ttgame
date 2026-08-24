using System;
using System.Runtime.InteropServices;
using TTSDK;
using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// Retention APIs from All-in-One guide §3.2 and Unity SDK §6 / §11:
    /// profile sidebar (startEntranceMission) and desktop shortcut (addShortcut).
    /// </summary>
    public static class BordyPlatform
    {
        public const string PrivacyUrl = "https://bordy-api.brainless.workers.dev/privacy.html";
        public const string TermsUrl = "https://bordy-api.brainless.workers.dev/terms.html";

        private static Action<bool> _onSidebar;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BordyNavigateToSidebar();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureHost()
        {
            if (GameObject.Find("BordyPlatformHost") != null)
                return;
            var go = new GameObject("BordyPlatformHost");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<BordyPlatformHost>();
        }

        /// <summary>
        /// §3.2: canIUse("startEntranceMission") then jump to TikTok profile sidebar.
        /// </summary>
        public static void OpenSidebar(Action<bool> onDone = null)
        {
            if (BordyAppConfig.WebStandalone)
            {
                onDone?.Invoke(false);
                return;
            }

            _onSidebar = onDone;
#if UNITY_WEBGL && !UNITY_EDITOR
            try { BordyNavigateToSidebar(); }
            catch (Exception e)
            {
                Debug.LogWarning("[BordyPlatform] JS startEntranceMission failed: " + e.Message);
                StartEntranceMissionCsharp();
            }
#else
            StartEntranceMissionCsharp();
#endif
        }

        private static void StartEntranceMissionCsharp()
        {
            try
            {
                TT.StartEntranceMission(new TTStartEntranceMissionParam
                {
                    success = _ =>
                    {
                        Debug.Log("[BordyPlatform] StartEntranceMission ok");
                        NotifySidebar(true);
                    },
                    fail = err =>
                    {
                        Debug.LogWarning("[BordyPlatform] StartEntranceMission fail: " + (err != null ? err.ErrMsg : ""));
                        NotifySidebar(false);
                    },
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BordyPlatform] StartEntranceMission exception: " + e.Message);
                NotifySidebar(false);
            }
        }

        public static void AddDesktopShortcut(Action<bool> onDone)
        {
            BordyShortcut.Add(onDone);
        }

        public static void InviteFriends()
        {
            if (BordyDaily.CompletedToday)
                BordyFriendCloud.ShareDailyResult(BordyDaily.CompletedSeconds);
            else
                BordyFriendCloud.ShareInvite(BordyStrings.Get(BordyStrings.Keys.SettingsInviteShare));
        }

        internal static void NotifySidebar(bool ok)
        {
            var cb = _onSidebar;
            _onSidebar = null;
            cb?.Invoke(ok);
        }
    }

    public sealed class BordyPlatformHost : MonoBehaviour
    {
        public void OnSidebarResult(string ok) => BordyPlatform.NotifySidebar(ok == "1");
        public void OnOpenLinkResult(string _) { }
    }
}
