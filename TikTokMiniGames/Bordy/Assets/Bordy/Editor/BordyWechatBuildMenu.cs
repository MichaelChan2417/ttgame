using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Bordy.Editor
{
    /// <summary>Editor helpers for switching between TikTok and WeChat Mini Game builds.</summary>
    public static class BordyWechatBuildMenu
    {
        private const string WechatSymbol = "WECHAT_MINIGAME";
        private const string ProductName = "星月棋";
        private const string ProductNameDefault = "Bordy";
        private const string WechatCompany = "上海诠界科技有限公司";
        private const string DefaultCompany = "Quanjie";

        [MenuItem("Bordy/Switch Build Target/WeChat Mini Game")]
        public static void SwitchToWechat()
        {
            SetSymbol(true);
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = WechatCompany;
            SetEmscriptenArg("-s ERROR_ON_UNDEFINED_SYMBOLS=0");
            Debug.Log("[BordyWechat] Switched to WeChat Mini Game build. " +
                "Set WECHAT_MINIGAME scripting symbol, productName=星月棋, companyName=上海诠界科技有限公司.");
        }

        [MenuItem("Bordy/Switch Build Target/TikTok Mini Game")]
        public static void SwitchToTikTok()
        {
            SetSymbol(false);
            PlayerSettings.productName = ProductNameDefault;
            PlayerSettings.companyName = DefaultCompany;
            Debug.Log("[BordyWechat] Switched to TikTok Mini Game build.");
        }

        private static void SetSymbol(bool enable)
        {
            var target = NamedBuildTarget.WebGL;
            string existing = PlayerSettings.GetScriptingDefineSymbols(target);
            var symbols = new System.Collections.Generic.HashSet<string>(
                existing.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));

            if (enable)
                symbols.Add(WechatSymbol);
            else
                symbols.Remove(WechatSymbol);

            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", symbols));
        }

        private static void SetEmscriptenArg(string flag)
        {
            string current = PlayerSettings.WebGL.emscriptenArgs ?? "";
            if (!current.Contains(flag))
                PlayerSettings.WebGL.emscriptenArgs = (current + " " + flag).Trim();
        }
    }
}
