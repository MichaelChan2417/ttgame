# Bordy Cloud API (Cloudflare Workers)

TikTok silent login + per-`open_id` save storage for Bordy.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/health` | Health check |
| POST | `/api/auth/login` | Body `{ "code": "<TT.Login code>" }` → `openId`, `sessionToken`, `save`, `isNewUser` |
| GET | `/api/save` | Header `Authorization: Bearer <token>` |
| PUT | `/api/save` | Header `Authorization: Bearer <token>` + save JSON body |

## One-time setup

```bash
cd TikTokMiniGames/Bordy/cloudflare/bordy-api
npm install

# Create KV namespace and paste the id into wrangler.toml
wrangler kv namespace create BORDY_KV

# Store TikTok credentials (from developers.tiktok.com → App → Credentials)
wrangler secret put TIKTOK_CLIENT_KEY
# → mgt6rr5wp9i8b059

wrangler secret put TIKTOK_CLIENT_SECRET
# → paste Client secret (never put in Unity)

# Deploy
npm run deploy
```

After deploy you get a URL like `https://bordy-api.<subdomain>.workers.dev`.

**Current production URL:** `https://bordy-api.brainless.workers.dev`

## Wire into Unity

Edit `Assets/Bordy/Scripts/BordyAppConfig.cs`:

```csharp
public const string ApiBaseUrl = "https://bordy-api.<account>.workers.dev";
```

Rebuild WebGL with **Window → TTSDK → Build Tool**.

## TikTok Developer Portal

**Security configurations** → add your Workers URL as a trusted request domain:

```
https://bordy-api.<account>.workers.dev
```

Without this, the mini-game container will block `UnityWebRequest` to your API.

## Test health

```bash
curl https://bordy-api.<account>.workers.dev/api/health
# {"ok":true,"service":"bordy-api"}
```

Login can only be tested inside the TikTok mini-game container (real `TT.Login` code).

## What gets stored per user

```json
{
  "openId": "tiktok-open-id",
  "tutorialCompleted": false,
  "locale": "en",
  "playCount": 1,
  "daily": { "completedDate": "", ... }
}
```

## Local development

- Leave `ApiBaseUrl = ""` in `BordyAppConfig.cs` → cloud disabled, local `TT.PlayerPrefs` only (Unity Editor behaviour).
- Cloud login runs only in **WebGL builds inside TikTok container**, not in Editor.
