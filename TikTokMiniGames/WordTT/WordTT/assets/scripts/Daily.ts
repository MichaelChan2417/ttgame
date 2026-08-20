/**
 * Daily.ts — deterministic daily challenge.
 *
 * Everyone gets the same word on the same (UTC) day, derived purely from a
 * date hash — no server needed. The puzzle number is the day index since a
 * fixed launch date.
 */

import { ANSWER_WORDS } from './WordList';

const DAY_MS = 86400000;
/** Launch/base date (UTC midnight) used for puzzle numbering. */
const BASE_UTC = Date.UTC(2024, 0, 1); // 2024-01-01

/** Whole days elapsed (UTC) since the base date. */
export function dayNumber(now?: Date): number {
    const d = now || new Date();
    const todayUTC = Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate());
    return Math.floor((todayUTC - BASE_UTC) / DAY_MS);
}

/** Stable 32-bit hash of a day index (FNV-1a over a salted string). */
export function dailyHash(n: number): number {
    let h = 2166136261 >>> 0;
    const s = 'WordTT-daily-' + n;
    for (let i = 0; i < s.length; i++) {
        h ^= s.charCodeAt(i);
        h = Math.imul(h, 16777619) >>> 0;
    }
    return h >>> 0;
}

/** The puzzle number shown to players (e.g. WordTT #123). */
export function puzzleId(now?: Date): number {
    return dayNumber(now);
}

/** Today's answer word (lowercase), identical for all players on this UTC day. */
export function dailyWord(now?: Date): string {
    const idx = dailyHash(dayNumber(now)) % ANSWER_WORDS.length;
    return ANSWER_WORDS[idx].toLowerCase();
}
