#!/usr/bin/env python3
"""
Bordy puzzle generator — offline dev/ops tool (NOT part of the Unity runtime).

Usage:
  # Campaign batch → baked into the game package
  python3 tools/generate_levels.py campaign --count 30 -o Assets/Bordy/Resources/Bordy/campaign-levels.json

  # One daily puzzle → upload to CDN as YYYYMMDD.json
  python3 tools/generate_levels.py daily --date 20260709 --size 8 --difficulty hard -o out/20260709.json

Run from TikTokMiniGames/Bordy/ (repo root for this game).
"""
from __future__ import annotations

import argparse
import json
import random
import sys
from pathlib import Path

SUN, MOON, EMPTY = 0, 1, -1


def clamp01(v: float) -> float:
    return max(0.0, min(1.0, v))


def lerp(a: float, b: float, t: float) -> float:
    return a + (b - a) * clamp01(t)


def config_for_campaign_level(index: int, total: int, hook_count: int = 5) -> tuple[int, float, float, str]:
    """
    Monetization curve:
      1..hook_count  — hook: most players clear without hints (retention / teach rules)
      hook+1..total  — hard: few players solve by logic alone (hints / ads)
    index is 1-based.
    """
    if index <= hook_count:
        # 6×6, generous givens + visible edge clues
        t = 0 if hook_count <= 1 else (index - 1) / (hook_count - 1)
        given = lerp(0.62, 0.52, t)
        edge = lerp(0.22, 0.28, t)
        return 6, given, edge, "hook"

    hard_index = index - hook_count
    hard_total = max(1, total - hook_count)
    t = (hard_index - 1) / max(1, hard_total - 1)

    if t < 0.70:
        size = 8
        given = lerp(0.32, 0.24, t / 0.70)
        edge = lerp(0.16, 0.10, t / 0.70)
        tier = "hard"
    else:
        # brutal: still 8×8 but very few givens + sparse edges (fast to generate, very hard to play)
        size = 8
        given = lerp(0.24, 0.20, (t - 0.70) / 0.30)
        edge = lerp(0.10, 0.07, (t - 0.70) / 0.30)
        tier = "brutal"

    return size, given, edge, tier


# Legacy smooth curve (daily presets still use this style)
def config_for_difficulty(t: float) -> tuple[int, float, float]:
    t = clamp01(t)
    if t < 0.40:
        return 6, lerp(0.50, 0.38, t / 0.40), lerp(0.12, 0.22, t / 0.40)
    if t < 0.75:
        return 8, lerp(0.42, 0.30, (t - 0.40) / 0.35), lerp(0.16, 0.26, (t - 0.40) / 0.35)
    return 8, lerp(0.34, 0.28, (t - 0.75) / 0.25), lerp(0.20, 0.28, (t - 0.75) / 0.25)


def config_for_daily(size: int, difficulty: str) -> tuple[float, float]:
    # Daily = engagement + hint pressure; default hard
    presets = {
        "easy": (0.45, 0.16),
        "normal": (0.34, 0.18),
        "hard": (0.26, 0.11),
    }
    return presets.get(difficulty, presets["hard"])


def read_line(grid, index, horizontal, size):
    if horizontal:
        return [grid[index][i] for i in range(size)]
    return [grid[i][index] for i in range(size)]


def line_partial_ok(grid, index, horizontal, size):
    line = read_line(grid, index, horizontal, size)
    sun, moon, empty = line.count(SUN), line.count(MOON), line.count(EMPTY)
    target = size // 2
    if sun > target or moon > target:
        return False
    if sun + empty < target or moon + empty < target:
        return False
    for i in range(size - 2):
        a, b, d = line[i], line[i + 1], line[i + 2]
        if a != EMPTY and a == b == d:
            return False
    return True


def cell_ok(grid, r, c, size):
    return line_partial_ok(grid, r, True, size) and line_partial_ok(grid, c, False, size)


def generate_solution(size, rng):
    grid = [[EMPTY] * size for _ in range(size)]
    cells = [(r, c) for r in range(size) for c in range(size)]
    rng.shuffle(cells)

    def fill(idx):
        if idx >= len(cells):
            return True
        r, c = cells[idx]
        vals = [SUN, MOON]
        if rng.randint(0, 1):
            vals.reverse()
        for v in vals:
            grid[r][c] = v
            if cell_ok(grid, r, c, size) and fill(idx + 1):
                return True
        grid[r][c] = EMPTY
        return False

    return grid if fill(0) else None


def generate_edges(solution, density, rng):
    size = len(solution)
    pool = []
    for r in range(size):
        for c in range(size):
            if c + 1 < size:
                pool.append({"row": r, "col": c, "horizontal": True,
                             "mustMatch": solution[r][c] == solution[r][c + 1]})
            if r + 1 < size:
                pool.append({"row": r, "col": c, "horizontal": False,
                             "mustMatch": solution[r][c] == solution[r + 1][c]})
    rng.shuffle(pool)
    pick = max(2, int(round(len(pool) * clamp01(density))))
    return pool[:pick]


