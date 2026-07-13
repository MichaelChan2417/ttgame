/**
 * Bordy cloud API — TikTok silent login + per-open_id save storage.
 *
 * POST /api/auth/login  { code }  → openId, sessionToken, save, isNewUser
 * GET  /api/save        Authorization: Bearer <token>
 * PUT  /api/save        Authorization: Bearer <token>  body = save JSON
 * GET  /api/health
 */

export interface Env {
  BORDY_KV: KVNamespace;
  TIKTOK_CLIENT_KEY: string;
  TIKTOK_CLIENT_SECRET: string;
}

interface DailySave {
  completedDate: string;
  completedSeconds: number;
  completedBoard: string;
  progressDate: string;
  progressBoard: string;
  progressSeconds: number;
}

interface UserSave {
  openId: string;
  createdAt: number;
  lastSeenAt: number;
  playCount: number;
  tutorialCompleted: boolean;
  campaignHighestUnlocked: number;
  locale: string;
  daily: DailySave;
}

interface SessionRecord {
  openId: string;
  expiresAt: number;
}

const SESSION_TTL_SEC = 7 * 24 * 3600;
const CORS_HEADERS: Record<string, string> = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, POST, PUT, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type, Authorization",
};

function json(data: unknown, status = 200): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: { "Content-Type": "application/json", ...CORS_HEADERS },
  });
}

function error(message: string, status = 400): Response {
  return json({ error: message }, status);
}

function defaultSave(openId: string): UserSave {
  const now = Math.floor(Date.now() / 1000);
  return {
    openId,
    createdAt: now,
    lastSeenAt: now,
    playCount: 0,
    tutorialCompleted: false,
    campaignHighestUnlocked: 1,
    locale: "en",
    daily: {
      completedDate: "",
      completedSeconds: 0,
      completedBoard: "",
      progressDate: "",
      progressBoard: "",
      progressSeconds: 0,
    },
  };
}

function randomToken(): string {
  const bytes = new Uint8Array(24);
  crypto.getRandomValues(bytes);
  return Array.from(bytes, (b) => b.toString(16).padStart(2, "0")).join("");
}

async function exchangeCode(env: Env, code: string): Promise<string> {
  const body = new URLSearchParams({
    client_key: env.TIKTOK_CLIENT_KEY,
    client_secret: env.TIKTOK_CLIENT_SECRET,
    code,
    grant_type: "authorization_code",
  });

  const res = await fetch("https://open.tiktokapis.com/v2/oauth/token/", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      "Cache-Control": "no-cache",
    },
    body,
  });

  const text = await res.text();
  let parsed: Record<string, unknown>;
  try {
    parsed = JSON.parse(text);
  } catch {
    throw new Error(`TikTok token response not JSON: ${text.slice(0, 200)}`);
  }

  if (!res.ok) {
    const msg =
      (parsed.error_description as string) ||
      (parsed.message as string) ||
      (parsed.error as string) ||
      text.slice(0, 200);
    throw new Error(`TikTok token HTTP ${res.status}: ${msg}`);
  }

  const data = (parsed.data as Record<string, unknown>) ?? parsed;
  const openId = data.open_id as string;
  if (!openId) throw new Error("TikTok token response missing open_id");
  return openId;
}

async function createSession(env: Env, openId: string): Promise<string> {
  const token = randomToken();
  const record: SessionRecord = {
    openId,
    expiresAt: Math.floor(Date.now() / 1000) + SESSION_TTL_SEC,
  };
  await env.BORDY_KV.put(`session:${token}`, JSON.stringify(record), {
    expirationTtl: SESSION_TTL_SEC,
  });
  return token;
}

async function resolveSession(env: Env, authHeader: string | null): Promise<string | null> {
  if (!authHeader?.startsWith("Bearer ")) return null;
  const token = authHeader.slice(7).trim();
  if (!token) return null;

  const raw = await env.BORDY_KV.get(`session:${token}`);
  if (!raw) return null;

  const record = JSON.parse(raw) as SessionRecord;
  if (record.expiresAt < Math.floor(Date.now() / 1000)) return null;
  return record.openId;
}

async function handleLogin(request: Request, env: Env): Promise<Response> {
  let body: { code?: string };
  try {
    body = await request.json();
  } catch {
    return error("Invalid JSON body");
  }

  const code = body.code?.trim();
  if (!code) return error("Missing code");

  let openId: string;
  try {
    openId = await exchangeCode(env, code);
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    console.error("[login] TikTok exchange failed:", msg);
    return error(msg, 502);
  }

  const userKey = `user:${openId}`;
  let save = await env.BORDY_KV.get(userKey, "json") as UserSave | null;
  const isNewUser = !save;

  if (!save) {
    save = defaultSave(openId);
    await env.BORDY_KV.put(userKey, JSON.stringify(save));
  } else {
    save.lastSeenAt = Math.floor(Date.now() / 1000);
    await env.BORDY_KV.put(userKey, JSON.stringify(save));
  }

  const sessionToken = await createSession(env, openId);

  return json({
    openId,
    sessionToken,
    save,
    isNewUser,
  });
}

