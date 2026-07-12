using System.Collections.Generic;

namespace Bordy
{
    /// <summary>Localized UI strings (language packs). / 本地化文案（语言包）。</summary>
    public static class BordyStrings
    {
        public static class Keys
        {
            public const string SettingsTitle = "settings.title";
            public const string SettingsFabLabel = "settings.fab";
            public const string SettingsLanguage = "settings.language";
            public const string SettingsLangZh = "settings.lang_zh";
            public const string SettingsLangEn = "settings.lang_en";
            public const string SettingsClose = "settings.close";

            public const string NavBack = "nav.back";

            public const string HomeSubtitle = "home.subtitle";
            public const string HomeStart = "home.start";
            public const string HomeFooter = "home.footer";
            public const string HomeLoginLoading = "home.login.loading";
            public const string HomeLoginFailed = "home.login.failed";
            public const string HomeLoginRetry = "home.login.retry";

            public const string LevelSelectTitle = "level_select.title";
            public const string LevelSelectHintUnlocked = "level_select.hint_unlocked";
            public const string LevelSelectHintLocked = "level_select.hint_locked";

            public const string LevelTutorialTitle = "level.tutorial.title";
            public const string LevelTutorialSubtitle = "level.tutorial.subtitle";
            public const string LevelDailyTitle = "level.daily.title";
            public const string LevelDailySubtitleDefault = "level.daily.subtitle_default";
            public const string LevelDailySubtitleLocked = "level.daily.subtitle_locked";
            public const string LevelDailySubtitleOpen = "level.daily.subtitle_open";
            public const string LevelDailySubtitleDone = "level.daily.subtitle_done";
            public const string LevelDailyLoading = "level.daily.loading";
            public const string LevelDailyLoadError = "level.daily.load_error";
            public const string Level1Title = "level.level1.title";
            public const string Level1Subtitle = "level.level1.subtitle";

            public const string CampaignTitle = "campaign.title";
            public const string CampaignHint = "campaign.hint";
            public const string CampaignEmpty = "campaign.empty";
            public const string CampaignHubTitle = "campaign.hub.title";
            public const string CampaignHubSubtitle = "campaign.hub.subtitle";
            public const string CampaignLevelTitleFmt = "campaign.level.title_fmt";
            public const string CampaignLevelOpen = "campaign.level.open";
            public const string CampaignLevelLocked = "campaign.level.locked";
            public const string CampaignLevelDone = "campaign.level.done";

            public const string GameplayReset = "gameplay.reset";
            public const string GameplayUndo = "gameplay.undo";
            public const string GameplayHint = "gameplay.hint";
            public const string GameplayRulesHeading = "gameplay.rules.heading";
            public const string GameplayRulesBody = "gameplay.rules.body";
            public const string GameplayRulesTutorialHeading = "gameplay.rules.tutorial.heading";
            public const string GameplayRulesTutorialBody = "gameplay.rules.tutorial.body";

            public const string StatusTap = "gameplay.status.tap";
            public const string StatusNoHint = "gameplay.status.no_hint";
            public const string StatusHintLoadingAd = "gameplay.status.hint_loading_ad";
            public const string StatusHintAdFailed = "gameplay.status.hint_ad_failed";
            public const string StatusHintEditorBlocked = "gameplay.status.hint_editor_blocked";
            public const string StatusHintSdkNotReady = "gameplay.status.hint_sdk_not_ready";
            public const string StatusHintAdNotConfigured = "gameplay.status.hint_ad_not_configured";
            public const string StatusHintFreeLeft = "gameplay.status.hint_free_left";
            public const string StatusHintWatchAd = "gameplay.status.hint_watch_ad";
            public const string StatusErrors = "gameplay.status.errors";
            public const string StatusWin = "gameplay.status.win";
            public const string StatusDailyDone = "gameplay.status.daily_done";
            public const string StatusDailyWin = "gameplay.status.daily_win";

            public const string TutorialWelcome = "tutorial.welcome";
            public const string TutorialStart = "tutorial.start";
            public const string TutorialGuideSun = "tutorial.guide_sun";
            public const string TutorialGuideMoon = "tutorial.guide_moon";
            public const string TutorialSymbols = "tutorial.symbols";
            public const string TutorialContinue = "tutorial.continue";
            public const string TutorialEquals = "tutorial.equals";
            public const string TutorialCross = "tutorial.cross";
            public const string TutorialFinishRest = "tutorial.finish_rest";
            public const string TutorialComplete = "tutorial.complete";
            public const string TutorialToLevelSelect = "tutorial.to_level_select";
        }

