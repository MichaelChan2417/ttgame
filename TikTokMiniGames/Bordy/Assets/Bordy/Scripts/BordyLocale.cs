using System;

namespace Bordy
{
    public enum BordyLanguage
    {
        ZhHans,
        En,
    }

    /// <summary>
    /// Persisted UI language. TikTok defaults to English; WeChat Mini Game defaults to Simplified Chinese.
    /// 持久化界面语言。TikTok 默认英文；微信小游戏默认简体中文。
    /// </summary>
    public static class BordyLocale
    {
        private const string StoreKey = "bordy.locale";

        public static event Action Changed;

#if WECHAT_MINIGAME
        public static BordyLanguage Current { get; private set; } = BordyLanguage.ZhHans;
        private const string DefaultCode = "zh";
#else
        public static BordyLanguage Current { get; private set; } = BordyLanguage.En;
        private const string DefaultCode = "en";
#endif

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadSaved()
        {
            Current = Parse(BordyStore.GetString(StoreKey, DefaultCode));
        }

        public static void SetLanguage(BordyLanguage language)
        {
            if (Current == language)
                return;

            Current = language;
            BordyStore.SetString(StoreKey, ToCode(language));
            BordyStore.Save();
            BordyCloudSync.PushNow();
            Changed?.Invoke();
        }

        /// <summary>Apply locale from cloud without re-uploading. / 从云端应用语言，不触发上传。</summary>
        public static void ApplyFromCloud(string localeCode)
        {
            var lang = Parse(localeCode);
            if (Current == lang)
                return;

            Current = lang;
            BordyStore.SetString(StoreKey, ToCode(lang));
            BordyStore.Save();
            Changed?.Invoke();
        }

        /// <summary>Re-read persisted language (e.g. after TT.PlayerPrefs becomes available). / 重新读取已保存语言。</summary>
        public static void ReloadFromStore()
        {
            var prev = Current;
            Current = Parse(BordyStore.GetString(StoreKey, DefaultCode));
            if (prev != Current)
                Changed?.Invoke();
        }

        public static string ToCode(BordyLanguage language)
            => language == BordyLanguage.ZhHans ? "zh" : "en";

        /// <summary>
        /// English unless the value is explicitly Chinese. Empty / unknown codes (including
        /// "en-US") must not fall through to Chinese.
        /// 只有明确是中文才返回中文；空值、未知码（包括 en-US）都走英文。
        /// </summary>
        private static BordyLanguage Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return DefaultCode == "zh" ? BordyLanguage.ZhHans : BordyLanguage.En;

            var code = raw.Trim();
            if (code.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                return BordyLanguage.ZhHans;

            return BordyLanguage.En;
        }
    }
}
