namespace Bordy
{
    /// <summary>
    /// Persistent unlock / completion flags. Persists through <see cref="BordyStore"/> so it
    /// uses <c>TT.PlayerPrefs</c> inside the container (Unity's PlayerPrefs does not persist there).
    /// 关卡解锁与完成进度。通过 <see cref="BordyStore"/> 持久化，容器内走 <c>TT.PlayerPrefs</c>
    /// （Unity 自带 PlayerPrefs 在容器内不会持久化）。
    /// </summary>
    public static class BordyProgress
    {
        private const string TutorialKey = "bordy.tutorial.done";

        public static bool TutorialCompleted
        {
            get => BordyStore.GetBool(TutorialKey, false);
            set
            {
                BordyStore.SetBool(TutorialKey, value);
                BordyStore.Save();
            }
        }

        /// <summary>Wipe progress flags (testing). / 清空进度标记（测试用）。</summary>
        public static void Reset()
        {
            BordyStore.DeleteKey(TutorialKey);
            BordyStore.Save();
        }

        public static bool IsLevelUnlocked(string levelId)
        {
            if (levelId == BordyLevelCatalog.TutorialId)
                return true;
            if (levelId == BordyLevelCatalog.Level1Id)
                return TutorialCompleted;
            return false;
        }
    }
}
