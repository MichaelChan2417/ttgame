#!/usr/bin/env bash
# Post-process Unity WebGL output for TikTok International Minis upload.
# Run after TikTokGame → Build Minigame (output: tt-minigame/webgl/).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WEBGL="$ROOT/tt-minigame/webgl"
CONFIG_SRC="$ROOT/minigame.config.json"
CLIENT_KEY="${BORDY_CLIENT_KEY:-mgt6rr5wp9i8b059}"
ZIP_OUT="$ROOT/tt-minigame/bordy-upload.zip"

die() { echo "[post-build] ERROR: $*" >&2; exit 1; }

command -v ttdx >/dev/null 2>&1 || die "ttdx not found. Run: npm install -g @tiktok-minis/cli@latest"
[[ -d "$WEBGL" ]] || die "Missing $WEBGL — run Unity TikTokGame → Build Minigame first."
[[ -f "$WEBGL/index.html" ]] || die "Missing $WEBGL/index.html"
[[ -f "$CONFIG_SRC" ]] || die "Missing $CONFIG_SRC"

echo "[post-build] 1/4 Copy minigame.config.json"
cp "$CONFIG_SRC" "$WEBGL/minigame.config.json"

echo "[post-build] 2/4 Inject Mini Games SDK into index.html"
python3 - "$WEBGL/index.html" "$CLIENT_KEY" <<'PY'
import sys
from pathlib import Path

path = Path(sys.argv[1])
key = sys.argv[2]
html = path.read_text(encoding="utf-8")
marker = "<!-- BORDY_TTMINIS_SDK -->"

if marker in html:
    print("[post-build] SDK already injected — skip")
    sys.exit(0)

if "connect.tiktok-minis.com/game/sdk.js" in html:
    print("[post-build] SDK script tag already present — skip inject")
    sys.exit(0)

inject = f"""{marker}
    <script src="https://connect.tiktok-minis.com/game/sdk.js"></script>
    <script>
      TTMinis.game.init({{
        clientKey: "{key}",
      }});
    </script>
"""

if "<head>" not in html:
    raise SystemExit("index.html has no <head> tag")

html = html.replace("<head>", "<head>\n" + inject, 1)

progress_hook = """
          if (window.TTMinis && TTMinis.game && TTMinis.game.setLoadingProgress) {
            TTMinis.game.setLoadingProgress({ progress: progress });
          }"""

if progress_hook.strip() not in html:
    html = html.replace(
        'progressBarFull.style.width = 100 * progress + "%";',
        'progressBarFull.style.width = 100 * progress + "%";' + progress_hook,
        1,
    )

done_hook = """
                if (window.TTMinis && TTMinis.game && TTMinis.game.setLoadingProgress) {
                  TTMinis.game.setLoadingProgress({ progress: 1 });
                }"""

if done_hook.strip() not in html:
    html = html.replace(
        "loadingBar.style.display = \"none\";",
        done_hook + "\n                loadingBar.style.display = \"none\";",
        1,
    )

path.write_text(html, encoding="utf-8")
print("[post-build] SDK injected")
PY

echo "[post-build] 3/4 ttdx minigame build:after"
(cd "$WEBGL" && ttdx minigame build:after)

echo "[post-build] 4/4 Validate + zip"
empty_files="$(find "$WEBGL" -type f -size 0 ! -name '.DS_Store' 2>/dev/null || true)"
if [[ -n "$empty_files" ]]; then
  echo "$empty_files" >&2
  die "Found zero-byte files (platform rejects these)"
fi

[[ -f "$WEBGL/minigame.config.json" ]] || die "minigame.config.json missing after build:after"
if [[ ! -f "$WEBGL/minigame.manifest.json" && ! -f "$WEBGL/minis.manifest.json" ]]; then
  die "manifest json missing — build:after failed?"
fi

rm -f "$ZIP_OUT"
(
  cd "$WEBGL"
  zip -r "$ZIP_OUT" . -x "*.DS_Store" -x "Build/webgl.symbols.json"
)

size_mb="$(du -m "$ZIP_OUT" | awk '{print $1}')"
echo "[post-build] Done."
echo "  Upload zip: $ZIP_OUT (${size_mb} MB)"
if (( size_mb > 200 )); then
  echo "[post-build] WARN: zip exceeds 200 MB platform limit" >&2
fi
