# Bordy

TikTok Minis 逻辑谜题（印尼区）。Unity 2022.3 + TTSDK Native。

## 快速开始

1. Unity Hub 打开本项目
2. **Bordy → Run Full Setup**（含闯关场景 + 关卡 JSON）
3. 打开 Home 场景 → Play

## 广告变现（激励视频 → Hint）★

本版本已接入 TikTok **激励视频**，用于 Hint 提示变现：

| 项 | 说明 |
|----|------|
| Ad Unit ID | `ad7660431701143963669`（`BordyAppConfig.RewardedVideoAdUnitId`） |
| 触发 | 闯关免费 Hint 用完后，点 Hint 拉起广告；**看完**才给提示 |
| 真机测试 | **必须** Build Native 包 + TikTok App 扫码；Editor 不播真广告 |

完整流程、免费次数档位、SDK 约定与排错 → **[ADS-INTEGRATION.zh.md](docs/ADS-INTEGRATION.zh.md)**（详细代码说明）。

生成试玩关卡：

```bash
python3 tools/generate_levels.py demo   # 4 关：简单→极难
```

## 文档

- **[开发与调试手册](docs/DEV-GUIDE.zh.md)** — 日常开发、Build、上传、排错
- **[登录状态实现详解](docs/LOGIN-STATE.zh.md)** — Boot、云端登录、存档同步（给同伴讲代码）
- **[广告接入实现详解](docs/ADS-INTEGRATION.zh.md)** — 激励视频 Hint、免费次数策略（给同伴讲代码）
- [玩法说明](docs/GAMEPLAY.zh.md)

## Build 上传（摘要）

```bash
# Unity: TikTokGame → Build Minigame
./scripts/pack-native-minigame.sh
# 上传 tt-minigame/bordy-upload-native.zip（Native，不是 webgl/）
```

## 已知问题

简体中文在**手机预览**仍可能字体丢失，Editor 里不一定复现。详见 [DEV-GUIDE 第 8 节](docs/DEV-GUIDE.zh.md#8-已知-bug里程碑记录)。
