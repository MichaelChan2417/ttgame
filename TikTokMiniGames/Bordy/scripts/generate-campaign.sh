#!/bin/bash
# Monetization curve: 5 easy hook levels + hard tail. Override: ./scripts/generate-campaign.sh 30 5
set -euo pipefail
cd "$(dirname "$0")/.."
COUNT="${1:-30}"
HOOK="${2:-5}"
python3 tools/generate_levels.py campaign --count "$COUNT" --hook-count "$HOOK" \
  -o Assets/Bordy/Resources/Bordy/campaign-levels.json
echo "Done. hook=$HOOK hard=$((COUNT - HOOK)). Commit JSON and rebuild."
