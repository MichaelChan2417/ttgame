using UnityEditor;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Testing helpers: wipe the saved player data (so the next run is treated as first-time)
    /// and print the current profile. Works both in Edit mode and Play mode.
    ///
    /// 测试辅助：清空已保存的玩家数据（让下次进入按首次处理），以及打印当前档案。
    /// 编辑模式和运行模式下都可用。
    /// </summary>
    public static class BordyResetTool
    {
        // Keys must match BordyUserService.ProfileKey / BordyProgress.TutorialKey.
        private const string ProfileKey = "bordy.user.profile";
        private const string TutorialKey = "bordy.tutorial.done";
        private static readonly string[] DailyKeys =
        {
            "bordy.daily.date", "bordy.daily.seconds", "bordy.daily.board",
            "bordy.daily.prog.date", "bordy.daily.prog.board", "bordy.daily.prog.seconds",
        };

        [MenuItem("Bordy/Reset Player Data")]
        public static void ResetPlayerData()
        {
            if (Application.isPlaying)
            {
                BordyUserService.ResetAll();
            }
            else
            {
                // Edit mode: the SDK isn't initialised, so clear the Unity fallback store.
                // 编辑模式下 SDK 未初始化，清掉 Unity 回退存储。
                PlayerPrefs.DeleteKey(ProfileKey);
                PlayerPrefs.DeleteKey(TutorialKey);
                foreach (var k in DailyKeys) PlayerPrefs.DeleteKey(k);
                PlayerPrefs.Save();
            }
            Debug.Log("[Bordy] Player data reset. Next Play will route to the tutorial (first-time).");
        }

        [MenuItem("Bordy/Reset Daily Challenge")]
        public static void ResetDaily()
        {
            if (Application.isPlaying)
                BordyDaily.Reset();
            else
            {
                foreach (var k in DailyKeys) PlayerPrefs.DeleteKey(k);
                PlayerPrefs.Save();
            }
            Debug.Log("[Bordy] Daily challenge reset — playable again today.");
        }

        [MenuItem("Bordy/Print Player Profile")]
        public static void PrintProfile()
        {
            if (Application.isPlaying)
            {
                Debug.Log("[Bordy] Profile: " + BordyUserService.Describe());
            }
            else
            {
                string json = PlayerPrefs.GetString(ProfileKey, "(none)");
                int done = PlayerPrefs.GetInt(TutorialKey, 0);
                Debug.Log($"[Bordy] (edit-mode, Unity fallback store) tutorialDone={done} profile={json}");
            }
        }
    }
}
