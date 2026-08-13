using System;
using TTSDK;
using UnityEngine;
#if WECHAT_MINIGAME
using WeChatWASM;
#endif

namespace Bordy
{
    /// <summary>
    /// TikTok Minis / WeChat Mini Game ad wrapper for gameplay (rewarded hints, optional interstitials).
    /// Requires <see cref="BordyUserService.SdkInited"/> in container builds.
    /// </summary>
    public static class BordyAdsService
    {
        private static bool _rewardedShowing;
        private static Action _pendingReward;
        private static Action<string> _pendingFail;

        /// <summary>True when a real Ad Unit ID is configured (not SDK demo placeholder).</summary>
        public static bool IsRewardedConfigured =>
            !string.IsNullOrEmpty(BordyAppConfig.RewardedVideoAdUnitId)
            && !BordyAppConfig.RewardedVideoAdUnitId.StartsWith("demo_", StringComparison.Ordinal);

        /// <summary>Call after SDK init succeeds — logs config state for debugging.</summary>
        public static void NotifySdkReady()
        {
#if !UNITY_EDITOR
            if (!IsRewardedConfigured)
            {
                Debug.LogWarning(
                    "[BordyAds] RewardedVideoAdUnitId is still a demo placeholder. " +
                    "Create a placement in TikTok Developer Portal → Monetization and set BordyAppConfig.RewardedVideoAdUnitId.");
            }
            else
            {
                Debug.Log($"[BordyAds] Rewarded ad ready (unit={BordyAppConfig.RewardedVideoAdUnitId}).");
            }
#endif
        }

        private static bool IsWechatRewardedConfigured =>
            !string.IsNullOrEmpty(BordyAppConfig.WechatRewardedAdUnitId);

        /// <summary>Show rewarded video; grant reward only when <c>isEnded == true</c>.</summary>
        public static void ShowRewarded(Action onReward, Action<string> onFail = null)
        {
            if (onReward == null)
                return;

#if UNITY_EDITOR
            if (BordyAppConfig.EditorSimulateRewardedAds)
            {
                Debug.Log("[BordyAds] Editor — simulate rewarded complete.");
                onReward();
                return;
            }

            onFail?.Invoke("editor_no_sim");
            return;
#else
            if (_rewardedShowing)
            {
                onFail?.Invoke("ad_busy");
                return;
            }

            if (!BordyUserService.SdkInited)
            {
                onFail?.Invoke("sdk_not_ready");
                return;
            }

#if WECHAT_MINIGAME
            if (!IsWechatRewardedConfigured)
            {
                onFail?.Invoke("not_configured");
                return;
            }
#else
            if (!IsRewardedConfigured)
            {
                onFail?.Invoke("not_configured");
                return;
            }
#endif

            _pendingReward = onReward;
            _pendingFail = onFail;
            _rewardedShowing = true;

            try
            {
#if WECHAT_MINIGAME
                ShowWechatRewarded();
#else
                ShowTikTokRewarded();
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BordyAds] Show rewarded threw: {e.Message}");
                FailRewarded("show_exception");
            }
#endif
        }

#if !UNITY_EDITOR && WECHAT_MINIGAME
        private static void ShowWechatRewarded()
        {
            var ad = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
            {
                adUnitId = BordyAppConfig.WechatRewardedAdUnitId,
            });
            if (ad == null)
            {
                FailRewarded("create_null");
                return;
            }

            ad.OnError(res =>
            {
                Debug.LogWarning($"[BordyAds] WeChat rewarded error: {res?.errMsg}");
                FailRewarded("error");
            });
            ad.OnClose(res =>
            {
                _rewardedShowing = false;
                if (res != null && res.isEnded)
                {
                    var reward = _pendingReward;
                    ClearPending();
                    reward?.Invoke();
                }
                else
                {
                    FailRewarded("skipped");
                }
            });

            // WeChat rewarded ads can usually be shown directly; if not, they fail via OnError.
            ad.Show();
        }
#endif

#if !UNITY_EDITOR && !WECHAT_MINIGAME
        private static void ShowTikTokRewarded()
        {
            var ad = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam
            {
                AdUnitId = BordyAppConfig.RewardedVideoAdUnitId,
            });
            if (ad == null)
            {
                FailRewarded("create_null");
                return;
            }

            ad.OnError += (code, msg) =>
            {
                Debug.LogWarning($"[BordyAds] Rewarded error {code}: {msg}");
                TryDestroy(ad);
                FailRewarded($"error_{code}");
            };
            ad.OnClose += isEnded =>
            {
                _rewardedShowing = false;
                TryDestroy(ad);
                if (isEnded)
                {
                    var reward = _pendingReward;
                    ClearPending();
                    reward?.Invoke();
                }
                else
                {
                    FailRewarded("skipped");
                }
            };

            ad.Show();
        }
#endif

        /// <summary>Optional interstitial (no reward). Safe to call; failures are logged only.</summary>
        public static void TryShowInterstitial()
        {
#if UNITY_EDITOR
            Debug.Log("[BordyAds] Editor — skip interstitial.");
            return;
#else
            if (!BordyUserService.SdkInited)
                return;

#if WECHAT_MINIGAME
            if (string.IsNullOrEmpty(BordyAppConfig.WechatInterstitialAdUnitId))
                return;
#else
            if (string.IsNullOrEmpty(BordyAppConfig.InterstitialAdUnitId)
                || BordyAppConfig.InterstitialAdUnitId.StartsWith("demo_", StringComparison.Ordinal))
                return;
#endif

            try
            {
#if WECHAT_MINIGAME
                var ad = WX.CreateInterstitialAd(new WXCreateInterstitialAdParam
                {
                    adUnitId = BordyAppConfig.WechatInterstitialAdUnitId,
                });
                if (ad == null)
                    return;

                ad.OnError(res =>
                {
                    Debug.LogWarning($"[BordyAds] WeChat interstitial error: {res?.errMsg}");
                });
                ad.OnClose(() =>
                {
                    Debug.Log("[BordyAds] WeChat interstitial closed.");
                });
                ad.Show();
#else
                var ad = TT.CreateInterstitialAd(new CreateInterstitialAdParam
                {
                    InterstitialAdId = BordyAppConfig.InterstitialAdUnitId,
                });
                if (ad == null)
                    return;

                ad.OnError += (code, msg) =>
                {
                    Debug.LogWarning($"[BordyAds] Interstitial error {code}: {msg}");
                    TryDestroy(ad);
                };
                ad.OnClose += () =>
                {
                    Debug.Log("[BordyAds] Interstitial closed.");
                    TryDestroy(ad);
                };

                ad.Show();
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BordyAds] Show interstitial threw: {e.Message}");
            }
#endif
        }

#if !UNITY_EDITOR
        private static void TryDestroy(TTRewardedVideoAd ad)
        {
            try { ad?.Destroy(); } catch (Exception e) { Debug.LogWarning($"[BordyAds] Destroy rewarded: {e.Message}"); }
        }

        private static void TryDestroy(TTInterstitialAd ad)
        {
            try { ad?.Destroy(); } catch (Exception e) { Debug.LogWarning($"[BordyAds] Destroy interstitial: {e.Message}"); }
        }

        private static void FailRewarded(string reason)
        {
            _rewardedShowing = false;
            var fail = _pendingFail;
            ClearPending();
            fail?.Invoke(reason);
        }

        private static void ClearPending()
        {
            _pendingReward = null;
            _pendingFail = null;
        }
#endif
    }
}
