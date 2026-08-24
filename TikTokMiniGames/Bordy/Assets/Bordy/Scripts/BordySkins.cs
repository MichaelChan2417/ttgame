using System;

namespace Bordy
{
    /// <summary>
    /// Persistent skin ownership + current selection for the sun / moon tokens.
    /// Classic is always owned and is the default equipped skin. Persists through
    /// <see cref="BordyStore"/> (TT.PlayerPrefs on the container, Unity prefs in Editor).
    ///
    /// 太阳 / 月亮棋子皮肤的持久化拥有状态与当前选择。经典皮肤永远拥有且默认装备。
    /// 通过 <see cref="BordyStore"/> 持久化（容器内走 TT.PlayerPrefs，Editor 走 Unity 存储）。
    /// </summary>
    public static class BordySkins
    {
        private const string UnlockPrefix = "bordy.skin.unlocked.";
        private const string SelectedKey = "bordy.skin.selected";

        /// <summary>Raised when a skin is unlocked or the equipped skin changes. / 解锁或切换皮肤时触发。</summary>
        public static event Action Changed;

        public static bool IsUnlocked(string skinId)
        {
            if (BordyAppConfig.UnlockAllSkinsForTesting)
                return true; // testing: everything unlocked
            var def = BordySkinCatalog.Get(skinId);
            if (def.Free)
                return true;
            return BordyStore.GetBool(UnlockPrefix + skinId, false);
        }

        /// <summary>Mark a skin as owned. Returns true if it flipped from locked to owned.</summary>
        public static bool Unlock(string skinId)
        {
            if (!BordySkinCatalog.Exists(skinId) || IsUnlocked(skinId))
                return false;

            BordyStore.SetBool(UnlockPrefix + skinId, true);
            BordyStore.Save();
            BordyCloudSync.PushNow();
            Changed?.Invoke();
            return true;
        }

        /// <summary>The currently equipped skin id (defaults to classic, falls back if invalid/locked).</summary>
        public static string Selected
        {
            get
            {
                var id = BordyStore.GetString(SelectedKey, BordySkinCatalog.ClassicId);
                if (!BordySkinCatalog.Exists(id) || !IsUnlocked(id))
                    return BordySkinCatalog.ClassicId;
                return id;
            }
        }

        /// <summary>Equip an owned skin. No-op if the skin is not owned. / 装备已拥有的皮肤，未拥有则忽略。</summary>
        public static void SetSelected(string skinId)
        {
            if (!IsUnlocked(skinId) || Selected == skinId)
                return;

            BordyStore.SetString(SelectedKey, skinId);
            BordyStore.Save();
            BordyCloudSync.PushNow();
            Changed?.Invoke();
        }

        public static int UnlockedCount()
        {
            int n = 0;
            foreach (var def in BordySkinCatalog.All)
                if (IsUnlocked(def.Id))
                    n++;
            return n;
        }
    }
}
