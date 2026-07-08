using System;

namespace Bordy
{
    public enum BordyLanguage
    {
        ZhHans,
        En,
    }

    /// <summary>Persisted UI language. / 持久化的界面语言。</summary>
    public static class BordyLocale
    {
        private const string StoreKey = "bordy.locale";

        public static event Action Changed;

        public static BordyLanguage Current { get; private set; } = BordyLanguage.En;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadSaved()
        {
            string raw = BordyStore.GetString(StoreKey, "en");
            Current = raw == "en" ? BordyLanguage.En : BordyLanguage.ZhHans;
        }

        public static void SetLanguage(BordyLanguage language)
        {
            if (Current == language)
                return;

            Current = language;
            BordyStore.SetString(StoreKey, language == BordyLanguage.En ? "en" : "zh");
            BordyStore.Save();
            BordyCloudSync.PushNow();
            Changed?.Invoke();
        }

        /// <summary>Apply locale from cloud without re-uploading. / 从云端应用语言，不触发上传。</summary>
        public static void ApplyFromCloud(string localeCode)
        {
            var lang = localeCode == "en" ? BordyLanguage.En : BordyLanguage.ZhHans;
            if (Current == lang)
                return;

            Current = lang;
            BordyStore.SetString(StoreKey, localeCode == "en" ? "en" : "zh");
            BordyStore.Save();
            Changed?.Invoke();
        }

        /// <summary>Re-read persisted language (e.g. after TT.PlayerPrefs becomes available). / 重新读取已保存语言。</summary>
        public static void ReloadFromStore()
        {
            var prev = Current;
            string raw = BordyStore.GetString(StoreKey, "en");
            Current = raw == "en" ? BordyLanguage.En : BordyLanguage.ZhHans;
            if (prev != Current)
                Changed?.Invoke();
        }

        public static string ToCode(BordyLanguage language)
            => language == BordyLanguage.En ? "en" : "zh";
    }
}
