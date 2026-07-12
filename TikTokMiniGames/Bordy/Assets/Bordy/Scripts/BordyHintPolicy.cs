namespace Bordy
{
    /// <summary>Free hint budget per campaign tier before rewarded ads. / 各档位免费提示次数。</summary>
    public static class BordyHintPolicy
    {
        /// <summary><c>-1</c> = unlimited (tutorial only).</summary>
        public static int ResolveBudget(string levelId, string tier)
        {
            if (levelId == BordyLevelCatalog.TutorialId)
                return -1;

            if (BordyCampaignCatalog.IsCampaignId(levelId))
                return FreeHintsForTier(tier);

            // Daily, legacy levels, etc. — no free hints; each hint needs a rewarded ad.
            return 0;
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
