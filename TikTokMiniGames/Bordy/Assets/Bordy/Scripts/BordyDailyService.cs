using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Bordy
{
    /// <summary>
    /// Supplies today's daily puzzle. Source of truth is a per-day JSON file on your server / CDN
    /// (<see cref="BaseUrl"/> + "YYYYMMDD.json"). Once fetched, today's template is cached locally
    /// so re-entry and the read-only result view work without another request. If the server can't
    /// be reached, the daily simply can't be played (no offline fallback — by design).
    ///
    /// For local development, leave <see cref="BaseUrl"/> empty: the built-in template from
    /// <see cref="BordyLevelCatalog"/> is used so you can test the whole flow without a server.
    ///
    /// 提供今天的每日题目。事实来源是服务器/CDN 上按天的 JSON（<see cref="BaseUrl"/> + "YYYYMMDD.json"）。
    /// 拉到后把当天模板缓存本地，再次进入/查看结算无需再请求。服务器不可达则每日挑战不可玩（不做离线兜底）。
    /// 本地开发时把 <see cref="BaseUrl"/> 留空：会用 <see cref="BordyLevelCatalog"/> 内置模板，便于无服务器联调。
    /// </summary>
    public static class BordyDailyService
    {
        /// <summary>
        /// Your CDN/base URL, ending with '/'. e.g. "https://cdn.example.com/bordy/dailies/".
        /// Empty = local dev mode (use the built-in template). NOTE: the domain must be added to the
        /// TikTok mini-game request whitelist in the developer console.
        /// 你的 CDN 基址，以 '/' 结尾。留空=本地开发模式。注意：域名需加入 TikTok 小游戏后台的 request 合法域名。
        /// </summary>
        public static string BaseUrl =
            string.IsNullOrEmpty(BordyAppConfig.ApiBaseUrl)
                ? ""
                : BordyAppConfig.ApiBaseUrl.TrimEnd('/') + "/api/daily/";

        private const string TemplateKey = "bordy.daily.template";       // cached JSON text
        private const string TemplateDateKey = "bordy.daily.template.date"; // yyyyMMdd of the cache

        /// <summary>Today's puzzle, once resolved. / 已就绪的今日题目。</summary>
        public static BordyPuzzleData TodayPuzzle { get; private set; }

        /// <summary>
        /// Synchronous best-effort: return today's puzzle if it's already in memory or the local
        /// cache (or dev mode). Returns null if it must be fetched first. Used by the board.
        /// 同步尽力返回：内存/本地缓存/开发模式里有就返回；需要先拉取则返回 null。棋盘用它。
        /// </summary>
        public static BordyPuzzleData GetTodayPuzzleOrNull()
        {
            if (TodayPuzzle != null)
                return TodayPuzzle;

            var cached = LoadCachedForToday();
            if (cached != null)
            {
                TodayPuzzle = cached;
                return TodayPuzzle;
            }

            if (string.IsNullOrEmpty(BaseUrl))
            {
                // Dev mode: use the built-in template.
                TodayPuzzle = BordyLevelCatalog.Get(BordyLevelCatalog.DailyId);
                return TodayPuzzle;
            }

            return null; // needs a network fetch first
        }

        /// <summary>
        /// Ensure today's puzzle is ready, fetching from the server if needed. Calls <paramref name="onReady"/>
        /// on success, or <paramref name="onError"/> if the server can't be reached / returns bad data.
        /// 确保今日题目就绪，必要时联网拉取。成功回调 <paramref name="onReady"/>，失败回调 <paramref name="onError"/>。
        /// </summary>
        public static void EnsureToday(MonoBehaviour runner, Action onReady, Action<string> onError)
        {
            if (GetTodayPuzzleOrNull() != null)
            {
                onReady?.Invoke();
                return;
            }
            runner.StartCoroutine(FetchToday(onReady, onError));
        }

        private static IEnumerator FetchToday(Action onReady, Action<string> onError)
        {
            string date = BordyDaily.TodayKey;
            string url = BaseUrl + date + ".json";

            using (var req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[BordyDaily] fetch failed: {req.error} ({url})");
                    onError?.Invoke(req.error);
                    yield break;
                }

                var dto = TryParse(req.downloadHandler.text);
                if (dto == null)
                {
                    onError?.Invoke("invalid daily json");
                    yield break;
                }

                TodayPuzzle = dto.ToPuzzle();
                BordyStore.SetString(TemplateKey, req.downloadHandler.text);
                BordyStore.SetString(TemplateDateKey, date);
                BordyStore.Save();
                Debug.Log($"[BordyDaily] template loaded for {date}.");
                onReady?.Invoke();
            }
        }

        private static BordyPuzzleData LoadCachedForToday()
        {
            if (BordyStore.GetString(TemplateDateKey, "") != BordyDaily.TodayKey)
                return null;
            var dto = TryParse(BordyStore.GetString(TemplateKey, ""));
            return dto?.ToPuzzle();
        }

        private static BordyDailyDto TryParse(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                var dto = JsonUtility.FromJson<BordyDailyDto>(json);
                return (dto != null && dto.IsValid()) ? dto : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
