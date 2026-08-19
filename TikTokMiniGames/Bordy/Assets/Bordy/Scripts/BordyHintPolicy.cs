namespace Bordy
{
    /// <summary>Free hint budget per campaign tier before rewarded ads. / 各档位免费提示次数。</summary>
    public static class BordyHintPolicy
    {
        /// <summary>Hard cap per level, including free uses and ad uses. Tutorial is unlimited. / 每关上限（含免费和广告）；教程不限。</summary>
        public const int MaxUsesPerLevel = 3;

        /// <summary><c>-1</c> = unlimited (tutorial only).</summary>
        public static int ResolveBudget(string levelId, string tier)
        {
            if (levelId == BordyLevelCatalog.TutorialId)
                return -1;

            int free = BordyCampaignCatalog.IsCampaignId(levelId)
                ? FreeHintsForTier(tier)
                : 0;
            return free > MaxUsesPerLevel ? MaxUsesPerLevel : free;
        }

        public static int FreeHintsForTier(string tier)
        {
            if (string.IsNullOrEmpty(tier))
                return 0;

            switch (tier)
            {
                case "easy":
                case "hook":
                    return 2;
                case "medium":
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
