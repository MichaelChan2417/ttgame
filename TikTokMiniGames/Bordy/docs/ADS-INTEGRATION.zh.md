# Bordy 广告接入 — 代码实现详解

TikTok Minis **激励视频**如何接到 Hint / Check，以及可选插屏的实现细节。

相关源码：

| 文件 | 职责 |
|------|------|
| `Assets/Bordy/Scripts/BordyAppConfig.cs` | Ad Unit ID、Editor 模拟开关 |
| `Assets/Bordy/Scripts/BordyAdsService.cs` | TTSDK 广告封装 |
| `Assets/Bordy/Scripts/BordyHintPolicy.cs` | 免费 Hint 次数 + 每关上限 3 |
| `Assets/Bordy/Scripts/BordyCheckPolicy.cs` | 免费 Check 次数 + 每关上限 3 |
| `Assets/Bordy/Scripts/BordyBoardController.cs` | Hint / Check 按钮、扣次数、调广告 |
| `Assets/Bordy/Scripts/BordyUserService.cs` | InitSDK 后 `NotifySdkReady()` |
| `Assets/Bordy/Scripts/BordyStrings.cs` | 广告相关状态文案 |

当前生产配置：

| 项 | 值 |
|----|-----|
| App ID | `7647437535525996565` |
| 激励视频 Ad Unit | `ad7660431701143963669` |
| 配置常量 | `BordyAppConfig.RewardedVideoAdUnitId` |
| 插屏（未申请） | `InterstitialAdUnitId = "demo_interstitial"` |

---

## 1. 产品逻辑

```
玩家点 Hint 或 Check
    │
    ├─ 本关已用满 3 次 → 按钮变灰，状态「本关已用完（最多 3 次）」
    │
    ├─ 本关仍有免费次数 → 直接给（Hint 填一格 / Check 进入选格）
    │
    └─ 免费次数用完且未满 3 次 → 播激励视频
            │
            ├─ 完整观看 (isEnded=true) → 给一次
            └─ 提前关闭 / 失败 → 不给，底部状态栏提示原因
```

教程：无限、不看广告。闯关 / 每日：Hint 与 Check **每关各最多 3 次**（含免费）。

Check：点按钮（按钮保持橙色）→ 再点一格 → 标出该行列相对标准答案填错的格子，改对前一直留着。没有空闲自动标错。

---

## 2. 次数策略

### Hint — `BordyHintPolicy`

| 档位 `tier` | 免费 Hint | 之后广告 | 本关合计上限 |
|-------------|-----------|----------|--------------|
| `easy` / `hook` | 2 | 1 | 3 |
| `medium` | 1 | 2 | 3 |
| `hard` / `brutal` | 0 | 3 | 3 |
| 教程 | 无限（`-1`） | 否 | 无 |
| 每日挑战 | 0 | 3 | 3 |

### Check — `BordyCheckPolicy`

| 来源 | 免费 Check | 之后广告 | 本关合计上限 |
|------|------------|----------|--------------|
| 教程 | 无限 | 否 | 无 |
| 闯关 / 每日 | 1 | 2 | 3 |

`MaxUsesPerLevel = 3`。教程 `ResolveBudget` 返回 `-1`，不扣费、不播广告。

---

## 3. 对局内实现 — BordyBoardController

### 3.1 初始化（Start 内）

```csharp
InitHintBudget();
InitCheckBudget();
```

- `_freeHintBudget` / `_freeCheckBudget >= 0`：有免费上限，用完后走广告，但不超过 `MaxUsesPerLevel`。
- `== -1`：教程无限。
- `HintCapReached()` / `CheckCapReached()`：本关已用满 3 次，按钮 `interactable = false`。

### 3.2 Hint() 主流程

```csharp
public void Hint()
{
    if (_won || _reviewMode) return;
    if (!HasHintableCell()) { ... return; }
    if (HintCapReached()) { ... return; }          // 本关 3 次用完
    if (NeedsRewardedAdForHint())                  // 免费用完且未达上限
    {
        RequestHintViaAd();
        return;
    }
    if (ApplyHintInternal())
        _hintsUsedThisSession++;
}

private bool NeedsRewardedAdForHint()
    => _freeHintBudget >= 0 && _hintsUsedThisSession >= _freeHintBudget && !HintCapReached();
```

Check 同结构：`CheckCapReached` → `NeedsRewardedAdForCheck` → `BeginCheckPick`（点亮按钮）→ 再点格 `ApplyCheckAt`。

看完广告若取消选格，这次额度仍保留，不必连看两次。

### 3.3 请求广告

```csharp
private void RequestHintViaAd()
{
    if (_hintAdInFlight) return;
    _hintAdInFlight = true;
    SetTransientStatusKey(StatusHintLoadingAd);

    BordyAdsService.ShowRewarded(
        onReward: () => {
            _hintAdInFlight = false;
            if (ApplyHintInternal()) _hintsUsedThisSession++;
            UpdateHintStatus();
        },
        onFail: reason => {
            _hintAdInFlight = false;
            SetTransientStatusKey(MapAdFailReason(reason));
        });
}
```

### 3.4 ApplyHintInternal()

遍历棋盘，找到第一个「非给定格且答案不对」的格子，填入 `Solution` 值，刷新 UI，触发 `EvaluateBoard()`。

### 3.5 通关插屏（可选）

