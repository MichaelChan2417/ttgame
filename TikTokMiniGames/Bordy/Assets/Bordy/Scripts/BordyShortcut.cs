using System;
using TTSDK;
using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// "Add to Home Screen" (desktop shortcut) helper. Per the TikTok retention guide this
    /// lifts retention/GMV a lot; we prompt once, after the first Daily Challenge is solved,
    /// and never again once added. / 加桌快捷方式:首次完成每日挑战后提示一次,添加后不再提示。
    /// </summary>
    public static class BordyShortcut
    {
        private const string AddedKey = "bordy.shortcut.added";
        private const string PromptedKey = "bordy.shortcut.prompted";

        /// <summary>True once the player has added the shortcut (via our button). / 已加桌。</summary>
        public static bool Added
        {
            get => BordyStore.GetBool(AddedKey, false);
            private set { BordyStore.SetBool(AddedKey, value); BordyStore.Save(); }
        }

        /// <summary>True once we've auto-prompted, so we don't nag. / 已自动提示过。</summary>
        public static bool Prompted
        {
            get => BordyStore.GetBool(PromptedKey, false);
            set { BordyStore.SetBool(PromptedKey, value); BordyStore.Save(); }
        }

        /// <summary>
        /// Whether the add-to-home prompt should be shown now. Shown after a daily solve until the
        /// player has actually added the shortcut (NOT once-only, so it keeps nudging until added).
        /// 是否该弹加桌提示：每次完成每日后都弹，直到玩家真正加桌为止（非一次性）。
        /// </summary>
        public static bool ShouldPrompt => !BordyAppConfig.WebStandalone && !Added;

        /// <summary>Trigger the platform add-to-home flow. / 触发平台加桌流程。</summary>
        public static void Add(Action<bool> onDone)
        {
            if (BordyAppConfig.WebStandalone)
            {
                onDone?.Invoke(false);
                return;
            }

            try
            {
                TT.AddShortcut(ok =>
                {
                    if (ok) Added = true;
                    onDone?.Invoke(ok);
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning("[BordyShortcut] AddShortcut failed: " + e.Message);
                onDone?.Invoke(false);
            }
        }
    }
}
