# Bordy Gameplay & Development Guide

[中文](GAMEPLAY.zh.md)

## Overview

Bordy is a **sun / moon logic puzzle** (Takuzu / Binairo style):

- Fill every cell with a sun or moon
- Equal sun/moon count per row and column
- No three identical symbols in a row
- `×` = adjacent cells must differ; `=` = adjacent cells must match

## Scene flow

```
Home ──[Start]──▶ first-time → Tutorial
                  returning  → LevelSelect
                                 ├─ Tutorial
                                 ├─ CampaignSelect → Play
                                 └─ Daily → Play
```

| Index | Scene | Role |
|-------|-------|------|
| 0 | `Home.unity` | Entry |
| 1 | `LevelSelect.unity` | Hub (tutorial / campaign / daily) |
| 2 | `CampaignSelect.unity` | Campaign grid |
| 3 | `Tutorial.unity` | Guided 4×4 (includes Check / Hint) |
| 4 | `Play.unity` | Shared board (campaign / daily) |
| 5 | `MainMenu.unity` | Legacy 6×6 demo |

## Key scripts

See [GAMEPLAY.zh.md](GAMEPLAY.zh.md) for the full script table (same content, Chinese labels).

## Editor menus

Run **Bordy → Run Full Setup** after cloning. Individual rebuild menus exist for each scene. All UI is code-generated via `Assets/Bordy/Editor/*SceneBuilder.cs`.

## Art pipeline

Two procedural sprites (sun + moon) are generated once at runtime and shared by all cells. Replace `BordyTokenSprites` to swap in designer PNGs.

## Legacy

`BordyMainMenu.cs` is the TikTok SDK API demo — not wired into active game scenes. `MainMenu.unity` is the level-1 board scene (historical name).
