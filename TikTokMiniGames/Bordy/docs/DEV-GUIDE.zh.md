# Bordy 开发与调试手册

印尼 TikTok Minis 逻辑谜题。Unity 2022.3 + TTSDK Native 小游戏。

**当前里程碑**：闯关模式（4 关试玩包）、激励视频 Hint 广告已接入（Ad Unit `ad7660431701143963669`），云端登录/存档已通，Native 包可上传预览。简中手机端字体仍有问题（见文末已知 Bug）。

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
| 1 | LevelSelect | 选关 hub（教学 / 闯关 / 每日） |
| 2 | CampaignSelect | 闯关关卡网格选关 |
| 3 | Tutorial | 4×4 教学 |
| 4 | Play | 共享对局场景（闯关 / 每日，运行时建盘） |
| 5 | MainMenu | 旧 6×6 演示（Build 里可禁用） |

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

改完 Play 模式走一遍：Home → 教学 → 闯关 → 每日挑战。

### 3.3 闯关关卡（离线生成 + 盈利向难度曲线）

设计目标：**前 3～5 关多数人能过**（留住用户、熟悉规则），**之后大量极难关**（多数人需提示/广告道具，少数人能纯逻辑通关）。

| 档位 `tier` | 关卡 | 典型参数 |
|-------------|------|----------|
| `hook` | 第 1～5 关 | 6×6，给定格 52%～62%，边线索偏多 |
| `hard` | 第 6 关起前半 | 8×8，给定格 26%～32%，边线索偏少 |
| `brutal` | 后 30% | 8×8，给定格 18%～24%，边线索极少 |

**顺序 = 游玩顺序**（不再按难度分排序打乱）。

**试玩 4 关**（1 简单 / 1 中等 / 1 偏难 / 1 极难）：

```bash
python3 tools/generate_levels.py demo
```

正式批量：

```bash
./scripts/generate-campaign.sh 30 5    # 30 关，前 5 关 hook
python3 tools/generate_levels.py campaign --count 30 --hook-count 5
```

JSON 字段：`tier`、`givenRatio` 便于你们调参和数据分析。

每日挑战默认 `hard`（给定格约 26%，少边线索），与主线后半难度一致。

### 3.4 广告变现（激励视频 → Hint）★

> **详细代码文档** → [ADS-INTEGRATION.zh.md](ADS-INTEGRATION.zh.md)（给同伴讲实现用这个）。

> **本版本重点**：Hint 提示已与 TikTok 激励视频广告打通，是闯关盈利的核心链路。  
> 代码：`BordyAdsService` → `BordyBoardController.Hint()` → `BordyHintPolicy`（免费次数）。

#### 当前配置（已上线代码）

| 项 | 值 |
|----|-----|
| App ID | `7647437535525996565` |
| 激励视频 Ad Unit ID | `ad7660431701143963669` |
| 配置位置 | `Assets/Bordy/Scripts/BordyAppConfig.cs` → `RewardedVideoAdUnitId` |
| SDK 初始化 | `BordyUserService` 内 `TT.InitSDK`，成功后 `BordyAdsService.NotifySdkReady()` |

换广告位时只改 `BordyAppConfig.RewardedVideoAdUnitId`，重新 Build 上传即可。

#### 玩家侧流程

1. 闯关关卡有 **免费 Hint 次数**（按难度档位，见下表）。
2. 免费次数用完后，点 **Hint** → 拉起激励视频。
3. 用户 **完整观看**（`OnClose(isEnded == true)`）→ 填入一格正确答案。
4. 提前关闭 / 广告失败 → **不给** Hint，底部状态栏有提示。

| 档位 `tier` | 免费 Hint |
|-------------|-----------|
| `easy` / `hook` | 2 次 |
| `medium` | 1 次 |
| `hard` / `brutal` | 0 次（首次 Hint 即走广告） |
| 教程关 | 无限（不走广告） |
| 每日挑战 | 0 次免费 |

#### SDK 调用约定（已实现，勿改顺序）

1. 须先 `TT.InitSDK` 成功（Home 启动时完成）。
2. **没有 `Load()`**：`TT.CreateRewardedVideoAd({ AdUnitId })` 返回后直接 `Show()`。
3. 每次展示 **新建广告实例**，关闭后 `Destroy()`（与 SDK Demo 一致）。
4. 仅在 `OnClose(isEnded == true)` 时发奖励。

核心代码路径：

```
Hint() → NeedsRewardedAdForHint?
  → BordyAdsService.ShowRewarded(onReward, onFail)
  → TT.CreateRewardedVideoAd / Show / OnClose
  → ApplyHintInternal()
```

#### 测试方式

| 环境 | 能否看到真广告 | 说明 |
|------|----------------|------|
| Unity Editor | **否** | 默认提示 “Editor ad sim is off”；本地测流程可设 `EditorSimulateRewardedAds = true` |
| TikTok App 预览 | **是** | **必须** Build Native 包 + 扫码；在容器外测无效 |

