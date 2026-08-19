using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// Bridge to the Douyin/TikTok relationship chain for the daily friend leaderboard.
    ///
    /// Upload (main domain): <see cref="UploadDailyTime"/> stores the player's own time via
    /// tt.setUserCloudStorage. Read (open data domain): <see cref="RequestFriendDaily"/> asks the
    /// open-data bundle to fetch friends' times; that bundle must call
    /// <c>SendMessage("BordyFriendCloud","OnFriendDaily", json)</c> with
    /// {"items":[{"name":..,"seconds":..}]}, handled by <see cref="BordyFriendCloudReceiver"/>.
    ///
    /// 抖音/TikTok 关系链桥接:上传自己的成绩(主域, setUserCloudStorage),请求好友成绩(开放数据域,
    /// 由开放数据域子包回传给 <see cref="BordyFriendCloudReceiver"/>)。
    /// </summary>
    public static class BordyFriendCloud
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BordyDouyinSetUserCloudStorage(string arrJson);
        [DllImport("__Internal")] private static extern void BordyDouyinFetchFriendDaily(string dateKey);
        [DllImport("__Internal")] private static extern void BordyDouyinShareInvite(string title);
#endif

        [System.Serializable] private class DailyVal { public int seconds; public string date; }
        [System.Serializable] private class KV { public string key; public string value; }

        /// <summary>
        /// Store today's solve time via tt.setUserCloudStorage. Param is an array of {key,value};
        /// value is a JSON string {"seconds":..}. / 用 setUserCloudStorage 上传今天成绩。
        /// </summary>
        public static void UploadDailyTime(string date, int seconds)
        {
#if WECHAT_MINIGAME
            return;
#else
            if (BordyAppConfig.WebStandalone)
                return;

            var inner = JsonUtility.ToJson(new DailyVal { seconds = seconds, date = date });
            var kv = new KV { key = "daily_" + date, value = inner };
            string arrJson = "[" + JsonUtility.ToJson(kv) + "]"; // [{"key":..,"value":"{\"seconds\":..}"}]

#if UNITY_WEBGL && !UNITY_EDITOR
            try { BordyDouyinSetUserCloudStorage(arrJson); }
            catch (System.Exception e) { Debug.LogWarning("[BordyFriendCloud] upload failed: " + e.Message); }
#else
            Debug.Log("[BordyFriendCloud] (editor) would upload " + arrJson);
#endif
#endif
        }

        /// <summary>Open the TikTok IM share sheet to invite friends. / 拉起 IM 分享面板邀请好友。</summary>
        public static void ShareInvite(string title)
        {
#if WECHAT_MINIGAME
            return;
#else
            if (BordyAppConfig.WebStandalone)
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            try { BordyDouyinShareInvite(title); }
            catch (System.Exception e) { Debug.LogWarning("[BordyFriendCloud] share failed: " + e.Message); }
#else
            Debug.Log("[BordyFriendCloud] (editor) would share: " + title);
#endif
#endif
        }

        /// <summary>
        /// Authorize + getFriendCloudStorage for the given day. Results arrive asynchronously at
        /// <see cref="BordyFriendCloudReceiver.OnFriendDaily"/>. / 授权并拉取好友当日成绩,结果异步回传。
        /// </summary>
        public static void RequestFriendDaily(string date)
        {
#if WECHAT_MINIGAME
            return;
#else
            if (BordyAppConfig.WebStandalone)
                return;

#if UNITY_WEBGL && !UNITY_EDITOR
            try { BordyDouyinFetchFriendDaily(date); }
            catch (System.Exception e) { Debug.LogWarning("[BordyFriendCloud] request failed: " + e.Message); }
#else
            Debug.Log("[BordyFriendCloud] (editor) would fetch friend daily for " + date);
#endif
#endif
        }
    }

    /// <summary>
    /// Receives friend data pushed from the open data domain via SendMessage, feeds it into
    /// <see cref="BordyFriendDaily"/>, and refreshes an open result popup.
    /// 接收开放数据域回传的好友数据,写入 <see cref="BordyFriendDaily"/> 并刷新弹窗。
    /// </summary>
    public class BordyFriendCloudReceiver : MonoBehaviour
    {
        [System.Serializable] private class FriendDto { public string name; public int seconds; }
        [System.Serializable] private class FriendListDto { public FriendDto[] items; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (GameObject.Find("BordyFriendCloud") != null)
                return;
            var go = new GameObject("BordyFriendCloud");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<BordyFriendCloudReceiver>();
        }

        // Invoked by the open-data bundle: SendMessage("BordyFriendCloud","OnFriendDaily", json).
        public void OnFriendDaily(string json)
        {
            try
            {
                var dto = JsonUtility.FromJson<FriendListDto>(json);
                var list = new List<BordyFriendDaily.Entry>();
                if (dto != null && dto.items != null)
                    foreach (var it in dto.items)
                        list.Add(new BordyFriendDaily.Entry { Name = it.name, Seconds = it.seconds });

                BordyFriendDaily.SetFriends(list);
                BordyDailyResultPopup.RefreshOpenFriends();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[BordyFriendCloud] OnFriendDaily parse failed: " + e.Message);
            }
        }
    }
}
