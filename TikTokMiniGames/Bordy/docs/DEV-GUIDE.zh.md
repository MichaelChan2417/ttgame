# Bordy 开发与调试手册

印尼 TikTok Minis 逻辑谜题。Unity 2022.3 + TTSDK Native 小游戏。

**当前里程碑**：云端登录/存档已通，Native 包可上传预览。简中手机端字体仍有问题（见文末已知 Bug）。

---

## 1. 环境

| 项 | 要求 |
|----|------|
| Unity | **2022.3.62f3c1**（与 `ProjectSettings/ProjectVersion.txt` 一致） |
| 包源 | 国内镜像用 `packages.unity.cn`，海外用 `packages.unity.com`。**不要提交** `Packages/packages-lock.json` 的 registry 差异 |
| TTSDK | 菜单 **TikTokGame**，需 Stark SDK |
| Node | 部署 Cloudflare Worker 时需要 |
| wrangler | `npm i -g wrangler`，登录 Cloudflare |

克隆后：

1. 用 Unity Hub 打开 `TikTokMiniGames/Bordy`
2. 首次打开等 Import 完成
3. 菜单 **Bordy → Full Setup**（建场景、关卡、UI 引用）
4. 若报错 `StarkBuilderSetting` 路径：菜单 **Bordy → Fix Stark Build Paths (This Machine)**

`StarkBuilderSetting.asset` 是**本机路径**，已在 `.gitignore`，每人本地自动生成，别提交。

---

## 2. 项目结构（常改的部分）

```
Assets/Bordy/
  Scripts/Runtime/     游戏逻辑、UI、云端
  Scripts/Editor/      Full Setup、字体导入、Stark 路径
  Scenes/              Home → LevelSelect → Tutorial → MainMenu
  Resources/           运行时加载（字体放 Resources/Bordy/）
  StreamingAssets/     levels.json 等
  Editor/StarkBuilderSetting.asset.example

cloudflare/bordy-api/  云端 Worker 源码
scripts/               打包、健康检查
tt-minigame/           Build 输出（不提交 git）
docs/                  文档
legal/                 隐私政策、用户协议（上传后台用）
```

场景 Build 顺序（**不能乱**）：

| Index | 场景 | 作用 |
|-------|------|------|
| 0 | Home | 入口，等 SDK + 云端就绪 |
| 1 | LevelSelect | 选关 / 每日挑战 |
| 2 | Tutorial | 4×4 教学 |
| 3 | MainMenu | 6×6 正式关 |

---

## 3. 日常开发流程

### 3.1 Editor 里跑

1. 打开 **Home** 场景，点 Play
2. 流程：加载 →（可选）TikTok 登录 → 云端拉档 → 进主页

Editor 里 **没有 TikTok 容器**，`BordyUserService` 会走 Editor 模拟用户，云端仍可调（看 `BordyAppConfig.ApiBaseUrl`）。

### 3.2 改玩法 / 关卡

- 关卡数据：`StreamingAssets/levels.json`，或 Editor 工具生成
- 规则：`BordyBoard`、`BordyGameController`
- 教学：`Tutorial` 场景 + `BordyTutorialController`

改完 Play 模式走一遍：Home → 教学 → 第一关 → 每日挑战。

### 3.3 改 UI / 文案

- 多语言：`Assets/Bordy/Scripts/Runtime/BordyI18n.cs` + 各 `*Ui.cs`
- 字体：`BordyFonts.cs`；CJK 需 `Resources/Bordy/BordyUI.ttf`（见已知 Bug）
- 圆角按钮：`BordyUi.Rounded()`，别再用 `UISprite.psd`（Play 模式会炸）

### 3.4 改云端

- Worker：`cloudflare/bordy-api/src/index.js`
- Unity 客户端：`BordyCloudBackend.cs`、`BordyCloudSave.cs`、`BordyCloudSync.cs`
- 配置：`BordyAppConfig.asset` → `ApiBaseUrl`

本地改 Worker 后：

```bash
cd cloudflare/bordy-api
npx wrangler deploy
curl -s https://bordy-api.brainless.workers.dev/api/health
```

根路径 `/` 返回服务说明；**健康检查用 `/api/health`**。大陆 curl `workers.dev` 可能超时，不影响 TikTok 容器内访问。

---

## 4. Build 与上传（Native，不是 H5）

### 4.1 Unity Build

菜单 **TikTokGame → Build Minigame**，等完成。

输出在 `tt-minigame/`：

| 目录 | 内容 | 要不要上传 |
|------|------|------------|
| `webgl/` | Unity 原始 WebGL | **否** |
| `tt-minigame/` | 带 `game.json` 的 Native 包 | **是**（打 zip 上传） |

常见误区：把 `webgl/` 当 H5 传上去 → 缺 `game.json`，预览白屏/卡加载。

### 4.2 打 zip

```bash
cd TikTokMiniGames/Bordy
./scripts/pack-native-minigame.sh
# → tt-minigame/bordy-upload-native.zip
```

zip 根目录必须有 `game.json`。

### 4.3 TikTok 开发者后台

