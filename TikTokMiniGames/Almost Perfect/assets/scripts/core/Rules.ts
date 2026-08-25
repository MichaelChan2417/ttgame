export const SLOT_COUNT = 5;
export const ROUND_COUNT = 3;

export interface BottleColor {
    id: number;
    name: string;
    r: number;
    g: number;
    b: number;
}

export const BOTTLE_PALETTE: BottleColor[] = [
    { id: 0, name: "红", r: 232, g: 78, b: 92 },
    { id: 1, name: "橙", r: 255, g: 148, b: 64 },
    { id: 2, name: "黄", r: 246, g: 208, b: 72 },
    { id: 3, name: "绿", r: 64, g: 186, b: 124 },
    { id: 4, name: "蓝", r: 72, g: 140, b: 236 },
];

export function shuffleInPlace<T>(items: T[]): T[] {
    for (let i = items.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        const tmp = items[i];
        items[i] = items[j];
        items[j] = tmp;
    }
    return items;
}

export function idsOfPalette(): number[] {
    return BOTTLE_PALETTE.map((c) => c.id);
}

/** Hidden target: one of each bottle, shuffled. */
export function createSecret(): number[] {
    return shuffleInPlace(idsOfPalette());
}

/** Starting table: a different permutation so the first check is rarely perfect. */
export function createStartingGuess(secret: number[]): number[] {
    let guess = shuffleInPlace(idsOfPalette());
    let guard = 0;
    while (countMatches(secret, guess) === SLOT_COUNT && guard < 12) {
        guess = shuffleInPlace(idsOfPalette());
        guard += 1;
    }
    return guess;
}

/** Only exact position matches. Host never says which slots. */
export function countMatches(secret: number[], guess: number[]): number {
    let n = 0;
    const len = Math.min(secret.length, guess.length);
    for (let i = 0; i < len; i++) {
        if (secret[i] === guess[i]) {
            n += 1;
        }
    }
    return n;
}

export function isPerfect(secret: number[], guess: number[]): boolean {
    return countMatches(secret, guess) === SLOT_COUNT;
}

export function swap<T>(items: T[], a: number, b: number): void {
    const tmp = items[a];
    items[a] = items[b];
    items[b] = tmp;
}

export interface Attempt {
    guess: number[];
    correct: number;
}