async function handleGetSave(request: Request, env: Env): Promise<Response> {
  const openId = await resolveSession(env, request.headers.get("Authorization"));
  if (!openId) return error("Unauthorized", 401);

  const save = await env.BORDY_KV.get(`user:${openId}`, "json");
  if (!save) return error("Save not found", 404);
  return json({ save });
}

async function handlePutSave(request: Request, env: Env): Promise<Response> {
  const openId = await resolveSession(env, request.headers.get("Authorization"));
  if (!openId) return error("Unauthorized", 401);

  let incoming: Partial<UserSave>;
  try {
    incoming = await request.json();
  } catch {
    return error("Invalid JSON body");
  }

  const existing =
    ((await env.BORDY_KV.get(`user:${openId}`, "json")) as UserSave | null) ??
    defaultSave(openId);

  const merged: UserSave = {
    openId,
    createdAt: existing.createdAt ?? incoming.createdAt ?? Math.floor(Date.now() / 1000),
    lastSeenAt: incoming.lastSeenAt ?? Math.floor(Date.now() / 1000),
    playCount: incoming.playCount ?? existing.playCount ?? 0,
    tutorialCompleted: incoming.tutorialCompleted ?? existing.tutorialCompleted ?? false,
    campaignHighestUnlocked:
      incoming.campaignHighestUnlocked ?? existing.campaignHighestUnlocked ?? 1,
    locale: incoming.locale ?? existing.locale ?? "en",
    daily: {
      completedDate: incoming.daily?.completedDate ?? existing.daily?.completedDate ?? "",
      completedSeconds: incoming.daily?.completedSeconds ?? existing.daily?.completedSeconds ?? 0,
      completedBoard: incoming.daily?.completedBoard ?? existing.daily?.completedBoard ?? "",
      progressDate: incoming.daily?.progressDate ?? existing.daily?.progressDate ?? "",
      progressBoard: incoming.daily?.progressBoard ?? existing.daily?.progressBoard ?? "",
      progressSeconds: incoming.daily?.progressSeconds ?? existing.daily?.progressSeconds ?? 0,
    },
  };

  await env.BORDY_KV.put(`user:${openId}`, JSON.stringify(merged));
  return json({ ok: true, save: merged });
}

async function handleGetDaily(env: Env, path: string): Promise<Response> {
  // path looks like: /api/daily/20260613.json
  const file = path.slice("/api/daily/".length); // "20260613.json"
  const date = file.replace(/\.json$/, ""); // "20260613"
  if (!/^\d{8}$/.test(date)) return error("Bad date (expected YYYYMMDD)", 400);

  const json = await env.BORDY_KV.get(`daily:${date}`);
  if (!json) return error("Daily not found for " + date, 404);

  // Return the stored JSON verbatim (already the BordyDailyDto shape) + CORS + cache.
  return new Response(json, {
    status: 200,
    headers: {
      "Content-Type": "application/json",
      "Cache-Control": "public, max-age=3600",
      ...CORS_HEADERS,
    },
  });
}

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    if (request.method === "OPTIONS") {
      return new Response(null, { status: 204, headers: CORS_HEADERS });
    }

    const url = new URL(request.url);
    const path = url.pathname;

    try {
      if (request.method === "GET" && (path === "/" || path === "")) {
        return json({
          ok: true,
          service: "bordy-api",
          hint: "Use GET /api/health for health check",
          endpoints: ["/api/health", "/api/auth/login", "/api/save"],
        });
      }

      if (request.method === "GET" && path === "/api/health") {
        return json({ ok: true, service: "bordy-api" });
      }

      if (request.method === "POST" && path === "/api/auth/login") {
        return await handleLogin(request, env);
      }

      if (request.method === "GET" && path === "/api/save") {
        return await handleGetSave(request, env);
      }

      if (request.method === "PUT" && path === "/api/save") {
        return await handlePutSave(request, env);
      }

      if (request.method === "GET" && path.startsWith("/api/daily/")) {
        return await handleGetDaily(env, path);
      }

      return error("Not found", 404);
    } catch (e) {
      console.error("[bordy-api] unhandled:", e);
      return error("Internal error", 500);
    }
  },
};
