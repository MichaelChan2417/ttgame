using System;
using UnityEngine;

namespace Bordy
{
    /// <summary>Pushes local progress to the cloud backend when logged in. / 登录后把本地进度推到云端。</summary>
    public static class BordyCloudSync
    {
        /// <summary>Skip push while applying a cloud download (avoid echo loop). / 应用云端下载时跳过上传。</summary>
        public static bool SuppressPush { get; set; }

        private static float _nextPushAllowed;

        public static void PushNow()
        {
            if (SuppressPush) return;
            if (!BordyUserService.CloudLoggedIn || BordyUserService.CloudBackend == null) return;

            BordyUserService.CloudBackend.PushSave(
                () => Debug.Log("[BordyCloud] Save synced."),
                err => Debug.LogWarning($"[BordyCloud] Save sync failed: {err}"));
        }

        /// <summary>Debounce rapid writes (e.g. daily in-progress snapshots). / 防抖，避免频繁上传。</summary>
        public static void PushDebounced(float delaySeconds = 2f)
        {
            if (SuppressPush) return;
            _nextPushAllowed = Time.realtimeSinceStartup + delaySeconds;
            BordyHttpRunner.Run(DebouncedPushCoroutine(delaySeconds));
        }

        private static System.Collections.IEnumerator DebouncedPushCoroutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (Time.realtimeSinceStartup < _nextPushAllowed - 0.01f)
                yield break;
            PushNow();
        }
    }
}
