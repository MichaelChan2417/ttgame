using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// Procedural Q-style sun / moon sprites for board tokens.
    /// 程序化生成的 Q 版太阳 / 月亮棋子图。
    /// </summary>
    public static class BordyTokenSprites
    {
        private const int Size = 128;
        private static Sprite s_sun;
        private static Sprite s_moon;

        public static Sprite Sun => s_sun ??= BuildSun();
        public static Sprite Moon => s_moon ??= BuildMoon();

        private static Sprite BuildSun()
        {
            var tex = Blank();
            var center = new Vector2(Size * 0.5f, Size * 0.5f);
            float faceR = Size * 0.30f;

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                var tip = center + Polar(faceR + Size * 0.17f, angle);
                var left = center + Polar(faceR + Size * 0.04f, angle - 0.22f);
                var right = center + Polar(faceR + Size * 0.04f, angle + 0.22f);
                FillTriangle(tex, tip, left, right, Hex("#FFB347"));
            }

            FillCircle(tex, center, faceR + Size * 0.05f, Hex("#FF9A1F"));
            FillCircle(tex, center, faceR, Hex("#FFD35A"));
            FillCircle(tex, center + new Vector2(-faceR * 0.42f, faceR * 0.08f), faceR * 0.16f, new Color(1f, 0.55f, 0.2f, 0.35f));
            FillCircle(tex, center + new Vector2(faceR * 0.36f, -faceR * 0.18f), faceR * 0.12f, new Color(1f, 0.9f, 0.45f, 0.45f));

            DrawCheek(tex, center + new Vector2(-faceR * 0.42f, -faceR * 0.18f), faceR * 0.14f);
            DrawCheek(tex, center + new Vector2(faceR * 0.42f, -faceR * 0.18f), faceR * 0.14f);
            DrawEye(tex, center + new Vector2(-faceR * 0.28f, faceR * 0.05f), faceR * 0.17f);
            DrawEye(tex, center + new Vector2(faceR * 0.28f, faceR * 0.05f), faceR * 0.17f);
            DrawSmile(tex, center + new Vector2(0f, -faceR * 0.12f), faceR * 0.42f, faceR * 0.22f, Hex("#8A3F00"));

            return ToSprite(tex, "BordySun");
        }

        private static Sprite BuildMoon()
        {
            var tex = Blank();
            var center = new Vector2(Size * 0.5f, Size * 0.52f);
            float faceR = Size * 0.31f;

            FillCircle(tex, center, faceR + Size * 0.05f, Hex("#5A7FD4"));
            FillCircle(tex, center, faceR, Hex("#8EB4FF"));
            FillCircle(tex, center + new Vector2(faceR * 0.28f, faceR * 0.22f), faceR * 0.55f, Hex("#6E9EF0"));
            FillCircle(tex, center + new Vector2(-faceR * 0.15f, faceR * 0.25f), faceR * 0.09f, new Color(1f, 1f, 1f, 0.35f));

            DrawCheek(tex, center + new Vector2(-faceR * 0.4f, -faceR * 0.16f), faceR * 0.12f, Hex("#FF9AB8"));
            DrawCheek(tex, center + new Vector2(faceR * 0.4f, -faceR * 0.16f), faceR * 0.12f, Hex("#FF9AB8"));
            DrawClosedEye(tex, center + new Vector2(-faceR * 0.27f, faceR * 0.04f), faceR * 0.18f, Hex("#2D4F9C"));
            DrawClosedEye(tex, center + new Vector2(faceR * 0.27f, faceR * 0.04f), faceR * 0.18f, Hex("#2D4F9C"));
            DrawSmile(tex, center + new Vector2(0f, -faceR * 0.14f), faceR * 0.28f, faceR * 0.14f, Hex("#2D4F9C"));

            FillCircle(tex, center + new Vector2(faceR * 0.55f, faceR * 0.42f), Size * 0.035f, Hex("#FFF4A8"));
            DrawStar(tex, center + new Vector2(faceR * 0.62f, faceR * 0.44f), Size * 0.05f, Hex("#FFF4A8"));

            return ToSprite(tex, "BordyMoon");
        }

        private static Texture2D Blank()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var clear = new Color(0f, 0f, 0f, 0f);
            var pixels = new Color[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = clear;
            tex.SetPixels(pixels);
            return tex;
        }

        private static Sprite ToSprite(Texture2D tex, string name)
        {
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        private static Vector2 Polar(float radius, float angle) =>
            new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

        private static void FillCircle(Texture2D tex, Vector2 center, float radius, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
            int maxX = Mathf.Min(Size - 1, Mathf.CeilToInt(center.x + radius));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
            int maxY = Mathf.Min(Size - 1, Mathf.CeilToInt(center.y + radius));
            float r2 = radius * radius;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x + 0.5f - center.x;
                    float dy = y + 0.5f - center.y;
                    if (dx * dx + dy * dy <= r2)
                        Blend(tex, x, y, color);
                }
            }
        }

        private static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            float minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
            float maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            float minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
            float maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));

            for (int y = Mathf.FloorToInt(minY); y <= Mathf.CeilToInt(maxY); y++)
            {
                for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++)
                {
                    if (x < 0 || y < 0 || x >= Size || y >= Size)
                        continue;
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (PointInTriangle(p, a, b, c))
                        Blend(tex, x, y, color);
                }
            }
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) =>
            (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

        private static void DrawEye(Texture2D tex, Vector2 center, float radius)
        {
            FillCircle(tex, center, radius, Color.white);
            FillCircle(tex, center + new Vector2(radius * 0.15f, -radius * 0.1f), radius * 0.55f, Hex("#2E1A00"));
            FillCircle(tex, center + new Vector2(radius * 0.28f, radius * 0.18f), radius * 0.18f, Color.white);
        }

        private static void DrawClosedEye(Texture2D tex, Vector2 center, float radius, Color color)
        {
            DrawArcStroke(tex, center, radius, 200f, 340f, Mathf.Max(2f, radius * 0.16f), color);
        }

        private static void DrawSmile(Texture2D tex, Vector2 center, float width, float height, Color color)
        {
            DrawArcStroke(tex, center, width, 200f, 340f, Mathf.Max(2f, height * 0.35f), color);
        }

        private static void DrawCheek(Texture2D tex, Vector2 center, float radius, Color? tint = null)
        {
            FillCircle(tex, center, radius, tint ?? new Color(1f, 0.55f, 0.55f, 0.45f));
        }

        private static void DrawArcStroke(Texture2D tex, Vector2 center, float radius, float startDeg, float endDeg, float thickness, Color color)
        {
            int steps = 48;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float deg = Mathf.Lerp(startDeg, endDeg, t) * Mathf.Deg2Rad;
                var point = center + Polar(radius, deg);
                FillCircle(tex, point, thickness * 0.5f, color);
            }
        }

        private static void DrawStar(Texture2D tex, Vector2 center, float radius, Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI * 0.5f;
                var tip = center + Polar(radius, angle);
                var tail = center + Polar(radius * 0.2f, angle);
                FillCircle(tex, tip, radius * 0.22f, color);
                FillCircle(tex, (tip + tail) * 0.5f, radius * 0.12f, color);
            }
        }

        private static void Blend(Texture2D tex, int x, int y, Color color)
        {
            var dst = tex.GetPixel(x, y);
            float a = color.a + dst.a * (1f - color.a);
            if (a <= 0f)
                return;
            var outColor = new Color(
                (color.r * color.a + dst.r * dst.a * (1f - color.a)) / a,
                (color.g * color.a + dst.g * dst.a * (1f - color.a)) / a,
                (color.b * color.a + dst.b * dst.a * (1f - color.a)) / a,
                a);
            tex.SetPixel(x, y, outColor);
        }

        private static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;
            return Color.white;
        }
    }
}
