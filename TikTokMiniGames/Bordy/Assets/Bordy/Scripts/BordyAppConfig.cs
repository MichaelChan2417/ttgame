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
    }
}
