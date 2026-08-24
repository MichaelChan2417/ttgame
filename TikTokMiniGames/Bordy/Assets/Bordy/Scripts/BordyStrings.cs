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
            public const string SettingsLangJa = "settings.lang_ja";
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
            { Keys.SettingsLangJa, "日本語" },
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
            { Keys.SettingsLangJa, "日本語" },
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

        private static readonly Dictionary<string, string> Ja = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "設定" },
            { Keys.SettingsFabLabel, "設定" },
            { Keys.SettingsLanguage, "言語" },
            { Keys.SettingsLanguageButton, "言語" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangJa, "日本語" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "閉じる" },
            { Keys.SettingsPlayerGuest, "プレイヤー" },
            { Keys.SettingsPlayerMetaFmt, "キャラ：{0}  ·  キャンペーン {1}" },
            { Keys.SettingsPrivacy, "プライバシーと利用規約" },
            { Keys.SettingsSidebar, "サイドバーに追加" },
            { Keys.SettingsShortcut, "デスクトップに追加" },
            { Keys.HomeChipSidebar, "サイドバー" },
            { Keys.HomeChipShortcut, "デスクトップ" },
            { Keys.SettingsInvite, "友だちを招待" },
            { Keys.SettingsInviteShare, "Bordy で一緒に遊ぼう！" },
            { Keys.ShareDailyTitle, "今日のチャレンジは {0} でクリア。超えられる？" },
            { Keys.ShareDailySubtitle, "同じパズル。勝負しない？" },
            { Keys.SettingsStatusPrivacy, "プライバシーと規約を開いています…" },
            { Keys.SettingsStatusPrivacyOk, "プライバシーと規約を開きました" },
            { Keys.SettingsStatusPrivacyFail, "ページを開けませんでした。全文を下に表示します。" },
            { Keys.SettingsStatusSidebar, "サイドバーを開いています…" },
            { Keys.SettingsStatusSidebarOk, "サイドバーを開きました" },
            { Keys.SettingsStatusSidebarFail, "サイドバーは今使えません。後でもう一度。" },
            { Keys.SettingsStatusShortcut, "ポップアップでデスクトップ追加を確認してください" },
            { Keys.SettingsStatusShortcutOk, "デスクトップに追加しました" },
            { Keys.SettingsStatusInvite, "招待を開いています…" },
            { Keys.SettingsStatusLanguage, "言語：日本語" },
            { Keys.SettingsLegalTitle, "Privacy & Terms" },
            { Keys.SettingsLegalBody,
                "Operator: Shanghai Quanjie Technology Co., Ltd.\n\n" +
                "Privacy: https://bordy-api.brainless.workers.dev/privacy.html\n\n" +
                "Terms: https://bordy-api.brainless.workers.dev/terms.html\n\n" +
                "We collect only what the TikTok Mini Game platform provides (OpenID, cloud save, ads). Data is stored on Cloudflare. Youth / general audience. Contact: 1531362757@qq.com" },
            { Keys.NavBack, "戻る" },

            { Keys.ShopFabLabel, "ショップ" },
            { Keys.ShopTitle, "アイコンショップ" },
            { Keys.ShopUse, "使う" },
            { Keys.ShopSelected, "使用中" },
            { Keys.ShopWatchAd, "広告を見て解除" },
            { Keys.ShopLoadingAd, "広告を読み込み中…" },
            { Keys.ShopUnlocked, "解除しました。装備済み！" },
            { Keys.ShopAdFailed, "広告を再生できません。後でもう一度。" },
            { Keys.ShopAdEditorBlocked, "ヒント解除にはリワード広告が必要です（Editor では広告シミュなし）" },
            { Keys.ShopAdSdkNotReady, "広告の準備中です。少し待ってからもう一度。" },
            { Keys.ShopAdNotConfigured, "広告枠が未設定です。ポータルでリワード広告を作成してください" },

            { Keys.HomeSubtitle, "ロジックパズル" },
            { Keys.HomeStart, "プレイ" },
            { Keys.HomeFooter, "ボタンをタップして始める" },
            { Keys.HomeLoginLoading, "ログイン中…" },
            { Keys.HomeLoginFailed, "ログインに失敗しました。通信を確認して再試行してください。" },
            { Keys.HomeLoginRetry, "再試行" },

            { Keys.LevelSelectTitle, "レベル選択" },
            { Keys.LevelSelectHintUnlocked, "レベルを選んで挑戦" },
            { Keys.LevelSelectHintLocked, "チュートリアルをクリアすると本編が開放されます" },

            { Keys.LevelTutorialTitle, "チュートリアル" },
            { Keys.LevelTutorialSubtitle, "遊び方を順番に覚えよう" },
            { Keys.LevelDailyTitle, "デイリーチャレンジ" },
            { Keys.LevelDailySubtitleDefault, "毎日1問 · 全員同じ" },
            { Keys.LevelDailySubtitleLocked, "チュートリアル後に開放" },
            { Keys.LevelDailySubtitleOpen, "毎日1問 · 全員同じ · 今日挑戦できる" },
            { Keys.LevelDailySubtitleDone, "今日クリア済み · {0} · タップで見る" },
            { Keys.LevelDailyLoading, "今日のパズルを読み込み中…" },
            { Keys.LevelDailyLoadError, "今日のパズルを読み込めません。タップで再試行" },
            { Keys.Level1Title, "レベル 1" },
            { Keys.Level1Subtitle, "6×6 チャレンジ" },

            { Keys.CampaignTitle, "キャンペーン" },
            { Keys.CampaignHint, "順番にクリアして次のレベルを開放" },
            { Keys.CampaignEmpty, "レベルがありません。Unity で Bordy → Generate Campaign Levels を実行" },
            { Keys.CampaignHubTitle, "キャンペーン" },
            { Keys.CampaignHubSubtitle, "本編 · だんだん難しく" },
            { Keys.CampaignLevelTitleFmt, "レベル {0}" },
            { Keys.CampaignLevelOpen, "{0}×{1} · タップして開始" },
            { Keys.CampaignLevelLocked, "{0}×{1} · 未開放" },
            { Keys.CampaignLevelDone, "{0}×{1} · クリア済み" },

            { Keys.GameplayReset, "リセット" },
            { Keys.GameplayUndo, "チェック" },
            { Keys.GameplayCheck, "チェック" },
            { Keys.GameplayHint, "ヒント" },
            { Keys.GameplayRulesHeading, "遊び方" },
            { Keys.GameplayRulesBody, "•  各行・各列で2種類のアイコン数が同じ。同じアイコンは隣り合って2つまで。\n•  = で区切られたマスは同じ、× で区切られたマスは反対。" },
            { Keys.GameplayRulesTutorialHeading, "ガイド" },
            { Keys.GameplayRulesTutorialBody, "•  ヒントに従って 6×6 を埋めていこう。\n•  各行・各列に各アイコンが3つ。\n•  = は同じ、× は反対。3つ連続は禁止。" },
            { Keys.RulesIconsFill, "•  各マスに入れるのは" },
            { Keys.RulesIconsOr, "または" },

            { Keys.StatusTap, "" },
            { Keys.StatusNoHint, "ヒントできるマスがありません" },
            { Keys.StatusHintLoadingAd, "広告を読み込み中…" },
            { Keys.StatusHintAdFailed, "広告を再生できません。後でもう一度。" },
            { Keys.StatusHintEditorBlocked, "ヒントにはリワード広告が必要です（Editor では広告シミュなし）" },
            { Keys.StatusHintSdkNotReady, "広告の準備中です。少し待ってからもう一度。" },
            { Keys.StatusHintAdNotConfigured, "広告枠が未設定です。ポータルでリワード広告を作成してください" },
            { Keys.StatusHintFreeLeft, "無料ヒント残り {0} 回" },
            { Keys.StatusHintWatchAd, "無料ヒントは使い切りました。広告を見てヒントを獲得" },
            { Keys.StatusHintCap, "このレベルのヒントは上限です（最大 {0} 回）" },
            { Keys.StatusErrors, "まだルールを満たしていません。赤いマスを確認" },
            { Keys.StatusWin, "クリア！" },
            { Keys.StatusDailyDone, "今日クリア済み · {0}（閲覧のみ · 明日また来てね）" },
            { Keys.StatusDailyWin, "デイリーチャレンジ完了！ {0}（閲覧のみ）" },

            { Keys.TutorialWelcome, "Bordy へようこそ！\n\nすべてのマスを {sun} か {moon} で埋めてね。" },
            { Keys.TutorialStart, "スタート" },
            { Keys.TutorialCoachTap, "どこかをタップして続ける" },
            { Keys.TutorialGuideSun, "ハイライトのマスを1回タップして\n{sun} を置こう" },
            { Keys.TutorialGuideMoon, "マスを2回タップすると {moon} になる\n\n目標：すべての行と列に\n{sun} と {moon} を3つずつ" },
            { Keys.TutorialSymbols, "マスの間に = と × が出るよ\n\n=  両側は同じ\n×  両側は反対\n\n順番に試してみよう。" },
            { Keys.TutorialContinue, "続ける" },
            { Keys.TutorialEquals, "= は両側が同じという意味。\nこの2マスをどちらも {sun} にしよう。" },
            { Keys.TutorialCross, "× は両側が反対という意味。\n上を {moon}、下を {sun} にしよう。" },
            { Keys.TutorialRowCount, "各行・各列に {sun} と {moon} が3つずつ。\nこの行はすでに {moon} が3つあるから、\nここは {sun}。" },
            { Keys.TutorialColCount, "列も同じ。\nこの列は {moon} が2つ、{sun} が1つだから、\nここは {sun}。" },
            { Keys.TutorialAvoidThree, "同じ行・列で同じアイコンを3つ連続させない。\n隣がすでに {moon} {moon} だから、\nここは {sun}。" },
            { Keys.TutorialRowNeedSun, "この行は太陽がもう1つ必要。太陽になるまでタップ。" },
            { Keys.TutorialColNeedSun, "この列は月がすでに2つあるから、ここは太陽。" },
            { Keys.TutorialCheckPlant, "間違えても大丈夫。\nまずこのマスを {moon} にしてみよう。" },
            { Keys.TutorialCheckUse, "消したい？もう1回タップすると\n空に戻るよ。" },
            { Keys.TutorialCheckFix, "もう1回タップして、正しい {sun} を置こう。" },
            { Keys.TutorialHintUse, "詰まったら「ヒント」。黄色い「ヒント」をタップすると、1マス正しく埋めてくれる。" },
            { Keys.TutorialLastMoon, "最後のマス：この行はすでに {sun} が2つあるから、\n{moon} にしよう。" },
            { Keys.TutorialComplete, "チュートリアル完了！\n\nキャンペーンとデイリーが開放されました。間違いは「チェック」、行き詰まったら「ヒント」。" },
            { Keys.TutorialToLevelSelect, "レベル選択" },
            { Keys.TutorialNudgeCell, "黄色いハイライトのマスをタップしてね。" },
            { Keys.TutorialNudgeAgain, "まだ終わってないよ。もう一度タップ。" },
            { Keys.TutorialNudgeCheck, "先に黄色い「チェック」をタップしてね。" },
            { Keys.TutorialNudgeHint, "黄色い「ヒント」をタップしてね。" },
            { Keys.TutorialNudgeIdle, "このステップはまだ終わっていません。ハイライトのマスかボタンをタップ。" },
        };

        private static readonly Dictionary<string, string> Es = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "Ajustes" },
            { Keys.SettingsFabLabel, "Ajustes" },
            { Keys.SettingsLanguage, "Idioma" },
            { Keys.SettingsLanguageButton, "Idioma" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangJa, "日本語" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "Cerrar" },
            { Keys.SettingsPlayerGuest, "Jugador" },
            { Keys.SettingsPlayerMetaFmt, "Personaje: {0}  ·  Campaña {1}" },
            { Keys.SettingsPrivacy, "Privacidad y términos" },
            { Keys.SettingsSidebar, "Añadir a la barra lateral" },
            { Keys.SettingsShortcut, "Añadir acceso directo" },
            { Keys.HomeChipSidebar, "Barra lateral" },
            { Keys.HomeChipShortcut, "Escritorio" },
            { Keys.SettingsInvite, "Invitar amigos" },
            { Keys.SettingsInviteShare, "¡Ven a jugar Bordy conmigo!" },
            { Keys.ShareDailyTitle, "Terminé el Bordy de hoy en {0}, ¿puedes superarme?" },
            { Keys.ShareDailySubtitle, "Mismo puzle. ¿Crees que eres más rápido?" },
            { Keys.SettingsStatusPrivacy, "Abriendo privacidad y términos…" },
            { Keys.SettingsStatusPrivacyOk, "Privacidad y términos abiertos" },
            { Keys.SettingsStatusPrivacyFail, "No se pudo abrir la página. La política se muestra abajo." },
            { Keys.SettingsStatusSidebar, "Abriendo la barra lateral…" },
            { Keys.SettingsStatusSidebarOk, "Barra lateral abierta" },
            { Keys.SettingsStatusSidebarFail, "La barra lateral no está disponible ahora. Inténtalo de nuevo." },
            { Keys.SettingsStatusShortcut, "Confirma añadir al escritorio en el aviso" },
            { Keys.SettingsStatusShortcutOk, "Añadido al escritorio" },
            { Keys.SettingsStatusInvite, "Abriendo invitación…" },
            { Keys.SettingsStatusLanguage, "Idioma: Español" },
            { Keys.SettingsLegalTitle, "Privacidad y términos" },
            { Keys.SettingsLegalBody,
                "Operador: Shanghai Quanjie Technology Co., Ltd.\n\n" +
                "Privacidad: https://bordy-api.brainless.workers.dev/privacy.html\n\n" +
                "Términos: https://bordy-api.brainless.workers.dev/terms.html\n\n" +
                "Solo recopilamos lo que proporciona la plataforma de minijuegos de TikTok (OpenID, guardado en la nube, anuncios). Los datos se almacenan en Cloudflare. Público general. Contacto: 1531362757@qq.com" },
            { Keys.NavBack, "Atrás" },

            { Keys.ShopFabLabel, "Tienda" },
            { Keys.ShopTitle, "Tienda de iconos" },
            { Keys.ShopUse, "Usar" },
            { Keys.ShopSelected, "En uso" },
            { Keys.ShopWatchAd, "Ver anuncio" },
            { Keys.ShopLoadingAd, "Cargando anuncio…" },
            { Keys.ShopUnlocked, "¡Desbloqueado y equipado!" },
            { Keys.ShopAdFailed, "Anuncio no disponible, inténtalo más tarde" },
            { Keys.ShopAdEditorBlocked, "Mira un anuncio con recompensa para desbloquear (sim. de anuncios del Editor desactivada)" },
            { Keys.ShopAdSdkNotReady, "Los anuncios aún se están cargando, inténtalo en un momento" },
            { Keys.ShopAdNotConfigured, "Unidad de anuncios no configurada: crea un espacio con recompensa en el portal de desarrolladores" },

            { Keys.HomeSubtitle, "Puzle de lógica" },
            { Keys.HomeStart, "Jugar" },
            { Keys.HomeFooter, "Toca el botón para jugar" },
            { Keys.HomeLoginLoading, "Iniciando sesión…" },
            { Keys.HomeLoginFailed, "Error al iniciar sesión. Revisa tu conexión e inténtalo de nuevo." },
            { Keys.HomeLoginRetry, "Reintentar" },

            { Keys.LevelSelectTitle, "Elegir nivel" },
            { Keys.LevelSelectHintUnlocked, "Elige un nivel para empezar" },
            { Keys.LevelSelectHintLocked, "Termina el tutorial para desbloquear los niveles principales" },

            { Keys.LevelTutorialTitle, "Tutorial" },
            { Keys.LevelTutorialSubtitle, "Aprende a jugar, paso a paso" },
            { Keys.LevelDailyTitle, "Reto diario" },
            { Keys.LevelDailySubtitleDefault, "Un puzle al día · Igual para todos" },
            { Keys.LevelDailySubtitleLocked, "Se desbloquea tras el tutorial" },
            { Keys.LevelDailySubtitleOpen, "Un puzle al día · Igual para todos · Juega hoy" },
            { Keys.LevelDailySubtitleDone, "Hecho hoy · Tiempo {0} · Toca para ver" },
            { Keys.LevelDailyLoading, "Cargando el puzle de hoy…" },
            { Keys.LevelDailyLoadError, "No se pudo cargar el puzle de hoy: toca para reintentar" },
            { Keys.Level1Title, "Nivel 1" },
            { Keys.Level1Subtitle, "Reto 6×6" },

            { Keys.CampaignTitle, "Campaña" },
            { Keys.CampaignHint, "Supera los niveles en orden para desbloquear el siguiente" },
            { Keys.CampaignEmpty, "No hay niveles cargados: ejecuta Bordy → Generate Campaign Levels en Unity" },
            { Keys.CampaignHubTitle, "Campaña" },
            { Keys.CampaignHubSubtitle, "Niveles de historia · fácil → difícil" },
            { Keys.CampaignLevelTitleFmt, "Nivel {0}" },
            { Keys.CampaignLevelOpen, "{0}×{1} · toca para jugar" },
            { Keys.CampaignLevelLocked, "{0}×{1} · bloqueado" },
            { Keys.CampaignLevelDone, "{0}×{1} · completado" },

            { Keys.GameplayReset, "Reiniciar" },
            { Keys.GameplayUndo, "Comprobar" },
            { Keys.GameplayCheck, "Comprobar" },
            { Keys.GameplayHint, "Pista" },
            { Keys.GameplayRulesHeading, "Cómo jugar" },
            { Keys.GameplayRulesBody, "•  Cada fila y columna tiene el mismo número de cada icono, con como máximo 2 iconos iguales seguidos.\n•  Las celdas separadas por = deben coincidir; las separadas por × deben diferir." },
            { Keys.GameplayRulesTutorialHeading, "Guía" },
            { Keys.GameplayRulesTutorialBody, "•  Sigue los consejos y completa la cuadrícula 6×6, paso a paso.\n•  Cada fila y columna tiene tres de cada icono.\n•  = significa igual, × significa opuesto; nunca 3 seguidos." },
            { Keys.RulesIconsFill, "•  Rellena cada celda con" },
            { Keys.RulesIconsOr, "o" },

            { Keys.StatusTap, "" },
            { Keys.StatusNoHint, "No quedan celdas para pistas" },
            { Keys.StatusHintLoadingAd, "Cargando anuncio…" },
            { Keys.StatusHintAdFailed, "Anuncio no disponible, inténtalo más tarde" },
            { Keys.StatusHintEditorBlocked, "Mira un anuncio con recompensa para una pista (sim. de anuncios del Editor desactivada)" },
            { Keys.StatusHintSdkNotReady, "Los anuncios aún se están cargando, inténtalo en un momento" },
            { Keys.StatusHintAdNotConfigured, "Unidad de anuncios no configurada: crea un espacio con recompensa en el portal de desarrolladores" },
            { Keys.StatusHintFreeLeft, "Quedan {0} pista(s) gratis" },
            { Keys.StatusHintWatchAd, "No quedan pistas gratis: mira un anuncio para una pista" },
            { Keys.StatusHintCap, "No quedan pistas en este nivel (máx {0})" },
            { Keys.StatusErrors, "Algunas reglas no se cumplen: revisa las celdas en rojo" },
            { Keys.StatusWin, "¡Puzle resuelto!" },
            { Keys.StatusDailyDone, "Hecho hoy · Tiempo {0} (solo lectura, vuelve mañana)" },
            { Keys.StatusDailyWin, "¡Reto diario completado! Tiempo {0} (solo lectura)" },

            { Keys.TutorialWelcome, "¡Bienvenido a Bordy!\n\nRellena cada casilla con un {sun} o una {moon}." },
            { Keys.TutorialStart, "Empezar" },
            { Keys.TutorialCoachTap, "Toca en cualquier lugar para continuar" },
            { Keys.TutorialGuideSun, "Toca la casilla resaltada\nuna vez para poner un {sun}" },
            { Keys.TutorialGuideMoon, "Toca una casilla dos veces para poner una {moon}\n\nMeta: rellena la cuadrícula para que cada fila y\ncolumna tenga tres {sun} y tres {moon}" },
            { Keys.TutorialSymbols, "Las celdas pueden tener = o × entre ellas\n\n=  ambos lados deben coincidir\n×  ambos lados deben diferir\n\nProbemos cada uno." },
            { Keys.TutorialContinue, "Continuar" },
            { Keys.TutorialEquals, "= significa que ambos lados coinciden.\nHaz que estas dos celdas sean {sun}." },
            { Keys.TutorialCross, "× significa que las dos celdas difieren.\nHaz la celda de arriba {moon}\ny la de abajo {sun}." },
            { Keys.TutorialRowCount, "Cada fila y columna tiene tres {sun}\ny tres {moon}. Esta fila ya tiene\ntres {moon}, así que esta debe ser un {sun}." },
            { Keys.TutorialColCount, "Las columnas funcionan igual.\nEsta columna tiene dos {moon} y un {sun},\nasí que esta celda debe ser un {sun}." },
            { Keys.TutorialAvoidThree, "Nunca 3 iguales seguidos en fila o columna.\nEstas dos ya son {moon} {moon},\nasí que esta debe ser un {sun}." },
            { Keys.TutorialRowNeedSun, "A esta fila le falta un Sol. Toca hasta que sea un Sol." },
            { Keys.TutorialColNeedSun, "Esta columna ya tiene 2 lunas, así que esta celda debe ser un Sol." },
            { Keys.TutorialCheckPlant, "¿Te equivocaste? No pasa nada.\nToca esta celda para poner una {moon} primero." },
            { Keys.TutorialCheckUse, "¿Quieres borrarla? Toca de nuevo\ny vuelve a estar vacía." },
            { Keys.TutorialCheckFix, "Toca una vez más para poner el {sun} correcto." },
            { Keys.TutorialHintUse, "Si te atascas, usa Pista. Toca Pista y el juego rellena una celda correctamente." },
            { Keys.TutorialLastMoon, "Última celda: esta fila ya tiene dos {sun},\nasí que ponla como {moon}." },
            { Keys.TutorialComplete, "¡Tutorial completado!\n\nLa Campaña y el Reto diario están desbloqueados. Usa Comprobar para hallar errores y Pista cuando te atasques." },
            { Keys.TutorialToLevelSelect, "Elegir nivel" },
            { Keys.TutorialNudgeCell, "Toca la celda resaltada en amarillo." },
            { Keys.TutorialNudgeAgain, "Aún no está: toca de nuevo." },
            { Keys.TutorialNudgeCheck, "Primero toca el botón amarillo Comprobar." },
            { Keys.TutorialNudgeHint, "Toca el botón amarillo Pista." },
            { Keys.TutorialNudgeIdle, "Este paso aún no está hecho. Toca la celda o el botón resaltado." },
        };

        private static readonly Dictionary<string, string> Id = new Dictionary<string, string>
        {
            { Keys.SettingsTitle, "Pengaturan" },
            { Keys.SettingsFabLabel, "Pengaturan" },
            { Keys.SettingsLanguage, "Bahasa" },
            { Keys.SettingsLanguageButton, "Bahasa" },
            { Keys.SettingsLangZh, "简体中文" },
            { Keys.SettingsLangJa, "日本語" },
            { Keys.SettingsLangEn, "English" },
            { Keys.SettingsClose, "Tutup" },
            { Keys.SettingsPlayerGuest, "Pemain" },
            { Keys.SettingsPlayerMetaFmt, "Karakter: {0}  ·  Kampanye {1}" },
            { Keys.SettingsPrivacy, "Privasi & Ketentuan" },
            { Keys.SettingsSidebar, "Tambahkan ke bilah sisi" },
            { Keys.SettingsShortcut, "Tambahkan pintasan" },
            { Keys.HomeChipSidebar, "Bilah sisi" },
            { Keys.HomeChipShortcut, "Desktop" },
            { Keys.SettingsInvite, "Undang teman" },
            { Keys.SettingsInviteShare, "Ayo main Bordy bareng aku!" },
            { Keys.ShareDailyTitle, "Aku menyelesaikan Bordy hari ini dalam {0} — bisa kalahkan aku?" },
            { Keys.ShareDailySubtitle, "Teka-teki yang sama. Merasa lebih cepat?" },
            { Keys.SettingsStatusPrivacy, "Membuka Privasi & Ketentuan…" },
            { Keys.SettingsStatusPrivacyOk, "Privasi & Ketentuan dibuka" },
            { Keys.SettingsStatusPrivacyFail, "Tidak bisa membuka halaman. Kebijakan ditampilkan di bawah." },
            { Keys.SettingsStatusSidebar, "Membuka bilah sisi…" },
            { Keys.SettingsStatusSidebarOk, "Bilah sisi dibuka" },
            { Keys.SettingsStatusSidebarFail, "Bilah sisi tidak tersedia saat ini. Coba lagi." },
            { Keys.SettingsStatusShortcut, "Konfirmasi tambah ke desktop pada permintaan" },
            { Keys.SettingsStatusShortcutOk, "Ditambahkan ke desktop" },
            { Keys.SettingsStatusInvite, "Membuka undangan…" },
            { Keys.SettingsStatusLanguage, "Bahasa: Indonesia" },
            { Keys.SettingsLegalTitle, "Privasi & Ketentuan" },
            { Keys.SettingsLegalBody,
                "Operator: Shanghai Quanjie Technology Co., Ltd.\n\n" +
                "Privasi: https://bordy-api.brainless.workers.dev/privacy.html\n\n" +
                "Ketentuan: https://bordy-api.brainless.workers.dev/terms.html\n\n" +
                "Kami hanya mengumpulkan data yang disediakan platform Mini Game TikTok (OpenID, simpanan awan, iklan). Data disimpan di Cloudflare. Untuk umum. Kontak: 1531362757@qq.com" },
            { Keys.NavBack, "Kembali" },

            { Keys.ShopFabLabel, "Toko" },
            { Keys.ShopTitle, "Toko Ikon" },
            { Keys.ShopUse, "Pakai" },
            { Keys.ShopSelected, "Dipakai" },
            { Keys.ShopWatchAd, "Tonton iklan" },
            { Keys.ShopLoadingAd, "Memuat iklan…" },
            { Keys.ShopUnlocked, "Terbuka — sudah dipasang untukmu!" },
            { Keys.ShopAdFailed, "Iklan tidak tersedia — coba lagi nanti" },
            { Keys.ShopAdEditorBlocked, "Tonton iklan berhadiah untuk membuka (simulasi iklan Editor mati)" },
            { Keys.ShopAdSdkNotReady, "Iklan masih dimuat — coba lagi sebentar" },
            { Keys.ShopAdNotConfigured, "Unit iklan belum diatur — buat penempatan berhadiah di portal pengembang" },

            { Keys.HomeSubtitle, "Teka-teki Logika" },
            { Keys.HomeStart, "Main" },
            { Keys.HomeFooter, "Ketuk tombol untuk bermain" },
            { Keys.HomeLoginLoading, "Masuk…" },
            { Keys.HomeLoginFailed, "Gagal masuk. Periksa koneksimu dan coba lagi." },
            { Keys.HomeLoginRetry, "Coba lagi" },

            { Keys.LevelSelectTitle, "Pilih Level" },
            { Keys.LevelSelectHintUnlocked, "Pilih level untuk mulai" },
            { Keys.LevelSelectHintLocked, "Selesaikan tutorial untuk membuka level utama" },

            { Keys.LevelTutorialTitle, "Tutorial" },
            { Keys.LevelTutorialSubtitle, "Belajar bermain, langkah demi langkah" },
            { Keys.LevelDailyTitle, "Tantangan Harian" },
            { Keys.LevelDailySubtitleDefault, "Satu teka-teki per hari · Sama untuk semua" },
            { Keys.LevelDailySubtitleLocked, "Terbuka setelah tutorial" },
            { Keys.LevelDailySubtitleOpen, "Satu teka-teki per hari · Sama untuk semua · Main hari ini" },
            { Keys.LevelDailySubtitleDone, "Selesai hari ini · Waktu {0} · Ketuk untuk lihat" },
            { Keys.LevelDailyLoading, "Memuat teka-teki hari ini…" },
            { Keys.LevelDailyLoadError, "Tidak bisa memuat teka-teki hari ini — ketuk untuk coba lagi" },
            { Keys.Level1Title, "Level 1" },
            { Keys.Level1Subtitle, "Tantangan 6×6" },

            { Keys.CampaignTitle, "Kampanye" },
            { Keys.CampaignHint, "Selesaikan level secara berurutan untuk membuka berikutnya" },
            { Keys.CampaignEmpty, "Belum ada level dimuat — jalankan Bordy → Generate Campaign Levels di Unity" },
            { Keys.CampaignHubTitle, "Kampanye" },
            { Keys.CampaignHubSubtitle, "Level cerita · mudah → sulit" },
            { Keys.CampaignLevelTitleFmt, "Level {0}" },
            { Keys.CampaignLevelOpen, "{0}×{1} · ketuk untuk main" },
            { Keys.CampaignLevelLocked, "{0}×{1} · terkunci" },
            { Keys.CampaignLevelDone, "{0}×{1} · selesai" },

            { Keys.GameplayReset, "Ulang" },
            { Keys.GameplayUndo, "Periksa" },
            { Keys.GameplayCheck, "Periksa" },
            { Keys.GameplayHint, "Petunjuk" },
            { Keys.GameplayRulesHeading, "Cara Bermain" },
            { Keys.GameplayRulesBody, "•  Tiap baris dan kolom memuat jumlah ikon yang sama, maksimal 2 ikon sama berdampingan.\n•  Sel yang dipisah = harus sama; sel yang dipisah × harus berbeda." },
            { Keys.GameplayRulesTutorialHeading, "Panduan" },
            { Keys.GameplayRulesTutorialBody, "•  Ikuti petunjuk dan isi kisi 6×6, selangkah demi selangkah.\n•  Tiap baris dan kolom punya tiga dari tiap ikon.\n•  = berarti sama, × berarti berbeda; jangan pernah 3 berturut-turut." },
            { Keys.RulesIconsFill, "•  Isi tiap sel dengan" },
            { Keys.RulesIconsOr, "atau" },

            { Keys.StatusTap, "" },
            { Keys.StatusNoHint, "Tidak ada sel tersisa untuk petunjuk" },
            { Keys.StatusHintLoadingAd, "Memuat iklan…" },
            { Keys.StatusHintAdFailed, "Iklan tidak tersedia — coba lagi nanti" },
            { Keys.StatusHintEditorBlocked, "Tonton iklan berhadiah untuk petunjuk (simulasi iklan Editor mati)" },
            { Keys.StatusHintSdkNotReady, "Iklan masih dimuat — coba lagi sebentar" },
            { Keys.StatusHintAdNotConfigured, "Unit iklan belum diatur — buat penempatan berhadiah di portal pengembang" },
            { Keys.StatusHintFreeLeft, "Sisa {0} petunjuk gratis" },
            { Keys.StatusHintWatchAd, "Petunjuk gratis habis — tonton iklan untuk petunjuk" },
            { Keys.StatusHintCap, "Petunjuk habis di level ini (maks {0})" },
            { Keys.StatusErrors, "Beberapa aturan belum terpenuhi — periksa sel yang merah" },
            { Keys.StatusWin, "Teka-teki terpecahkan!" },
            { Keys.StatusDailyDone, "Selesai hari ini · Waktu {0} (hanya lihat — kembali besok)" },
            { Keys.StatusDailyWin, "Tantangan Harian selesai! Waktu {0} (hanya lihat)" },

            { Keys.TutorialWelcome, "Selamat datang di Bordy!\n\nIsi tiap kotak dengan {sun} atau {moon}." },
            { Keys.TutorialStart, "Mulai" },
            { Keys.TutorialCoachTap, "Ketuk di mana saja untuk lanjut" },
            { Keys.TutorialGuideSun, "Ketuk kotak yang disorot\nsekali untuk menaruh {sun}" },
            { Keys.TutorialGuideMoon, "Ketuk kotak dua kali untuk menaruh {moon}\n\nTujuan: isi kisi agar tiap baris dan\nkolom punya tiga {sun} dan tiga {moon}" },
            { Keys.TutorialSymbols, "Sel bisa punya = atau × di antaranya\n\n=  kedua sisi harus sama\n×  kedua sisi harus berbeda\n\nAyo coba satu per satu." },
            { Keys.TutorialContinue, "Lanjut" },
            { Keys.TutorialEquals, "= berarti kedua sisi harus sama.\nBuat kedua sel ini {sun}." },
            { Keys.TutorialCross, "× berarti kedua sel harus berbeda.\nBuat sel atas {moon}\ndan sel bawah {sun}." },
            { Keys.TutorialRowCount, "Tiap baris dan kolom punya tiga {sun}\ndan tiga {moon}. Baris ini sudah punya\ntiga {moon}, jadi ini harus {sun}." },
            { Keys.TutorialColCount, "Kolom bekerja dengan cara sama.\nKolom ini punya dua {moon} dan satu {sun},\njadi sel ini harus {sun}." },
            { Keys.TutorialAvoidThree, "Jangan pernah 3 sama berturut-turut di baris atau kolom.\nKedua ini sudah {moon} {moon},\njadi yang ini harus {sun}." },
            { Keys.TutorialRowNeedSun, "Baris ini masih butuh satu Matahari. Ketuk sampai jadi Matahari." },
            { Keys.TutorialColNeedSun, "Kolom ini sudah punya 2 bulan, jadi sel ini harus Matahari." },
            { Keys.TutorialCheckPlant, "Salah? Tidak masalah.\nKetuk sel ini jadi {moon} dulu." },
            { Keys.TutorialCheckUse, "Ingin menghapusnya? Ketuk lagi\ndan kembali kosong." },
            { Keys.TutorialCheckFix, "Ketuk sekali lagi untuk menaruh {sun} yang benar." },
            { Keys.TutorialHintUse, "Kalau buntu, pakai Petunjuk. Ketuk Petunjuk dan permainan mengisi satu sel dengan benar." },
            { Keys.TutorialLastMoon, "Sel terakhir: baris ini sudah punya dua {sun},\njadi jadikan {moon}." },
            { Keys.TutorialComplete, "Tutorial selesai!\n\nKampanye dan Tantangan Harian terbuka. Pakai Periksa untuk menemukan kesalahan dan Petunjuk saat buntu." },
            { Keys.TutorialToLevelSelect, "Pilih Level" },
            { Keys.TutorialNudgeCell, "Ketuk sel yang disorot kuning." },
            { Keys.TutorialNudgeAgain, "Belum selesai — ketuk lagi." },
            { Keys.TutorialNudgeCheck, "Ketuk tombol Periksa kuning dulu." },
            { Keys.TutorialNudgeHint, "Ketuk tombol Petunjuk kuning." },
            { Keys.TutorialNudgeIdle, "Langkah ini belum selesai. Ketuk sel atau tombol yang disorot." },
        };

        public static string Get(string key)
        {
            var table = TableFor(BordyLocale.Current);
            if (table.TryGetValue(key, out var value))
                return value;
            return En.TryGetValue(key, out value) ? value : key;
        }

        private static Dictionary<string, string> TableFor(BordyLanguage language)
        {
            switch (language)
            {
                case BordyLanguage.ZhHans: return Zh;
                case BordyLanguage.Ja: return Ja;
                case BordyLanguage.Es: return Es;
                case BordyLanguage.Id: return Id;
                default: return En;
            }
        }

        /// <summary>Language row label — ASCII fallback when CJK font not bundled.</summary>
        public static string SettingsLangZhLabel()
            => BordyFonts.HasCjk ? Get(Keys.SettingsLangZh) : "Chinese (Simplified)";

        public static string SettingsLangJaLabel()
            => BordyFonts.HasCjk ? Get(Keys.SettingsLangJa) : "Japanese";

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

            if (BordyLocale.Current == BordyLanguage.Ja)
            {
                switch (tier)
                {
                    case "easy":
                    case "hook": return "かんたん";
                    case "medium": return "ふつう";
                    case "hard": return "むずかしい";
                    case "brutal": return "超難関";
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