def line_complete_valid(state, index, horizontal, size):
    line = read_line(state, index, horizontal, size)
    if line.count(SUN) != line.count(MOON):
        return False
    for i in range(size - 2):
        if line[i] == line[i + 1] == line[i + 2]:
            return False
    return True


def edges_ok(state, edges, size):
    for e in edges:
        ar, ac = e["row"], e["col"]
        br, bc = (ar, ac + 1) if e["horizontal"] else (ar + 1, ac)
        a, b = state[ar][ac], state[br][bc]
        if a == EMPTY or b == EMPTY:
            continue
        if e["mustMatch"] and a != b:
            return False
        if not e["mustMatch"] and a == b:
            return False
    return True


def count_solutions(size, state, edges, limit=2):
    count = 0

    def find_empty():
        best, best_score = None, 999
        for r in range(size):
            for c in range(size):
                if state[r][c] != EMPTY:
                    continue
                score = sum(1 for v in (SUN, MOON) if (setv(r, c, v) or True) and cell_ok(state, r, c, size))
                setv(r, c, EMPTY)
                if score < best_score:
                    best_score, best = score, (r, c)
        return best

    def setv(r, c, v):
        state[r][c] = v
        return True

    def solve():
        nonlocal count
        if count >= limit:
            return
        empty = find_empty()
        if empty is None:
            ok = all(line_complete_valid(state, i, True, size) for i in range(size))
            ok = ok and all(line_complete_valid(state, i, False, size) for i in range(size))
            if ok and edges_ok(state, edges, size):
                count += 1
            return
        r, c = empty
        if not cell_ok(state, r, c, size):
            return
        for v in (SUN, MOON):
            state[r][c] = v
            if cell_ok(state, r, c, size):
                solve()
            if count >= limit:
                return
        state[r][c] = EMPTY

    solve()
    return count


def generate_givens(size, solution, edges, given_ratio, rng):
    givens = [[True] * size for _ in range(size)]
    cells = [(r, c) for r in range(size) for c in range(size)]
    rng.shuffle(cells)
    target_hidden = max(1, int(round(size * size * (1 - clamp01(given_ratio)))))
    interval = 4 if size >= 8 else 2
    since = 0

    for r, c in cells:
        hidden = sum(1 for rr in range(size) for cc in range(size) if not givens[rr][cc])
        if hidden >= target_hidden:
            break
        givens[r][c] = False
        since += 1
        if since < interval and hidden + 1 < target_hidden:
            continue
        since = 0
        state = [[solution[rr][cc] if givens[rr][cc] else EMPTY for cc in range(size)] for rr in range(size)]
        if count_solutions(size, state, edges, 2) != 1:
            givens[r][c] = True

    state = [[solution[r][c] if givens[r][c] else EMPTY for c in range(size)] for r in range(size)]
    if sum(givens[r][c] for r in range(size) for c in range(size)) < 2:
        return None
    return givens if count_solutions(size, state, edges, 2) == 1 else None


def try_generate(size, given_ratio, edge_density, seed):
    rng = random.Random(seed)
    for attempt in range(80):
        solution = generate_solution(size, rng)
        if not solution:
            continue
        edges = generate_edges(solution, edge_density, rng)
        givens = generate_givens(size, solution, edges, given_ratio, rng)
        if givens is None:
            continue
        given = sum(givens[r][c] for r in range(size) for c in range(size))
        diff = size * 2 + (1 - given / (size * size)) * 10 + len(edges) / (2 * size * (size - 1)) * 4
        return solution, givens, edges, diff
    return None


def flatten(grid):
    return [grid[r][c] for r in range(len(grid)) for c in range(len(grid))]


def flatten_bool(givens):
    return [givens[r][c] for r in range(len(givens)) for c in range(len(givens))]


def to_level_dto(level_id, index, size, solution, givens, edges, difficulty, tier):
    given_count = sum(givens[r][c] for r in range(size) for c in range(size))
    return {
        "id": level_id,
        "index": index,
        "tier": tier,
        "size": size,
        "difficulty": round(difficulty, 3),
        "givenRatio": round(given_count / (size * size), 3),
        "solution": flatten(solution),
        "givens": flatten_bool(givens),
        "edges": edges,
    }


# Fixed 4-level sample: easy → medium → hard → brutal (play-test before full batch).
DEMO_LEVELS = [
    ("easy", 6, 0.58, 0.24),
    ("medium", 6, 0.42, 0.18),
    ("hard", 8, 0.30, 0.12),
    ("brutal", 8, 0.22, 0.08),
]