        private static readonly Dictionary<string, string> Zh = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "设置" },
            { Keys.SettingsFabLabel, "设置" },
            { Keys.SettingsLanguage, "语言" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "关闭" },
            { Keys.NavBack, "返回" },

            { Keys.HomeSubtitle, "逻辑谜题" },
            { Keys.HomeStart, "开始游戏" },
            { Keys.HomeFooter, "轻触按钮开始游戏" },
            { Keys.HomeLoginLoading, "正在登录…" },
            { Keys.HomeLoginFailed, "登录失败，请检查网络后重试" },
            { Keys.HomeLoginRetry, "重试" },

            { Keys.LevelSelectTitle, "选择关卡" },
            { Keys.LevelSelectHintUnlocked, "选择一个关卡开始挑战" },
            { Keys.LevelSelectHintLocked, "请先完成新手引导，解锁正式关卡" },

            { Keys.LevelTutorialTitle, "新手引导" },
            { Keys.LevelTutorialSubtitle, "4×4 教学关卡" },
            { Keys.LevelDailyTitle, "每日挑战" },
            { Keys.LevelDailySubtitleDefault, "每日一题 · 全球同题" },
            { Keys.LevelDailySubtitleLocked, "完成新手引导后开放" },
            { Keys.LevelDailySubtitleOpen, "每日一题 · 全球同题 · 今日可挑战" },
            { Keys.LevelDailySubtitleDone, "今日已完成 · 用时 {0} · 点击查看" },
            { Keys.LevelDailyLoading, "正在加载今日题目…" },
            { Keys.LevelDailyLoadError, "无法加载今日题目，点击重试" },
            { Keys.Level1Title, "第一关" },
            { Keys.Level1Subtitle, "6×6 正式挑战" },

            { Keys.CampaignTitle, "闯关模式" },
            { Keys.CampaignHint, "按顺序通关解锁下一关" },
            { Keys.CampaignEmpty, "暂无关卡，请在 Unity 运行 Bordy → Generate Campaign Levels" },
            { Keys.CampaignHubTitle, "闯关模式" },
            { Keys.CampaignHubSubtitle, "主线关卡 · 难度递增" },
            { Keys.CampaignLevelTitleFmt, "第 {0} 关" },
            { Keys.CampaignLevelOpen, "{0}×{1} · 点击开始" },
            { Keys.CampaignLevelLocked, "{0}×{1} · 未解锁" },
            { Keys.CampaignLevelDone, "{0}×{1} · 已完成" },

            { Keys.GameplayReset, "重置" },
            { Keys.GameplayUndo, "撤销" },
            { Keys.GameplayHint, "提示" },
            { Keys.GameplayRulesHeading, "游戏玩法" },
            { Keys.GameplayRulesBody, "•  填充网格，使每个格子都有一个太阳或一个月亮。\n•  每行（和每列）最多 2 个相同图案相邻，且太阳与月亮数量相等。\n•  由 = 分隔的格子必须相同；由 × 分隔的格子必须相反。" },
            { Keys.GameplayRulesTutorialHeading, "引导提示" },
            { Keys.GameplayRulesTutorialBody, "•  跟随底部卡片完成教学步骤。\n•  4×4 棋盘每行/列各 2 个太阳、2 个月亮。\n•  × 要相反，= 要相同，不能连出 3 个一样。" },

            { Keys.StatusTap, "点击空格填入太阳或月亮" },
            { Keys.StatusNoHint, "没有可提示的格子了" },
            { Keys.StatusHintLoadingAd, "正在加载广告…" },
            { Keys.StatusHintAdFailed, "广告暂时不可用，请稍后再试" },
            { Keys.StatusHintEditorBlocked, "需观看激励视频获得提示（Editor 未开启广告模拟）" },
            { Keys.StatusHintSdkNotReady, "广告加载中，请稍后再试" },
            { Keys.StatusHintAdNotConfigured, "广告位未配置，请在后台创建激励视频并填入 Ad Unit ID" },
            { Keys.StatusHintFreeLeft, "剩余免费提示 {0} 次" },
            { Keys.StatusHintWatchAd, "免费提示已用完，观看广告获得提示" },
            { Keys.StatusErrors, "还有规则未满足，请检查标红的格子" },
            { Keys.StatusWin, "恭喜通关！" },
            { Keys.StatusDailyDone, "今日已完成 · 用时 {0}（只能查看，明天再来）" },
            { Keys.StatusDailyWin, "恭喜完成每日挑战！用时 {0}（只能查看）" },

