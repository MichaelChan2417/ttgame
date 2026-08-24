export const FISH_COUNT = 8;

/** Hidden lengths in cm. Same-looking toy fish, different stretch. */
export const FISH_LENGTHS_CM = [18, 24, 31, 38, 46, 55, 67, 82] as const;

export type PlayerId = 0 | 1;

export interface ToyFish {
    id: number;
    lengthCm: number;
    hue: number;
    caughtBy: PlayerId | null;
}

export interface MatchResult {
    winner: PlayerId | "tie";
    p1Longest: number;
    p2Longest: number;
    p1Total: number;
    p2Total: number;
}

export function shuffleInPlace<T>(items: T[]): T[] {
    for (let i = items.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        const tmp = items[i];
        items[i] = items[j];
        items[j] = tmp;
    }
    return items;
}

export function createMatchFish(): ToyFish[] {
    const lengths = shuffleInPlace([...FISH_LENGTHS_CM]);
    return lengths.map((lengthCm, id) => ({
        id,
        lengthCm,
        hue: (id * 47 + 18) % 360,
        caughtBy: null,
    }));
}

export function longestOf(fish: ToyFish[], player: PlayerId): number {
    return fish
        .filter((f) => f.caughtBy === player)
        .reduce((best, f) => Math.max(best, f.lengthCm), 0);
}

export function totalOf(fish: ToyFish[], player: PlayerId): number {
    return fish
        .filter((f) => f.caughtBy === player)
        .reduce((sum, f) => sum + f.lengthCm, 0);
}

export function judgeMatch(fish: ToyFish[]): MatchResult {
    const p1Longest = longestOf(fish, 0);
    const p2Longest = longestOf(fish, 1);
    const p1Total = totalOf(fish, 0);
    const p2Total = totalOf(fish, 1);
    let winner: PlayerId | "tie" = "tie";
    if (p1Longest > p2Longest) winner = 0;
    else if (p2Longest > p1Longest) winner = 1;
    return { winner, p1Longest, p2Longest, p1Total, p2Total };
}

export function playerName(player: PlayerId): string {
    return player === 0 ? "PLAYER 1" : "PLAYER 2";
}
