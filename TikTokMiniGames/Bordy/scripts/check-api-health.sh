#!/usr/bin/env bash
# Health-check Bordy API, bypassing poisoned local DNS for *.workers.dev (common in CN).
set -euo pipefail

HOST="${BORDY_API_HOST:-bordy-api.brainless.workers.dev}"
PATH_SUFFIX="${BORDY_API_PATH:-/api/health}"
URL="https://${HOST}${PATH_SUFFIX}"

resolve_ip() {
  python3 - "$HOST" <<'PY'
import json, sys, urllib.request
host = sys.argv[1]
q = f"https://1.1.1.1/dns-query?name={host}&type=A"
req = urllib.request.Request(q, headers={"accept": "application/dns-json"})
with urllib.request.urlopen(req, timeout=10) as r:
    data = json.load(r)
ips = [a["data"] for a in data.get("Answer", []) if a.get("type") == 1]
if not ips:
    raise SystemExit(f"No A record for {host}")
print(ips[0])
PY
}

echo "[check-api] URL: $URL"

if curl -sS --max-time 12 "$URL" 2>/dev/null; then
  echo
  exit 0
fi

echo "[check-api] Direct curl failed — retry with Cloudflare DNS (1.1.1.1) IP…" >&2
IP="$(resolve_ip)"
echo "[check-api] Resolved via DoH: $HOST -> $IP" >&2

curl -sS --max-time 12 --resolve "${HOST}:443:${IP}" "$URL"
echo
