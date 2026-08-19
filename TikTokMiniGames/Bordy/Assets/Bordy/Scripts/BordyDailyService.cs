using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Bordy
{
    /// <summary>
    /// Supplies today's daily puzzle. Source of truth is a per-day JSON file on your server / CDN
    /// (<see cref="BaseUrl"/> + "YYYYMMDD.json"). Once fetched, today's template is cached locally
    /// so re-entry and the read-only result view work without another request.
    ///
    /// If the server can't be reached or returns bad data, fall back to the built-in template from
    /// <see cref="BordyLevelCatalog"/> so Daily is always playable (WeChat Mini Game especially
    /// cannot rely on unregistered request domains).
    ///
    /// For local development / WeChat, leave <see cref="BaseUrl"/> empty: the built-in template is
    /// used immediately with no network request.
    ///
    /// 提供今天的每日题目。事实来源是服务器/CDN 上按天的 JSON。拉到后缓存本地。
    /// 服务器不可达或数据无效时，回退到 <see cref="BordyLevelCatalog"/> 内置固定题，保证每日可玩。
    /// 微信小游戏审核不能请求未备案域名，因此微信构建不联网、直接用内置题。
    /// </summary>
    public static class BordyDailyService
    {
        /// <summary>
        /// CDN/base URL, ending with '/'. Empty = use the built-in template (no network).
        /// WeChat Mini Game always stays empty so review builds do not hit workers.dev.
        /// 你的 CDN 基址，以 '/' 结尾。留空=内置模板。微信构建强制留空。
        /// </summary>
        public static string BaseUrl
        {
            get
            {
#if WECHAT_MINIGAME
                return "";
#else
                return string.IsNullOrEmpty(BordyAppConfig.ApiBaseUrl)
                    ? ""
                    : BordyAppConfig.ApiBaseUrl.TrimEnd('/') + "/api/daily/";
#endif
            }
        }

        private const string TemplateKey = "bordy.daily.template";       // cached JSON text
        private const string TemplateDateKey = "bordy.daily.template.date"; // yyyyMMdd of the cache

        /// <summary>Today's puzzle, once resolved. / 已就绪的今日题目。</summary>
        public static BordyPuzzleData TodayPuzzle { get; private set; }

        /// <summary>
        /// Use the shipped 6×6 daily template. Always succeeds.
        /// 使用随包内置的 6×6 每日题，一定能拿到。
        /// </summary>
        public static BordyPuzzleData UseBuiltInFallback(string reason)
        {
            Debug.LogWarning($"[BordyDaily] source = built-in template ({reason}).");
            TodayPuzzle = BordyLevelCatalog.Get(BordyLevelCatalog.DailyId);
            return TodayPuzzle;
        }

        /// <summary>
        /// Synchronous best-effort: return today's puzzle if it's already in memory or the local
        /// cache (or built-in mode). Returns null if it must be fetched first. Used by the board.
        /// 同步尽力返回：内存/本地缓存/内置模式里有就返回；需要先拉取则返回 null。棋盘用它。
        /// </summary>
        public static BordyPuzzleData GetTodayPuzzleOrNull()
        {
            if (TodayPuzzle != null)
                return TodayPuzzle;

            var cached = LoadCachedForToday();
            if (cached != null)
            {
                Debug.Log("[BordyDaily] source = LOCAL CACHE (no download this run).");
                TodayPuzzle = cached;
                return TodayPuzzle;
            }

            if (string.IsNullOrEmpty(BaseUrl))
                return UseBuiltInFallback("BaseUrl empty");

            return null; // needs a network fetch first
        }

        /// <summary>
        /// Ensure today's puzzle is ready, fetching from the server if needed.
        /// Always ends with a playable puzzle: cloud on success, built-in template on failure.
        /// 确保今日题目就绪。成功用云端题；失败则用内置固定题，保证能进每日。
        /// </summary>
        public static void EnsureToday(MonoBehaviour runner, Action onReady, Action<string> onError)
        {
            if (GetTodayPuzzleOrNull() != null)
            {
                onReady?.Invoke();
                return;
            }

            if (runner == null)
            {
                UseBuiltInFallback("no runner for fetch");
                onReady?.Invoke();
                return;
            }

            runner.StartCoroutine(FetchToday(onReady));
        }

        private static IEnumerator FetchToday(Action onReady)
        {
            string date = BordyDaily.TodayKey;
            string url = BaseUrl + date + ".json";

            using (var req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[BordyDaily] fetch failed: {req.error} ({url}) — using built-in template.");
                    UseBuiltInFallback(req.error);
                    onReady?.Invoke();
                    yield break;
                }

                var dto = TryParse(req.downloadHandler.text);
                if (dto == null)
                {
                    Debug.LogWarning("[BordyDaily] invalid daily json — using built-in template.");
                    UseBuiltInFallback("invalid daily json");
                    onReady?.Invoke();
                    yield break;
                }

                TodayPuzzle = dto.ToPuzzle();
                BordyStore.SetString(TemplateKey, req.downloadHandler.text);
                BordyStore.SetString(TemplateDateKey, date);
                BordyStore.Save();
                Debug.Log($"[BordyDaily] source = CLOUD download OK for {date} ({url}).");
                onReady?.Invoke();
            }
        }

        /// <summary>Drop the cached template + in-memory puzzle so the next entry re-downloads. / 清掉缓存模板与内存题目，下次进入重新下载。</summary>
        public static void ClearCache()
        {
            TodayPuzzle = null;
            BordyStore.DeleteKey(TemplateKey);
            BordyStore.DeleteKey(TemplateDateKey);
            BordyStore.Save();
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
