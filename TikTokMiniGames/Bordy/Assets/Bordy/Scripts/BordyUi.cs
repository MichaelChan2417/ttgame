using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Runtime UI sprite helpers. Avoids <c>Resources.GetBuiltinResource&lt;Sprite&gt;("UI/Skin/…")</c>,
    /// which works in the Editor but returns null (and logs an error) at runtime in WebGL / device
    /// builds. The rounded sprite is generated once and shared.
    /// </summary>
    public static class BordyUi
    {
        private static Sprite _rounded;
        private static Sprite _solidWhite;

        /// <summary>A white, 9-sliced rounded-rectangle sprite (tint it via Image.color).</summary>
        public static Sprite Rounded()
        {
            if (_rounded != null)
                return _rounded;

            const int size = 48;
            const int radius = 12;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    px[y * size + x] = Inside(x, y, size, radius)
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            tex.SetPixels32(px);
            tex.Apply();

            _rounded = Sprite.Create(
                tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            return _rounded;
        }

        public static Sprite SolidWhite()
        {
            if (_solidWhite != null)
                return _solidWhite;

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _solidWhite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _solidWhite;
        }

        /// <summary>Assign runtime-safe 9-slice sprite for buttons and cards.</summary>
        public static void ApplySliced(Image image)
        {
            if (image == null || IsFlatBackground(image))
                return;

            image.sprite = Rounded();
            image.type = Image.Type.Sliced;
        }

        /// <summary>Guarantee a drawable sprite on interactive UI only — never round-crop page backgrounds.</summary>
        public static void EnsureImageSprite(Image image)
        {
            if (image == null || image.sprite != null)
                return;

            if (IsFlatBackground(image))
            {
                ApplyFlatFill(image);
                return;
            }

            ApplySliced(image);
        }

        public static void ApplyFlatFill(Image image)
        {
            if (image == null)
                return;

            image.sprite = SolidWhite();
            image.type = Image.Type.Simple;
        }

        /// <summary>Repair Images whose built-in Editor sprites did not survive a build.</summary>
        public static void FixMissingSprites(GameObject root)
        {
            if (root == null)
                return;

            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite != null)
                    continue;

                if (IsFlatBackground(image))
                    ApplyFlatFill(image);
                else
                    ApplySliced(image);
            }
        }

        /// <summary>
        /// Compact Home chips. Slots:
        /// 1 Shop = screen bottom-left, 2 Settings = screen bottom-right,
        /// 3 Sidebar / 4 Desktop = stacked under Play's right edge.
        /// </summary>
        public const float HomeSideW = 168f;
        public const float HomeSideH = 64f;
        public const float HomeSideGap = 28f;
        public const int HomeSideFont = 24;
        public const float HomeCornerW = 220f;
        public const float HomeCornerH = 84f;
        public const int HomeCornerFont = 30;
        public const float HomeChipPad = 16f;
        public const float HomeChipDown = 16f;
        public const float HomeChipCorner = 36f;
        public const int HomeChipSidebar = 0;
        public const int HomeChipShortcut = 1;
        public const int HomeChipShop = 2;
        public const int HomeChipSettings = 3;

        public static Text CreateHomeChip(Transform parent, string name, Color fill, UnityEngine.Events.UnityAction onClick)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(HomeSideW, HomeSideH);

            var shadowGo = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadowGo.transform.SetParent(root.transform, false);
            var shadow = shadowGo.GetComponent<Image>();
            shadow.color = new Color(0f, 0f, 0f, 0.28f);
            shadow.raycastTarget = false;
            ApplySliced(shadow);
            StretchRect(shadow.rectTransform);
            shadow.rectTransform.anchoredPosition = new Vector2(0f, -5f);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(root.transform, false);
            var pill = fillGo.GetComponent<Image>();
            pill.color = fill;
            ApplySliced(pill);
            StretchRect(pill.rectTransform);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelGo.transform.SetParent(pill.transform, false);
            var label = labelGo.GetComponent<Text>();
            label.font = BordyFonts.Ui;
            label.fontSize = HomeSideFont;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            StretchRect(label.rectTransform);

            var btn = root.AddComponent<Button>();
            btn.targetGraphic = pill;
            btn.onClick.AddListener(onClick);
            return label;
        }

        public static RectTransform HomeChipRoot(Text label)
            => label != null ? label.GetComponentInParent<Button>()?.transform as RectTransform : null;

        /// <summary>
        /// Shop → bottom-left, Settings → bottom-right,
        /// Sidebar then Desktop stacked just under Play's right side.
        /// </summary>
        public static void PlaceHomeChipByPlay(Text label, int slot)
        {
            var chip = HomeChipRoot(label);
            if (chip == null)
                return;

            bool corner = slot == HomeChipShop || slot == HomeChipSettings;
            chip.sizeDelta = corner
                ? new Vector2(HomeCornerW, HomeCornerH)
                : new Vector2(HomeSideW, HomeSideH);
            if (label != null)
                label.fontSize = corner ? HomeCornerFont : HomeSideFont;

            if (slot == HomeChipShop)
            {
                chip.anchorMin = chip.anchorMax = new Vector2(0f, 0f);
                chip.pivot = new Vector2(0f, 0f);
                chip.anchoredPosition = new Vector2(HomeChipCorner, HomeChipCorner);
                return;
            }

            if (slot == HomeChipSettings)
            {
                chip.anchorMin = chip.anchorMax = new Vector2(1f, 0f);
                chip.pivot = new Vector2(1f, 0f);
                chip.anchoredPosition = new Vector2(-HomeChipCorner, HomeChipCorner);
                return;
            }

            var canvas = chip.parent;
            var play = canvas != null ? canvas.Find("StartButton") as RectTransform : null;
            chip.anchorMin = chip.anchorMax = new Vector2(0.5f, 0.5f);
            chip.pivot = new Vector2(0f, 1f);

            float playRight = play != null ? play.anchoredPosition.x + play.sizeDelta.x * 0.5f : 280f;
            float playBottom = play != null ? play.anchoredPosition.y - play.sizeDelta.y * 0.5f : -155f;
            int row = slot == HomeChipShortcut ? 2 : 1;
            chip.anchoredPosition = new Vector2(
                playRight + HomeChipPad,
                playBottom - HomeChipDown - row * (HomeSideH + HomeSideGap));
        }

        /// <summary>Drop-shadow sibling behind the Home Play button.</summary>
        public static void EnsurePlayButtonShadow(Transform canvas)
        {
            if (canvas == null)
                return;
            var play = canvas.Find("StartButton") as RectTransform;
            if (play == null || canvas.Find("StartButtonShadow") != null)
                return;

            var shadowGo = new GameObject("StartButtonShadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadowGo.transform.SetParent(canvas, false);
            shadowGo.transform.SetSiblingIndex(play.GetSiblingIndex());
            var img = shadowGo.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.28f);
            img.raycastTarget = false;
            ApplySliced(img);

            var rt = shadowGo.GetComponent<RectTransform>();
            rt.anchorMin = play.anchorMin;
            rt.anchorMax = play.anchorMax;
            rt.pivot = play.pivot;
            rt.sizeDelta = play.sizeDelta;
            rt.anchoredPosition = play.anchoredPosition + new Vector2(0f, -10f);
        }

        private static void StretchRect(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Full-screen page fills must stay rectangular — not rounded 9-slice tiles.</summary>
        public static bool IsFlatBackground(Image image)
        {
            if (image == null)
                return false;

            return image.gameObject.name == "Background";
        }

        private static bool Inside(int x, int y, int size, int r)
        {
            float cx = Mathf.Clamp(x, r, size - 1 - r);
            float cy = Mathf.Clamp(y, r, size - 1 - r);
            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy <= r * r + 0.5f;
        }
    }
}
