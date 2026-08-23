/**
 * Platform.ts — TikTok mini-game platform abstraction.
 *
 * Real TikTok runtime exposes `TTMinis.game.*` (web) or `tt.*` (native).
 * In the Cocos editor / browser preview none of that exists, so every call
 * falls back to a mock so the game logic stays fully testable off-device.
 *
 * Reference: TikTok 排行榜接入指南 / IM 分享能力接入 (open data domain model).
 */

type AnyObj = { [k: string]: any };

/** Returns the TikTok API object (TTMinis.game or tt), or null in preview. */
function api(): AnyObj | null {
    const g = globalThis as AnyObj;
    if (g.TTMinis && g.TTMinis.game) return g.TTMinis.game;
    if (g.tt) return g.tt;
    return null;
}

export function isTikTok(): boolean {
    return api() !== null;
}

/**
 * Show a rewarded video ad. Resolves true when the user watched to the end
 * (reward granted). In preview, simulates a completed ad after a short delay.
 * @param adUnitId Ad slot id configured in the TikTok dev portal.
 */
export function showRewardedAd(adUnitId: string): Promise<boolean> {
    const a = api();
    if (!a || typeof a.createRewardedVideoAd !== 'function') {
        return new Promise(res => setTimeout(() => res(true), 400)); // mock: watched
    }
    return new Promise(resolve => {
        let done = false;
        const finish = (ok: boolean) => { if (!done) { done = true; resolve(ok); } };
        try {
            const ad = a.createRewardedVideoAd({ adUnitId });
            ad.onClose((res: AnyObj) => finish(!res || res.isEnded !== false));
            if (ad.onError) ad.onError(() => finish(false));
            Promise.resolve(ad.load ? ad.load() : null)
                .then(() => (ad.show ? ad.show() : null))
                .catch(() => finish(false));
        } catch (e) {
            finish(false);
        }
    });
}

/** Whether IM share is available (client >= 40.3.0). Preview: always true. */
export function canShare(): boolean {
    const a = api();
    if (!a) return true;
    if (typeof a.canIUse === 'function') return !!a.canIUse('shareAppMessage');
    return typeof a.shareAppMessage === 'function';
}

export interface ShareOptions {
    title: string;
    subtitle?: string;
    imageUrl?: string;
    query?: string;
    templateType?: number; // 1 | 2
}

/** Share to a TikTok IM chat. Preview: logs and resolves true. */
export function shareAppMessage(opts: ShareOptions): Promise<boolean> {
    const a = api();
    if (!a || typeof a.shareAppMessage !== 'function') {
        console.log('[WordTT][mock share]', opts.title, '|', opts.subtitle || '');
        return Promise.resolve(true);
    }
    return new Promise(resolve => {
        a.shareAppMessage({
            templateType: opts.templateType == null ? 1 : opts.templateType,
            title: opts.title,
            subtitle: opts.subtitle,
            imageUrl: opts.imageUrl,
            query: opts.query,
            success: () => resolve(true),
            fail: () => resolve(false),
        });
    });
}

// ---- Retention: add-to-desktop shortcut ------------------------------------

/** Create a home-screen shortcut for the mini game. Preview: mock success. */
export function addShortcut(): Promise<boolean> {
    const a = api();
    if (!a || typeof a.addShortcut !== 'function') {
        console.log('[WordTT][mock addShortcut]');
        return Promise.resolve(true);
    }
    return new Promise(resolve => {
        a.addShortcut({
            success: () => resolve(true),
            fail: () => resolve(false),
        });
    });
}

/** Whether a shortcut already exists. Preview: false. */
export function checkShortcut(): Promise<boolean> {
    const a = api();
    if (!a || typeof a.checkShortcut !== 'function') return Promise.resolve(false);
    return new Promise(resolve => {
        a.checkShortcut({
            success: (res: AnyObj) => resolve(!!(res && res.status && res.status.exist)),
            fail: () => resolve(false),
        });
    });
}

/** Whether the sidebar entry is available for this user. Preview: false. */
export function checkSidebar(): Promise<boolean> {
    const a = api();
    if (!a || typeof a.checkScene !== 'function') return Promise.resolve(false);
    return new Promise(resolve => {
        a.checkScene({
            scene: 'sidebar',
            success: (res: AnyObj) => resolve(!!(res && res.isExist)),
            fail: () => resolve(false),
        });
    });
}

/**
 * Open TikTok's profile-sidebar flow (All-in-One guide §3.2).
 * Primary API: startEntranceMission. Falls back to navigateToScene('sidebar').
 * Preview (no TikTok SDK): mock success.
 */
export function navigateToSidebar(): Promise<boolean> {
    const a = api();
    if (!a) {
        console.log('[WordTT][mock startEntranceMission]');
        return Promise.resolve(true);
    }
    return new Promise(resolve => {
        let done = false;
        const finish = (ok: boolean) => { if (!done) { done = true; resolve(ok); } };
        try {
            const canMission = typeof a.canIUse !== 'function' || !!a.canIUse('startEntranceMission');
            if (canMission && typeof a.startEntranceMission === 'function') {
                a.startEntranceMission({
                    success: () => finish(true),
                    fail: () => tryNavigateScene(a, finish),
                    complete: () => { /* no-op */ },
                });
                return;
            }
            tryNavigateScene(a, finish);
        } catch (e) {
            finish(false);
        }
    });
}

function tryNavigateScene(a: AnyObj, finish: (ok: boolean) => void) {
    if (typeof a.navigateToScene !== 'function') { finish(false); return; }
    a.navigateToScene({
        scene: 'sidebar',
        success: () => finish(true),
        fail: () => finish(false),
    });
}

// ---- Leaderboard (open data domain) ----------------------------------------

/** Ask the user to authorize avatar/nickname + friend relationship. */
export function authorizeOpenContext(): Promise<boolean> {
    const a = api();
    if (!a || typeof a.authorizeOpenContext !== 'function') return Promise.resolve(true);
    return new Promise(resolve => {
        a.authorizeOpenContext({
            get_status_only: false,
            success: () => resolve(true),
            fail: () => resolve(false),
            complete: () => { /* no-op */ },
        });
    });
}

/** Write the player's own score into cloud storage (main domain). */
export function setUserCloudStorage(kv: { key: string; value: string }[]): Promise<boolean> {
    const a = api();
    if (!a || typeof a.setUserCloudStorage !== 'function') {
        console.log('[WordTT][mock setUserCloudStorage]', JSON.stringify(kv));
        return Promise.resolve(true);
    }
    return new Promise(resolve => {
        a.setUserCloudStorage({
            data: kv,
            success: () => resolve(true),
            fail: () => resolve(false),
        });
    });
}

/** Post a message to the open data context (which renders the friend board). */
export function postToOpenData(msg: AnyObj): boolean {
    const a = api();
    if (!a || typeof a.getOpenDataContext !== 'function') return false;
    try {
        a.getOpenDataContext().postMessage(msg);
        return true;
    } catch (e) {
        return false;
    }
}