def cmd_demo(args):
    levels = []
    for i, (tier, size, given_ratio, edge_density) in enumerate(DEMO_LEVELS):
        index = i + 1
        seed = args.seed + i * 9973
        print(
            f"  [{index}/4] tier={tier} {size}×{size} given≈{given_ratio:.0%} edges≈{edge_density:.0%}",
            flush=True,
        )

        result = try_generate(size, given_ratio, edge_density, seed)
        if result is None and tier in ("easy", "medium"):
            result = try_generate(size, min(0.68, given_ratio + 0.06), edge_density, seed + 17)
        if result is None and tier in ("hard", "brutal"):
            result = try_generate(size, min(0.36, given_ratio + 0.06), edge_density, seed + 17)
        if result is None and tier == "brutal":
            result = try_generate(size, 0.26, edge_density, seed + 33)
        if result is None:
            print(f"Failed demo level {index} ({tier})", file=sys.stderr)
            return 1

        solution, givens, edges, diff = result
        levels.append(to_level_dto(f"campaign-{index:02d}", index, size, solution, givens, edges, diff, tier))

    out = Path(args.output)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps({"version": 2, "hookCount": 1, "levels": levels}, indent=2), encoding="utf-8")
    print(f"Wrote 4 demo levels → {out}")
    return 0


def cmd_campaign(args):
    hook = max(1, min(args.hook_count, args.count - 1))
    levels = []

    for i in range(args.count):
        index = i + 1
        size, given_ratio, edge_density, tier = config_for_campaign_level(index, args.count, hook)
        seed = args.seed + i * 9973
        print(f"  [{index}/{args.count}] tier={tier} {size}×{size} given≈{given_ratio:.0%} edges≈{edge_density:.0%}", flush=True)

        result = try_generate(size, given_ratio, edge_density, seed)
        if result is None and tier == "hook":
            result = try_generate(size, min(0.68, given_ratio + 0.06), edge_density, seed + 17)
        if result is None and tier != "hook":
            result = try_generate(size, min(0.36, given_ratio + 0.06), edge_density, seed + 17)
        if result is None:
            print(f"Failed level {index}", file=sys.stderr)
            return 1

        solution, givens, edges, diff = result
        levels.append(to_level_dto(f"campaign-{index:02d}", index, size, solution, givens, edges, diff, tier))

    out = Path(args.output)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps({"version": 2, "hookCount": hook, "levels": levels}, indent=2), encoding="utf-8")
    print(f"Wrote {len(levels)} levels (hook={hook}, hard={len(levels) - hook}) → {out}")
    return 0


def cmd_daily(args):
    given_ratio, edge_density = config_for_daily(args.size, args.difficulty)
    result = try_generate(args.size, given_ratio, edge_density, args.seed)
    if result is None:
        result = try_generate(args.size, given_ratio + 0.1, edge_density, args.seed + 99)
    if result is None:
        print("Failed to generate daily puzzle", file=sys.stderr)
        return 1

    solution, givens, edges, _ = result
    date = args.date
    dto = {
        "date": f"{date[:4]}-{date[4:6]}-{date[6:8]}" if len(date) == 8 else date,
        "size": args.size,
        "solution": flatten(solution),
        "givens": flatten_bool(givens),
        "edges": edges,
    }
    out = Path(args.output)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(dto, indent=2), encoding="utf-8")
    print(f"Wrote daily → {out}")
    return 0


def main():
    root = Path(__file__).resolve().parent.parent
    default_campaign = root / "Assets/Bordy/Resources/Bordy/campaign-levels.json"

    p = argparse.ArgumentParser(description="Bordy offline puzzle generator")
    sub = p.add_subparsers(dest="cmd", required=True)

    demo = sub.add_parser("demo", help="4-level play-test sample (easy/medium/hard/brutal)")
    demo.add_argument("--seed", type=int, default=20260709)
    demo.add_argument("-o", "--output", default=str(default_campaign))
    demo.set_defaults(func=cmd_demo)

    c = sub.add_parser("campaign", help="batch for game package")
    c.add_argument("--count", type=int, default=30)
    c.add_argument("--hook-count", type=int, default=5, dest="hook_count",
                     help="first N levels are easy hooks; rest are hard (monetization curve)")
    c.add_argument("--seed", type=int, default=20260709)
    c.add_argument("-o", "--output", default=str(default_campaign))
    c.set_defaults(func=cmd_campaign)

    d = sub.add_parser("daily", help="single puzzle for CDN")
    d.add_argument("--date", required=True, help="YYYYMMDD")
    d.add_argument("--size", type=int, default=8, choices=[6, 8, 10])
    d.add_argument("--difficulty", default="hard", choices=["easy", "normal", "hard"])
    d.add_argument("--seed", type=int, default=None)
    d.add_argument("-o", "--output", required=True)
    d.set_defaults(func=cmd_daily)

    args = p.parse_args()
    if args.cmd == "daily" and args.seed is None:
        args.seed = int(args.date) if args.date.isdigit() else 20260709
    return args.func(args)


if __name__ == "__main__":
    raise SystemExit(main())
