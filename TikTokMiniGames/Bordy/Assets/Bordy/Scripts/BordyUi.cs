using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// Runtime UI sprite helpers. Avoids <c>Resources.GetBuiltinResource&lt;Sprite&gt;("UI/Skin/…")</c>,
    /// which works in the Editor but returns null (and logs an error) at runtime in WebGL / device
    /// builds. The rounded sprite is generated once and shared.
    ///
    /// 运行时 UI 精灵工具。避免 <c>Resources.GetBuiltinResource&lt;Sprite&gt;("UI/Skin/…")</c>——它在
    /// Editor 能用，但在 WebGL / 真机运行时返回 null 并报错。圆角精灵只生成一次并复用。
    /// </summary>
    public static class BordyUi
    {
        private static Sprite _rounded;

        /// <summary>A white, 9-sliced rounded-rectangle sprite (tint it via Image.color). / 白色九宫格圆角矩形精灵（用 Image.color 上色）。</summary>
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
                for (int x = 0; x < size; x++)
                    px[y * size + x] = Inside(x, y, size, radius)
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
            tex.SetPixels32(px);
            tex.Apply();

            // Border = radius → Image.Type.Sliced keeps the corners crisp when stretched.
            // 边框 = 半径 → Image.Type.Sliced 拉伸时圆角保持清晰。
            _rounded = Sprite.Create(
                tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            return _rounded;
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
