using System.IO;
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
        private const string ExportMarkerName = "EXPORT_WECHAT_NOW";

        [InitializeOnLoadMethod]
        private static void AutoExportIfRequested()
        {
            EditorApplication.delayCall += TryExportFromMarker;
        }

        private static void TryExportFromMarker()
        {
            string marker = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ExportMarkerName);
            if (!File.Exists(marker))
                return;

            try { File.Delete(marker); }
            catch (System.Exception e)
            {
                Debug.LogWarning("[BordyWechat] Could not delete export marker: " + e.Message);
                return;
            }

            Debug.Log("[BordyWechat] Export marker found — starting WeChat Mini Game convert.");
            ExportForReview();
        }

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

        [MenuItem("Bordy/WeChat Mini Game/Export For Review")]
        public static void ExportForReview()
        {
            SwitchToWechat();
            var err = WeChatWASM.WXConvertCore.DoExport(true);
            if (err != WeChatWASM.WXConvertCore.WXExportError.SUCCEED)
                throw new System.Exception("[BordyWechat] Export failed: " + err);
            Debug.Log("[BordyWechat] Export succeeded. Open 微信开发者工具 with the DST minigame folder.");
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
