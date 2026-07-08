using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>Bottom-right settings button and language picker overlay. / 右下角设置与语言选择面板。</summary>
    public class BordySettingsUi : MonoBehaviour
    {
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColOverlay = new Color(0f, 0f, 0f, 0.45f);

        private static readonly Color ColFabShadow = new Color(0f, 0f, 0f, 0.28f);
        private static readonly Color ColFabBorder = new Color(0.85f, 0.45f, 0.02f);

        private GameObject _panelRoot;
        private Text _fabLabel;
        private Text _titleLabel;
        private Text _langHeading;
        private Text _zhLabel;
        private Text _enLabel;
        private Text _closeLabel;
        private Image _zhCheck;
        private Image _enCheck;

        public static void EnsureOn(Canvas canvas)
        {
            if (canvas.GetComponentInChildren<BordySettingsUi>(true) != null)
                return;
            canvas.gameObject.AddComponent<BordySettingsUi>();
        }

        private void Awake()
        {
            BuildFab();
            BuildPanel();
            RefreshPanel();
            transform.Find("SettingsFab")?.SetAsLastSibling();
            if (_panelRoot != null)
                _panelRoot.transform.SetAsLastSibling();
            BordyLocale.Changed += OnLocaleChanged;
        }

        private void OnDestroy()
        {
            BordyLocale.Changed -= OnLocaleChanged;
        }

        private void OnLocaleChanged() => RefreshPanel();

        private void BuildFab()
        {
            const float fabW = 240f;
            const float fabH = 88f;

            var root = new GameObject("SettingsFab", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(1f, 0f);
            rootRt.anchorMax = new Vector2(1f, 0f);
            rootRt.pivot = new Vector2(1f, 0f);
            rootRt.sizeDelta = new Vector2(fabW, fabH);
            rootRt.anchoredPosition = new Vector2(-32f, 40f);

            var shadow = CreatePanel("Shadow", root.transform, ColFabShadow);
            BordyUi.ApplySliced(shadow);
            shadow.raycastTarget = false;
            var shadowRt = shadow.rectTransform;
            Stretch(shadowRt);
            shadowRt.anchoredPosition = new Vector2(-4f, -6f);

            var border = CreatePanel("Border", root.transform, ColFabBorder);
            BordyUi.ApplySliced(border);
            border.raycastTarget = false;
            Stretch(border.rectTransform);
            border.rectTransform.offsetMin = new Vector2(-4f, -4f);
            border.rectTransform.offsetMax = new Vector2(4f, 4f);

            var fab = CreatePanel("Fill", root.transform, ColAccent);
            BordyUi.ApplySliced(fab);
            Stretch(fab.rectTransform);

            _fabLabel = CreateText("Label", fab.transform, "", 34, FontStyle.Bold);
            _fabLabel.alignment = TextAnchor.MiddleCenter;
            _fabLabel.color = Color.white;
            _fabLabel.raycastTarget = false;
            Stretch(_fabLabel.rectTransform);
            var outline = _fabLabel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.35f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = root.AddComponent<Button>();
            btn.targetGraphic = fab;
            btn.onClick.AddListener(() => SetPanelVisible(true));
        }

        private void BuildPanel()
        {
            _panelRoot = CreatePanel("SettingsPanel", transform, ColOverlay).gameObject;
            _panelRoot.SetActive(false);
            Stretch(_panelRoot.GetComponent<RectTransform>());

            var card = CreatePanel("Card", _panelRoot.transform, Color.white);
            BordyUi.ApplySliced(card);
            card.raycastTarget = true;
            var cardRt = card.rectTransform;
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(780f, 520f);

            _titleLabel = CreateText("Title", card.transform, "", 48, FontStyle.Bold);
            Anchor(_titleLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _titleLabel.rectTransform.sizeDelta = new Vector2(-48f, 80f);
            _titleLabel.rectTransform.anchoredPosition = new Vector2(0f, -32f);
            _titleLabel.alignment = TextAnchor.MiddleCenter;
            _titleLabel.color = ColInk;

            _langHeading = CreateText("LangHeading", card.transform, "", 34, FontStyle.Bold);
            Anchor(_langHeading.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
            _langHeading.rectTransform.sizeDelta = new Vector2(-64f, 56f);
            _langHeading.rectTransform.anchoredPosition = new Vector2(32f, -120f);
            _langHeading.alignment = TextAnchor.MiddleLeft;
            _langHeading.color = ColMuted;

            BuildLangRow(card.transform, "ZhRow", 0, BordyLanguage.ZhHans, out _zhLabel, out _zhCheck);
            BuildLangRow(card.transform, "EnRow", 1, BordyLanguage.En, out _enLabel, out _enCheck);

            var closePill = CreatePill("CloseButton", card.transform, "", ColAccent, Color.white);
            var closeRt = closePill.rectTransform;
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
            closeRt.pivot = new Vector2(0.5f, 0f);
            closeRt.sizeDelta = new Vector2(360f, 88f);
            closeRt.anchoredPosition = new Vector2(0f, 40f);
            _closeLabel = closePill.transform.Find("Text").GetComponent<Text>();
            closePill.gameObject.AddComponent<Button>().onClick.AddListener(() => SetPanelVisible(false));
        }

        private void BuildLangRow(Transform parent, string name, int index, BordyLanguage lang,
            out Text label, out Image check)
        {
            float y = -200f - index * 100f;
            var row = CreatePanel(name, parent, ColPill);
            BordyUi.ApplySliced(row);
            var rt = row.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(680f, 80f);
            rt.anchoredPosition = new Vector2(0f, y);

            label = CreateText("Label", row.transform, "", 32, FontStyle.Normal);
            Anchor(label.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f));
            label.rectTransform.offsetMin = new Vector2(28f, 0f);
            label.rectTransform.offsetMax = new Vector2(-80f, 0f);
            label.alignment = TextAnchor.MiddleLeft;
            label.color = ColInk;

            check = CreatePanel("Check", row.transform, ColAccent);
            var checkRt = check.rectTransform;
            checkRt.anchorMin = checkRt.anchorMax = new Vector2(1f, 0.5f);
            checkRt.pivot = new Vector2(1f, 0.5f);
            checkRt.sizeDelta = new Vector2(28f, 28f);
            checkRt.anchoredPosition = new Vector2(-28f, 0f);
            check.gameObject.SetActive(false);

            var btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = row;
            btn.onClick.AddListener(() => BordyLocale.SetLanguage(lang));
        }

        private void RefreshPanel()
        {
            if (_titleLabel == null)
                return;

            if (_fabLabel != null)
                _fabLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsFabLabel);

            _titleLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsTitle);
            _langHeading.text = BordyStrings.Get(BordyStrings.Keys.SettingsLanguage);
            _zhLabel.text = BordyStrings.SettingsLangZhLabel();
            _enLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsLangEn);
            _closeLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsClose);

            bool zh = BordyLocale.Current == BordyLanguage.ZhHans;
            _zhCheck.gameObject.SetActive(zh);
            _enCheck.gameObject.SetActive(!zh);
        }

        private void SetPanelVisible(bool visible)
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(visible);
        }

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static Image CreatePill(string name, Transform parent, string label, Color fill, Color textColor)
        {
            var img = CreatePanel(name, parent, fill);
            BordyUi.ApplySliced(img);
            var t = CreateText("Text", img.transform, label, 32, FontStyle.Bold);
            t.alignment = TextAnchor.MiddleCenter;
            t.color = textColor;
            Stretch(t.rectTransform);
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

        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }
    }
}
