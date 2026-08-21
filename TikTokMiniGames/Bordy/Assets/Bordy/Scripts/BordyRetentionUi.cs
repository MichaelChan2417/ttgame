using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Home-screen first-level retention buttons (All-in-One guide §3.2):
    /// profile sidebar + desktop shortcut, always visible at the bottom of Home.
    /// </summary>
    public class BordyRetentionUi : MonoBehaviour
    {
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColSidebar = new Color(0.36f, 0.44f, 0.86f);
        private static readonly Color ColShadow = new Color(0f, 0f, 0f, 0.28f);

        private Text _sidebarLabel;
        private Text _shortcutLabel;
        private Text _statusLabel;

        public static void EnsureOn(Transform canvasRoot)
        {
            if (canvasRoot.GetComponentInChildren<BordyRetentionUi>(true) != null)
                return;
            canvasRoot.gameObject.AddComponent<BordyRetentionUi>();
        }

        private void Awake()
        {
            Build();
            RefreshLocale();
            BordyLocale.Changed += RefreshLocale;
        }

        private void OnDestroy()
        {
            BordyLocale.Changed -= RefreshLocale;
        }

        private void Build()
        {
            var bar = new GameObject("RetentionBar", typeof(RectTransform));
            bar.transform.SetParent(transform, false);
            var barRt = bar.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.5f, 0f);
            barRt.anchorMax = new Vector2(0.5f, 0f);
            barRt.pivot = new Vector2(0.5f, 0f);
            barRt.sizeDelta = new Vector2(920f, 128f);
            barRt.anchoredPosition = new Vector2(0f, 28f);

            _sidebarLabel = BuildButton(bar.transform, "SidebarButton", new Vector2(-234f, 20f), ColSidebar, OnSidebar);
            _shortcutLabel = BuildButton(bar.transform, "ShortcutButton", new Vector2(234f, 20f), ColAccent, OnShortcut);

            _statusLabel = CreateText("Status", bar.transform, "", 22, FontStyle.Normal);
            var statusRt = _statusLabel.rectTransform;
            statusRt.anchorMin = new Vector2(0f, 1f);
            statusRt.anchorMax = new Vector2(1f, 1f);
            statusRt.pivot = new Vector2(0.5f, 1f);
            statusRt.sizeDelta = new Vector2(-16f, 32f);
            statusRt.anchoredPosition = new Vector2(0f, 4f);
            _statusLabel.alignment = TextAnchor.MiddleCenter;
            _statusLabel.color = ColMuted;
        }

        private Text BuildButton(Transform parent, string name, Vector2 pos, Color fill, UnityEngine.Events.UnityAction onClick)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(440f, 88f);
            rt.anchoredPosition = pos;

            var shadow = CreatePanel("Shadow", root.transform, ColShadow);
            BordyUi.ApplySliced(shadow);
            shadow.raycastTarget = false;
            Stretch(shadow.rectTransform);
            shadow.rectTransform.anchoredPosition = new Vector2(0f, -5f);

            var pill = CreatePanel("Fill", root.transform, fill);
            BordyUi.ApplySliced(pill);
            Stretch(pill.rectTransform);

            var label = CreateText("Label", pill.transform, "", 30, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            Stretch(label.rectTransform);
            label.raycastTarget = false;

            var btn = root.AddComponent<Button>();
            btn.targetGraphic = pill;
            btn.onClick.AddListener(onClick);
            return label;
        }

        private void OnSidebar()
        {
            SetStatus(BordyStrings.Get(BordyStrings.Keys.SettingsStatusSidebar));
            BordyPlatform.OpenSidebar(ok =>
            {
                SetStatus(BordyStrings.Get(ok
                    ? BordyStrings.Keys.SettingsStatusSidebarOk
                    : BordyStrings.Keys.SettingsStatusSidebarFail));
            });
        }

        private void OnShortcut()
        {
            BordyPlatform.AddDesktopShortcut(ok =>
            {
                SetStatus(BordyStrings.Get(ok
                    ? BordyStrings.Keys.SettingsStatusShortcutOk
                    : BordyStrings.Keys.SettingsStatusShortcut));
            });
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
                _statusLabel.text = text ?? "";
        }

        private void RefreshLocale()
        {
            if (_sidebarLabel != null)
                _sidebarLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsSidebar);
            if (_shortcutLabel != null)
                _shortcutLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsShortcut);
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.font = BordyFonts.Ui;
            t.raycastTarget = false;
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
