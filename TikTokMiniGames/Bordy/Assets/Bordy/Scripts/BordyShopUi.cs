using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Icon Shop: a home-screen button that opens a grid of sun / moon skins. Classic is
    /// free and equipped by default; other skins are unlocked by watching a rewarded ad, then
    /// equipped. Built entirely at runtime (like <see cref="BordySettingsUi"/>) and attached
    /// on the Home scene by <see cref="BordyUiBootstrap"/>.
    ///
    /// 图标商店：主页按钮，打开太阳 / 月亮皮肤网格。经典免费且默认装备，其余皮肤观看激励视频后
    /// 解锁并装备。完全运行时构建（同 <see cref="BordySettingsUi"/>），由
    /// <see cref="BordyUiBootstrap"/> 在主页场景挂载。
    /// </summary>
    public class BordyShopUi : MonoBehaviour
    {
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColCard = new Color(0.96f, 0.95f, 0.92f);
        private static readonly Color ColAccent = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColOwned = new Color(0.30f, 0.68f, 0.38f);
        private static readonly Color ColLocked = new Color(0.55f, 0.57f, 0.62f);
        private static readonly Color ColOverlay = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color ColFabFill = new Color(0.36f, 0.44f, 0.86f);

        private GameObject _panelRoot;
        private RectTransform _content;
        private Text _fabLabel;
        private Text _titleLabel;
        private Text _statusLabel;
        private Text _closeLabel;
        private bool _adInFlight;

        public static void EnsureOn(Transform canvasRoot)
        {
            if (canvasRoot.GetComponentInChildren<BordyShopUi>(true) != null)
                return;
            canvasRoot.gameObject.AddComponent<BordyShopUi>();
        }

        private void Awake()
        {
            BuildFab();
            BuildPanel();
            RefreshLocaleText();
            BordyLocale.Changed += OnLocaleChanged;
        }

        private void OnDestroy()
        {
            BordyLocale.Changed -= OnLocaleChanged;
        }

        private void OnLocaleChanged()
        {
            RefreshLocaleText();
            if (_panelRoot != null && _panelRoot.activeSelf)
                RefreshGrid();
        }

        // -----------------------------------------------------------------
        // Compact chip at Play's bottom-right, stacked above Settings.
        // -----------------------------------------------------------------
        private void BuildFab()
        {
            _fabLabel = BordyUi.CreateHomeChip(transform, "ShopFab", ColFabFill, () => SetPanelVisible(true));
            BordyUi.PlaceHomeChipByPlay(_fabLabel, BordyUi.HomeChipShop);
        }

        // -----------------------------------------------------------------
        // Panel (dim overlay + card + scrollable grid). / 面板（遮罩 + 卡片 + 滚动网格）。
        // -----------------------------------------------------------------
        private void BuildPanel()
        {
            _panelRoot = CreatePanel("ShopPanel", transform, ColOverlay).gameObject;
            _panelRoot.SetActive(false);
            Stretch(_panelRoot.GetComponent<RectTransform>());

            var card = CreatePanel("Card", _panelRoot.transform, Color.white);
            BordyUi.ApplySliced(card);
            card.raycastTarget = true;
            var cardRt = card.rectTransform;
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(880f, 1320f);

            _titleLabel = CreateText("Title", card.transform, "", 52, FontStyle.Bold);
            Anchor(_titleLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _titleLabel.rectTransform.sizeDelta = new Vector2(-48f, 80f);
            _titleLabel.rectTransform.anchoredPosition = new Vector2(0f, -36f);
            _titleLabel.alignment = TextAnchor.MiddleCenter;
            _titleLabel.color = ColInk;

            _statusLabel = CreateText("Status", card.transform, "", 28, FontStyle.Normal);
            Anchor(_statusLabel.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _statusLabel.rectTransform.sizeDelta = new Vector2(-64f, 44f);
            _statusLabel.rectTransform.anchoredPosition = new Vector2(0f, -122f);
            _statusLabel.alignment = TextAnchor.MiddleCenter;
            _statusLabel.color = ColMuted;

            // Scroll viewport. / 滚动视口。
            var viewportGo = new GameObject("ShopViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewportGo.transform.SetParent(card.transform, false);
            var viewportImg = viewportGo.GetComponent<Image>();
            viewportImg.color = new Color(1f, 1f, 1f, 0.001f); // near-invisible but a valid mask graphic
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            viewportRt.anchorMin = new Vector2(0f, 0f);
            viewportRt.anchorMax = new Vector2(1f, 1f);
            viewportRt.offsetMin = new Vector2(24f, 150f);
            viewportRt.offsetMax = new Vector2(-24f, -172f);

            _content = new GameObject("ShopGrid", typeof(RectTransform)).GetComponent<RectTransform>();
            _content.SetParent(viewportGo.transform, false);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, 400f);

            var grid = _content.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(392f, 340f);
            grid.spacing = new Vector2(24f, 24f);
            grid.padding = new RectOffset(8, 8, 8, 24);
            grid.childAlignment = TextAnchor.UpperCenter;

            var fitter = _content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewportGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.viewport = viewportRt;
            scroll.content = _content;
            scroll.scrollSensitivity = 24f;

            // Close button. / 关闭按钮。
            var closePill = CreatePill("CloseButton", card.transform, "", ColAccent, Color.white);
            var closeRt = closePill.rectTransform;
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
            closeRt.pivot = new Vector2(0.5f, 0f);
            closeRt.sizeDelta = new Vector2(360f, 92f);
            closeRt.anchoredPosition = new Vector2(0f, 34f);
            _closeLabel = closePill.transform.Find("Text").GetComponent<Text>();
            closePill.gameObject.AddComponent<Button>().onClick.AddListener(() => SetPanelVisible(false));
        }

        private void SetPanelVisible(bool visible)
        {
            if (_panelRoot == null)
                return;
            _panelRoot.transform.SetAsLastSibling();
            _panelRoot.SetActive(visible);
            if (visible)
            {
                SetStatus("");
                RefreshGrid();
            }
        }

        // -----------------------------------------------------------------
        // Grid build. / 网格构建。
        // -----------------------------------------------------------------
        private void RefreshGrid()
        {
            if (_content == null)
                return;

            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);

            foreach (var def in BordySkinCatalog.All)
                BuildCard(def);
        }

        private void BuildCard(BordySkinCatalog.SkinDef def)
        {
            bool unlocked = BordySkins.IsUnlocked(def.Id);
            bool selected = unlocked && BordySkins.Selected == def.Id;

            // Selected cards get a green border: paint the base green and inset an inner fill.
            // 选中卡片显示绿色描边：底色刷绿，再叠一层内缩的浅色填充。
            var cell = CreatePanel("Skin_" + def.Id, _content, selected ? ColOwned : ColCard);
            BordyUi.ApplySliced(cell);
            cell.raycastTarget = true;

            if (selected)
            {
                var inner = CreatePanel("Inner", cell.transform, ColCard);
                BordyUi.ApplySliced(inner);
                inner.raycastTarget = false;
                Stretch(inner.rectTransform);
                inner.rectTransform.offsetMin = new Vector2(6f, 6f);
                inner.rectTransform.offsetMax = new Vector2(-6f, -6f);
            }

            float previewAlpha = unlocked ? 1f : 0.35f;

            var sun = CreateTokenImage("Sun", cell.transform, BordyTokenSprites.SunFor(def.Id), previewAlpha);
            sun.rectTransform.anchorMin = sun.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            sun.rectTransform.pivot = new Vector2(0.5f, 1f);
            sun.rectTransform.sizeDelta = new Vector2(120f, 120f);
            sun.rectTransform.anchoredPosition = new Vector2(-70f, -30f);

            var moon = CreateTokenImage("Moon", cell.transform, BordyTokenSprites.MoonFor(def.Id), previewAlpha);
            moon.rectTransform.anchorMin = moon.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            moon.rectTransform.pivot = new Vector2(0.5f, 1f);
            moon.rectTransform.sizeDelta = new Vector2(120f, 120f);
            moon.rectTransform.anchoredPosition = new Vector2(70f, -30f);

            var name = CreateText("Name", cell.transform, BordySkinCatalog.DisplayName(def), 34, FontStyle.Bold);
            Anchor(name.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            name.rectTransform.sizeDelta = new Vector2(-24f, 48f);
            name.rectTransform.anchoredPosition = new Vector2(0f, -178f);
            name.alignment = TextAnchor.MiddleCenter;
            name.color = ColInk;

            // Action pill. / 操作按钮。
            string label;
            Color fill;
            if (selected)
            {
                label = BordyStrings.Get(BordyStrings.Keys.ShopSelected);
                fill = ColOwned;
            }
            else if (unlocked)
            {
                label = BordyStrings.Get(BordyStrings.Keys.ShopUse);
                fill = ColAccent;
            }
            else
            {
                label = BordyStrings.Get(BordyStrings.Keys.ShopWatchAd);
                fill = ColLocked;
            }

            var pill = CreatePill("Action", cell.transform, label, fill, Color.white);
            var pillRt = pill.rectTransform;
            pillRt.anchorMin = pillRt.anchorMax = new Vector2(0.5f, 0f);
            pillRt.pivot = new Vector2(0.5f, 0f);
            pillRt.sizeDelta = new Vector2(320f, 84f);
            pillRt.anchoredPosition = new Vector2(0f, 24f);

            var btn = pill.gameObject.AddComponent<Button>();
            btn.targetGraphic = pill;
            string skinId = def.Id;
            if (selected)
            {
                btn.interactable = false;
            }
            else if (unlocked)
            {
                btn.onClick.AddListener(() =>
                {
                    BordySkins.SetSelected(skinId);
                    SetStatus("");
                    RefreshGrid();
                });
            }
            else
            {
                btn.onClick.AddListener(() => UnlockViaAd(skinId));
            }
        }

        // -----------------------------------------------------------------
        // Rewarded-ad unlock. / 激励视频解锁。
        // -----------------------------------------------------------------
        private void UnlockViaAd(string skinId)
        {
            if (_adInFlight)
                return;

            // Testing shortcut: unlock instantly, no ad. / 测试快捷：免广告直接解锁。
            if (BordyAppConfig.ShopFreeUnlockForTesting)
            {
                BordySkins.Unlock(skinId);
                BordySkins.SetSelected(skinId);
                SetStatus(BordyStrings.Get(BordyStrings.Keys.ShopUnlocked));
                RefreshGrid();
                return;
            }

            _adInFlight = true;
            SetStatus(BordyStrings.Get(BordyStrings.Keys.ShopLoadingAd));

            BordyAdsService.ShowRewarded(
                () =>
                {
                    _adInFlight = false;
                    BordySkins.Unlock(skinId);
                    BordySkins.SetSelected(skinId);
                    SetStatus(BordyStrings.Get(BordyStrings.Keys.ShopUnlocked));
                    RefreshGrid();
                },
                reason =>
                {
                    _adInFlight = false;
                    SetStatus(MapAdFailReason(reason));
                });
        }

        private static string MapAdFailReason(string reason)
        {
            switch (reason)
            {
                case "editor_no_sim":
                    return BordyStrings.Get(BordyStrings.Keys.ShopAdEditorBlocked);
                case "sdk_not_ready":
                    return BordyStrings.Get(BordyStrings.Keys.ShopAdSdkNotReady);
                case "not_configured":
                    return BordyStrings.Get(BordyStrings.Keys.ShopAdNotConfigured);
                default:
                    return BordyStrings.Get(BordyStrings.Keys.ShopAdFailed);
            }
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
                _statusLabel.text = text;
        }

        private void RefreshLocaleText()
        {
            if (_fabLabel != null)
                _fabLabel.text = BordyStrings.Get(BordyStrings.Keys.ShopFabLabel);
            if (_titleLabel != null)
                _titleLabel.text = BordyStrings.Get(BordyStrings.Keys.ShopTitle);
            if (_closeLabel != null)
                _closeLabel.text = BordyStrings.Get(BordyStrings.Keys.SettingsClose);
        }

        // -----------------------------------------------------------------
        // Helpers. / 辅助方法。
        // -----------------------------------------------------------------
        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static Image CreateTokenImage(string name, Transform parent, Sprite sprite, float alpha)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, alpha);
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
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
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