1. [TikTok for Developers](https://developers.tiktok.com/) → 你的 Minis 应用
2. 上传 **Native** 包（不是 H5）
3. 沙盒：**Partial rollout 0%**，用 Preview 扫码
4. **Security**：登记 `https://bordy-api.brainless.workers.dev`
5. 基础信息、行业资质、隐私链接（`legal/` 里 HTML）

应用信息：

- App ID: `7647437535525996565`
- Client Key: `mgt6rr5wp9i8b059`（Secret 只在 Worker 环境变量，别进 git）

---

## 5. 云端账号与存档

已部署：`https://bordy-api.brainless.workers.dev`

流程简述：

1. 容器内 `tt.login` → code
2. POST `/api/auth/tiktok` → `sessionToken`
3. GET/PUT `/api/save` 同步进度（教学、每日、语言等）

Unity 侧：

- `BordyHomeGate`：Home 场景挡住 UI，直到 SDK + 云端就绪
- `InitSDK` **延迟到 AfterSceneLoad**，避免抢在容器就绪前调用（曾导致预览一直 loading）
- 超时：SDK 12s、云端登录 15s，失败仍进游戏（本地档）

Worker 密钥（Cloudflare Dashboard 或 `wrangler secret put`）：

- `TIKTOK_CLIENT_KEY`
- `TIKTOK_CLIENT_SECRET`

KV 命名空间 ID 在 `cloudflare/bordy-api/wrangler.toml`。

---

## 6. 调试：Editor vs 手机预览

| 现象 | Editor | 手机预览 |
|------|--------|----------|
| 登录 | 模拟 openId | 真 `tt.login` |
| 字体 | 系统字体 | WebGL 仅内置 Arial/LegacyRuntime |
| 返回键 | 可见 | 需用 `BordyUiChrome` 药丸按钮，别靠 Unicode ← |
| 网络 | 本机直连 Worker | 容器内 HTTPS |

**建议**：逻辑在 Editor 测；登录、字体、性能、真机 UI 必须扫码预览。

---

## 7. 问题排查（我们踩过的坑）

### 预览一直 loading

- **原因**：`InitSDK` 调太早，或云端/login 挂死无超时
- **现状**：已改延迟 Init + 超时；仍失败会进游戏
- **查**：TikTok 后台 Security 域名、Worker `/api/health`、Unity `ApiBaseUrl`

### 上传后报缺 game.json

- 传错目录：传了 `webgl/` 而不是 Native 包
- 用 `./scripts/pack-native-minigame.sh` 再传

### Play 模式报 UISprite.psd

- 旧 UI 引用了 Editor 专用图
- 跑 **Bordy → Full Setup** 或依赖 `BordyUi.FixMissingSprites()`

### Full Setup 报 debugSymbols

- 已改为 `WebGLDebugSymbolMode.Off`，更新代码后再跑 Setup

### 导入字体报 77MB / PingFang

- 别把系统 PingFang 整包复制进项目
- 菜单 **Bordy → Import UI Font (macOS)**：只接受 &lt;15MB；或自备 **子集化** 的 `BordyUI.ttf` 放到 `Resources/Bordy/`

### 设置里简体中文看不见

- Editor 可能正常，**手机预览仍丢字体** → 见已知 Bug

### 返回键看不见

- WebGL 不渲染某些 Unicode 符号
- 已用 `BordyUiChrome` 白底药丸 + 文字「返回」；改 UI 后需 **重新 Build + 上传** 才在预览生效

### curl Worker 不通

- 大陆访问 `*.workers.dev` 经常超时
- 用浏览器或海外节点测；TikTok 容器走另一套网络

### git 冲突 StarkBuilderSetting.asset

- 别合并这个文件；删本地改动，跑 **Fix Stark Build Paths**

### packages-lock.json 全是 unity.cn

- 本机 Unity 镜像导致，**别提交**；还原或加入个人 ignore

---

## 8. 已知 Bug（里程碑记录）

| Bug | 状态 | 说明 |
|-----|------|------|
| **简体中文手机预览字体丢失** | 未修复 | WebGL 打包不带系统中文字体；`LegacyRuntime.ttf` 无 CJK。设置页语言名、简中文案在真机预览可能空白或 tofu。需要体积可控的子集 TTF（如 Noto Sans SC 子集）放进 `Resources/Bordy/BordyUI.ttf`，并确认 `BordyFonts` 在 WebGL 下加载。PingFang 全量 ~77MB 不可行。 |
| Editor 简中显示正常但预览不一致 | 同上 | 以手机预览为准验收 i18n |

---

## 9. Git 协作

- 分支：`main` 保护，功能用短分支
- **不提交**：`tt-minigame/`、`StarkBuilderSetting.asset`、`.DS_Store`、含 Secret 的文件、`packages-lock.json` registry 改动
- 大改 UI/场景前先说一声，减少场景 merge 痛苦
- 提交前：Editor Play 一遍；要上预览再 Build + pack

---

## 10. 其他文档

| 文档 | 用途 |
|------|------|
| [GAMEPLAY.zh.md](GAMEPLAY.zh.md) | 玩法规则、场景说明 |
| [GAMEPLAY.md](GAMEPLAY.md) | 英文玩法（可选） |
| [README.md](README.md) | 文档索引 |

玩法细节不看本文，看 GAMEPLAY。