            { Keys.TutorialWelcome, "欢迎来到 Bordy！\n\n这是一个太阳 / 月亮逻辑谜题。点击空格可以在「空 → 太阳 → 月亮」之间切换。" },
            { Keys.TutorialStart, "开始" },
            { Keys.TutorialGuideSun, "引导：点击高亮格，直到变成太阳" },
            { Keys.TutorialGuideMoon, "引导：再点击下一格，直到变成月亮" },
            { Keys.TutorialSymbols, "格子之间会出现 = 和 × 两种符号：\n\n= 表示相邻两格必须相同；× 表示相邻两格必须相反。\n\n下面来分别试一下。" },
            { Keys.TutorialContinue, "继续" },
            { Keys.TutorialEquals, "= 号：两侧必须相同，把这两格都点成月亮" },
            { Keys.TutorialCross, "× 号：两侧必须不同，让这两格一个太阳、一个月亮" },
            { Keys.TutorialFinishRest, "完成剩余格子，通关后即可解锁闯关模式" },
            { Keys.TutorialComplete, "恭喜完成新手引导！\n\n闯关模式和每日挑战已解锁。" },
            { Keys.TutorialToLevelSelect, "关卡选择" },
        };

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "Settings" },
            { Keys.SettingsFabLabel, "Settings" },
            { Keys.SettingsLanguage, "Language" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "Close" },
            { Keys.NavBack, "Back" },

            { Keys.HomeSubtitle, "Logic Puzzle" },
            { Keys.HomeStart, "Play" },
            { Keys.HomeFooter, "Tap the button to play" },
            { Keys.HomeLoginLoading, "Signing in…" },
            { Keys.HomeLoginFailed, "Sign-in failed. Check your connection and retry." },
            { Keys.HomeLoginRetry, "Retry" },

            { Keys.LevelSelectTitle, "Select Level" },
            { Keys.LevelSelectHintUnlocked, "Pick a level to start" },
            { Keys.LevelSelectHintLocked, "Finish the tutorial to unlock the main levels" },

            { Keys.LevelTutorialTitle, "Tutorial" },
            { Keys.LevelTutorialSubtitle, "4×4 lesson" },
            { Keys.LevelDailyTitle, "Daily Challenge" },
            { Keys.LevelDailySubtitleDefault, "One puzzle a day · Same for all" },
            { Keys.LevelDailySubtitleLocked, "Unlocks after the tutorial" },
            { Keys.LevelDailySubtitleOpen, "One puzzle a day · Same for everyone · Play today" },
            { Keys.LevelDailySubtitleDone, "Done today · Time {0} · Tap to view" },
            { Keys.LevelDailyLoading, "Loading today's puzzle…" },
            { Keys.LevelDailyLoadError, "Couldn't load today's puzzle — tap to retry" },
            { Keys.Level1Title, "Level 1" },
            { Keys.Level1Subtitle, "6×6 challenge" },

            { Keys.CampaignTitle, "Campaign" },
            { Keys.CampaignHint, "Clear levels in order to unlock the next" },
            { Keys.CampaignEmpty, "No levels loaded — run Bordy → Generate Campaign Levels in Unity" },
            { Keys.CampaignHubTitle, "Campaign" },
            { Keys.CampaignHubSubtitle, "Story levels · easy → hard" },
            { Keys.CampaignLevelTitleFmt, "Level {0}" },
            { Keys.CampaignLevelOpen, "{0}×{1} · tap to play" },
            { Keys.CampaignLevelLocked, "{0}×{1} · locked" },
            { Keys.CampaignLevelDone, "{0}×{1} · completed" },

            { Keys.GameplayReset, "Reset" },
            { Keys.GameplayUndo, "Undo" },
            { Keys.GameplayHint, "Hint" },
            { Keys.GameplayRulesHeading, "How to Play" },
            { Keys.GameplayRulesBody, "•  Fill the grid so every cell holds a sun or a moon.\n•  Each row (and column) has equal suns and moons, with at most 2 identical symbols adjacent.\n•  Cells split by = must match; cells split by × must differ." },
            { Keys.GameplayRulesTutorialHeading, "Guide" },
            { Keys.GameplayRulesTutorialBody, "•  Follow the cards at the bottom to complete the lesson.\n•  Each row / column on the 4×4 board has 2 suns and 2 moons.\n•  × means opposite, = means same; never 3 identical in a row." },

            { Keys.StatusTap, "Tap an empty cell to place a sun or moon" },
            { Keys.StatusNoHint, "No cells left to hint" },
            { Keys.StatusHintLoadingAd, "Loading ad…" },
            { Keys.StatusHintAdFailed, "Ad unavailable — try again later" },
            { Keys.StatusHintEditorBlocked, "Watch a rewarded ad for a hint (Editor ad sim is off)" },
            { Keys.StatusHintSdkNotReady, "Ads are still loading — try again in a moment" },
            { Keys.StatusHintAdNotConfigured, "Ad unit not configured — create a rewarded placement in the developer portal" },
            { Keys.StatusHintFreeLeft, "{0} free hint(s) left" },
            { Keys.StatusHintWatchAd, "No free hints left — watch an ad for a hint" },
            { Keys.StatusErrors, "Some rules aren't satisfied — check the cells in red" },
            { Keys.StatusWin, "Puzzle solved!" },
            { Keys.StatusDailyDone, "Done today · Time {0} (view only — come back tomorrow)" },
            { Keys.StatusDailyWin, "Daily Challenge complete! Time {0} (view only)" },

            { Keys.TutorialWelcome, "Welcome to Bordy!\n\nThis is a sun / moon logic puzzle. Tap an empty cell to cycle Empty → Sun → Moon." },
            { Keys.TutorialStart, "Start" },
            { Keys.TutorialGuideSun, "Guide: tap the highlighted cell until it becomes a Sun" },
            { Keys.TutorialGuideMoon, "Guide: tap the next cell until it becomes a Moon" },
            { Keys.TutorialSymbols, "Two symbols appear between cells:\n\n= means the two cells must match;  × means they must differ.\n\nLet's try each one below." },
            { Keys.TutorialContinue, "Continue" },
            { Keys.TutorialEquals, "= : both sides must match — make both cells Moons" },
            { Keys.TutorialCross, "× : both sides must differ — make one Sun and one Moon" },
            { Keys.TutorialFinishRest, "Fill the remaining cells — solve it to unlock Campaign mode" },
            { Keys.TutorialComplete, "Tutorial complete!\n\nCampaign and Daily Challenge are unlocked." },
            { Keys.TutorialToLevelSelect, "Level Select" },
        };

        public static string Get(string key)
        {
            var table = BordyLocale.Current == BordyLanguage.En ? En : Zh;
            return table.TryGetValue(key, out var value) ? value : key;
        }

        /// <summary>Language row label — ASCII fallback when CJK font not bundled.</summary>
        public static string SettingsLangZhLabel()
            => BordyFonts.HasCjk ? Get(Keys.SettingsLangZh) : "Chinese (Simplified)";

        public static string Format(string key, params object[] args)
            => args == null || args.Length == 0 ? Get(key) : string.Format(Get(key), args);

        public static string LevelTitle(string levelId)
        {
            if (levelId == BordyLevelCatalog.TutorialId) return Get(Keys.LevelTutorialTitle);
            if (levelId == BordyLevelCatalog.DailyId) return Get(Keys.LevelDailyTitle);
            if (levelId == BordyLevelCatalog.Level1Id) return Get(Keys.Level1Title);
            if (BordyCampaignCatalog.IsCampaignId(levelId) && BordyCampaignCatalog.TryGetEntry(levelId, out var entry))
                return CampaignLevelTitle(entry.Index);
            return levelId;
        }

        public static string CampaignLevelTitle(int index) => Format(Keys.CampaignLevelTitleFmt, index);

        public static string CampaignTierLabel(string tier)
        {
            if (string.IsNullOrEmpty(tier))
                return "";

            if (BordyLocale.Current == BordyLanguage.En)
            {
                switch (tier)
                {
                    case "easy":
                    case "hook": return "Easy";
                    case "medium": return "Medium";
                    case "hard": return "Hard";
                    case "brutal": return "Extreme";
                    default: return tier;
                }
            }

            switch (tier)
            {
                case "easy":
                case "hook": return "简单";
                case "medium": return "中等";
                case "hard": return "偏难";
                case "brutal": return "极难";
                default: return tier;
            }
        }
    }
}
