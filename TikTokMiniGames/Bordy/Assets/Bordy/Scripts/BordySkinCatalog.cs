using System.Collections.Generic;

namespace Bordy
{
    /// <summary>
    /// Catalog of sun / moon token skins shown in the Shop. The first entry (classic) is
    /// free and selected by default; the rest are unlocked by watching a rewarded ad.
    /// Colours are hex strings turned into sprites by <see cref="BordyTokenSprites"/>.
    ///
    /// 商店里的太阳 / 月亮棋子皮肤目录。第一个（经典）免费且默认选中，其余需观看激励视频解锁。
    /// 颜色以十六进制字符串给出，由 <see cref="BordyTokenSprites"/> 生成贴图。
    /// </summary>
    public static class BordySkinCatalog
    {
        public const string ClassicId = "classic";

        /// <summary>
        /// What art fills a token slot. Palette-based Sun/Moon use the hex fields below;
        /// the illustrated kinds (Cow/Pig/ColaRed/ColaBlue) draw fixed art and ignore the palette.
        /// 图案类型。Sun/Moon 使用下面的调色板；插画类型（奶牛/母猪/红蓝可乐）绘制固定美术，忽略调色板。
        /// </summary>
        public enum TokenArt { Sun, Moon, Cow, Pig, ColaRed, ColaBlue }

        /// <summary>One token skin: a themed sun-slot + moon-slot pair. / 一套皮肤：太阳位 + 月亮位一对。</summary>
        public sealed class SkinDef
        {
            public string Id;
            public string NameEn;
            public string NameZh;
            public bool Free;
            public bool DrawFace;

            // Which art fills the sun slot / moon slot.
            public TokenArt SunArt = TokenArt.Sun;
            public TokenArt MoonArt = TokenArt.Moon;

            // Sun palette (used only when SunArt == Sun).
            public string SunRay;
            public string SunRim;
            public string SunFace;

            // Moon palette (used only when MoonArt == Moon).
            public string MoonRim;
            public string MoonFace;
            public string MoonShade;
        }

        // Order defines display order in the shop grid.
        // 顺序即商店网格里的展示顺序。
        private static readonly List<SkinDef> Skins = new List<SkinDef>
        {
            new SkinDef
            {
                Id = ClassicId, NameEn = "Classic", NameZh = "经典", Free = true, DrawFace = true,
                SunRay = "#FFB347", SunRim = "#FF9A1F", SunFace = "#FFD35A",
                MoonRim = "#5A7FD4", MoonFace = "#8EB4FF", MoonShade = "#6E9EF0",
            },
            new SkinDef
            {
                Id = "farm", NameEn = "Cow & Pig", NameZh = "奶牛 & 母猪", Free = false, DrawFace = true,
                SunArt = TokenArt.Cow, MoonArt = TokenArt.Pig,
            },
            new SkinDef
            {
                Id = "cola", NameEn = "Cola", NameZh = "可乐", Free = false, DrawFace = false,
                SunArt = TokenArt.ColaRed, MoonArt = TokenArt.ColaBlue,
            },
            new SkinDef
            {
                Id = "berry", NameEn = "Berry", NameZh = "莓果", Free = false, DrawFace = false,
                SunRay = "#FF9DB0", SunRim = "#FF6F91", SunFace = "#FFC2CE",
                MoonRim = "#A23E97", MoonFace = "#C86FBE", MoonShade = "#B455AB",
            },
            new SkinDef
            {
                Id = "mono", NameEn = "Slate", NameZh = "石墨", Free = false, DrawFace = false,
                SunRay = "#FFC65C", SunRim = "#F2A93B", SunFace = "#FFD98A",
                MoonRim = "#4A5568", MoonFace = "#718096", MoonShade = "#5A667A",
            },
        };

        public static IReadOnlyList<SkinDef> All => Skins;

        public static int Count => Skins.Count;

        public static SkinDef Get(string id)
        {
            for (int i = 0; i < Skins.Count; i++)
                if (Skins[i].Id == id)
                    return Skins[i];
            return Skins[0]; // fall back to classic
        }

        public static bool Exists(string id)
        {
            for (int i = 0; i < Skins.Count; i++)
                if (Skins[i].Id == id)
                    return true;
            return false;
        }

        public static string DisplayName(SkinDef def)
            => BordyLocale.Current == BordyLanguage.En ? def.NameEn : def.NameZh;
    }
}
