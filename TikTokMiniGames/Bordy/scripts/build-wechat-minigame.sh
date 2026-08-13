#!/bin/bash
# Build Bordy / 星月棋 for WeChat Mini Game (WeChat Unity WebGL Transform).
# This script runs OUTSIDE Unity; you must first export the webgl build via
# Unity menu: 微信小游戏 -> 转换小游戏.
set -euo pipefail

cd "$(dirname "$0")/.."

PROJECT_DIR="$(pwd)"
WEBGL_OUT="${PROJECT_DIR}/tt-minigame/wechat-minigame/webgl"
MINIGAME_OUT="${PROJECT_DIR}/tt-minigame/wechat-minigame/minigame"

echo "[build-wechat] Project: ${PROJECT_DIR}"
echo "[build-wechat] Expected Unity WebGL output: ${WEBGL_OUT}"
echo "[build-wechat] Expected minigame entry: ${MINIGAME_OUT}"

if [ ! -d "${WEBGL_OUT}" ] || [ ! -d "${MINIGAME_OUT}" ]; then
    echo "[build-wechat] ERROR: Unity export not found."
    echo "[build-wechat] Please export from Unity first:"
    echo "    1. Install WX SDK: Window -> Package Manager -> + -> Add package from git URL"
    echo "       https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk.git"
    echo "    2. Menu: 微信小游戏 -> 转换小游戏"
    echo "    3. Set output directory to: ${PROJECT_DIR}/tt-minigame/wechat-minigame"
    exit 1
fi

echo "[build-wechat] Unity export found."

# Optional: zip for manual upload or distribution
ZIP_OUT="${PROJECT_DIR}/tt-minigame/wechat-minigame.zip"
rm -f "${ZIP_OUT}"
( cd "${PROJECT_DIR}/tt-minigame/wechat-minigame" && zip -r "${ZIP_OUT}" ./minigame ./webgl )

echo "[build-wechat] Packaged: ${ZIP_OUT}"
echo "[build-wechat] Next: open 微信开发者工具 -> import ${MINIGAME_OUT}"
