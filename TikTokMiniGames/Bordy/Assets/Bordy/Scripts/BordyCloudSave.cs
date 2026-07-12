using System;

namespace Bordy
{
    /// <summary>Wire format for daily progress stored on the server. / 服务器每日存档格式。</summary>
    [Serializable]
    public class BordyCloudDailySave
    {
        public string completedDate = "";
        public int completedSeconds;
        public string completedBoard = "";
        public string progressDate = "";
        public string progressBoard = "";
        public int progressSeconds;
    }

    /// <summary>Per-user cloud save keyed by TikTok open_id. / 按 TikTok open_id 存储的云存档。</summary>
    [Serializable]
    public class BordyCloudSave
    {
        public string openId = "";
        public long createdAt;
        public long lastSeenAt;
        public int playCount;
        public bool tutorialCompleted;
        public int campaignHighestUnlocked = 1;
        public string locale = "en";
        public BordyCloudDailySave daily = new BordyCloudDailySave();

        /// <summary>Apply server save to local <see cref="BordyStore"/> (source of truth from cloud). / 把云端存档写入本地。</summary>
        public static void ApplyToLocal(BordyCloudSave save)
        {
            if (save == null) return;

            BordyCloudSync.SuppressPush = true;
            try
            {
                BordyProgress.SetTutorialCompleted(save.tutorialCompleted);
                BordyProgress.SetCampaignHighestUnlocked(save.campaignHighestUnlocked);
                BordyDaily.ApplyFromCloud(save.daily);
                if (!string.IsNullOrEmpty(save.locale))
                    BordyLocale.ApplyFromCloud(save.locale);
            }
            finally
            {
                BordyCloudSync.SuppressPush = false;
            }
        }

        /// <summary>Snapshot current local state for upload. / 采集本地状态用于上传。</summary>
        public static BordyCloudSave CaptureFromLocal(string openId)
        {
            var daily = BordyDaily.CaptureForCloud();
            return new BordyCloudSave
            {
                openId = openId,
                lastSeenAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                playCount = BordyUserService.Profile?.playCount ?? 0,
                tutorialCompleted = BordyProgress.TutorialCompleted,
                campaignHighestUnlocked = BordyProgress.CampaignHighestUnlocked,
                locale = BordyLocale.ToCode(BordyLocale.Current),
                daily = daily,
            };
        }
    }

    /// <summary>Response from POST /api/auth/login. / 登录接口响应。</summary>
    [Serializable]
    public class BordyLoginResponse
    {
        public string openId;
        public string sessionToken;
        public BordyCloudSave save;
        public bool isNewUser;
    }

    [Serializable]
    internal class BordyLoginResponseWrapper
    {
        public string openId;
        public string sessionToken;
        public BordyCloudSave save;
        public bool isNewUser;
        public string error;
    }

    [Serializable]
    internal class BordySavePutResponse
    {
        public bool ok;
        public BordyCloudSave save;
        public string error;
    }
}
