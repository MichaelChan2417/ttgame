# Bordy 协作开发指南

## 环境

- Unity **2022.3.62f3c1**（不要用团结引擎）
- 首次打开后执行 **Bordy → Run Full Setup**

## 原则

1. **场景用代码构建** — 改 UI 布局请改 `Assets/Bordy/Editor/*SceneBuilder.cs`，然后重建对应场景，不要手改 `.unity` 文件（易冲突）。
2. **谜题数据集中管理** — 新关卡加到 `BordyLevelCatalog.cs`。
3. **最小改动** — 游戏逻辑在 `Assets/Bordy/Scripts/`，不要改 `Assets/Plugins/com.tiktok.minigame/`。
4. **提交前** — Play 走一遍：主页 → 选关 → 教程/第一关 → 重置。

## 常用菜单

```
Bordy → Run Full Setup          # 全量重建（推荐新同学首次使用）
Bordy → Rebuild Home Scene
Bordy → Rebuild Level Select Scene
Bordy → Rebuild Tutorial Scene
Bordy → Rebuild MainMenu Scene
Bordy → Open Home Scene         # 设置 Play 从主页启动
```

## 目录约定

| 路径 | 用途 |
|------|------|
| `Assets/Bordy/Scripts/` | 运行时游戏逻辑 |
| `Assets/Bordy/Editor/` | 场景构建与工程配置 |
| `Assets/Bordy/Scenes/` | 生成后的场景（可提交） |
| `docs/` | 玩法说明、阶段总结、协作文档 |

## Git 注意

- 忽略 `Library/`、`Temp/`、`tt-minigame/`（见根目录 `.gitignore`）
- 若 `Packages/manifest.json` 被改坏，勿提交 `com.coplaydev.unity-mcp` 等无关 Git 包
- 与队友对齐 Unity 小版本 **2022.3.62f3c1**

## 文档

- [GAMEPLAY.zh.md](GAMEPLAY.zh.md) — 玩法与架构
- [PHASE-SUMMARY.md](PHASE-SUMMARY.md) — 本阶段交付说明
