using System;
using UnityEngine;
using TTSDK;

namespace Bordy
{
    /// <summary>
    /// Persistent key/value storage for Bordy.
    ///
    /// IMPORTANT: on the TikTok mini-game container you MUST use <c>TT.PlayerPrefs</c> —
    /// the official docs warn that Unity's own <see cref="UnityEngine.PlayerPrefs"/> does
    /// NOT persist inside the container. This wrapper routes everything through
    /// <c>TT.PlayerPrefs</c> and only falls back to Unity's store if the SDK call throws
    /// (e.g. in the Editor before InitSDK, or when the SDK is unavailable), so saves never
    /// crash the game.
    ///
    /// Bordy 的持久化 KV 封装。重要：在 TikTok 小游戏容器里必须用 <c>TT.PlayerPrefs</c>，
    /// 官方文档明确说明 Unity 自带的 <see cref="UnityEngine.PlayerPrefs"/> 在容器内不会持久化。
    /// 本封装优先走 <c>TT.PlayerPrefs</c>，仅在 SDK 调用抛错时（如 Editor 未 InitSDK、SDK 不可用）
    /// 回退到 Unity 存储，保证存档不会让游戏崩溃。
    /// </summary>
    public static class BordyStore
    {
        public static string GetString(string key, string defaultValue = "")
        {
            try { return TT.PlayerPrefs.GetString(key, defaultValue); }
            catch (Exception) { return PlayerPrefs.GetString(key, defaultValue); }
        }

        public static void SetString(string key, string value)
        {
            try { TT.PlayerPrefs.SetString(key, value); }
            catch (Exception) { PlayerPrefs.SetString(key, value); }
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            try { return TT.PlayerPrefs.GetInt(key, defaultValue); }
            catch (Exception) { return PlayerPrefs.GetInt(key, defaultValue); }
        }

        public static void SetInt(string key, int value)
        {
            try { TT.PlayerPrefs.SetInt(key, value); }
            catch (Exception) { PlayerPrefs.SetInt(key, value); }
        }

        public static bool GetBool(string key, bool defaultValue = false)
            => GetInt(key, defaultValue ? 1 : 0) == 1;

        public static void SetBool(string key, bool value)
            => SetInt(key, value ? 1 : 0);

        public static void DeleteKey(string key)
        {
            try { TT.PlayerPrefs.DeleteKey(key); }
            catch (Exception) { /* ignore */ }
            // Also clear the Unity fallback copy so a reset is thorough in the Editor.
            // 同时清掉 Unity 回退副本，保证 Editor 里重置彻底。
            try { PlayerPrefs.DeleteKey(key); } catch (Exception) { }
        }

        /// <summary>Flush pending writes to disk / container storage. / 把改动落盘 / 落到容器存储。</summary>
        public static void Save()
        {
            try { TT.PlayerPrefs.Save(); }
            catch (Exception) { PlayerPrefs.Save(); }
        }
    }
}
