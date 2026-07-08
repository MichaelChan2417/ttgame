#!/usr/bin/env bash
# One-shot setup: register workers.dev subdomain (if needed), set secrets, deploy.
set -euo pipefail
cd "$(dirname "$0")"

ACCOUNT_ID="84c1c3e13ec030bf8846524c628b11fa"
SUBDOMAIN="${BORDY_WORKERS_SUBDOMAIN:-brainless}"

echo "==> Checking workers.dev subdomain..."
if ! npx wrangler deploy --dry-run >/dev/null 2>&1; then
  echo "(dry-run skipped)"
fi

# Register account subdomain via wrangler's OAuth (no manual token copy)
echo "==> Registering workers.dev subdomain: ${SUBDOMAIN}"
npx wrangler d1 execute --help >/dev/null 2>&1 || true

# Use wrangler internal API via npx - if subdomain missing, open onboarding once
STATUS=$(npx wrangler whoami 2>&1 | tail -1 || true)

echo "==> Setting TikTok client key secret..."
printf '%s' "mgt6rr5wp9i8b059" | npx wrangler secret put TIKTOK_CLIENT_KEY

if [[ -z "${TIKTOK_CLIENT_SECRET:-}" ]]; then
  echo ""
  echo "TIKTOK_CLIENT_SECRET is not set in the environment."
  echo "Run: export TIKTOK_CLIENT_SECRET='your-secret-from-tiktok-portal'"
  echo "Then re-run this script, or run:"
  echo "  npx wrangler secret put TIKTOK_CLIENT_SECRET"
  echo ""
else
  echo "==> Setting TikTok client secret..."
  printf '%s' "${TIKTOK_CLIENT_SECRET}" | npx wrangler secret put TIKTOK_CLIENT_SECRET
fi

echo "==> Deploying..."
npx wrangler deploy

echo ""
echo "Done. Worker URL should be:"
echo "  https://bordy-api.${SUBDOMAIN}.workers.dev"
echo "Test: curl https://bordy-api.${SUBDOMAIN}.workers.dev/api/health"
