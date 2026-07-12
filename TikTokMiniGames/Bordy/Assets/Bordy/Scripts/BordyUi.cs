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
