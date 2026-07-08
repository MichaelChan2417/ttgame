#!/usr/bin/env bash
# Package Unity TTSDK native minigame output for TikTok Developer Portal upload.
# Run after TikTokGame → Build Minigame.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
NATIVE_SRC="$ROOT/tt-minigame/tt-minigame"
PROJECT_CONFIG="$ROOT/project.config.json"
MINIGAME_CONFIG="$ROOT/minigame.config.json"
ZIP_OUT="$ROOT/tt-minigame/bordy-upload-native.zip"

die() { echo "[pack-native] ERROR: $*" >&2; exit 1; }

[[ -d "$NATIVE_SRC" ]] || die "Missing $NATIVE_SRC — run Unity TikTokGame → Build Minigame first."
[[ -f "$NATIVE_SRC/game.json" ]] || die "Missing game.json in $NATIVE_SRC"
[[ -f "$PROJECT_CONFIG" ]] || die "Missing $PROJECT_CONFIG"

echo "[pack-native] 1/4 Copy project.config.json + minigame.config.json"
cp "$PROJECT_CONFIG" "$NATIVE_SRC/project.config.json"
[[ -f "$MINIGAME_CONFIG" ]] && cp "$MINIGAME_CONFIG" "$NATIVE_SRC/minigame.config.json"

echo "[pack-native] 2/4 Patch game.json (disable WebGL2 for broader device support)"
python3 - "$NATIVE_SRC/game.json" <<'PY'
import json, sys
from pathlib import Path
path = Path(sys.argv[1])
data = json.loads(path.read_text(encoding="utf-8"))
if data.get("enableWebGL2"):
    data["enableWebGL2"] = False
    path.write_text(json.dumps(data, indent=4, ensure_ascii=False) + "\n", encoding="utf-8")
    print("[pack-native] enableWebGL2 -> false")
else:
    print("[pack-native] enableWebGL2 already false")
PY

echo "[pack-native] 3/4 Fix zero-byte subpackage stubs (platform rejects empty files)"
for stub in "$NATIVE_SRC"/wasmcode/game.js "$NATIVE_SRC"/data-package/game.js; do
  if [[ -f "$stub" ]] && [[ ! -s "$stub" ]]; then
    printf '// subpackage entry\n' > "$stub"
  fi
done

echo "[pack-native] 4/4 Validate + zip (game.json at archive root)"
empty_files="$(find "$NATIVE_SRC" -type f -size 0 ! -name '.DS_Store' 2>/dev/null || true)"
if [[ -n "$empty_files" ]]; then
  echo "$empty_files" >&2
  die "Found zero-byte files"
fi

rm -f "$ZIP_OUT"
(
  cd "$NATIVE_SRC"
  zip -r "$ZIP_OUT" . \
    -x "*.DS_Store" \
    -x "__TTMG_TEMP__/*"
)

size_mb="$(du -m "$ZIP_OUT" | awk '{print $1}')"
echo "[pack-native] Done."
echo "  Upload zip: $ZIP_OUT (${size_mb} MB)"
echo "  Portal type: Native (not H5)"
if (( size_mb > 200 )); then
  echo "[pack-native] WARN: zip exceeds 200 MB platform limit" >&2
fi

# Verify game.json is at zip root (not nested in a subfolder)
if ! unzip -l "$ZIP_OUT" | awk '{print $4}' | grep -qx 'game.json'; then
  die "game.json not at zip root"
fi
