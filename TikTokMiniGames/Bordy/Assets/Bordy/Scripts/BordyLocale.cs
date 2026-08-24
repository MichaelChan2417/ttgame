using System;
using TTSDK;
using UnityEngine;

namespace Bordy
{
    public enum BordyLanguage
    {
        ZhHans,
        Ja,
        En,
        Es,
        Id,
    }

    /// <summary>
    /// Persisted UI language. First launch follows TikTok <c>GetSystemInfo().language</c>
    /// (or Unity system language in Editor). After the player picks in Settings, that choice wins.
    /// </summary>
    public static class BordyLocale
    {
        private const string StoreKey = "bordy.locale";

        public static event Action Changed;

        public static BordyLanguage Current { get; private set; } = BordyLanguage.En;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadSaved()
        {
            Current = Resolve();
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
            if (string.IsNullOrEmpty(localeCode))
                return;

            var lang = Parse(localeCode);
            if (Current == lang)
                return;

            Current = lang;
            BordyStore.SetString(StoreKey, ToCode(lang));
            BordyStore.Save();
            Changed?.Invoke();
        }

        /// <summary>Re-read persisted language after TT.PlayerPrefs / SDK is available.</summary>
        public static void ReloadFromStore()
        {
            var prev = Current;
            Current = Resolve();
            if (prev != Current)
                Changed?.Invoke();
        }

        public static string ToCode(BordyLanguage language)
        {
            switch (language)
            {
                case BordyLanguage.ZhHans: return "zh";
                case BordyLanguage.Ja: return "ja";
                case BordyLanguage.Es: return "es";
                case BordyLanguage.Id: return "id";
                default: return "en";
            }
        }

        /// <summary>TikTok host language, e.g. "en", "zh-Hans", "ja". Empty if unavailable.</summary>
        public static string HostLanguageCode()
        {
            try
            {
                var info = TT.GetSystemInfo();
                if (info != null && !string.IsNullOrEmpty(info.language))
                    return info.language.Trim();
            }
            catch (Exception e)
            {
                Debug.Log("[BordyLocale] GetSystemInfo language unavailable: " + e.Message);
            }

            return "";
        }

        private static BordyLanguage Resolve()
        {
            var saved = "";
            try { saved = BordyStore.GetString(StoreKey, ""); }
            catch { /* store may not be ready yet */ }

            if (!string.IsNullOrEmpty(saved))
                return Parse(saved);

            var host = HostLanguageCode();
            if (!string.IsNullOrEmpty(host))
            {
                Debug.Log("[BordyLocale] TikTok system language=" + host);
                return Parse(host);
            }

            return ParseUnitySystemLanguage();
        }

        private static BordyLanguage ParseUnitySystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return BordyLanguage.ZhHans;
                case SystemLanguage.Japanese:
                    return BordyLanguage.Ja;
                case SystemLanguage.Spanish:
                    return BordyLanguage.Es;
                case SystemLanguage.Indonesian:
                    return BordyLanguage.Id;
                default:
                    return BordyLanguage.En;
            }
        }

        private static BordyLanguage Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return BordyLanguage.En;

            var code = raw.Trim().Replace("_", "-");
            if (code.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                return BordyLanguage.ZhHans;
            if (code.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
                return BordyLanguage.Ja;
            if (code.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                return BordyLanguage.Es;
            // Indonesian: modern "id", legacy "in".
            if (code.StartsWith("id", StringComparison.OrdinalIgnoreCase)
                || code.StartsWith("in-", StringComparison.OrdinalIgnoreCase)
                || code.Equals("in", StringComparison.OrdinalIgnoreCase))
                return BordyLanguage.Id;

            return BordyLanguage.En;
        }
    }
}
