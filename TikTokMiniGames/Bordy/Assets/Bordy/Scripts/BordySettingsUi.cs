using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Home-screen settings FAB and panel: privacy/terms, invite friends, language.
    /// </summary>
    public class BordySettingsUi : MonoBehaviour
    {
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColOverlay = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color ColAction = new Color(0.36f, 0.44f, 0.86f);

        private GameObject _panelRoot;
        private GameObject _languageRoot;
        private GameObject _legalRoot;
        private Text _fabLabel;
        private Text _titleLabel;
        private Text _statusLabel;
        private Text _closeLabel;
        private Text _privacyLabel;
        private Text _inviteLabel;
        private Text _languageLabel;
        private Text _langTitleLabel;
        private Text _langCloseLabel;
        private Text _langEnLabel;
        private Image _langEnCheck;
        private Text _legalTitleLabel;
        private Text _legalBodyLabel;
        private Text _legalCloseLabel;
        private ScrollRect _legalScroll;

        public static void EnsureOn(Transform canvasRoot)
        {
            if (canvasRoot.GetComponentInChildren<BordySettingsUi>(true) != null)
                return;
            canvasRoot.gameObject.AddComponent<BordySettingsUi>();
        }

        public static void EnsureOn(Canvas canvas)
        {
            if (canvas == null)
                return;
            EnsureOn(canvas.transform);
        }

        private void Awake()
        {
            BordyLocale.SetLanguage(BordyLanguage.En);
            BuildFab();
            BuildPanel();
            BuildLanguagePanel();
            BuildLegalPanel();
            RefreshPanel();
            transform.Find("SettingsFab")?.SetAsLastSibling();
            if (_panelRoot != null)
                _panelRoot.transform.SetAsLastSibling();
            if (_languageRoot != null)
                _languageRoot.transform.SetAsLastSibling();
            if (_legalRoot != null)
                _legalRoot.transform.SetAsLastSibling();
            BordyLocale.Changed += OnLocaleChanged;
        }

        private void OnDestroy()
        {
            BordyLocale.Changed -= OnLocaleChanged;
        }

        private void OnLocaleChanged() => RefreshPanel();

        private void BuildFab()
        {
            _fabLabel = BordyUi.CreateHomeChip(transform, "SettingsFab", ColAccent, () => SetPanelVisible(true));
            BordyUi.PlaceHomeChipByPlay(_fabLabel, BordyUi.HomeChipSettings);
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
            cardRt.sizeDelta = new Vector2(880f, 720f);

            _titleLabel = CreateText("Title", card.transform, "", 48, FontStyle.Bold);
            Anchor(_titleLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _titleLabel.rectTransform.sizeDelta = new Vector2(-48f, 72f);
            _titleLabel.rectTransform.anchoredPosition = new Vector2(0f, -28f);
            _titleLabel.alignment = TextAnchor.MiddleCenter;
            _titleLabel.color = ColInk;

            _statusLabel = CreateText("Status", card.transform, "", 26, FontStyle.Normal);
            Anchor(_statusLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _statusLabel.rectTransform.sizeDelta = new Vector2(-64f, 40f);
            _statusLabel.rectTransform.anchoredPosition = new Vector2(0f, -108f);
            _statusLabel.alignment = TextAnchor.MiddleCenter;
            _statusLabel.color = ColMuted;

            _privacyLabel = BuildActionButton(card.transform, "PrivacyButton", -168f, OnPrivacy);
            _inviteLabel = BuildActionButton(card.transform, "InviteButton", -272f, OnInvite);
            _languageLabel = BuildActionButton(card.transform, "LanguageButton", -376f, OnLanguage);

            var closePill = CreatePill("CloseButton", card.transform, "", ColAccent, Color.white);
            var closeRt = closePill.rectTransform;
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
            closeRt.pivot = new Vector2(0.5f, 0f);
            closeRt.sizeDelta = new Vector2(360f, 88f);
            closeRt.anchoredPosition = new Vector2(0f, 36f);
            _closeLabel = closePill.transform.Find("Text").GetComponent<Text>();
            closePill.gameObject.AddComponent<Button>().onClick.AddListener(() => SetPanelVisible(false));
        }

        private Text BuildActionButton(Transform parent, string name, float yFromTop, UnityEngine.Events.UnityAction onClick)
        {
            var pill = CreatePanel(name, parent, ColAction);
            BordyUi.ApplySliced(pill);
            var rt = pill.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(800f, 88f);
            rt.anchoredPosition = new Vector2(0f, yFromTop);

            var label = CreateText("Text", pill.transform, "", 32, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            Stretch(label.rectTransform);

            var btn = pill.gameObject.AddComponent<Button>();
            btn.targetGraphic = pill;
            btn.onClick.AddListener(onClick);
            return label;
        }

        private void OnPrivacy()
        {
            SetStatus("");
            SetLegalPanelVisible(true);
        }

        private void OnInvite()
        {
            BordyPlatform.InviteFriends();
            SetStatus(BordyStrings.Get(BordyStrings.Keys.SettingsStatusInvite));
        }

        private void OnLanguage()
        {
            SetLanguagePanelVisible(true);
        }

        private void OnSelectEnglish()
        {
            BordyLocale.SetLanguage(BordyLanguage.En);
            RefreshLanguagePanel();
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
                _statusLabel.text = text ?? "";
        }

        private void RefreshPanel()
        {
            if (_titleLabel == null)
                return;

            if (_fabLabel != null)
                _fabLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsFabLabel);

            _titleLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsTitle);
            _closeLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsClose);
            _privacyLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsPrivacy);
            _inviteLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsInvite);
            _languageLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsLanguage);
            RefreshLanguagePanel();
            RefreshLegalPanel();
        }

        private void RefreshLanguagePanel()
        {
            if (_langTitleLabel == null)
                return;

            _langTitleLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsLanguage);
            _langCloseLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsClose);
            _langEnLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsLangEn);
            if (_langEnCheck != null)
                _langEnCheck.gameObject.SetActive(BordyLocale.Current == BordyLanguage.En);
        }

        private void SetPanelVisible(bool visible)
        {
            if (_panelRoot != null)
            {
                _panelRoot.transform.SetAsLastSibling();
                _panelRoot.SetActive(visible);
            }
            if (!visible)
            {
                SetLanguagePanelVisible(false);
                SetLegalPanelVisible(false);
            }
            else
                SetStatus("");
        }

        private void SetLanguagePanelVisible(bool visible)
        {
            if (_languageRoot == null)
                return;
            _languageRoot.transform.SetAsLastSibling();
            _languageRoot.SetActive(visible);
            if (visible)
                RefreshLanguagePanel();
        }

        private void BuildLanguagePanel()
        {
            _languageRoot = CreatePanel("LanguagePanel", transform, ColOverlay).gameObject;
            _languageRoot.SetActive(false);
            Stretch(_languageRoot.GetComponent<RectTransform>());

            var card = CreatePanel("Card", _languageRoot.transform, Color.white);
            BordyUi.ApplySliced(card);
            card.raycastTarget = true;
            var cardRt = card.rectTransform;
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(720f, 420f);

            _langTitleLabel = CreateText("Title", card.transform, "", 44, FontStyle.Bold);
            Anchor(_langTitleLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _langTitleLabel.rectTransform.sizeDelta = new Vector2(-48f, 72f);
            _langTitleLabel.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            _langTitleLabel.alignment = TextAnchor.MiddleCenter;
            _langTitleLabel.color = ColInk;

            var row = CreatePanel("EnRow", card.transform, ColPill);
            BordyUi.ApplySliced(row);
            var rowRt = row.rectTransform;
            rowRt.anchorMin = rowRt.anchorMax = new Vector2(0.5f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.sizeDelta = new Vector2(640f, 88f);
            rowRt.anchoredPosition = new Vector2(0f, -124f);

            _langEnLabel = CreateText("Label", row.transform, "", 32, FontStyle.Normal);
            Anchor(_langEnLabel.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f));
            _langEnLabel.rectTransform.offsetMin = new Vector2(28f, 0f);
            _langEnLabel.rectTransform.offsetMax = new Vector2(-80f, 0f);
            _langEnLabel.alignment = TextAnchor.MiddleLeft;
            _langEnLabel.color = ColInk;

            _langEnCheck = CreatePanel("Check", row.transform, ColAccent);
            var checkRt = _langEnCheck.rectTransform;
            checkRt.anchorMin = checkRt.anchorMax = new Vector2(1f, 0.5f);
            checkRt.pivot = new Vector2(1f, 0.5f);
            checkRt.sizeDelta = new Vector2(28f, 28f);
            checkRt.anchoredPosition = new Vector2(-28f, 0f);

            var rowBtn = row.gameObject.AddComponent<Button>();
            rowBtn.targetGraphic = row;
            rowBtn.onClick.AddListener(OnSelectEnglish);

            var closePill = CreatePill("CloseButton", card.transform, "", ColAccent, Color.white);
            var closeRt = closePill.rectTransform;
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
            closeRt.pivot = new Vector2(0.5f, 0f);
            closeRt.sizeDelta = new Vector2(280f, 80f);
            closeRt.anchoredPosition = new Vector2(0f, 32f);
            _langCloseLabel = closePill.transform.Find("Text").GetComponent<Text>();
            closePill.gameObject.AddComponent<Button>().onClick.AddListener(() => SetLanguagePanelVisible(false));
        }

        private void RefreshLegalPanel()
        {
            if (_legalTitleLabel == null)
                return;
            _legalTitleLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsLegalTitle);
            _legalBodyLabel.fontSize = 24;
            _legalBodyLabel.lineSpacing = 1.15f;
            _legalBodyLabel.text = BordyLegalText.Full;
            BordyFonts.Apply(_legalBodyLabel);
            _legalCloseLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsClose);
            if (_legalScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_legalBodyLabel.rectTransform);
                if (_legalScroll.content != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_legalScroll.content);
                _legalScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void SetLegalPanelVisible(bool visible)
        {
            if (_legalRoot == null)
                return;
            _legalRoot.transform.SetAsLastSibling();
            _legalRoot.SetActive(visible);
            if (visible)
                RefreshLegalPanel();
        }

        private void BuildLegalPanel()
        {
            _legalRoot = CreatePanel("LegalPanel", transform, ColOverlay).gameObject;
            _legalRoot.SetActive(false);
            Stretch(_legalRoot.GetComponent<RectTransform>());

            var card = CreatePanel("Card", _legalRoot.transform, Color.white);
            BordyUi.ApplySliced(card);
            card.raycastTarget = true;
            var cardRt = card.rectTransform;
            cardRt.anchorMin = new Vector2(0.06f, 0.05f);
            cardRt.anchorMax = new Vector2(0.94f, 0.95f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            _legalTitleLabel = CreateText("Title", card.transform, "", 40, FontStyle.Bold);
            Anchor(_legalTitleLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _legalTitleLabel.rectTransform.sizeDelta = new Vector2(-48f, 72f);
            _legalTitleLabel.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            _legalTitleLabel.alignment = TextAnchor.MiddleCenter;
            _legalTitleLabel.color = ColInk;

            var viewportGo = new GameObject("LegalViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewportGo.transform.SetParent(card.transform, false);
            var viewportImg = viewportGo.GetComponent<Image>();
            viewportImg.color = new Color(0.97f, 0.97f, 0.96f, 1f);
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            viewportRt.anchorMin = new Vector2(0f, 0f);
            viewportRt.anchorMax = new Vector2(1f, 1f);
            viewportRt.offsetMin = new Vector2(28f, 140f);
            viewportRt.offsetMax = new Vector2(-56f, -100f);

            var content = new GameObject("LegalContent", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewportGo.transform, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 800f);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 32);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _legalBodyLabel = CreateText("Body", content, "", 26, FontStyle.Normal);
            _legalBodyLabel.alignment = TextAnchor.UpperLeft;
            _legalBodyLabel.color = ColInk;
            _legalBodyLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
            _legalBodyLabel.verticalOverflow = VerticalWrapMode.Overflow;
            _legalBodyLabel.raycastTarget = true;
            var bodyFitter = _legalBodyLabel.gameObject.AddComponent<ContentSizeFitter>();
            bodyFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollbar = CreateVerticalScrollbar(card.transform);
            var barRt = scrollbar.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(1f, 0f);
            barRt.anchorMax = new Vector2(1f, 1f);
            barRt.pivot = new Vector2(1f, 0.5f);
            barRt.sizeDelta = new Vector2(22f, -240f);
            barRt.anchoredPosition = new Vector2(-28f, 20f);

            _legalScroll = viewportGo.GetComponent<ScrollRect>();
            _legalScroll.horizontal = false;
            _legalScroll.vertical = true;
            _legalScroll.movementType = ScrollRect.MovementType.Clamped;
            _legalScroll.viewport = viewportRt;
            _legalScroll.content = content;
            _legalScroll.scrollSensitivity = 48f;
            _legalScroll.verticalScrollbar = scrollbar;
            _legalScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            _legalScroll.verticalScrollbarSpacing = 8f;

            var closePill = CreatePill("CloseButton", card.transform, "", ColAccent, Color.white);
            var closeRt = closePill.rectTransform;
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
            closeRt.pivot = new Vector2(0.5f, 0f);
            closeRt.sizeDelta = new Vector2(280f, 80f);
            closeRt.anchoredPosition = new Vector2(0f, 32f);
            _legalCloseLabel = closePill.transform.Find("Text").GetComponent<Text>();
            closePill.gameObject.AddComponent<Button>().onClick.AddListener(() => SetLegalPanelVisible(false));
        }

        private Scrollbar CreateVerticalScrollbar(Transform parent)
        {
            var track = CreatePanel("Scrollbar", parent, ColPill);
            BordyUi.ApplySliced(track);

            var sliding = new GameObject("Sliding Area", typeof(RectTransform));
            sliding.transform.SetParent(track.transform, false);
            Stretch(sliding.GetComponent<RectTransform>());
            var slidingRt = sliding.GetComponent<RectTransform>();
            slidingRt.offsetMin = new Vector2(3f, 8f);
            slidingRt.offsetMax = new Vector2(-3f, -8f);

            var handle = CreatePanel("Handle", sliding.transform, ColAccent);
            BordyUi.ApplySliced(handle);
            Stretch(handle.rectTransform);

            var bar = track.gameObject.AddComponent<Scrollbar>();
            bar.handleRect = handle.rectTransform;
            bar.targetGraphic = handle;
            bar.direction = Scrollbar.Direction.BottomToTop;
            return bar;
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