```csharp
if (entry.Tier == "brutal")
    BordyAdsService.TryShowInterstitial();
```

仅 `brutal` 关通关后尝试插屏；ID 仍为 demo 占位时真机直接跳过。

---

## 4. 广告 SDK 封装 — BordyAdsService

### 4.1 前置条件

真机 `ShowRewarded` 检查顺序：

1. 非 Editor
2. `!_rewardedShowing`（防连点）
3. `BordyUserService.SdkInited == true`
4. `IsRewardedConfigured`（Ad Unit 非空且不以 `demo_` 开头）

InitSDK 在 `BordyUserService.BootRoutine` 里完成；成功后调用 `NotifySdkReady()` 打 log。

### 4.2 TikTok SDK 调用约定（与官方 Demo 一致）

**没有 `Load()`**。每次展示：

```csharp
var ad = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam {
    AdUnitId = BordyAppConfig.RewardedVideoAdUnitId,
});
ad.OnError += (code, msg) => { ... FailRewarded; ad.Destroy(); };
ad.OnClose += isEnded => {
    ad.Destroy();
    if (isEnded) onReward();   // 只有看完才发 Hint
    else onFail("skipped");
};
ad.Show();
```

- **每次 Show 新建实例**，关闭后 `Destroy()`（与 `BordyMainMenu` Demo 一致）。
- 奖励判定：**仅** `OnClose(isEnded == true)`。

### 4.3 Editor 行为

```csharp
#if UNITY_EDITOR
if (BordyAppConfig.EditorSimulateRewardedAds)
    onReward();           // 模拟看完
else
    onFail("editor_no_sim");  // 默认：提示需真机或开模拟
#endif
```

本地测 Hint 次数逻辑可设：

```csharp
public const bool EditorSimulateRewardedAds = true;
```

### 4.4 插屏 TryShowInterstitial

- 静默失败，不影响游戏。
- 需要真实 `InterstitialAdUnitId`（非 `demo_*`）且 `SdkInited`。

---

## 5. 失败原因与用户文案

| `onFail` reason | 用户看到的 key | 含义 |
|-----------------|----------------|------|
| `editor_no_sim` | `StatusHintEditorBlocked` | Editor 未开模拟 |
| `sdk_not_ready` | `StatusHintSdkNotReady` | InitSDK 未完成 |
| `not_configured` | `StatusHintAdNotConfigured` | Ad Unit 仍是 demo 占位 |
| `skipped` | `StatusHintAdFailed` | 用户提前关闭 |
| `error_*` / 其它 | `StatusHintAdFailed` | 创建/展示失败 |

---

## 6. 端到端时序（真机）

```mermaid
sequenceDiagram
    participant User
    participant Board as BordyBoardController
    participant Ads as BordyAdsService
    participant TT as TTSDK

    User->>Board: 点 Hint（免费次数已用完）
    Board->>Board: NeedsRewardedAdForHint? yes
    Board->>Ads: ShowRewarded(onReward, onFail)
    Ads->>TT: CreateRewardedVideoAd + Show()
    TT-->>User: 全屏激励视频
    TT-->>Ads: OnClose(isEnded=true)
    Ads->>Board: onReward()
    Board->>Board: ApplyHintInternal()
```

---

## 7. 配置与发布 checklist

1. TikTok 后台 **Monetization** 创建 Rewarded Video，复制 Ad Unit ID。
2. 写入 `BordyAppConfig.RewardedVideoAdUnitId`（当前已填 `ad7660431701143963669`）。
3. **TikTokGame → Build Minigame** → 打 zip → App 扫码预览（Editor 无真广告）。
4. 用完免费次数后点 Hint 或 Check，确认视频弹出且看完后给奖励（Hint 填格并金色高亮；Check 进入选格）。
5. 第 4 次应提示本关用完，按钮变灰。

---

## 8. 与登录模块的关系

| 能力 | 依赖 |
|------|------|
| 激励视频 Hint / Check | `SdkInited`（InitSDK 成功） |
| 云存档 | `CloudLoggedIn`（Worker 登录） |

两者独立：云登录失败时 Play 可能被 Home 挡住，但已进入对局后 Hint 广告仍依赖 SDK init，不依赖 `CloudLoggedIn`。

详见 [LOGIN-STATE.zh.md](LOGIN-STATE.zh.md)。

---

## 9. 调试清单

| 现象 | 处理 |
|------|------|
| Editor 提示 ad sim is off | 正常；开 `EditorSimulateRewardedAds` 或打真机包 |
| 真机 not configured | 检查 `RewardedVideoAdUnitId` |
| 真机 sdk not ready | 等 Home 加载完再进关；查 InitSDK log |
| 有广告但没 Hint | 是否提前关闭（`isEnded=false`） |
| 完全无填充 | 广告位审核 / 地区；查 `OnError` code |

---

## 10. 扩展建议（未实现）

- 换 Ad Unit：只改 `BordyAppConfig`，重新 Build。
- 增加插屏：后台申请 Interstitial ID，替换 `InterstitialAdUnitId`，可在关卡结算统一调 `TryShowInterstitial()`。
- 激励复用实例：当前按 SDK Demo 每次 Create；若官方建议复用可再封装一层 pool。

参考 Demo：`Assets/Bordy/Scripts/BordyMainMenu.cs` → `DoRewardedAd()` / `DoInterstitialAd()`。
