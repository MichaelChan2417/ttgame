using System;
using System.Collections;
using UnityEngine;
using TTSDK;

namespace Bordy
{
    /// <summary>
    /// Optional backend that turns a one-time login <c>code</c> into a stable <c>openid</c>.
    /// </summary>
    public interface IBordyAuthBackend
    {
        void ExchangeCodeForOpenId(string code, Action<string> onOpenId, Action<string> onError);
    }

    /// <summary>
    /// Identity + cloud save orchestration. Server is source of truth when <see cref="CloudEnabled"/>.
    ///
    /// Boot flow:
    ///   1. TT.InitSDK
    ///   2. TT.Login (silent) → POST /api/auth/login → apply cloud save
    ///   3. IsReady = true → Home can route (first-time → tutorial)
    /// </summary>
    public static class BordyUserService
    {
        private const string ProfileKey = "bordy.user.profile";

        public static IBordyAuthBackend AuthBackend;
        public static BordyCloudBackend CloudBackend { get; private set; }

        public static BordyUserProfile Profile { get; private set; }
        public static bool SdkInited { get; private set; }
        public static string LastLoginCode { get; private set; }

        public static bool IsNewUser { get; private set; }
        public static bool IsFirstTimePlayer => !BordyProgress.TutorialCompleted;

        public static bool CloudEnabled =>
            !string.IsNullOrEmpty(BordyAppConfig.ApiBaseUrl) && !Application.isEditor;

        public static bool CloudLoggedIn { get; private set; }
        public static bool IsReady { get; private set; }
        public static bool CloudLoginFailed { get; private set; }
        public static string LastCloudError { get; private set; }

        public static event Action ReadyChanged;

        private static bool _booted;
        private static bool _loginInFlight;
        private const float InitSdkTimeoutSec = 12f;
        private const float CloudLoginTimeoutSec = 15f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Boot()
        {
            if (_booted) return;
            _booted = true;
            BordyHttpRunner.Run(BootRoutine());
        }

