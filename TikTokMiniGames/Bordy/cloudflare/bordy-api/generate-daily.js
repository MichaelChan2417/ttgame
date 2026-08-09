// Generate a valid, unique 6x6 Bordy daily puzzle. No dependencies (plain Node).
// Usage: node generate-daily.js [YYYYMMDD]   (defaults to today UTC)
// Output: <date>.json in BordyDailyDto format (date/size/solution/givens/edges).
//
// Rules enforced: each row & column has 3 suns + 3 moons, never 3 identical in a row,
// and every "=" edge matches / "×" edge differs. Clues are removed while the solution
// stays UNIQUE, so each day is a real, solvable puzzle. Seeded by the date → reproducible.

const fs = require("fs");

const N = 6;
const TARGET = 3;      // suns == moons == 3 per line
const SUN = 0, MOON = 1, EMPTY = -1;

// ---- seeded RNG (deterministic per date) ----
function fnv1a(str) {
  let h = 2166136261 >>> 0;
  for (let i = 0; i < str.length; i++) { h ^= str.charCodeAt(i); h = Math.imul(h, 16777619); }
  return h >>> 0;
}
function mulberry32(seed) {
  let a = seed >>> 0;
  return function () {
    a = (a + 0x6D2B79F5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
function shuffle(arr, rand) {
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(rand() * (i + 1));
    [arr[i], arr[j]] = [arr[j], arr[i]];
  }
  return arr;
}

// ---- validity ----
function lineComplete(vals) {
  let s = 0, m = 0;
  for (const v of vals) { if (v === SUN) s++; else if (v === MOON) m++; }
  if (s !== TARGET || m !== TARGET) return false;
  for (let i = 0; i <= N - 3; i++)
    if (vals[i] === vals[i + 1] && vals[i + 1] === vals[i + 2]) return false;
  return true;
}
function partialOk(grid, edges) {
  for (let r = 0; r < N; r++) {
    let s = 0, m = 0;
    for (let c = 0; c < N; c++) { const v = grid[r][c]; if (v === SUN) s++; else if (v === MOON) m++; }
    if (s > TARGET || m > TARGET) return false;
  }
  for (let c = 0; c < N; c++) {
    let s = 0, m = 0;
    for (let r = 0; r < N; r++) { const v = grid[r][c]; if (v === SUN) s++; else if (v === MOON) m++; }
    if (s > TARGET || m > TARGET) return false;
  }
  for (let r = 0; r < N; r++)
    for (let i = 0; i <= N - 3; i++) {
      const a = grid[r][i], b = grid[r][i + 1], d = grid[r][i + 2];
      if (a !== EMPTY && a === b && b === d) return false;
    }
  for (let c = 0; c < N; c++)
    for (let i = 0; i <= N - 3; i++) {
      const a = grid[i][c], b = grid[i + 1][c], d = grid[i + 2][c];
      if (a !== EMPTY && a === b && b === d) return false;
    }
  for (const e of edges) {
    const r2 = e.horizontal ? e.row : e.row + 1;
    const c2 = e.horizontal ? e.col + 1 : e.col;
    const a = grid[e.row][e.col], b = grid[r2][c2];
    if (a !== EMPTY && b !== EMPTY) {
      if (e.mustMatch && a !== b) return false;
      if (!e.mustMatch && a === b) return false;
    }
  }
  return true;
}

function emptyGrid() { return Array.from({ length: N }, () => Array(N).fill(EMPTY)); }

function generateSolution(rand) {
  const grid = emptyGrid();
  const cells = [];
  for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) cells.push([r, c]);
  function bt(i) {
    if (i === cells.length) {
      for (let r = 0; r < N; r++) if (!lineComplete(grid[r])) return false;
      for (let c = 0; c < N; c++) {
        const col = []; for (let r = 0; r < N; r++) col.push(grid[r][c]);
        if (!lineComplete(col)) return false;
      }
      return true;
    }
    const [r, c] = cells[i];
    for (const v of (rand() < 0.5 ? [SUN, MOON] : [MOON, SUN])) {
      grid[r][c] = v;
      if (partialOk(grid, []) && bt(i + 1)) return true;
    }
    grid[r][c] = EMPTY;
    return false;
  }
  bt(0);
  return grid;
}

function pickEdges(sol, rand, count) {
  const cand = [];
  for (let r = 0; r < N; r++)
    for (let c = 0; c < N; c++) {
      if (c + 1 < N) cand.push({ row: r, col: c, horizontal: true });
      if (r + 1 < N) cand.push({ row: r, col: c, horizontal: false });
    }
  shuffle(cand, rand);
  const edges = [];
  for (const e of cand) {
    if (edges.length >= count) break;
    const r2 = e.horizontal ? e.row : e.row + 1;
    const c2 = e.horizontal ? e.col + 1 : e.col;
    e.mustMatch = sol[e.row][e.col] === sol[r2][c2];
    edges.push(e);
  }
  return edges;
}

function countSolutions(fixed, edges, limit) {
  const grid = fixed.map((row) => row.slice());
  const cells = [];
  for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) if (grid[r][c] === EMPTY) cells.push([r, c]);
  let count = 0;
  function bt(i) {
    if (count >= limit) return;
    if (i === cells.length) { count++; return; }
    const [r, c] = cells[i];
    for (const v of [SUN, MOON]) {
      grid[r][c] = v;
      if (partialOk(grid, edges)) bt(i + 1);
      grid[r][c] = EMPTY;
      if (count >= limit) return;
    }
  }
  bt(0);
  return count;
}