真机验证步骤：

1. **TikTokGame → Build Minigame** → `./scripts/pack-native-minigame.sh`
2. 上传 zip，TikTok App 扫码预览
3. 进闯关 **第 3 / 4 关**（或第 1 关用完 2 次免费 Hint）
4. 点 Hint → 应弹出激励视频；看完后棋盘填入一格

启动日志（TTSDK 调试终端）应出现：

`[BordyAds] Rewarded ad ready (unit=ad7660431701143963669)`

#### 后台申请新广告位（如需更换）

1. [TikTok for Developers](https://developers.tiktok.com/) → Minis 应用 → **Monetization**
2. 新建 **Rewarded Video**，复制 Ad Unit ID
3. 写入 `BordyAppConfig.RewardedVideoAdUnitId`
4. 重新 Build + 上传（新位可能需审核数小时）

插屏广告（通关 `brutal` 关后可选）仍用占位 `InterstitialAdUnitId = "demo_interstitial"`，未申请可忽略。

#### 排查

| 现象 | 可能原因 |
|------|----------|
| Editor “ad sim is off” | 正常；开 `EditorSimulateRewardedAds` 或打真机包 |
| “Ad unit not configured” | ID 仍为 `demo_*` 或空 |
| “Ads are still loading” | SDK 未 init；等 Home 就绪再进关 |
| 广告弹出但没 Hint | 用户提前关闭 |
| 完全没广告 | 广告位审核中 / 无填充 / 查 `[BordyAds]` 与 TTSDK 日志 |

### 3.5 改 UI / 文案

- 多语言：`Assets/Bordy/Scripts/Runtime/BordyI18n.cs` + 各 `*Ui.cs`
- 字体：`BordyFonts.cs`；CJK 需 `Resources/Bordy/BordyUI.ttf`（见已知 Bug）
- 圆角按钮：`BordyUi.Rounded()`，别再用 `UISprite.psd`（Play 模式会炸）

### 3.6 改云端

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

### 闯关选关列表空白但能点

- **原因**：`ScrollViewport` 用了无 sprite 的 `Mask`，WebGL 下子节点不渲染
- **现状**：已改 `RectMask2D` + 网格方块 UI；旧包需重新 Build

### 主页背景变成椭圆 / 四角露色

- **原因**：`FixMissingSprites` 曾给 `Background` 套圆角九宫格
- **现状**：`Background` 只用纯色平铺；圆角 sprite 仅用于按钮/关卡块

### curl Worker 不通

- 大陆访问 `*.workers.dev` 经常超时
- 用浏览器或海外节点测；TikTok 容器走另一套网络

### git 冲突 StarkBuilderSetting.asset

- 别合并这个文件；删本地改动，跑 **Fix Stark Build Paths**

### packages-lock.json / PackageManagerSettings 一直 modified

- 本机 Unity 镜像（`packages.unity.cn` vs `.com`）导致
- 已在 `.gitignore` 且不应进 git；若仍被追踪，执行 `git rm --cached` 后提交 ignore 变更

---

## 8. 已知 Bug（里程碑记录）

| Bug | 状态 | 说明 |
|-----|------|------|
| **简体中文手机预览字体丢失** | 未修复 | WebGL 打包不带系统中文字体；`LegacyRuntime.ttf` 无 CJK。设置页语言名、简中文案在真机预览可能空白或 tofu。需要体积可控的子集 TTF（如 Noto Sans SC 子集）放进 `Resources/Bordy/BordyUI.ttf`，并确认 `BordyFonts` 在 WebGL 下加载。PingFang 全量 ~77MB 不可行。 |
| Editor 简中显示正常但预览不一致 | 同上 | 以手机预览为准验收 i18n |

---

## 9. Git 协作

- 分支：`main` 保护，功能用短分支
- **不提交**：`tt-minigame/`、`StarkBuilderSetting.asset`、`.DS_Store`、含 Secret 的文件、`Packages/packages-lock.json` 与 `ProjectSettings/PackageManagerSettings.asset`（本机 Package Manager 镜像）
- 大改 UI/场景前先说一声，减少场景 merge 痛苦
- 提交前：Editor Play 一遍；要上预览再 Build + pack

---

## 10. 其他文档

| 文档 | 用途 |
|------|------|
| [LOGIN-STATE.zh.md](LOGIN-STATE.zh.md) | **登录状态代码详解**（Boot、Worker、Home 门控） |
| [ADS-INTEGRATION.zh.md](ADS-INTEGRATION.zh.md) | **广告接入代码详解**（Hint 激励视频） |
| [GAMEPLAY.zh.md](GAMEPLAY.zh.md) | 玩法规则、场景说明 |
| [GAMEPLAY.md](GAMEPLAY.md) | 英文玩法（可选） |
| [README.md](README.md) | 文档索引 |

玩法细节不看本文，看 GAMEPLAY。
