# Bordy 微信小游戏上线方案（星月棋）

> 目标：以「星月棋」为中文名，以 IAA（纯广告、无内购）模式上架微信小游戏，绕过版号；通过纯中文名称绕过软著触发条件。

---

## 1. 资质与平台配置

| 项目 | 状态 | 说明 |
|------|------|------|
| 注册平台 | `mp.weixin.qq.com` | 企业主体：上海诠界科技有限公司 |
| 小程序名称 | **星月棋** | 纯中文，不含英文/「软件」字样，降低软著要求 |
| 英文名/简称 | Bordy | 仅在后台/简介中作为副名称 |
| 类目 | 游戏 → 休闲益智 / 棋类 | 以玩法匹配为准 |
| 微信认证 | 必须 | 费用 ¥300/年，企业主体才能提交游戏类 |
| 支付资质 | **不申请** | 走 IAA 纯广告，无内购，无需版号 |
| 软著 | 目标：免交 | 中文名不含英文/软件字样，按规则不触发；但建议并行申请电子版权认证作为备用 |
| ICP 备案 | 必须 | 7～20 个工作日，关键路径 |
| 小游戏备案 | 必须 | 与 ICP 并行 |

---

## 2. 技术路线

### 2.1 基础策略

Unity 2022.3 + 微信小游戏转换 SDK：

```text
https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk.git
```

- 使用 `BordyAppConfig.WebStandalone = true` 模式：跳过 TTSDK 登录、云端存档、TT 广告
- 微信小游戏内使用：本地存储 + 微信激励视频广告 + 微信登录（可选）
- 微信 AppID: `wxc8a4ef945116dc27`

### 2.2 关键分支

- 代码分支：`feature/wechat-minigame`
- 主分支 `main` 保持 TikTok 版本不变
- 该分支只做「微信小游戏」相关的适配，不修改核心玩法

### 2.3 构建产物

```text
tt-minigame/wechat-minigame/
  minigame/          # 小游戏入口与 JS 运行时
  webgl/             # Unity WebGL 构建输出
```

---

## 3. 代码侧待确认清单

| 模块 | 当前 `WebStandalone` 行为 | 微信分支需要 |
|------|-----------------------------|--------------|
| `BordyUserService` | 跳过登录、默认游客 | 可选接入微信静默登录 |
| `BordyCloudSave` | 本地 `BordyStore` | 继续本地存储 |
| `BordyAdsService` | 不加载广告 | 接入微信 `wx.createRewardedVideoAd` |
| `BordyDailyService` | 本地模板或 `ApiBaseUrl` | 建议：内置本地模板（避免 CDN 域名） |
| `BordyShopUi` | 看广告解锁 | 微信广告奖励解锁 |
| 字体 | CJK 子集字体 | 必须解决，否则中文预览会 tofu |
| 包体 | 首包大小 | 需要分包 + CDN 托管资源 |

---

## 4. 广告接入方案（IAA）

微信小游戏 IAA 支持：

- `wx.createRewardedVideoAd`（激励视频）→ 对应 Hint、Shop 解锁
- `wx.createInterstitialAd`（插屏）→ 可选，通关后低频触发
- Banner 广告 → 可选，但谨慎避免影响体验

策略：

- Hint 免费次数用完后 → 激励视频
- Shop 皮肤解锁 → 激励视频
- 插屏：通关 brutal 关后，带冷却时间

---

## 5. 每日挑战

- 微信构建 **不请求 CDN**（`BordyDailyService.BaseUrl` 在 `WECHAT_MINIGAME` 下为空），直接用 `BordyLevelCatalog` 内置 6×6 固定题。
- 好处：无需配置 request 合法域名、审核不会打到 `workers.dev`。
- TikTok 仍可拉云端题；失败时同样回退到这份内置题。
- 上线后若要真正「每日一题」，把 JSON 放到已备案域名后再打开联网。

---

## 6. 构建与提审

产出目录（Unity 转换 SDK 配置）：

```text
/Users/holya/WeChatProjects/星月棋/
  minigame/     # 用微信开发者工具打开这个目录
  webgl/
```

步骤：

1. 分支：`feature/wechat-minigame`（已含教程 / Check / 30 关闯关 / 简中 / 设置按钮）
2. Unity 2022.3：菜单 **Bordy → Switch Build Target → WeChat Mini Game**，再 **微信小游戏 → 转换小游戏**
   - 或命令行：`-executeMethod Bordy.Editor.BordyWechatBuildMenu.ExportForReview`
3. 微信开发者工具 → 导入 `minigame` → AppID `wxc8a4ef945116dc27` → 预览 / 真机
4. 上传代码 → 提交审核

注意：

- 游戏内名称、后台名称均为 **星月棋**；默认简体中文，右下角设置可切英文。
- 激励视频广告位 `WechatRewardedAdUnitId` 若为空，Check/Hint 用完免费次数后无法看广告续次（教程不受影响）。
- 不得请求未备案域名。

---

## 7. 注意事项

- 所有提交后台的名称、备案名称、游戏内名称必须一致：「星月棋」
- 游戏内不得出现任何未备案的域名或外链
- 广告触发必须在用户明确操作后（如点击「提示」按钮），不能自动播放
- 必须包含未成年人适龄提示（12+）

---

## 8. 并行任务

- 你：注册小程序、微信认证、提交 ICP/小游戏备案、拿到 AppID
- 同伴：继续 Steam KYC 文档提交
- 代码：完成 `feature/wechat-minigame` 分支的移植与打包
