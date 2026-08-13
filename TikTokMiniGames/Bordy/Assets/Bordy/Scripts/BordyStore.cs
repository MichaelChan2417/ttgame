using System;
using UnityEngine;
using TTSDK;

namespace Bordy
{
    /// <summary>
    /// Persistent key/value storage for Bordy.
    ///
    /// TikTok: must use <c>TT.PlayerPrefs</c> — Unity's own PlayerPrefs does not persist
    /// inside the TikTok container. WeChat: the WeChat Mini Game runtime hijacks
    /// <c>UnityEngine.PlayerPrefs</c> so it persists correctly, so we use that directly.
    ///
    /// Bordy 的持久化 KV 封装。TikTok 容器内优先用 TT.PlayerPrefs；微信小游戏运行时
    /// 会自动劫持 UnityEngine.PlayerPrefs 使其持久化，因此直接走 Unity 接口。
    /// </summary>
    public static class BordyStore
    {
        public static string GetString(string key, string defaultValue = "")
        {
#if WECHAT_MINIGAME
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
#else
            try { return TT.PlayerPrefs.GetString(key, defaultValue); }
            catch (Exception) { return UnityEngine.PlayerPrefs.GetString(key, defaultValue); }
#endif
        }

        public static void SetString(string key, string value)
        {
#if WECHAT_MINIGAME
            UnityEngine.PlayerPrefs.SetString(key, value);
#else
            try { TT.PlayerPrefs.SetString(key, value); }
            catch (Exception) { UnityEngine.PlayerPrefs.SetString(key, value); }
#endif
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
#if WECHAT_MINIGAME
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
#else
            try { return TT.PlayerPrefs.GetInt(key, defaultValue); }
            catch (Exception) { return UnityEngine.PlayerPrefs.GetInt(key, defaultValue); }
#endif
        }

        public static void SetInt(string key, int value)
        {
#if WECHAT_MINIGAME
            UnityEngine.PlayerPrefs.SetInt(key, value);
#else
            try { TT.PlayerPrefs.SetInt(key, value); }
            catch (Exception) { UnityEngine.PlayerPrefs.SetInt(key, value); }
#endif
        }

        public static bool GetBool(string key, bool defaultValue = false)
            => GetInt(key, defaultValue ? 1 : 0) == 1;

        public static void SetBool(string key, bool value)
            => SetInt(key, value ? 1 : 0);

        public static void DeleteKey(string key)
        {
#if WECHAT_MINIGAME
            UnityEngine.PlayerPrefs.DeleteKey(key);
#else
            try { TT.PlayerPrefs.DeleteKey(key); }
            catch (Exception) { /* ignore */ }
            try { UnityEngine.PlayerPrefs.DeleteKey(key); } catch (Exception) { }
#endif
        }

        /// <summary>Flush pending writes to disk / container storage. / 把改动落盘 / 落到容器存储。</summary>
        public static void Save()
        {
#if WECHAT_MINIGAME
            UnityEngine.PlayerPrefs.Save();
#else
            try { TT.PlayerPrefs.Save(); }
            catch (Exception) { UnityEngine.PlayerPrefs.Save(); }
#endif
        }
    }
}
