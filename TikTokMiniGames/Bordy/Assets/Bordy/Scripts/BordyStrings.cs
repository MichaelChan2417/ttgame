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
            public const string SettingsLanguageButton = "settings.language.button";
            public const string SettingsLangZh = "settings.lang_zh";
            public const string SettingsLangEn = "settings.lang_en";
            public const string SettingsClose = "settings.close";
            public const string SettingsPlayerGuest = "settings.player.guest";
            public const string SettingsPlayerMetaFmt = "settings.player.meta_fmt";
            public const string SettingsPrivacy = "settings.privacy";
            public const string SettingsSidebar = "settings.sidebar";
            public const string SettingsShortcut = "settings.shortcut";
            public const string HomeChipSidebar = "home.chip.sidebar";
            public const string HomeChipShortcut = "home.chip.shortcut";
            public const string SettingsInvite = "settings.invite";
            public const string SettingsInviteShare = "settings.invite.share";
            public const string ShareDailyTitle = "share.daily.title";
            public const string ShareDailySubtitle = "share.daily.subtitle";
            public const string SettingsStatusPrivacy = "settings.status.privacy";
            public const string SettingsStatusPrivacyOk = "settings.status.privacy_ok";
            public const string SettingsStatusPrivacyFail = "settings.status.privacy_fail";
            public const string SettingsStatusSidebar = "settings.status.sidebar";
            public const string SettingsStatusSidebarOk = "settings.status.sidebar_ok";
            public const string SettingsStatusSidebarFail = "settings.status.sidebar_fail";
            public const string SettingsStatusShortcut = "settings.status.shortcut";
            public const string SettingsStatusShortcutOk = "settings.status.shortcut_ok";
            public const string SettingsStatusInvite = "settings.status.invite";
            public const string SettingsStatusLanguage = "settings.status.language";
            public const string SettingsLegalTitle = "settings.legal.title";
            public const string SettingsLegalBody = "settings.legal.body";

            public const string NavBack = "nav.back";

            public const string ShopFabLabel = "shop.fab";
            public const string ShopTitle = "shop.title";
            public const string ShopUse = "shop.use";
            public const string ShopSelected = "shop.selected";
            public const string ShopWatchAd = "shop.watch_ad";
            public const string ShopLoadingAd = "shop.loading_ad";
            public const string ShopUnlocked = "shop.unlocked";
            public const string ShopAdFailed = "shop.ad_failed";
            public const string ShopAdEditorBlocked = "shop.ad_editor_blocked";
            public const string ShopAdSdkNotReady = "shop.ad_sdk_not_ready";
            public const string ShopAdNotConfigured = "shop.ad_not_configured";

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
            public const string GameplayCheck = "gameplay.check";
            public const string GameplayHint = "gameplay.hint";
            public const string GameplayRulesHeading = "gameplay.rules.heading";
            public const string GameplayRulesBody = "gameplay.rules.body";
            public const string GameplayRulesTutorialHeading = "gameplay.rules.tutorial.heading";
            public const string GameplayRulesTutorialBody = "gameplay.rules.tutorial.body";
            public const string RulesIconsFill = "gameplay.rules.icons_fill";
            public const string RulesIconsOr = "gameplay.rules.icons_or";

            public const string StatusTap = "gameplay.status.tap";
            public const string StatusNoHint = "gameplay.status.no_hint";
            public const string StatusHintLoadingAd = "gameplay.status.hint_loading_ad";
            public const string StatusHintAdFailed = "gameplay.status.hint_ad_failed";
            public const string StatusHintEditorBlocked = "gameplay.status.hint_editor_blocked";
            public const string StatusHintSdkNotReady = "gameplay.status.hint_sdk_not_ready";
            public const string StatusHintAdNotConfigured = "gameplay.status.hint_ad_not_configured";
            public const string StatusHintFreeLeft = "gameplay.status.hint_free_left";
            public const string StatusHintWatchAd = "gameplay.status.hint_watch_ad";
            public const string StatusHintCap = "gameplay.status.hint_cap";
            public const string StatusErrors = "gameplay.status.errors";
            public const string StatusWin = "gameplay.status.win";
            public const string StatusDailyDone = "gameplay.status.daily_done";
            public const string StatusDailyWin = "gameplay.status.daily_win";

            public const string TutorialWelcome = "tutorial.welcome";
            public const string TutorialStart = "tutorial.start";
            public const string TutorialGuideSun = "tutorial.guide_sun";
            public const string TutorialGuideMoon = "tutorial.guide_moon";
            public const string TutorialCoachTap = "tutorial.coach_tap";
            public const string TutorialSymbols = "tutorial.symbols";
            public const string TutorialContinue = "tutorial.continue";
            public const string TutorialEquals = "tutorial.equals";
            public const string TutorialCross = "tutorial.cross";
            public const string TutorialRowCount = "tutorial.row_count";
            public const string TutorialColCount = "tutorial.col_count";
            public const string TutorialAvoidThree = "tutorial.avoid_three";
            public const string TutorialRowNeedSun = "tutorial.row_need_sun";
            public const string TutorialColNeedSun = "tutorial.col_need_sun";
            public const string TutorialCheckPlant = "tutorial.check_plant";
            public const string TutorialCheckUse = "tutorial.check_use";
            public const string TutorialCheckFix = "tutorial.check_fix";
            public const string TutorialHintUse = "tutorial.hint_use";
            public const string TutorialLastMoon = "tutorial.last_moon";
            public const string TutorialComplete = "tutorial.complete";
            public const string TutorialToLevelSelect = "tutorial.to_level_select";
            public const string TutorialNudgeCell = "tutorial.nudge.cell";
            public const string TutorialNudgeAgain = "tutorial.nudge.again";
            public const string TutorialNudgeCheck = "tutorial.nudge.check";
            public const string TutorialNudgeHint = "tutorial.nudge.hint";
            public const string TutorialNudgeIdle = "tutorial.nudge.idle";
        }

        private static readonly Dictionary<string, string> Zh = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "设置" },
            { Keys.SettingsFabLabel, "设置" },
            { Keys.SettingsLanguage, "Language" },
            { Keys.SettingsLanguageButton, "Language" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "关闭" },
            { Keys.SettingsPlayerGuest, "玩家" },
            { Keys.SettingsPlayerMetaFmt, "角色：{0}  ·  闯关第 {1} 关" },
            { Keys.SettingsPrivacy, "隐私与服务条款" },
            { Keys.SettingsSidebar, "添加侧边栏" },
            { Keys.SettingsShortcut, "添加桌面快捷方式" },
            { Keys.HomeChipSidebar, "侧边栏" },
            { Keys.HomeChipShortcut, "桌面" },
            { Keys.SettingsInvite, "邀请好友同玩" },
            { Keys.SettingsInviteShare, "一起来玩 Bordy！" },
            { Keys.ShareDailyTitle, "今日挑战我 {0} 就过了，你能 beat 我吗？" },
            { Keys.ShareDailySubtitle, "同一道题，敢不敢来比？" },
            { Keys.SettingsStatusPrivacy, "正在打开隐私与条款…" },
            { Keys.SettingsStatusPrivacyOk, "已打开隐私与条款" },
            { Keys.SettingsStatusPrivacyFail, "无法打开网页，已显示政策全文" },
            { Keys.SettingsStatusSidebar, "正在打开侧边栏…" },
            { Keys.SettingsStatusSidebarOk, "已打开侧边栏" },
            { Keys.SettingsStatusSidebarFail, "侧边栏暂不可用，请稍后重试" },
            { Keys.SettingsStatusShortcut, "请在弹窗中确认添加到桌面" },
            { Keys.SettingsStatusShortcutOk, "已添加到桌面" },
            { Keys.SettingsStatusInvite, "正在打开邀请…" },
            { Keys.SettingsStatusLanguage, "Language: English" },
            { Keys.SettingsLegalTitle, "Privacy & Terms" },
            { Keys.SettingsLegalBody,
                "Operator: Shanghai Quanjie Technology Co., Ltd.\n\n" +
                "Privacy: https://bordy-api.brainless.workers.dev/privacy.html\n\n" +
                "Terms: https://bordy-api.brainless.workers.dev/terms.html\n\n" +
                "We collect only what the TikTok Mini Game platform provides (OpenID, cloud save, ads). Data is stored on Cloudflare. Youth / general audience. Contact: 1531362757@qq.com" },
            { Keys.NavBack, "返回" },

            { Keys.ShopFabLabel, "商店" },
            { Keys.ShopTitle, "图标商店" },
            { Keys.ShopUse, "使用" },
            { Keys.ShopSelected, "使用中" },
            { Keys.ShopWatchAd, "看广告解锁" },
            { Keys.ShopLoadingAd, "正在加载广告…" },
            { Keys.ShopUnlocked, "解锁成功，已为你装备！" },
            { Keys.ShopAdFailed, "广告暂时不可用，请稍后再试" },
            { Keys.ShopAdEditorBlocked, "需观看激励视频解锁（Editor 未开启广告模拟）" },
            { Keys.ShopAdSdkNotReady, "广告加载中，请稍后再试" },
            { Keys.ShopAdNotConfigured, "广告位未配置，请在后台创建激励视频并填入 Ad Unit ID" },

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
            { Keys.LevelTutorialSubtitle, "新手推荐 · 一步步学会玩法" },
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
            { Keys.GameplayUndo, "检查" },
            { Keys.GameplayCheck, "检查" },
            { Keys.GameplayHint, "提示" },
            { Keys.GameplayRulesHeading, "游戏玩法" },
            { Keys.GameplayRulesBody, "•  每行、每列两种图案数量相等，且相邻相同图案不超过 2 个。\n•  由 = 分隔的格子必须相同；由 × 分隔的格子必须相反。" },
            { Keys.GameplayRulesTutorialHeading, "引导提示" },
            { Keys.GameplayRulesTutorialBody, "•  跟随提示，一步步填满 6×6。\n•  每行、每列两种图案各 3 个。\n•  = 两侧相同，× 两侧相反；不能连续 3 个一样。" },
            { Keys.RulesIconsFill, "•  每格填入" },
            { Keys.RulesIconsOr, "或" },

            { Keys.StatusTap, "" },
            { Keys.StatusNoHint, "没有可提示的格子了" },
            { Keys.StatusHintLoadingAd, "正在加载广告…" },
            { Keys.StatusHintAdFailed, "广告暂时不可用，请稍后再试" },
            { Keys.StatusHintEditorBlocked, "需观看激励视频获得提示（Editor 未开启广告模拟）" },
            { Keys.StatusHintSdkNotReady, "广告加载中，请稍后再试" },
            { Keys.StatusHintAdNotConfigured, "广告位未配置，请在后台创建激励视频并填入 Ad Unit ID" },
            { Keys.StatusHintFreeLeft, "剩余免费提示 {0} 次" },
            { Keys.StatusHintWatchAd, "免费提示已用完，观看广告获得提示" },
            { Keys.StatusHintCap, "本关提示已用完（最多 {0} 次）" },
            { Keys.StatusErrors, "还有规则未满足，请检查标红的格子" },
            { Keys.StatusWin, "恭喜通关！" },
            { Keys.StatusDailyDone, "今日已完成 · 用时 {0}（只能查看，明天再来）" },
            { Keys.StatusDailyWin, "恭喜完成每日挑战！用时 {0}（只能查看）" },

            { Keys.TutorialWelcome, "欢迎来到 Bordy！\n\n把每个格子填成 {sun} 或 {moon}。" },
            { Keys.TutorialStart, "开始" },
            { Keys.TutorialCoachTap, "点击任意处继续" },
            { Keys.TutorialGuideSun, "点一下高亮的格子\n放上一个 {sun}" },
            { Keys.TutorialGuideMoon, "在旁边的格子点两下\n放上一个 {moon}\n\n目标：填满棋盘，让每行每列\n都有三个 {sun}、三个 {moon}" },
            { Keys.TutorialSymbols, "格子之间会出现 = 和 ×\n\n=  两侧必须相同\n×  两侧必须相反\n\n下面来分别试一下。" },
            { Keys.TutorialContinue, "继续" },
            { Keys.TutorialEquals, "= 号：两侧必须相同。\n把这两格都点成 {sun}。" },
            { Keys.TutorialCross, "× 号：两侧必须相反。\n上面点成 {moon}，下面点成 {sun}。" },
            { Keys.TutorialRowCount, "每行每列各有三个 {sun}、三个 {moon}。\n这一行已经有三个 {moon}，\n所以这格必须是 {sun}。" },
            { Keys.TutorialColCount, "列也一样。\n这列已经有 2 个 {moon}、1 个 {sun}，\n所以这格必须是 {sun}。" },
            { Keys.TutorialAvoidThree, "同一行或列里，不能连续 3 个相同。\n这两格已经是 {moon} {moon}，\n所以这格必须是 {sun}。" },
            { Keys.TutorialRowNeedSun, "这一行还差一个太阳。点成太阳。" },
            { Keys.TutorialColNeedSun, "这一列已经有 2 个月亮，所以这格必须是太阳。" },
            { Keys.TutorialCheckPlant, "填错了也没关系。\n先在这一格点出一个 {moon}。" },
            { Keys.TutorialCheckUse, "想清空？再点一下，\n它就会变回空白。" },
            { Keys.TutorialCheckFix, "再点一下，放上正确的 {sun}。" },
            { Keys.TutorialHintUse, "卡住时可以用「提示」。点底部发黄的「提示」，游戏会帮你填对一格。" },
            { Keys.TutorialLastMoon, "最后一格：这行已经有 2 个 {sun}，\n点成 {moon}。" },
            { Keys.TutorialComplete, "恭喜完成新手引导！\n\n闯关模式和每日挑战已解锁。之后可以用「检查」查错、用「提示」求助。" },
            { Keys.TutorialToLevelSelect, "关卡选择" },
            { Keys.TutorialNudgeCell, "请点黄色高亮的格子。" },
            { Keys.TutorialNudgeAgain, "还没完成，再点一次。" },
            { Keys.TutorialNudgeCheck, "请先点发黄的「检查」按钮。" },
            { Keys.TutorialNudgeHint, "请点发黄的「提示」按钮。" },
            { Keys.TutorialNudgeIdle, "这一步还没完成，点高亮的格子或按钮。" },
        };

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "Settings" },
            { Keys.SettingsFabLabel, "Settings" },
            { Keys.SettingsLanguage, "Language" },
            { Keys.SettingsLanguageButton, "Language" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "Close" },
            { Keys.SettingsPlayerGuest, "Player" },
            { Keys.SettingsPlayerMetaFmt, "Character: {0}  ·  Campaign {1}" },
            { Keys.SettingsPrivacy, "Privacy & Terms" },
            { Keys.SettingsSidebar, "Add to sidebar" },
            { Keys.SettingsShortcut, "Add desktop shortcut" },
            { Keys.HomeChipSidebar, "Sidebar" },
            { Keys.HomeChipShortcut, "Desktop" },
            { Keys.SettingsInvite, "Invite friends" },
            { Keys.SettingsInviteShare, "Come play Bordy with me!" },
            { Keys.ShareDailyTitle, "I finished today's Bordy in {0} — can you beat me?" },
            { Keys.ShareDailySubtitle, "Same puzzle. Think you're faster?" },
            { Keys.SettingsStatusPrivacy, "Opening Privacy & Terms…" },
            { Keys.SettingsStatusPrivacyOk, "Opened Privacy & Terms" },
            { Keys.SettingsStatusPrivacyFail, "Couldn't open the page. Policy is shown below." },
            { Keys.SettingsStatusSidebar, "Opening sidebar…" },
            { Keys.SettingsStatusSidebarOk, "Opened sidebar" },
            { Keys.SettingsStatusSidebarFail, "Sidebar isn't available right now. Try again." },
            { Keys.SettingsStatusShortcut, "Confirm add-to-desktop in the prompt" },
            { Keys.SettingsStatusShortcutOk, "Added to desktop" },
            { Keys.SettingsStatusInvite, "Opening invite…" },
            { Keys.SettingsStatusLanguage, "Language: English" },
            { Keys.SettingsLegalTitle, "Privacy & Terms" },
            { Keys.SettingsLegalBody,
                "Operator: Shanghai Quanjie Technology Co., Ltd.\n\n" +
                "Privacy: https://bordy-api.brainless.workers.dev/privacy.html\n\n" +
                "Terms: https://bordy-api.brainless.workers.dev/terms.html\n\n" +
                "We collect only what the TikTok Mini Game platform provides (OpenID, cloud save, ads). Data is stored on Cloudflare. Youth / general audience. Contact: 1531362757@qq.com" },
            { Keys.NavBack, "Back" },

            { Keys.ShopFabLabel, "Shop" },
            { Keys.ShopTitle, "Icon Shop" },
            { Keys.ShopUse, "Use" },
            { Keys.ShopSelected, "In use" },
            { Keys.ShopWatchAd, "Watch ad" },
            { Keys.ShopLoadingAd, "Loading ad…" },
            { Keys.ShopUnlocked, "Unlocked — equipped for you!" },
            { Keys.ShopAdFailed, "Ad unavailable — try again later" },
            { Keys.ShopAdEditorBlocked, "Watch a rewarded ad to unlock (Editor ad sim is off)" },
            { Keys.ShopAdSdkNotReady, "Ads are still loading — try again in a moment" },
            { Keys.ShopAdNotConfigured, "Ad unit not configured — create a rewarded placement in the developer portal" },

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
            { Keys.LevelTutorialSubtitle, "Learn to play, step by step" },
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
            { Keys.GameplayUndo, "Check" },
            { Keys.GameplayCheck, "Check" },
            { Keys.GameplayHint, "Hint" },
            { Keys.GameplayRulesHeading, "How to Play" },
            { Keys.GameplayRulesBody, "•  Each row and column holds an equal number of each icon, with at most 2 identical icons adjacent.\n•  Cells split by = must match; cells split by × must differ." },
            { Keys.GameplayRulesTutorialHeading, "Guide" },
            { Keys.GameplayRulesTutorialBody, "•  Follow the tips and fill the 6×6, one step at a time.\n•  Each row and column has three of each icon.\n•  = means same, × means opposite; never 3 in a row." },
            { Keys.RulesIconsFill, "•  Fill each cell with" },
            { Keys.RulesIconsOr, "or" },

            { Keys.StatusTap, "" },
            { Keys.StatusNoHint, "No cells left to hint" },
            { Keys.StatusHintLoadingAd, "Loading ad…" },
            { Keys.StatusHintAdFailed, "Ad unavailable — try again later" },
            { Keys.StatusHintEditorBlocked, "Watch a rewarded ad for a hint (Editor ad sim is off)" },
            { Keys.StatusHintSdkNotReady, "Ads are still loading — try again in a moment" },
            { Keys.StatusHintAdNotConfigured, "Ad unit not configured — create a rewarded placement in the developer portal" },
            { Keys.StatusHintFreeLeft, "{0} free hint(s) left" },
            { Keys.StatusHintWatchAd, "No free hints left — watch an ad for a hint" },
            { Keys.StatusHintCap, "No hints left this level (max {0})" },
            { Keys.StatusErrors, "Some rules aren't satisfied — check the cells in red" },
            { Keys.StatusWin, "Puzzle solved!" },
            { Keys.StatusDailyDone, "Done today · Time {0} (view only — come back tomorrow)" },
            { Keys.StatusDailyWin, "Daily Challenge complete! Time {0} (view only)" },

            { Keys.TutorialWelcome, "Welcome to Bordy!\n\nFill every square with a {sun} or a {moon}." },
            { Keys.TutorialStart, "Start" },
            { Keys.TutorialCoachTap, "Tap anywhere to continue" },
            { Keys.TutorialGuideSun, "Tap the highlighted square\nonce to place a {sun}" },
            { Keys.TutorialGuideMoon, "Tap a square twice to place a {moon}\n\nGoal: fill the grid so every row and\ncolumn has three {sun} and three {moon}" },
            { Keys.TutorialSymbols, "Cells can have = or × between them\n\n=  both sides must match\n×  both sides must differ\n\nLet's try each one." },
            { Keys.TutorialContinue, "Continue" },
            { Keys.TutorialEquals, "= means both sides must match.\nMake both of these cells {sun}." },
            { Keys.TutorialCross, "× means the two cells must differ.\nMake the top cell a {moon}\nand the bottom a {sun}." },
            { Keys.TutorialRowCount, "Each row and column has three {sun}\nand three {moon}. This row already has\nthree {moon}, so this must be a {sun}." },
            { Keys.TutorialColCount, "Columns work the same way.\nThis column has two {moon} and one {sun},\nso this cell must be a {sun}." },
            { Keys.TutorialAvoidThree, "Never 3 of the same in a row or column.\nThese two are already {moon} {moon},\nso this one must be a {sun}." },
            { Keys.TutorialRowNeedSun, "This row still needs one Sun. Tap until it becomes a Sun." },
            { Keys.TutorialColNeedSun, "This column already has 2 moons, so this cell must be a Sun." },
            { Keys.TutorialCheckPlant, "Made a mistake? No problem.\nTap this cell to a {moon} first." },
            { Keys.TutorialCheckUse, "Want to clear it? Tap again\nand it goes back to empty." },
            { Keys.TutorialCheckFix, "Tap once more to place the correct {sun}." },
            { Keys.TutorialHintUse, "If you get stuck, use Hint. Tap Hint and the game fills one cell correctly." },
            { Keys.TutorialLastMoon, "Last cell: this row already has two {sun},\nso make it a {moon}." },
            { Keys.TutorialComplete, "Tutorial complete!\n\nCampaign and Daily Challenge are unlocked. You can use Check to find mistakes and Hint when you get stuck." },
            { Keys.TutorialToLevelSelect, "Level Select" },
            { Keys.TutorialNudgeCell, "Tap the yellow highlighted cell." },
            { Keys.TutorialNudgeAgain, "Not done yet — tap again." },
            { Keys.TutorialNudgeCheck, "Tap the yellow Check button first." },
            { Keys.TutorialNudgeHint, "Tap the yellow Hint button." },
            { Keys.TutorialNudgeIdle, "This step isn't done yet. Tap the highlighted cell or button." },
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
