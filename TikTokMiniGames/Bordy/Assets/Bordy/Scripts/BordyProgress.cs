using UnityEngine;

namespace Bordy
{
    /// <summary>Persistent unlock / completion flags. / 关卡解锁与完成进度。</summary>
    public static class BordyProgress
    {
        private const string TutorialKey = "bordy.tutorial.done";

        public static bool TutorialCompleted
        {
            get => PlayerPrefs.GetInt(TutorialKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(TutorialKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
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
