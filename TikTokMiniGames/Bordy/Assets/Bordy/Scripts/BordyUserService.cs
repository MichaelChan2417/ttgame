using System;
using UnityEngine;
using TTSDK;

namespace Bordy
{
    /// <summary>
    /// Optional backend that turns a one-time login <c>code</c> into a stable <c>openid</c>.
    /// In production you implement this against YOUR server, which calls Douyin's
    /// <c>code2session</c> with the app secret (the secret must never live in the client).
    ///
    /// 可选后端：把一次性登录 <c>code</c> 换成稳定的 <c>openid</c>。正式环境由你的服务器实现，
    /// 服务器用 app secret 调抖音 <c>code2session</c>（secret 绝不能放进客户端）。
    /// </summary>
    public interface IBordyAuthBackend
    {
        void ExchangeCodeForOpenId(string code, Action<string> onOpenId, Action<string> onError);
    }

    /// <summary>
    /// Single source of truth for "who is playing".
    ///
    /// Flow on app start (see <see cref="Boot"/>, fired automatically before the first scene):
    ///   1. <c>TT.InitSDK</c> — required before any container API.
    ///   2. <c>TT.Login</c> — silent login (a platform "must-integrate" capability). Returns a
    ///      one-time <c>code</c>. With no backend we just keep the code; if <see cref="AuthBackend"/>
    ///      is set we exchange it for a real openid and upgrade the profile.
    ///   3. Load (or create) the persisted <see cref="BordyUserProfile"/> from <see cref="BordyStore"/>.
    ///
    /// "First-time player" is defined as "tutorial not completed" (see <see cref="BordyProgress"/>),
    /// which <see cref="BordyNav.StartGame"/> uses to force the tutorial on a first visit.
    ///
    /// “是谁在玩”的唯一事实来源。应用启动时（<see cref="Boot"/> 在首个场景前自动触发）：
    ///   1. <c>TT.InitSDK</c>；2. <c>TT.Login</c> 静默登录（平台必接），拿一次性 code，
    ///   无后端时仅保留 code，设置了 <see cref="AuthBackend"/> 则换成 openid 并升级档案；
    ///   3. 从 <see cref="BordyStore"/> 读取/创建 <see cref="BordyUserProfile"/>。
    /// “首次游玩”等价于“教程未完成”，由 <see cref="BordyNav.StartGame"/> 用来强制新手教程。
    /// </summary>
    public static class BordyUserService
    {
        private const string ProfileKey = "bordy.user.profile";

        /// <summary>Set this before <see cref="Boot"/> to enable real openid resolution. / 接后端时在 Boot 前赋值。</summary>
        public static IBordyAuthBackend AuthBackend;

        public static BordyUserProfile Profile { get; private set; }
        public static bool SdkInited { get; private set; }
        public static string LastLoginCode { get; private set; }

        /// <summary>True when this launch created a brand-new profile (no saved data found). / 本次启动新建了档案（存储里没有旧数据）时为 true。</summary>
        public static bool IsNewUser { get; private set; }

        /// <summary>The brand-new player check used for routing. / 路由用的首次玩家判断。</summary>
        public static bool IsFirstTimePlayer => !BordyProgress.TutorialCompleted;

        private static bool _booted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Boot()
        {
            if (_booted) return;
            _booted = true;

            // IMPORTANT: do NOT read storage here. At BeforeSceneLoad the container's persistent
            // store (TT.PlayerPrefs / IndexedDB) isn't ready yet, so reading the profile would come
            // back empty and every returning user would look "new". Load the profile only AFTER
            // InitSDK succeeds (or in the Editor fallback below).
            // 重要：这里不要读存储。BeforeSceneLoad 时容器的持久化存储（TT.PlayerPrefs / IndexedDB）
            // 还没就绪，此刻读档案会读到空，导致老用户被误判为新用户。要等 InitSDK 成功后再读。
            try
            {
                TT.InitSDK((code, env) =>
                {
                    SdkInited = code == 0;
                    Debug.Log($"[BordyUser] InitSDK code={code} inited={SdkInited}");
                    LoadProfileAndReport();   // storage is reliable only now / 现在存储才可靠
                    if (SdkInited)
                        SilentLogin();
                });
            }
            catch (Exception e)
            {
                // Editor without the SDK, or an old SDK: storage still works locally, so proceed.
                // Editor 无 SDK 或旧 SDK：本地存储仍可用，继续。
                Debug.LogWarning($"[BordyUser] InitSDK threw (continuing with local profile): {e.Message}");
                LoadProfileAndReport();
            }
        }

