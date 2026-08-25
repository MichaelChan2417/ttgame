import { sys } from "cc";
import { readTikTokLanguage, tikTokApi } from "../Platform";

export type Lang = "en" | "zh" | "ja";

const STORE_KEY = "almostperfect.locale";

let current: Lang = "en";

export function getLang(): Lang {
    return current;
}

export function initLocale(): Lang {
    const saved = safeGet(STORE_KEY);
    if (saved) {
        current = parseLang(saved);
        return current;
    }
    const host = readTikTokLanguage();
    if (host) {
        current = parseLang(host);
        console.log("[AlmostPerfect] TikTok language=", host, "→", current);
        return current;
    }
    current = "en";
    return current;
}

export function setLang(lang: Lang): void {
    current = lang;
    try {
        sys.localStorage.setItem(STORE_KEY, lang);
    } catch {
        /* ignore quota / private mode */
    }
}

export function followTikTokLanguageAsync(onChange: (lang: Lang) => void): void {
    if (safeGet(STORE_KEY)) {
        return;
    }
    const a = tikTokApi();
    if (!a || typeof a.getSystemInfo !== "function") {
        return;
    }
    try {
        a.getSystemInfo({
            success: (info: { language?: string }) => {
                if (safeGet(STORE_KEY)) {
                    return;
                }
                const next = parseLang((info && info.language) || "");
                if (next !== current) {
                    current = next;
                    onChange(next);
                }
            },
        });
    } catch {
        /* preview / missing API */
    }
}

export function parseLang(raw: string): Lang {
    if (!raw) {
        return "en";
    }
    const code = raw.trim().replace(/_/g, "-").toLowerCase();
    if (code.startsWith("zh")) {
        return "zh";
    }
    if (code.startsWith("ja")) {
        return "ja";
    }
    return "en";
}

function safeGet(key: string): string {
    try {
        return sys.localStorage.getItem(key) || "";
    } catch {
        return "";
    }
}
