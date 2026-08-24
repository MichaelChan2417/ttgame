using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// UI fonts for WebGL / TikTok container. LegacyRuntime lacks CJK and some symbols (e.g. ←).
    /// 小游戏真机字体：LegacyRuntime 不含中文与部分符号。
    /// </summary>
    public static class BordyFonts
    {
        private static Font _ui;

        public static Font Ui
        {
            get
            {
                if (_ui != null)
                    return _ui;

                _ui = Resources.Load<Font>("Bordy/BordyUI");
                if (_ui == null)
                    _ui = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                return _ui;
            }
        }

        public static bool HasCjk => Resources.Load<Font>("Bordy/BordyUI") != null;

        public static void Apply(Text text)
        {
            if (text == null)
                return;

            // BordyUI is a Noto Sans CJK subset that also carries full Latin + accents,
            // so once it's bundled we use it for everything — CJK, Spanish/Indonesian
            // accents, and ASCII all render from one embedded face. LegacyRuntime (Arial)
            // has no CJK glyphs and shows blank on device, so it's only a last-resort dev
            // fallback when the font asset is missing.
            // BordyUI 是含完整拉丁+重音的 Noto Sans CJK 子集，打进包后统一用它，避免真机空字。
            if (HasCjk)
                text.font = Ui;
            else
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static bool NeedsCjkFont(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char ch in value)
            {
                if (ch >= 0x3040 && ch <= 0x30FF)
                    return true;
                if (ch >= 0xFF66 && ch <= 0xFF9D)
                    return true;
                if (ch >= 0x2E80 && ch <= 0x9FFF)
                    return true;
                if (ch >= 0xF900 && ch <= 0xFAFF)
                    return true;
            }

            return false;
        }

        public static void ApplyAllUnder(Transform root)
        {
            if (root == null)
                return;

            foreach (var text in root.GetComponentsInChildren<Text>(true))
                Apply(text);
        }
    }
}
