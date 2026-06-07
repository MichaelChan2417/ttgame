# Bordy · TikTok Mini Game (Unity SDK reference)

**English** · [中文](README.zh.md)

> ## 📦 Get the SDK
>
> **[⬇️ Download `com.tiktok.minigame@1.1.1-Release.unitypackage`](./com.tiktok.minigame@1.1.1-Release.unitypackage)** (1.6 MB)
>
> The complete TikTok Mini Game SDK is shipped in this repo as a single Unity package.
> Drag-and-drop it into any Unity 2022.3 project, or follow
> [Integrating the SDK](#integrating-the-sdk-in-your-own-project) for the full step-by-step.

A reference Unity project that integrates the **TikTok Mini Game SDK (TTSDK)** and exposes
every public `TT.*` API through clickable buttons grouped by category. The log panel mirrors
every callback so you can see exactly what each API returns, both inside Unity Editor and
inside the real Douyin (TikTok) mini-game container.

| Collapsed home | After Init SDK + a few calls |
| --- | --- |
| ![Main menu collapsed](docs/screenshots/01-main-collapsed.png) | ![Categories expanded with log output](docs/screenshots/02-expanded-with-log.png) |

---

## What's in this repo

| Path | Purpose |
| --- | --- |
| `com.tiktok.minigame@1.1.1-Release.unitypackage` | Ready-to-import SDK package (TTSDK v1.1.1). Drag-and-drop into any Unity 2022.3 project. |
| `Assets/Plugins/com.tiktok.minigame/` | The SDK already imported into this project. |
| `Assets/Bordy/Scripts/BordyNav.cs` | Scene navigation — `StartGame()` (home → game) and `BackToHome()` (game → home). |
| `Assets/Bordy/Scripts/BordyMainMenu.cs` | Runtime menu — accordion-style category list, one button per `TT.*` API, log panel. |
| `Assets/Bordy/Editor/BordyHomeSceneBuilder.cs` | Rebuilds the `Home.unity` start scene from code (menu: **Bordy → Rebuild Home Scene**). |
| `Assets/Bordy/Editor/BordySceneBuilder.cs` | Rebuilds `MainMenu.unity` from code (menu: **Bordy → Rebuild MainMenu Scene**). |
| `Assets/Bordy/Editor/BordySetup.cs` | One-click PlayerSettings + both scenes + WebGL switch (menu: **Bordy → Run Full Setup**). |
| `Assets/Bordy/Scenes/Home.unity` | Home / start scene — title + **开始游戏** button. Build Settings scene **0** (entry point). |
| `Assets/Bordy/Scenes/MainMenu.unity` | Gameplay scene (Bordy board), entered via **开始游戏**; the header **←** returns home. Build Settings scene **1**. |

---

## Requirements

- Unity **2022.3.34f1** (any 2022.3.x LTS should work). Do **not** use the Tuanjie / 团结引擎
  fork — the SDK's `asmdef` will collide.
- TTSDK **1.1.1** or newer. v1.1.1 ships a separate `ttsdk-editor.dll` so the Mock
  implementation runs inside Unity Editor without throwing.
- Build target: WebGL (IL2CPP). Other platforms are not supported by the container.

---

## Quick start

```bash
git clone <this repo>
cd Bordy
# Open the folder via Unity Hub → Add project → 2022.3.34f1
```

Inside Unity:

1. Wait for the initial asset import to finish.
2. Run **menu: Bordy → Run Full Setup** once. It builds both scenes (`Home.unity`,
   `MainMenu.unity`), wires the buttons, and registers them in Build Settings with
   Home as scene 0.
3. Open `Assets/Bordy/Scenes/Home.unity` and press Play. Tap **开始游戏** to enter the
   game; tap the header **←** to come back to the home page.

Navigation flow:

```
Home.unity ──[开始游戏]──▶ MainMenu.unity
     ▲                          │
     └──────────[←]─────────────┘
```

If anything looks off, rebuild either scene at any time via **Bordy → Rebuild Home Scene**
or **Bordy → Rebuild MainMenu Scene**.

---

## Integrating the SDK in your own project

You can grab the SDK package directly from this repo:

```
./com.tiktok.minigame@1.1.1-Release.unitypackage
```

In your project:

1. **Drag-and-drop** the `.unitypackage` into the Unity Editor Project window, or use
   **Assets → Import Package → Custom Package…** and select the file.
2. Accept all items in the import dialog.
3. After import you should have:
   ```
   Assets/Plugins/com.tiktok.minigame/
     TTSDK/ttsdk.dll          ← runtime (WebGL only)
     TTSDK/ttsdk-editor.dll   ← editor mock (Editor only)
     Editor/ttsdk_tools.dll   ← build tool
     WebGL/                   ← platform glue
     LitJson/, DefaultTemplate/
   ```
4. Configure **Player Settings**:
   - Build target: **WebGL**
   - Scripting backend: **IL2CPP** (Mono is not supported)
   - Company / product name as you wish — they're surfaced in the container UI
5. Confirm the WebGL platform-specific dll filters in PluginImporter (Unity should do this
   automatically from `.meta`): `ttsdk.dll` ticked only for WebGL, `ttsdk-editor.dll` ticked
   only for Editor.

That's it — `using TTSDK;` becomes available.

---

## Initializing the SDK

Initialization is a single async call. Every container-dependent API (Login / Pay / Ads /
Share / …) requires this to succeed first.

```csharp
using TTSDK;
using UnityEngine;

public class Boot : MonoBehaviour
{
    void Start()
    {
        TT.InitSDK((code, env) =>
        {
            if (code == 0)
            {
                Debug.Log($"[TT] InitSDK ok, containerEnv={env?.GetType().Name}");
                // Safe to call TT.Login / TT.Pay / TT.CreateRewardedVideoAd / …
            }
            else
            {
                Debug.LogError($"[TT] InitSDK failed, code={code}");
            }
        });
    }
}
```

In Bordy the same flow lives in
[`BordyMainMenu.InitSdk()`](Assets/Bordy/Scripts/BordyMainMenu.cs).

### Editor vs WebGL

| Environment | What runs | Notes |
| --- | --- | --- |
| Unity Editor (Play mode) | `TTAPIMock` via `ttsdk-editor.dll` | Mock returns reasonable defaults so you can iterate on the UI / wiring without rebuilding WebGL every time. |
| WebGL build inside Douyin Developer Tools or production | `TTAPIImpl` via `ttsdk.dll` | Talks to the real container; logins / payments / ads behave for real. |

You **must** use SDK ≥ 1.1.1 for Editor mock to work — earlier versions only shipped the
WebGL-target dll and would throw `NotSupportedException: 不支持的平台` in Editor.

---

## API reference

Bordy covers every public `TT.*` API in v1.1.1. Buttons are grouped under collapsible
category headers; each row below is one button.

### 系统 / System

| Button | API | Purpose |
| --- | --- | --- |
| System Info | `TT.GetSystemInfo()` | Host, device, locale, screen, SDK / host version. |
| Container Version | `TT.GetContainerVersion()`, `TT.InContainerEnv`, `TT.TTSDKVersion`, `TT.GameVersion`, `TT.GamePublishVersion` | Version constants for compat checks. |
| Launch Options | `TT.GetLaunchOptionsSync()` | Scene / path / query the container passed on launch. |
| Clean File Cache | `TT.CleanAllFileCache(cb)` | Wipe the SDK-managed cache directory. |

### 生命周期 / Lifecycle

| Button | API | Purpose |
| --- | --- | --- |
| Register App Show/Hide | `TT.GetAppLifeCycle().OnShow / OnHide` | Foreground / background events. |
| Set Before-Exit Listener | `TT.GetAppLifeCycle().SetOnBeforeExitAppListener(...)` | Intercept the user's exit gesture. Return `true` to handle it yourself, `false` for default exit. |

### 账号 / Auth

| Button | API | Purpose |
| --- | --- | --- |
| Login | `TT.Login(onSuccess, onFail)` | Exchange container session for a short-lived `code` your backend uses to fetch openid. |
| Authorize userInfo | `TT.Authorize("scope.userInfo", onOk, onFail)` | Prompt the user to grant a scope. |

### 分享 / Share

| Button | API | Purpose |
| --- | --- | --- |
| Share App Message | `TT.ShareAppMessage(ShareAppMessageParam)` | Open the container share sheet. `Path` / `Query` deep-link back into the game. |

### 支付 / Payment

| Button | API | Purpose |
| --- | --- | --- |
| Pay | `TT.Pay(TTPayParam)` | Pay for a `trade_order_id` issued by your backend. |
| Check Balance | `TT.CheckBalance(TTCheckBalanceParam)` | Has the user enough virtual currency? |
| Recharge | `TT.Recharge(TTRechargeParam)` | Open the recharge flow for a tier id. |
| Navigate To Balance | `TT.NavigateToBalance(TTNavigateToBalanceParam)` | Jump to the user's wallet page. |

### 任务奖励 / Mission

| Button | API | Purpose |
| --- | --- | --- |
| Start Entrance Mission | `TT.StartEntranceMission(...)` | Kick off an entry-channel mission. |
| Get Entrance Mission Reward | `TT.GetEntranceMissionReward(...)` | Query / claim the reward. |

### 广告 / Ads

| Button | API | Purpose |
| --- | --- | --- |
| Rewarded Video Ad | `TT.CreateRewardedVideoAd(CreateRewardedVideoAdParam)` | Returns an ad object — call `Show()` directly (no `Load()`). `OnClose(isEnded)` tells you to grant the reward. |
| Interstitial Ad | `TT.CreateInterstitialAd(CreateInterstitialAdParam)` | Same shape, no reward callback. |

### 网络 / Network

| Button | API | Purpose |
| --- | --- | --- |
| Get Network Type | `TT.GetNetWorkType(GetNetworkTypeParam)` | One-shot connectivity query. |
| On / Off NetworkStatus Change | `TT.OnNetworkStatusChange(cb)` / `TT.OffNetworkStatusChange(cb)` | Connection on/off subscription. |
| On / Off NetworkWeak Change | `TT.OnNetworkWeakChange(cb)` / `TT.OffNetworkWeakChange(cb)` | Weak-network toggle (lag detection). |

### 设备 / Device

| Button | API | Purpose |
| --- | --- | --- |
| Vibrate Short | `TT.VibrateShort(VibrateShortParam)` | ~15 ms haptic. |
| Vibrate Long | `TT.VibrateLong(VibrateLongParam)` | ~400 ms haptic. |
| Set FPS 30 / 60 | `TT.SetPreferredFramesPerSecond(fps)` | Hint the engine + container. |

### 键盘 / Keyboard

| Button | API | Purpose |
| --- | --- | --- |
| Show Keyboard | `TT.ShowKeyboard(TTKeyboard.ShowKeyboardOptions, onOk, onErr)` | Bring up the soft keyboard (WebGL only). |
| Hide Keyboard | `TT.HideKeyboard(onOk, onErr)` | Dismiss it. |

### 桌面快捷 / Shortcut

| Button | API | Purpose |
| --- | --- | --- |
| Add Shortcut | `TT.AddShortcut(cb)` | Prompt the user to add the game to the home screen. |

### 存档 / Storage

| Button | API | Purpose |
| --- | --- | --- |
| PlayerPrefs ++counter | `TT.PlayerPrefs.GetInt/SetInt/Save()` | Cross-platform KV store, same surface as Unity's `PlayerPrefs`. |
| Save / Load / Delete `<BordySaving>` | `TT.Save<T>(obj)`, `TT.LoadSaving<T>()`, `TT.DeleteSaving<T>()` | Typed object persistence — one slot per type unless you pass `saveName`. |
| Clear All Savings | `TT.ClearAllSavings()` | Wipe every slot. |
| Saving Disk Size | `TT.GetSavingDiskSize()` | Bytes used by savings on disk. |

### Feed 直玩 / Feed

| Button | API | Purpose |
| --- | --- | --- |
| On / Off Feed Status Change | `TT.OnFeedStatusChange(cb)` / `TT.OffFeedStatusChange(cb)` | Feed-entry / feed-exit notifications for 即看即玩. |

### 上报 / Analytics

| Button | API | Purpose |
| --- | --- | --- |
| Report Event | `TT.ReportEvent(ReportEventParam)` | Custom event with arbitrary JSON params. |
| Report Scene | `TT.ReportScene(json, onOk, onFail, onComplete)` | Scene / page change for funnel analytics. |

For the exact field shape of each `Param` type, see the SDK source under
`Assets/Plugins/com.tiktok.minigame/TTSDK/Modules/Interface/*`, or open the
`com.tiktok.minigame` package in your IDE for IntelliSense.

---

## Building to TikTok mini game

The SDK ships its own build pipeline that produces the WebGL bundle plus the metadata
files the Douyin developer tools expect.

1. Open **menu: Window → TTSDK → Build Tool**.
2. Fill in your AppID, game version, etc.
3. Click **Build** — the bundle lands in `./tt-minigame/webgl/` along with a `game.json`
   that includes the `sdkVersion` field (auto-injected by SDK 1.1.1).
4. Upload `tt-minigame/` to the Douyin developer back-end for preview / review.

Plain Unity File → Build Settings → WebGL also works for a smoke test, but it won't
produce the `tt-minigame/` layout the container expects.

---

## Editor playmode notes

- `TT.InitSDK` succeeds in Editor via Mock (with SDK ≥ 1.1.1). Most callbacks return
  reasonable sample defaults — they are **not** representative of production behaviour.
- For real device behaviour (haptics, share sheet, payments, ads, real openid), you
  must build to WebGL and run inside Douyin Developer Tools or production.
- If `Init SDK` throws `NotSupportedException: 不支持的平台`, you are most likely using an
  older SDK that lacks `ttsdk-editor.dll`. Re-import the package shipped with this repo.

---

## Project layout

```
.
├── com.tiktok.minigame@1.1.1-Release.unitypackage   # SDK package, drop into other projects
├── Assets/
│   ├── Bordy/
│   │   ├── Editor/
│   │   │   ├── BordyHomeSceneBuilder.cs              # Code-driven Home scene rebuild
│   │   │   ├── BordySceneBuilder.cs                  # Code-driven gameplay scene rebuild
│   │   │   └── BordySetup.cs                         # One-click setup pipeline
│   │   ├── Scenes/
│   │   │   ├── Home.unity                           # Home / start scene (built scene 0)
│   │   │   └── MainMenu.unity                       # Gameplay scene (built scene 1)
│   │   └── Scripts/
│   │       ├── BordyNav.cs                          # Start / back scene navigation
│   │       └── BordyMainMenu.cs                      # Runtime menu + API wiring
│   └── Plugins/com.tiktok.minigame/                 # Imported SDK (do not edit)
├── docs/screenshots/                                # README assets
├── Packages/manifest.json                           # Unity package list
├── ProjectSettings/                                 # Unity project settings
└── README.m