        private static IEnumerator BootRoutine()
        {
            // Let Unity render first frame / dismiss container loading page before TT bridge calls.
            yield return null;
            yield return null;

            if (CloudEnabled)
            {
                CloudBackend = new BordyCloudBackend(BordyAppConfig.ApiBaseUrl);
                AuthBackend = CloudBackend;
            }

            bool sdkDone = false;
            try
            {
                TT.InitSDK((code, env) =>
                {
                    SdkInited = code == 0;
                    Debug.Log($"[BordyUser] InitSDK code={code} inited={SdkInited}");
                    sdkDone = true;
                    if (SdkInited)
                    {
                        BordyLocale.ReloadFromStore();
                        BeginCloudOrLocalLogin();
                    }
                    else
                        FinishOfflineBoot();
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BordyUser] InitSDK threw (offline): {e.Message}");
                FinishOfflineBoot();
                yield break;
            }

            float deadline = Time.realtimeSinceStartup + InitSdkTimeoutSec;
            while (!sdkDone && Time.realtimeSinceStartup < deadline)
                yield return null;

            if (!sdkDone)
            {
                Debug.LogWarning("[BordyUser] InitSDK timeout — continue offline.");
                FinishOfflineBoot();
            }
        }

        /// <summary>Retry cloud login from Home overlay. / 主页重试云端登录。</summary>
        public static void RetryCloudLogin()
        {
            if (!CloudEnabled || !SdkInited || _loginInFlight) return;
            CloudLoginFailed = false;
            LastCloudError = null;
            IsReady = false;
            ReadyChanged?.Invoke();
            BeginCloudOrLocalLogin();
        }

        private static void BeginCloudOrLocalLogin()
        {
            if (CloudEnabled)
                SilentLogin();
            else
                FinishOfflineBoot();
        }

        private static void FinishOfflineBoot()
        {
            LoadProfileAndReport();
            MarkReady();
        }

        private static void LoadProfileAndReport()
        {
            EnsureProfileLoaded();

            if (IsNewUser)
                Debug.Log(
                    "\n========== BORDY USER ==========\n" +
                    "🆕 NEW USER (local/offline)\n" +
                    $"   userId={Profile.userId}\n" +
                    "================================");
            else
                Debug.Log(
                    "\n========== BORDY USER ==========\n" +
                    "↩️ RETURNING USER (local/offline)\n" +
                    $"   userId={Profile.userId}  playCount={Profile.playCount}\n" +
                    "================================");

            Debug.Log($"[BordyUser] firstTimePlayer={IsFirstTimePlayer} tutorialDone={BordyProgress.TutorialCompleted}");
        }

        private static void SilentLogin()
        {
            if (_loginInFlight) return;
            _loginInFlight = true;

            try
            {
                TT.Login(
                    code =>
                    {
                        LastLoginCode = code;
                        Debug.Log("[BordyUser] TT.Login ok.");
                        CompleteCloudLogin(code);
                    },
                    err =>
                    {
                        _loginInFlight = false;
                        FailCloudLogin($"TT.Login failed: {err}");
                    });
            }
            catch (Exception e)
            {
                _loginInFlight = false;
                FailCloudLogin($"TT.Login threw: {e.Message}");
            }
        }

        private static void CompleteCloudLogin(string code)
        {
            if (CloudBackend == null)
            {
                _loginInFlight = false;
                FinishOfflineBoot();
                return;
            }

            BordyHttpRunner.Run(CompleteCloudLoginRoutine(code));
        }

        private static IEnumerator CompleteCloudLoginRoutine(string code)
        {
            bool done = false;
            string err = null;
            BordyLoginResponse res = null;

            CloudBackend.LoginWithCode(code,
                r => { res = r; done = true; },
                e => { err = e; done = true; });

            float deadline = Time.realtimeSinceStartup + CloudLoginTimeoutSec;
            while (!done && Time.realtimeSinceStartup < deadline)
                yield return null;

            _loginInFlight = false;

            if (!done)
            {
                FailCloudLogin("Cloud login timeout");
                yield break;
            }

            if (err != null)
                FailCloudLogin(err);
            else if (res != null)
                ApplyCloudLogin(res);
            else
                FailCloudLogin("Empty cloud login response");
        }

        private static void ApplyCloudLogin(BordyLoginResponse res)
        {
            CloudLoginFailed = false;
            LastCloudError = null;
            CloudLoggedIn = true;
            IsNewUser = res.isNewUser;

            if (res.save != null)
                BordyCloudSave.ApplyToLocal(res.save);

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Profile = new BordyUserProfile
            {
                userId = res.openId,
                isAnonymous = false,
                firstSeenUnix = res.save?.createdAt > 0 ? res.save.createdAt : now,
                lastSeenUnix = now,
                playCount = res.save?.playCount ?? 0,
            };
            SaveProfile();

            Debug.Log(
                "\n========== BORDY USER (CLOUD) ==========\n" +
                (IsNewUser ? "🆕 NEW TikTok user\n" : "↩️ RETURNING TikTok user\n") +
                $"   openId={res.openId}\n" +
                $"   tutorialDone={BordyProgress.TutorialCompleted}\n" +
                "========================================");

            MarkReady();
        }

        private static void FailCloudLogin(string error)
        {
            CloudLoginFailed = true;
            LastCloudError = error;
            CloudLoggedIn = false;
            Debug.LogWarning($"[BordyUser] Cloud login failed: {error}");
            MarkReady();
        }

        private static void MarkReady()
        {
            if (IsReady) return;
            IsReady = true;
            ReadyChanged?.Invoke();
        }

        public static void EnsureProfileLoaded()
        {
            if (Profile != null) return;

            string json = BordyStore.GetString(ProfileKey, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { Profile = JsonUtility.FromJson<BordyUserProfile>(json); }
                catch (Exception) { Profile = null; }
            }

            if (Profile == null)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Profile = new BordyUserProfile
                {
                    userId = Guid.NewGuid().ToString("N"),
                    isAnonymous = true,
                    firstSeenUnix = now,
                    lastSeenUnix = now,
                    playCount = 0,
                };
                SaveProfile();
                IsNewUser = true;
            }
            else
            {
                IsNewUser = false;
            }
        }

        public static void NoteGameEntered()
        {
            EnsureProfileLoaded();
            Profile.playCount += 1;
            Profile.lastSeenUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveProfile();
            BordyCloudSync.PushNow();
        }

        public static void SaveProfile()
        {
            if (Profile == null) return;
            BordyStore.SetString(ProfileKey, JsonUtility.ToJson(Profile));
            BordyStore.Save();
        }

        public static void ResetAll()
        {
            BordyStore.DeleteKey(ProfileKey);
            BordyProgress.Reset();
            BordyDaily.Reset();
            Profile = null;
            CloudLoggedIn = false;
            EnsureProfileLoaded();
            BordyCloudSync.PushNow();
            Debug.Log("[BordyUser] Player data reset.");
        }

        public static string Describe()
        {
            EnsureProfileLoaded();
            return $"userId={Profile.userId} anonymous={Profile.isAnonymous} cloud={CloudLoggedIn} " +
                   $"playCount={Profile.playCount} tutorialDone={BordyProgress.TutorialCompleted}";
        }
    }
}
