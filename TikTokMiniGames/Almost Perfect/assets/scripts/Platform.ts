type AnyObj = { [k: string]: any };

/** TikTok mini-game API (`TTMinis.game` or `tt`). Null in Cocos preview. */
export function tikTokApi(): AnyObj | null {
    const g = globalThis as AnyObj;
    if (g.TTMinis && g.TTMinis.game) {
        return g.TTMinis.game;
    }
    if (g.tt) {
        return g.tt;
    }
    return null;
}

export function isTikTok(): boolean {
    return tikTokApi() !== null;
}

/**
 * Host app language from TikTok, e.g. "en", "zh-Hans", "ja".
 * Empty in editor / browser preview.
 */
export function readTikTokLanguage(): string {
    const a = tikTokApi();
    if (!a) {
        return "";
    }
    try {
        if (typeof a.getSystemInfoSync === "function") {
            const info = a.getSystemInfoSync() || {};
            const lang = info.language || info.languageCode || info.appLanguage || "";
            return String(lang).trim();
        }
    } catch (err) {
        console.warn("[AlmostPerfect] getSystemInfoSync failed", err);
    }
    return "";
}
