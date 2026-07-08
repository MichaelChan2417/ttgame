# Bordy

TikTok Minis 逻辑谜题（印尼区）。Unity 2022.3 + TTSDK Native。

## 快速开始

1. Unity Hub 打开本项目
2. **Bordy → Full Setup**
3. 打开 Home 场景 → Play

## 文档

- **[开发与调试手册](docs/DEV-GUIDE.zh.md)** — 日常开发、Build、上传、排错
- [玩法说明](docs/GAMEPLAY.zh.md)

## Build 上传（摘要）

```bash
# Unity: TikTokGame → Build Minigame
./scripts/pack-native-minigame.sh
# 上传 tt-minigame/bordy-upload-native.zip（Native，不是 webgl/）
```

## 已知问题

简体中文在**手机预览**仍可能字体丢失，Editor 里不一定复现。详见 [DEV-GUIDE 第 8 节](docs/DEV-GUIDE.zh.md#8-已知-bug里程碑记录)。
