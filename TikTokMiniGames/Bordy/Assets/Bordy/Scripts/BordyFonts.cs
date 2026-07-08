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
            text.font = Ui;
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
