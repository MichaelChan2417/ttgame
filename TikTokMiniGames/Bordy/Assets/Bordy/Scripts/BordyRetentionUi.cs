using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Home-screen first-level retention chips (All-in-One guide §3.2):
    /// profile sidebar + desktop shortcut, stacked with Shop / Settings on Play's right.
    /// </summary>
    public class BordyRetentionUi : MonoBehaviour
    {
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColSidebar = new Color(0.36f, 0.44f, 0.86f);

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
            _sidebarLabel = BordyUi.CreateHomeChip(transform, "SidebarButton", ColSidebar, OnSidebar);
            BordyUi.PlaceHomeChipByPlay(_sidebarLabel, BordyUi.HomeChipSidebar);

            _shortcutLabel = BordyUi.CreateHomeChip(transform, "ShortcutButton", ColAccent, OnShortcut);
            BordyUi.PlaceHomeChipByPlay(_shortcutLabel, BordyUi.HomeChipShortcut);

            _statusLabel = CreateText("RetentionStatus", transform, "", 22, FontStyle.Normal);
            var statusRt = _statusLabel.rectTransform;
            statusRt.anchorMin = new Vector2(0.5f, 0f);
            statusRt.anchorMax = new Vector2(0.5f, 0f);
            statusRt.pivot = new Vector2(0.5f, 0f);
            statusRt.sizeDelta = new Vector2(720f, 36f);
            statusRt.anchoredPosition = new Vector2(0f, 140f);
            _statusLabel.alignment = TextAnchor.MiddleCenter;
            _statusLabel.color = ColMuted;
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
                _sidebarLabel.text = BordyStrings.Get(BordyStrings.Keys.HomeChipSidebar);
            if (_shortcutLabel != null)
                _shortcutLabel.text = BordyStrings.Get(BordyStrings.Keys.HomeChipShortcut);
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
    }
}
