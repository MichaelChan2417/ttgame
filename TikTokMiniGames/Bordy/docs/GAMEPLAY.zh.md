# Bordy 游戏玩法与开发说明

[English](GAMEPLAY.md)

## 游戏概述

Bordy 是一款 **太阳 / 月亮逻辑谜题**（类似 Takuzu / Binairo）：

- 在棋盘上填入太阳或月亮
- 每行、每列太阳与月亮数量相等
- 每行、每列不能连续出现 3 个相同图案
- `×` 表示相邻两格必须**相反**；`=` 表示必须**相同**

## 场景流程

```
Home（主页）
  └─[开始游戏]─▶ 首次玩家 → Tutorial（4×4 新手引导）
                 老玩家   → LevelSelect（关卡选择）
                              ├─[新手引导]─▶ Tutorial
                              ├─[闯关模式]─▶ CampaignSelect → Play（6×6 / 8×8）
                              ├─[每日挑战]─▶ Play（6×6，全球同题，UTC 切日）
                              └─[第一关]───▶ MainMenu（旧 6×6 演示）
```

| Build Index | 场景 | 说明 |
|-------------|------|------|
| 0 | `Home.unity` | 入口：标题 + 开始游戏 |
| 1 | `LevelSelect.unity` | 关卡选择（教学 / 闯关 / 每日） |
| 2 | `CampaignSelect.unity` | 闯关关卡网格 |
| 3 | `Tutorial.unity` | 4×4 新手引导（含 Check / Hint 教学） |
| 4 | `Play.unity` | 共享对局（闯关 / 每日，运行时建盘） |
| 5 | `MainMenu.unity` | 旧 6×6 演示（可禁用） |

## 核心脚本

| 脚本 | 职责 |
|------|------|
| `BordyNav.cs` | 场景跳转；`StartGame()` 首次玩家直达教程 |
| `BordyUserService.cs` | 启动时 `TT.InitSDK` + `TT.Login`；用户档案 |
| `BordyStore.cs` | 持久化封装（容器内走 `TT.PlayerPrefs`） |
| `BordyLevelCatalog.cs` | 关卡数据（教程 + 第一关 + 每日挑战） |
| `BordyDaily.cs` | 每日挑战状态（UTC 切日、断点续玩、只读复盘） |
| `BordyBoardController.cs` | 棋盘：点格、Check（查错）、Hint、重置、判胜 |
| `BordyTutorialGuide.cs` | 新手引导：规则 → Check → Hint；高亮直到该格完成；空白处橙底提示 |
| `BordyHintPolicy.cs` / `BordyCheckPolicy.cs` | 免费次数与每关 3 次上限 |
| `BordyCampaignCatalog.cs` | 闯关关卡 JSON |
| `BordyEdgeRules.cs` | 竖等号；去掉等号旁多余的 × |
| `BordyLevelSelectController.cs` | 关卡解锁与每日挑战卡片状态 |
| `BordyProgress.cs` | 教程完成进度 |
| `BordyTokenSprites.cs` | Q 版太阳/月亮贴图（运行时生成 2 张，全局复用） |
| `BordyTokenView.cs` | 棋子显示与动效 |
| `BordyTimer.cs` | 计时器（重置棋盘时清零） |
| `BordyUiBootstrap.cs` | 场景加载后修复 EventSystem、挂载运行时组件 |
| `BordyAppConfig.cs` | App ID、Client Key（出包 / Minis CLI） |
| `BordyLocale.cs` / `BordyStrings.cs` | 语言偏好与双语文案包 |
| `BordySettingsUi.cs` | 右下角设置与语言选择 |

## 编辑器菜单

| 菜单项 | 作用 |
|--------|------|
| **Bordy → Run Full Setup** | 一键重建全部场景 + PlayerSettings + WebGL |
| **Bordy → Rebuild Home Scene** | 重建主页 |
| **Bordy → Rebuild Level Select Scene** | 重建关卡选择 |
| **Bordy → Rebuild Tutorial Scene** | 重建新手引导 |
| **Bordy → Rebuild MainMenu Scene** | 重建第一关 |
| **Bordy → Open Home Scene** | 打开主页并设置 Play 模式入口 |
| **Bordy → Reset Player Data** | 清空档案+教程+每日+语言（测试首次进入） |
| **Bordy → Reset Daily Challenge** | 仅重置今日每日挑战 |
| **Bordy → Debug → Unlock All Campaign Levels** | 完成教程并解锁全部闯关（不标记通关） |
| **Bordy → Print Player Profile** | 打印当前用户档案 |

场景均由 **代码构建**（`Assets/Bordy/Editor/*SceneBuilder.cs`），避免 UI 布局漂移。

Build、上传、云端与排错见 **[DEV-GUIDE.zh.md](DEV-GUIDE.zh.md)**。

## 添加新关卡

1. 在 `BordyLevelCatalog.cs` 增加 `BordyPuzzleData` 定义
2. 在 `BordyLevelCatalog` 注册场景名与 ID
3. 复制 `BordySceneBuilder` 或复用 `BordyGameplaySceneBuilder.BuildHierarchy()`
4. 在 `LevelSelect` 场景增加按钮，并在 `BordyLevelSelectController` 接线
5. 更新 `BordyHomeSceneBuilder.RegisterBuildScenes()`

## 美术说明

棋子不使用独立 PNG 资源。首次需要时由 `BordyTokenSprites` 程序化绘制 **1 张太阳 + 1 张月亮** Sprite，所有格子共用。若要换成设计师资源，只需改 `BordyTokenSprites` 改为加载 `Resources` 或 `Sprite` 引用。

## 本地化

- 右下角 **⚙ 设置** → 语言：**简体中文** / **English**
- 文案集中在 `BordyStrings.cs`（语言包）；切换后各场景自动刷新
- 偏好保存在 `BordyStore`（`bordy.locale`）。TikTok 默认英文（只有 `zh*` 才走中文）；微信小游戏默认简体中文，设置里可切换。
- 每日题目优先拉 CDN；失败或微信构建则用 `BordyLevelCatalog` 内置固定题，保证能进每日。

## 对局道具

- **Check**：点按钮再点一格，标出该行列填错的格子（对照标准答案）。改对前一直留着。闯关/每日每关 1 次免费，之后看广告，最多 3 次。
- **Hint**：填一格正确答案，金色高亮直到下次点棋盘或本次 Hint 通关。免费次数按关卡档位，之后看广告，最多 3 次。
- 教程里两者不看广告；会专门教一遍。空闲自动标错已去掉。

## 遗留说明

- `BordyMainMenu.cs`：TikTok SDK API 演示菜单，**未挂到当前游戏场景**，仅作 SDK 参考保留。
- 场景名 `MainMenu.unity` 为历史命名，实际是 **第一关游戏场景**。

## 开发与调试

Build、上传、云端、排错见 **[DEV-GUIDE.zh.md](DEV-GUIDE.zh.md)**。
