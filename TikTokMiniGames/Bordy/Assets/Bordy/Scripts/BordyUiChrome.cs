using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Runtime fixes for nav chrome (visible back pill, safe-area offset).
    /// WebGL LegacyRuntime does not render ← — upgrade old text-only Back nodes at load.
    /// </summary>
    public static class BordyUiChrome
    {
        private static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);

        public static void EnsureBackButton(Transform root)
        {
            var back = root.Find("Back");
            if (back == null)
                return;

            if (back.Find("Text") != null && back.GetComponent<Image>() != null)
            {
                RefreshBackLabel(root);
                return;
            }

            var btn = back.GetComponent<Button>();
            if (btn == null)
                return;

            var rt = back.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 76);
            rt.anchoredPosition = new Vector2(32, -200);

            var img = back.GetComponent<Image>();
            if (img == null)
            {
                img = back.gameObject.AddComponent<Image>();
                img.sprite = BordyUi.Rounded();
                img.type = Image.Type.Sliced;
                img.color = ColPill;
            }

            btn.targetGraphic = img;

            Text label = back.Find("Text")?.GetComponent<Text>() ?? back.GetComponent<Text>();
            if (label == null)
                label = CreateLabel(back);
            else
            {
                label.raycastTarget = false;
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = 30;
                label.fontStyle = FontStyle.Bold;
                label.color = ColInk;
                if (label.transform != back)
                    Stretch(label.rectTransform);
            }

            BordyFonts.Apply(label);
            RefreshBackLabel(root);
        }

        public static void RefreshBackLabel(Transform root)
        {
            var back = root.Find("Back");
            if (back == null)
                return;

            var label = back.Find("Text")?.GetComponent<Text>() ?? back.GetComponent<Text>();
            if (label == null)
                return;

            label.text = "< " + BordyStrings.Get(BordyStrings.Keys.NavBack);
            BordyFonts.Apply(label);
        }

        private static Text CreateLabel(Transform parent)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.alignment = TextAnchor.MiddleCenter;
            t.fontSize = 30;
            t.fontStyle = FontStyle.Bold;
            t.color = ColInk;
            t.raycastTarget = false;
            Stretch(t.rectTransform);
            return t;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
