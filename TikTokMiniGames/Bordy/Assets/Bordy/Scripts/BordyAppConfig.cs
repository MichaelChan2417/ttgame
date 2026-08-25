namespace Bordy
{
    /// <summary>TikTok Developer Portal credentials for Bordy (no secrets in client). / 开发者后台凭据（不含 secret）。</summary>
    public static class BordyAppConfig
    {
        public const string AppId = "7647437535525996565";
        public const string ClientKey = "mgt6rr5wp9i8b059";

        /// <summary>
        /// Cloudflare Workers API base URL (no trailing slash).
        /// Leave empty to disable cloud login/save (Editor / offline dev).
        /// </summary>
        public const string ApiBaseUrl = "https://bordy-api.brainless.workers.dev";

        /// <summary>
        /// Set true for a plain browser build (Cloudflare Pages, shareable link): skips ALL TikTok
        /// SDK + cloud login, runs game logic only with local storage. Set false for the TikTok
        /// mini-game build.
        /// 独立网页版（Cloudflare Pages、可分享链接）设为 true：跳过所有 TikTok SDK 与云登录，
        /// 只跑游戏逻辑 + 本地存储。TikTok 小游戏包设为 false。
        /// </summary>
        public const bool WebStandalone = false;

        /// <summary>
        /// Rewarded video ad unit from TikTok Developer Portal → Monetization.
        /// </summary>
        public const string RewardedVideoAdUnitId = "ad7660431701143963669";

        /// <summary>
        /// Interstitial ad unit (optional, e.g. after brutal level clear).
        /// </summary>
        public const string InterstitialAdUnitId = "demo_interstitial";

        /// <summary>
        /// When true, Unity Editor simulates a completed rewarded ad. Keep false while tuning hint limits.
        /// </summary>
        public const bool EditorSimulateRewardedAds = false;

        /// <summary>
        /// TESTING ONLY: when true, the Shop unlocks skins instantly without a rewarded ad.
        /// Set back to false before shipping so unlocks require watching an ad.
        /// 仅测试用：为 true 时商店直接免广告解锁皮肤。上线前改回 false，让解锁必须看广告。
        /// </summary>
        public const bool ShopFreeUnlockForTesting = false;

        /// <summary>
        /// TESTING ONLY: when true, every skin is treated as already unlocked (no ad, no tap).
        /// Set back to false before shipping. / 仅测试用：所有皮肤视为已解锁。上线前改回 false。
        /// </summary>
        public const bool UnlockAllSkinsForTesting = false;
    }
}
