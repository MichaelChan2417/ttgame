using System.Collections.Generic;
using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// Procedural Q-style sun / moon sprites for board tokens, now skin-aware. Each skin's
    /// pair is generated once from its palette and cached. <see cref="Sun"/> / <see cref="Moon"/>
    /// return the currently-equipped skin (see <see cref="BordySkins"/>), unless
    /// <see cref="ForceSkinId"/> is set (tutorial always uses classic sun / moon). Use
    /// <see cref="SunFor"/> / <see cref="MoonFor"/> for a specific skin (e.g. shop previews).
    ///
    /// 程序化生成的 Q 版太阳 / 月亮棋子图，支持皮肤。每套皮肤按调色板生成一次并缓存。
    /// <see cref="Sun"/> / <see cref="Moon"/> 默认跟当前装备皮肤；新手引导会设置
    /// <see cref="ForceSkinId"/> 锁死经典太阳/月亮。<see cref="SunFor"/> /
    /// <see cref="MoonFor"/> 返回指定皮肤（如商店预览）。
    /// </summary>
    public static class BordyTokenSprites
    {
        private const int Size = 128;
        private static readonly Dictionary<string, Sprite> s_sun = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite> s_moon = new Dictionary<string, Sprite>();

        /// <summary>
        /// When set, <see cref="Sun"/> / <see cref="Moon"/> ignore the equipped shop skin.
        /// Tutorial sets this to <see cref="BordySkinCatalog.ClassicId"/> so copy about
        /// sun / moon always matches the board.
        /// 设置后太阳/月亮图不再跟商店皮肤走。新手引导固定经典皮肤，文案才对得上。
        /// </summary>
        public static string ForceSkinId;

        private static string ActiveSkinId =>
            string.IsNullOrEmpty(ForceSkinId) ? BordySkins.Selected : ForceSkinId;

        /// <summary>Currently visible sun (forced skin, else equipped). / 当前展示的太阳。</summary>
        public static Sprite Sun => SunFor(ActiveSkinId);

        /// <summary>Currently visible moon (forced skin, else equipped). / 当前展示的月亮。</summary>
        public static Sprite Moon => MoonFor(ActiveSkinId);

        public static Sprite SunFor(string skinId)
        {
            if (s_sun.TryGetValue(skinId, out var sprite) && sprite != null)
                return sprite;
            var skin = BordySkinCatalog.Get(skinId);
            sprite = BuildArt(skin, skin.SunArt);
            s_sun[skinId] = sprite;
            return sprite;
        }

        public static Sprite MoonFor(string skinId)
        {
            if (s_moon.TryGetValue(skinId, out var sprite) && sprite != null)
                return sprite;
            var skin = BordySkinCatalog.Get(skinId);
            sprite = BuildArt(skin, skin.MoonArt);
            s_moon[skinId] = sprite;
            return sprite;
        }

        private static Sprite BuildArt(BordySkinCatalog.SkinDef skin, BordySkinCatalog.TokenArt art)
        {
            switch (art)
            {
                case BordySkinCatalog.TokenArt.Cow: return BuildCow();
                case BordySkinCatalog.TokenArt.Pig: return BuildPig();
                case BordySkinCatalog.TokenArt.ColaRed: return BuildColaRed();
                case BordySkinCatalog.TokenArt.ColaBlue: return BuildColaBlue();
                case BordySkinCatalog.TokenArt.Basketball: return LoadTokenImage("basketball");
                case BordySkinCatalog.TokenArt.Football: return LoadTokenImage("soccer");
                case BordySkinCatalog.TokenArt.Moon: return BuildMoon(skin);
                default: return BuildSun(skin);
            }
        }

        /// <summary>
        /// Load a real photo token from Resources/Bordy/tokens/&lt;name&gt;.png (transparent circular
        /// PNG) and wrap it as a Sprite. Used for the sports skin (basketball / soccer).
        /// 从 Resources/Bordy/tokens 加载真实球图（透明圆形 PNG）作为棋子精灵——用于体育皮肤。
        /// </summary>
        private static Sprite LoadTokenImage(string name)
        {
            var tex = Resources.Load<Texture2D>("Bordy/tokens/" + name);
            if (tex == null)
            {
                Debug.LogWarning("[BordyTokenSprites] Missing Resources/Bordy/tokens/" + name + " — falling back to a blank sun.");
                return BuildSun(BordySkinCatalog.Get(BordySkinCatalog.ClassicId));
            }
            var sprite = Sprite.Create(
                tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect);
            sprite.name = "BordyImg_" + name;
            return sprite;
        }

        private static Sprite BuildSun(BordySkinCatalog.SkinDef skin)
        {
            var tex = Blank();
            var center = new Vector2(Size * 0.5f, Size * 0.5f);
            float faceR = Size * 0.30f;

            Color ray = Hex(skin.SunRay);
            Color rim = Hex(skin.SunRim);
            Color face = Hex(skin.SunFace);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                var tip = center + Polar(faceR + Size * 0.17f, angle);
                var left = center + Polar(faceR + Size * 0.04f, angle - 0.22f);
                var right = center + Polar(faceR + Size * 0.04f, angle + 0.22f);
                FillTriangle(tex, tip, left, right, ray);
            }

            FillCircle(tex, center, faceR + Size * 0.05f, rim);
            FillCircle(tex, center, faceR, face);
            FillCircle(tex, center + new Vector2(-faceR * 0.42f, faceR * 0.08f), faceR * 0.16f, new Color(1f, 0.55f, 0.2f, 0.25f));
            FillCircle(tex, center + new Vector2(faceR * 0.36f, -faceR * 0.18f), faceR * 0.12f, new Color(1f, 1f, 1f, 0.35f));

            if (skin.DrawFace)
            {
                DrawCheek(tex, center + new Vector2(-faceR * 0.42f, -faceR * 0.18f), faceR * 0.14f);
                DrawCheek(tex, center + new Vector2(faceR * 0.42f, -faceR * 0.18f), faceR * 0.14f);
                DrawEye(tex, center + new Vector2(-faceR * 0.28f, faceR * 0.05f), faceR * 0.17f);
                DrawEye(tex, center + new Vector2(faceR * 0.28f, faceR * 0.05f), faceR * 0.17f);
                DrawSmile(tex, center + new Vector2(0f, -faceR * 0.12f), faceR * 0.42f, faceR * 0.22f, Hex("#8A3F00"));
            }

            return ToSprite(tex, "BordySun_" + skin.Id);
        }

        private static Sprite BuildMoon(BordySkinCatalog.SkinDef skin)
        {
            var tex = Blank();
            var center = new Vector2(Size * 0.5f, Size * 0.52f);
            float faceR = Size * 0.31f;

            Color rim = Hex(skin.MoonRim);
            Color face = Hex(skin.MoonFace);
            Color shade = Hex(skin.MoonShade);

            FillCircle(tex, center, faceR + Size * 0.05f, rim);
            FillCircle(tex, center, faceR, face);
            FillCircle(tex, center + new Vector2(faceR * 0.28f, faceR * 0.22f), faceR * 0.55f, shade);
            FillCircle(tex, center + new Vector2(-faceR * 0.15f, faceR * 0.25f), faceR * 0.09f, new Color(1f, 1f, 1f, 0.35f));

            if (skin.DrawFace)
            {
                DrawCheek(tex, center + new Vector2(-faceR * 0.4f, -faceR * 0.16f), faceR * 0.12f, Hex("#FF9AB8"));
                DrawCheek(tex, center + new Vector2(faceR * 0.4f, -faceR * 0.16f), faceR * 0.12f, Hex("#FF9AB8"));
                DrawClosedEye(tex, center + new Vector2(-faceR * 0.27f, faceR * 0.04f), faceR * 0.18f, Hex("#2D4F9C"));
                DrawClosedEye(tex, center + new Vector2(faceR * 0.27f, faceR * 0.04f), faceR * 0.18f, Hex("#2D4F9C"));
                DrawSmile(tex, center + new Vector2(0f, -faceR * 0.14f), faceR * 0.28f, faceR * 0.14f, Hex("#2D4F9C"));

                FillCircle(tex, center + new Vector2(faceR * 0.55f, faceR * 0.42f), Size * 0.035f, Hex("#FFF4A8"));
                DrawStar(tex, center + new Vector2(faceR * 0.62f, faceR * 0.44f), Size * 0.05f, Hex("#FFF4A8"));
            }

            return ToSprite(tex, "BordyMoon_" + skin.Id);
        }

        // -----------------------------------------------------------------
        // Illustrated tokens. / 插画棋子。
        // -----------------------------------------------------------------

        /// <summary>Cute cow face (sun slot). / 可爱奶牛脸（太阳位）。</summary>
        private static Sprite BuildCow()
        {
            var tex = Blank();
            var c = new Vector2(Size * 0.5f, Size * 0.5f);
            Color white = Hex("#FFFFFF");
            Color patch = Hex("#333844");
            Color pink = Hex("#F7A6B8");
            Color pinkDeep = Hex("#E07D95");
            Color horn = Hex("#EAD8AC");
            Color ink = Hex("#2B2F3A");

            // Horns (behind head). / 犄角（在头后）。
            FillEllipse(tex, c + new Vector2(-24f, 34f), 9f, 12f, horn);
            FillEllipse(tex, c + new Vector2(24f, 34f), 9f, 12f, horn);

            // Ears. / 耳朵。
            FillEllipse(tex, c + new Vector2(-38f, 20f), 16f, 11f, white);
            FillEllipse(tex, c + new Vector2(38f, 20f), 16f, 11f, white);
            FillEllipse(tex, c + new Vector2(-38f, 20f), 8f, 6f, pink);
            FillEllipse(tex, c + new Vector2(38f, 20f), 8f, 6f, pink);

            // Head. / 头。
            FillCircle(tex, c + new Vector2(0f, 2f), 42f, white);

            // Cow patches. / 奶牛斑纹。
            FillEllipse(tex, c + new Vector2(-22f, 16f), 14f, 12f, patch);
            FillEllipse(tex, c + new Vector2(26f, -8f), 12f, 10f, patch);

            // Muzzle. / 口鼻。
            FillEllipse(tex, c + new Vector2(0f, -20f), 24f, 16f, pink);
            FillEllipse(tex, c + new Vector2(-9f, -22f), 4.5f, 6f, pinkDeep);
            FillEllipse(tex, c + new Vector2(9f, -22f), 4.5f, 6f, pinkDeep);

            // Eyes. / 眼睛。
            FillCircle(tex, c + new Vector2(-15f, 6f), 6f, ink);
            FillCircle(tex, c + new Vector2(15f, 6f), 6f, ink);
            FillCircle(tex, c + new Vector2(-13f, 8f), 2f, white);
            FillCircle(tex, c + new Vector2(17f, 8f), 2f, white);

            return ToSprite(tex, "BordyCow");
        }

        /// <summary>Cute pig face (moon slot). / 可爱母猪脸（月亮位）。</summary>
        private static Sprite BuildPig()
        {
            var tex = Blank();
            var c = new Vector2(Size * 0.5f, Size * 0.5f);
            Color pink = Hex("#FF9FB6");
            Color pinkDeep = Hex("#F5789B");
            Color nostril = Hex("#B84E6C");
            Color ink = Hex("#5A2438");
            Color white = Hex("#FFFFFF");

            // Ears (triangles, behind head). / 耳朵（三角，头后）。
            FillTriangle(tex, c + new Vector2(-44f, 40f), c + new Vector2(-22f, 40f), c + new Vector2(-30f, 12f), pinkDeep);
            FillTriangle(tex, c + new Vector2(44f, 40f), c + new Vector2(22f, 40f), c + new Vector2(30f, 12f), pinkDeep);

            // Head. / 头。
            FillCircle(tex, c + new Vector2(0f, 0f), 42f, pink);

            // Snout. / 猪鼻。
            FillEllipse(tex, c + new Vector2(0f, -14f), 24f, 18f, pinkDeep);
            FillEllipse(tex, c + new Vector2(-9f, -14f), 4.5f, 7f, nostril);
            FillEllipse(tex, c + new Vector2(9f, -14f), 4.5f, 7f, nostril);

            // Eyes. / 眼睛。
            FillCircle(tex, c + new Vector2(-16f, 14f), 6f, ink);
            FillCircle(tex, c + new Vector2(16f, 14f), 6f, ink);
            FillCircle(tex, c + new Vector2(-14f, 16f), 2f, white);
            FillCircle(tex, c + new Vector2(18f, 16f), 2f, white);

            return ToSprite(tex, "BordyPig");
        }

        /// <summary>Toy-style basketball (sun slot). / Q 版篮球（太阳位）。</summary>
        private static Sprite BuildBasketball()
        {
            var tex = Blank();
            var c = new Vector2(Size * 0.5f, Size * 0.5f);
            float r = Size * 0.40f;
            Color leather = Hex("#F08C2B");
            Color shade = Hex("#C85A10");
            Color light = Hex("#FFC56A");
            Color outline = Hex("#8A3A0C");
            Color seam = Hex("#3A2216");
            Color seamSoft = Hex("#6A3A1C");

            FillEllipse(tex, c + new Vector2(3f, -10f), r * 0.78f, r * 0.22f, new Color(0f, 0f, 0f, 0.16f));
            FillCircle(tex, c, r + 3.5f, outline);
            FillCircle(tex, c, r, leather);
            FillCircleClipped(tex, c + new Vector2(r * 0.28f, -r * 0.32f), r * 0.62f, new Color(shade.r, shade.g, shade.b, 0.38f), c, r);
            FillCircleClipped(tex, c + new Vector2(-r * 0.28f, r * 0.30f), r * 0.42f, new Color(light.r, light.g, light.b, 0.55f), c, r);

            float inset = r * 0.86f;
            float t = 4.2f;
            DrawLineClipped(tex, c + new Vector2(0f, inset), c + new Vector2(0f, -inset), t + 1.2f, seamSoft, c, r);
            DrawLineClipped(tex, c + new Vector2(0f, inset), c + new Vector2(0f, -inset), t, seam, c, r);
            DrawArcStrokeClipped(tex, c + new Vector2(r * 0.40f, 0f), r * 0.90f, 122f, 238f, t + 1.0f, seamSoft, c, r);
            DrawArcStrokeClipped(tex, c + new Vector2(r * 0.40f, 0f), r * 0.90f, 122f, 238f, t, seam, c, r);
            DrawArcStrokeClipped(tex, c + new Vector2(-r * 0.40f, 0f), r * 0.90f, -58f, 58f, t + 1.0f, seamSoft, c, r);
            DrawArcStrokeClipped(tex, c + new Vector2(-r * 0.40f, 0f), r * 0.90f, -58f, 58f, t, seam, c, r);

            FillCircleClipped(tex, c + new Vector2(-r * 0.34f, r * 0.36f), r * 0.13f, new Color(1f, 1f, 1f, 0.42f), c, r);
            FillCircleClipped(tex, c + new Vector2(-r * 0.28f, r * 0.42f), r * 0.06f, new Color(1f, 1f, 1f, 0.55f), c, r);
            return ToSprite(tex, "BordyBasketball");
        }

        /// <summary>Toy-style soccer ball (moon slot). / Q 版足球（月亮位）。</summary>
        private static Sprite BuildFootball()
        {
            var tex = Blank();
            var c = new Vector2(Size * 0.5f, Size * 0.5f);
            float r = Size * 0.40f;
            Color white = Hex("#F7F8FB");
            Color shade = Hex("#C5CDD8");
            Color outline = Hex("#2B3340");
            Color patch = Hex("#1C222C");
            Color seam = Hex("#3A4452");

            FillEllipse(tex, c + new Vector2(3f, -10f), r * 0.78f, r * 0.22f, new Color(0f, 0f, 0f, 0.16f));
            FillCircle(tex, c, r + 3.5f, outline);
            FillCircle(tex, c, r, white);
            FillCircleClipped(tex, c + new Vector2(r * 0.26f, -r * 0.34f), r * 0.60f, new Color(shade.r, shade.g, shade.b, 0.40f), c, r);

            var face = c + new Vector2(0f, r * 0.06f);
            FillRegularPolygonClipped(tex, face, 5, r * 0.24f, patch, -90f, c, r);
            DrawRegularPolygonStrokeClipped(tex, face, 6, r * 0.40f, seam, 2.4f, -90f, c, r);

            for (int i = 0; i < 5; i++)
            {
                float deg = -90f + 36f + i * 72f;
                float ang = deg * Mathf.Deg2Rad;
                var p = c + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (r * 0.72f);
                FillRegularPolygonClipped(tex, p, 5, r * 0.16f, patch, deg - 90f, c, r);

                var inner = face + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (r * 0.40f);
                DrawLineClipped(tex, inner, p, 2.2f, seam, c, r);
            }

            FillCircleClipped(tex, c + new Vector2(-r * 0.32f, r * 0.38f), r * 0.12f, new Color(1f, 1f, 1f, 0.50f), c, r);
            FillCircleClipped(tex, c + new Vector2(-r * 0.26f, r * 0.44f), r * 0.055f, new Color(1f, 1f, 1f, 0.62f), c, r);
            return ToSprite(tex, "BordyFootball");
        }

        private static void DrawLine(Texture2D tex, Vector2 a, Vector2 b, float thickness, Color color)
        {
            DrawLineClipped(tex, a, b, thickness, color, default, -1f);
        }

        private static void DrawLineClipped(Texture2D tex, Vector2 a, Vector2 b, float thickness, Color color, Vector2 clip, float clipR)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) * 1.4f));
            for (int i = 0; i <= steps; i++)
                FillCircleClipped(tex, Vector2.Lerp(a, b, i / (float)steps), thickness * 0.5f, color, clip, clipR);
        }

        private static void FillRegularPolygon(Texture2D tex, Vector2 center, int sides, float radius, Color color, float rotationDeg)
            => FillRegularPolygonClipped(tex, center, sides, radius, color, rotationDeg, default, -1f);

        private static void FillRegularPolygonClipped(Texture2D tex, Vector2 center, int sides, float radius, Color color, float rotationDeg, Vector2 clip, float clipR)
        {
            var pts = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = (rotationDeg + i * 360f / sides) * Mathf.Deg2Rad;
                pts[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            for (int i = 1; i < sides - 1; i++)
                FillTriangleClipped(tex, pts[0], pts[i], pts[i + 1], color, clip, clipR);
        }

        private static void DrawRegularPolygonStrokeClipped(Texture2D tex, Vector2 center, int sides, float radius, Color color, float thickness, float rotationDeg, Vector2 clip, float clipR)
        {
            Vector2 prev = default;
            for (int i = 0; i <= sides; i++)
            {
                float a = (rotationDeg + (i % sides) * 360f / sides) * Mathf.Deg2Rad;
                var p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                if (i > 0)
                    DrawLineClipped(tex, prev, p, thickness, color, clip, clipR);
                prev = p;
            }
        }

        private static void FillCircleClipped(Texture2D tex, Vector2 center, float radius, Color color, Vector2 clip, float clipR)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
            int maxX = Mathf.Min(Size - 1, Mathf.CeilToInt(center.x + radius));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
            int maxY = Mathf.Min(Size - 1, Mathf.CeilToInt(center.y + radius));
            float r2 = radius * radius;
            float clipR2 = clipR > 0f ? clipR * clipR : -1f;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x + 0.5f - center.x;
                    float dy = y + 0.5f - center.y;
                    if (dx * dx + dy * dy > r2)
                        continue;
                    if (clipR2 > 0f)
                    {
                        float cx = x + 0.5f - clip.x;
                        float cy = y + 0.5f - clip.y;
                        if (cx * cx + cy * cy > clipR2)
                            continue;
                    }
                    Blend(tex, x, y, color);
                }
            }
        }

        private static void FillTriangleClipped(Texture2D tex, Vector2 a, Vector2 b, Vector2 c, Color color, Vector2 clip, float clipR)
        {
            float minX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
            float maxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            float minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
            float maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));
            float clipR2 = clipR > 0f ? clipR * clipR : -1f;

            for (int y = Mathf.FloorToInt(minY); y <= Mathf.CeilToInt(maxY); y++)
            {
                for (int x = Mathf.FloorToInt(minX); x <= Mathf.CeilToInt(maxX); x++)
                {
                    if (x < 0 || y < 0 || x >= Size || y >= Size)
                        continue;
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (!PointInTriangle(p, a, b, c))
                        continue;
                    if (clipR2 > 0f)
                    {
                        float dx = p.x - clip.x;
                        float dy = p.y - clip.y;
                        if (dx * dx + dy * dy > clipR2)
                            continue;
                    }
                    Blend(tex, x, y, color);
                }
            }
        }

        private static void DrawArcStrokeClipped(Texture2D tex, Vector2 center, float radius, float startDeg, float endDeg, float thickness, Color color, Vector2 clip, float clipR)
        {
            int steps = 56;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float deg = Mathf.Lerp(startDeg, endDeg, t) * Mathf.Deg2Rad;
                FillCircleClipped(tex, center + Polar(radius, deg), thickness * 0.5f, color, clip, clipR);
            }
        }

        private static Sprite BuildColaRed()
            => BuildCan(Hex("#E8232E"), Hex("#B0141F"), Hex("#F2707A"), "BordyColaRed");

        /// <summary>Blue cola CAN (moon slot). Generic, no logo. / 蓝色可乐易拉罐（月亮位），通用无标识。</summary>
        private static Sprite BuildColaBlue()
            => BuildCan(Hex("#1A4FA0"), Hex("#123A79"), Hex("#6D93D6"), "BordyColaBlue");

        /// <summary>
        /// A soda can: silver lid ellipse + pull tab, cylindrical body with straight sides,
        /// rounded bottom, and left highlight / right shade to sell the cylinder. / 汽水易拉罐：
        /// 银色罐盖椭圆 + 拉环，直边罐身，圆底，左高光右阴影表现圆柱体。
        /// </summary>
        private static Sprite BuildCan(Color body, Color bodyDark, Color bodyLight, string name)
        {
            var tex = Blank();
            float cx = Size * 0.5f;

            Color lid = Hex("#D9DDE4");
            Color lidRim = Hex("#A7AEBA");
            Color lidDark = Hex("#8B93A0");

            const float left = 40f, right = 88f;   // body x-range (width 48)
            const float topY = 98f, botY = 26f;    // body y-range
            const float rimRy = 9f;                 // lid ellipse half-height
            float midX = (left + right) * 0.5f;
            float rx = (right - left) * 0.5f;

            // Bottom curve (behind body). / 罐底弧线（在罐身后）。
            FillEllipse(tex, new Vector2(midX, botY), rx, 8f, bodyDark);

            // Body. / 罐身。
            FillRect(tex, left, botY, right, topY, body);

            // Cylinder shading: left highlight, right shade. / 圆柱明暗：左高光右阴影。
            FillRect(tex, left, botY, left + 12f, topY, new Color(bodyLight.r, bodyLight.g, bodyLight.b, 0.55f));
            FillRect(tex, right - 12f, botY, right, topY, new Color(bodyDark.r, bodyDark.g, bodyDark.b, 0.45f));
            // Specular streak. / 竖直高光条。
            FillRect(tex, left + 6f, botY + 4f, left + 12f, topY - 6f, new Color(1f, 1f, 1f, 0.28f));

            // Neck taper hint near top (slightly darker band). / 罐肩暗带。
            FillRect(tex, left, topY - 10f, right, topY - 4f, new Color(bodyDark.r, bodyDark.g, bodyDark.b, 0.35f));

            // Lid: rim + top plate + inner. / 罐盖：外圈 + 顶面 + 内圈。
            FillEllipse(tex, new Vector2(midX, topY), rx, rimRy, lidRim);
            FillEllipse(tex, new Vector2(midX, topY + 1.5f), rx - 3f, rimRy - 2f, lid);
            FillEllipse(tex, new Vector2(midX, topY + 1.5f), rx - 8f, rimRy - 4f, new Color(lidDark.r, lidDark.g, lidDark.b, 0.5f));

            // Pull tab. / 拉环。
            FillEllipse(tex, new Vector2(midX + 3f, topY + 2f), 7f, 3.5f, lidDark);
            FillEllipse(tex, new Vector2(midX + 3f, topY + 2f), 3.5f, 1.8f, lid);

            return ToSprite(tex, name);
        }

        private static void FillRect(Texture2D tex, float x0, float y0, float x1, float y1, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(x0, x1)));
            int maxX = Mathf.Min(Size - 1, Mathf.CeilToInt(Mathf.Max(x0, x1)));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(y0, y1)));
            int maxY = Mathf.Min(Size - 1, Mathf.CeilToInt(Mathf.Max(y0, y1)));
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                    Blend(tex, x, y, color);
        }

        private static void FillEllipse(Texture2D tex, Vector2 center, float rx, float ry, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - rx));
            int maxX = Mathf.Min(Size - 1, Mathf.CeilToInt(center.x + rx));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - ry));
            int maxY = Mathf.Min(Size - 1, Mathf.CeilToInt(center.y + ry));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = (x + 0.5f - center.x) / rx;
                    float dy = (y + 0.5f - center.y) / ry;
                    if (dx * dx + dy * dy <= 1f)
                        Blend(tex, x, y, color);
                }
            }
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
            var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
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
