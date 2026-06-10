# Bordy 本阶段开发总结

> 阶段范围：从「无法 Play」到「可玩的基础版本 + 关卡选择 + 新手引导」  
> Unity：**2022.3.62f3c1** · 协作仓库：`MichaelChan2417/ttgame`（Bordy 子目录）

## 已完成

### 工程与启动

- 修复 Package Manager 卡在 resolving（移除 `com.coplaydev.unity-mcp` Git 依赖）
- 修复 Play 进入空白 Untitled 场景（`BordyPlayModeEntry` 强制从 `Home.unity` 启动）
- 修复 Home → 游戏场景后按钮全部失效（`BordyUiBootstrap` 重建 EventSystem）

### 核心玩法（6×6 第一关）

- `BordyBoardController`：点击循环填子、撤销、提示、重置、规则校验、通关判定
- `BordyTimer`：跨场景计时；**重置棋盘时清零**
- Q 版太阳/月亮程序化贴图 + 弹出/待机动效（`BordyTokenSprites` / `BordyTokenView`）
- 修复重置后错误红格不消失的问题

### 关卡与引导

- **关卡选择页** `LevelSelect.unity`
- **新手引导** `Tutorial.unity`（4×4 棋盘 + 分步 overlay）
- 完成引导后解锁第一关（`BordyProgress` / PlayerPrefs）
- 关卡数据统一到 `BordyLevelCatalog` + `BordyPuzzleData`

### 场景构建

- 共用布局：`BordyGameplaySceneBuilder`
- 独立构建器：Home / LevelSelect / Tutorial / MainMenu
- `Bordy → Run Full Setup` 一键生成四场景并写入 Build Settings

## 场景导航（当前）

```
Home → LevelSelect → Tutorial（4×4）
                  → MainMenu（6×6，需完成教程）
```

## 主要文件清单

```
Assets/Bordy/Scripts/
  BordyBoardController.cs      # 棋盘玩法
  BordyLevelCatalog.cs         # 关卡数据
  BordyPuzzleData.cs           # 谜题结构
  BordyTutorialGuide.cs        # 新手引导
  BordyLevelSelectController.cs
  BordyProgress.cs
  BordyNav.cs
  BordyTimer.cs
  BordyTokenSprites.cs / BordyTokenView.cs
  BordyUiBootstrap.cs
  BordyMainMenu.cs             # SDK 演示（未接入场景）

Assets/Bordy/Editor/
  BordyGameplaySceneBuilder.cs
  BordyHomeSceneBuilder.cs
  BordyLevelSelectSceneBuilder.cs
  BordyTutorialSceneBuilder.cs
  BordySceneBuilder.cs
  BordySetup.cs
  BordyPlayModeEntry.cs
```

## 已知限制 / 下一步建议

| 项 | 说明 |
|----|------|
| 关卡数量 | 仅教程 + 第一关 |
| TikTok SDK | 游戏场景未接入 `TT.InitSDK` |
| 美术 | 棋子为程序化 Q 版，可换 PNG |
| 存档 | 仅教程完成标记，无关卡星级 |
| `BordyMainMenu.cs` | SDK 参考代码，与谜题场景分离 |

建议下一阶段：更多 6×6 关卡、接入 TT 登录/存档、设计师棋子资源、引导中途重置时恢复高亮。

## 本地验证

1. **Bordy → Run Full Setup**
2. Play → 开始游戏 → 完成新手引导 → 进入第一关
3. 测试撤销 / 提示 / 重置 / 计时器清零 / 错误标红后重置
