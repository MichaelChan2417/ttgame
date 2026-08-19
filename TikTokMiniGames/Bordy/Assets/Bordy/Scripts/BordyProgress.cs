namespace Bordy
{
    /// <summary>
    /// Persistent unlock / completion flags. Persists through <see cref="BordyStore"/>.
    /// 关卡解锁与完成进度。
    /// </summary>
    public static class BordyProgress
    {
        private const string TutorialKey = "bordy.tutorial.done";
        private const string CampaignHighestKey = "bordy.campaign.highest";
        private const string CampaignDonePrefix = "bordy.campaign.done.";

        public static bool TutorialCompleted
        {
            get => BordyStore.GetBool(TutorialKey, false);
            set => SetTutorialCompleted(value);
        }

        public static void SetTutorialCompleted(bool value)
        {
            BordyStore.SetBool(TutorialKey, value);
            BordyStore.Save();
            BordyCloudSync.PushNow();
        }

        /// <summary>Highest unlocked campaign level index (1-based). / 已解锁的最高闯关序号（从 1 起）。</summary>
        public static int CampaignHighestUnlocked
        {
            get => System.Math.Max(1, BordyStore.GetInt(CampaignHighestKey, 1));
            private set
            {
                BordyStore.SetInt(CampaignHighestKey, System.Math.Max(1, value));
                BordyStore.Save();
                BordyCloudSync.PushNow();
            }
        }

        public static void SetCampaignHighestUnlocked(int value)
        {
            CampaignHighestUnlocked = value;
        }

        public static void Reset()
        {
            BordyStore.DeleteKey(TutorialKey);
            BordyStore.DeleteKey(CampaignHighestKey);
            for (int i = 1; i <= 64; i++)
                BordyStore.DeleteKey(CampaignDonePrefix + $"{BordyCampaignCatalog.IdPrefix}{i:D2}");
            BordyStore.Save();
        }

        public static bool IsLevelUnlocked(string levelId)
        {
            if (levelId == BordyLevelCatalog.TutorialId)
                return true;
            if (levelId == BordyLevelCatalog.DailyId)
                return TutorialCompleted;
            if (levelId == BordyLevelCatalog.CampaignModeId)
                return TutorialCompleted;
            return false;
        }

        public static bool IsCampaignLevelUnlocked(int index)
        {
            if (!TutorialCompleted)
                return false;
            return index <= CampaignHighestUnlocked;
        }

        public static bool IsCampaignLevelCompleted(string levelId)
            => BordyStore.GetBool(CampaignDonePrefix + levelId, false);

        /// <summary>True when every campaign level has been completed. / 所有闯关关卡都已通关。</summary>
        public static bool AllCampaignCompleted()
        {
            var levels = BordyCampaignCatalog.Levels;
            if (levels == null || levels.Count == 0)
                return false;
            foreach (var e in levels)
                if (!IsCampaignLevelCompleted(e.Id))
                    return false;
            return true;
        }

        public static void CompleteCampaignLevel(string levelId, int index)
        {
            BordyStore.SetBool(CampaignDonePrefix + levelId, true);
            if (index >= CampaignHighestUnlocked)
                CampaignHighestUnlocked = System.Math.Min(index + 1, System.Math.Max(BordyCampaignCatalog.Count, index + 1));
            BordyStore.Save();
            BordyCloudSync.PushNow();
        }

        /// <summary>
        /// Debug: mark the tutorial done and unlock every campaign level (does not mark them completed,
        /// so boards stay playable). / 调试：完成教程并解锁全部闯关，不标记通关，关卡仍可玩。
        /// </summary>
        public static void UnlockAllCampaignForDebug()
        {
            TutorialCompleted = true;
            int n = BordyCampaignCatalog.Count;
            if (n <= 0)
                n = 30;
            CampaignHighestUnlocked = n;
        }
    }
}