        /// <summary>
        /// Load (or create) the profile and print the NEW/RETURNING banner. Called after storage
        /// is ready so the new-user check is accurate.
        /// 读取/新建档案并打印 新/老用户 横幅。在存储就绪后调用，保证新用户判断准确。
        /// </summary>
        private static void LoadProfileAndReport()
        {
            EnsureProfileLoaded();

            if (IsNewUser)
                Debug.Log(
                    "\n========== BORDY USER ==========\n" +
                    "🆕 NEW USER (first time ever)\n" +
                    $"   基于：存储里没有档案（key='{ProfileKey}'），刚新建\n" +
                    $"   userId={Profile.userId}\n" +
                    "================================");
            else
                Debug.Log(
                    "\n========== BORDY USER ==========\n" +
                    "↩️ RETURNING USER\n" +
                    $"   基于：存储里读到已有档案（key='{ProfileKey}'）\n" +
                    $"   userId={Profile.userId}  playCount={Profile.playCount}\n" +
                    "================================");

            // Separate from identity: which puzzle to route to (tutorial gates the rest).
            // 与身份区分：进游戏时路由到哪（教程决定后续解锁）。
            Debug.Log($"[BordyUser] Route as first-time player (tutorial not done)? {IsFirstTimePlayer} (tutorialDone={BordyProgress.TutorialCompleted})");
        }

        /// <summary>
        /// Silent login. We never block gameplay on it; routing relies on the local profile.
        /// 静默登录。绝不因此阻塞玩法；路由依赖本地档案。
        /// </summary>
        private static void SilentLogin()
        {
            try
            {
                TT.Login(
                    code =>
                    {
                        LastLoginCode = code;
                        Debug.Log("[BordyUser] Login ok (got one-time code).");
                        TryResolveOpenId(code);
                    },
                    err => Debug.LogWarning($"[BordyUser] Login failed: {err}"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BordyUser] Login threw: {e.Message}");
            }
        }

        private static void TryResolveOpenId(string code)
        {
            if (AuthBackend == null) return; // test phase: stay anonymous / 测试阶段：保持匿名

            AuthBackend.ExchangeCodeForOpenId(code,
                openId =>
                {
                    if (string.IsNullOrEmpty(openId)) return;
                    Profile.userId = openId;
                    Profile.isAnonymous = false;
                    SaveProfile();
                    Debug.Log("[BordyUser] openid resolved & profile upgraded.");
                },
                err => Debug.LogWarning($"[BordyUser] openid exchange failed: {err}"));
        }

        /// <summary>Load the profile, creating a fresh one on first launch. / 读取档案，首启时新建。</summary>
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
                IsNewUser = true;   // no saved profile existed → brand-new user / 存储里没有档案 → 全新用户
            }
            else
            {
                IsNewUser = false;  // a saved profile was loaded → returning user / 读到了旧档案 → 老用户
            }
        }

        /// <summary>Call when the player actually enters gameplay from Home. / 玩家从主页真正进入游戏时调用。</summary>
        public static void NoteGameEntered()
        {
            EnsureProfileLoaded();
            Profile.playCount += 1;
            Profile.lastSeenUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveProfile();
        }

        public static void SaveProfile()
        {
            if (Profile == null) return;
            BordyStore.SetString(ProfileKey, JsonUtility.ToJson(Profile));
            BordyStore.Save();
        }

        /// <summary>Wipe the profile + progress so the next entry is treated as first-time. / 清空档案+进度，让下次进入按首次处理。</summary>
        public static void ResetAll()
        {
            BordyStore.DeleteKey(ProfileKey);
            BordyProgress.Reset();
            BordyDaily.Reset();
            Profile = null;
            EnsureProfileLoaded();
            Debug.Log("[BordyUser] Player data reset — next StartGame will be treated as first-time.");
        }

        /// <summary>Human-readable dump of the current profile. / 打印当前档案。</summary>
        public static string Describe()
        {
            EnsureProfileLoaded();
            return $"userId={Profile.userId} anonymous={Profile.isAnonymous} playCount={Profile.playCount} " +
                   $"tutorialDone={BordyProgress.TutorialCompleted} firstSeen={Profile.firstSeenUnix} lastSeen={Profile.lastSeenUnix}";
        }
    }
}