function makeUniqueGivens(sol, edges, rand) {
  const given = Array.from({ length: N }, () => Array(N).fill(true));
  const order = [];
  for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) order.push([r, c]);
  shuffle(order, rand);
  for (const [r, c] of order) {
    given[r][c] = false;
    const fixed = sol.map((row, rr) => row.map((v, cc) => (given[rr][cc] ? v : EMPTY)));
    if (countSolutions(fixed, edges, 2) !== 1) given[r][c] = true; // keep the clue
  }
  return given;
}

// ---- main ----
const date = (process.argv[2] || new Date().toISOString().slice(0, 10).replace(/-/g, "")).trim();
if (!/^\d{8}$/.test(date)) { console.error("Bad date, expected YYYYMMDD"); process.exit(1); }

const rand = mulberry32(fnv1a("bordy-daily-" + date));
const solution = generateSolution(rand);
const edges = pickEdges(solution, rand, 6);
const givens = makeUniqueGivens(solution, edges, rand);

// The minimal unique set is very hard. Reveal extra clues up to a friendlier count —
// adding clues never breaks uniqueness. Tune MIN_CLUES for difficulty (higher = easier).
// 最小唯一解太难；补足线索到较友好的数量（加线索不影响唯一性）。MIN_CLUES 越大越简单。
const MIN_CLUES = process.argv[3] ? parseInt(process.argv[3], 10) : 16;
{
  const hidden = [];
  for (let r = 0; r < N; r++) for (let c = 0; c < N; c++) if (!givens[r][c]) hidden.push([r, c]);
  shuffle(hidden, rand);
  let have = givens.flat().filter(Boolean).length;
  for (const [r, c] of hidden) {
    if (have >= MIN_CLUES) break;
    givens[r][c] = true;
    have++;
  }
}

const dto = {
  date,
  size: N,
  solution: solution.flat(),
  givens: givens.flat(),
  edges: edges.map((e) => ({ row: e.row, col: e.col, horizontal: e.horizontal, mustMatch: e.mustMatch })),
};

const file = `${date}.json`;
fs.writeFileSync(file, JSON.stringify(dto, null, 2));
const clueCount = dto.givens.filter(Boolean).length;
console.log(`wrote ${file}  (givens=${clueCount}/36, edges=${edges.length}, unique solution)`);
