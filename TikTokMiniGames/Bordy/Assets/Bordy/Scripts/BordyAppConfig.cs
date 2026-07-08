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

        /// 部署后填 https://bordy-api.&lt;subdomain&gt;.workers.dev

        /// </summary>

        public const string ApiBaseUrl = "https://bordy-api.brainless.workers.dev";

    }

}

