#!/bin/bash
# Generate one daily puzzle JSON for CDN upload.
# Example: ./scripts/generate-daily.sh 20260709 hard
set -euo pipefail
cd "$(dirname "$0")/.."
DATE="${1:?usage: generate-daily.sh YYYYMMDD [easy|normal|hard]}"
DIFF="${2:-hard}"
OUT_DIR="${3:-out/dailies}"
mkdir -p "$OUT_DIR"
python3 tools/generate_levels.py daily --date "$DATE" --size 8 --difficulty "$DIFF" \
  -o "$OUT_DIR/${DATE}.json"
echo "Upload $OUT_DIR/${DATE}.json to your CDN (BordyDailyService.BaseUrl)."
