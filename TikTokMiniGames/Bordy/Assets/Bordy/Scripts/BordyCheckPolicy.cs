namespace Bordy
{
    /// <summary>Free Check uses per level before a rewarded ad. / 各关免费检查次数，用完后看激励视频。</summary>
    public static class BordyCheckPolicy
    {
        /// <summary>Hard cap per level, including free uses and ad uses. Tutorial is unlimited. / 每关上限（含免费和广告）；教程不限。</summary>
        public const int MaxUsesPerLevel = 3;

        /// <summary>Tutorial is unlimited and ad-free. Campaign and Daily get one free Check. / 教程不限次数、不看广告；闯关和每日各 1 次免费。</summary>
        public static int ResolveBudget(string levelId)
        {
            if (levelId == BordyLevelCatalog.TutorialId)
                return -1;

            return 1;
        }
    }
}
