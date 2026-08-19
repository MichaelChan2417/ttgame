// Generate N campaign levels: distinct, unique-solution 6x6 puzzles, increasing difficulty
// (fewer clues = harder). Writes the BordyCampaignBundleDto JSON to the Unity Resources path.
const fs = require("fs");
const N = 6, TARGET = 3, SUN = 0, MOON = 1, EMPTY = -1;
function fnv1a(s){let h=2166136261>>>0;for(let i=0;i<s.length;i++){h^=s.charCodeAt(i);h=Math.imul(h,16777619);}return h>>>0;}
function mulberry32(seed){let a=seed>>>0;return function(){a=(a+0x6D2B79F5)|0;let t=Math.imul(a^(a>>>15),1|a);t=(t+Math.imul(t^(t>>>7),61|t))^t;return((t^(t>>>14))>>>0)/4294967296;};}
function shuffle(arr,rand){for(let i=arr.length-1;i>0;i--){const j=Math.floor(rand()*(i+1));[arr[i],arr[j]]=[arr[j],arr[i]];}return arr;}
function lineComplete(v){let s=0,m=0;for(const x of v){if(x===SUN)s++;else if(x===MOON)m++;}if(s!==TARGET||m!==TARGET)return false;for(let i=0;i<=N-3;i++)if(v[i]===v[i+1]&&v[i+1]===v[i+2])return false;return true;}
function partialOk(g,edges){for(let r=0;r<N;r++){let s=0,m=0;for(let c=0;c<N;c++){const v=g[r][c];if(v===SUN)s++;else if(v===MOON)m++;}if(s>TARGET||m>TARGET)return false;}for(let c=0;c<N;c++){let s=0,m=0;for(let r=0;r<N;r++){const v=g[r][c];if(v===SUN)s++;else if(v===MOON)m++;}if(s>TARGET||m>TARGET)return false;}for(let r=0;r<N;r++)for(let i=0;i<=N-3;i++){const a=g[r][i],b=g[r][i+1],d=g[r][i+2];if(a!==EMPTY&&a===b&&b===d)return false;}for(let c=0;c<N;c++)for(let i=0;i<=N-3;i++){const a=g[i][c],b=g[i+1][c],d=g[i+2][c];if(a!==EMPTY&&a===b&&b===d)return false;}for(const e of edges){const r2=e.horizontal?e.row:e.row+1,c2=e.horizontal?e.col+1:e.col;const a=g[e.row][e.col],b=g[r2][c2];if(a!==EMPTY&&b!==EMPTY){if(e.mustMatch&&a!==b)return false;if(!e.mustMatch&&a===b)return false;}}return true;}
function emptyGrid(){return Array.from({length:N},()=>Array(N).fill(EMPTY));}
function generateSolution(rand){const g=emptyGrid();const cells=[];for(let r=0;r<N;r++)for(let c=0;c<N;c++)cells.push([r,c]);function bt(i){if(i===cells.length){for(let r=0;r<N;r++)if(!lineComplete(g[r]))return false;for(let c=0;c<N;c++){const col=[];for(let r=0;r<N;r++)col.push(g[r][c]);if(!lineComplete(col))return false;}return true;}const[r,c]=cells[i];for(const v of(rand()<0.5?[SUN,MOON]:[MOON,SUN])){g[r][c]=v;if(partialOk(g,[])&&bt(i+1))return true;}g[r][c]=EMPTY;return false;}bt(0);return g;}
function pickEdges(sol,rand,count){
  const cand=[];
  for(let r=0;r<N;r++)for(let c=0;c<N;c++){
    if(c+1<N)cand.push({row:r,col:c,horizontal:true,mustMatch:sol[r][c]===sol[r][c+1]});
    if(r+1<N)cand.push({row:r,col:c,horizontal:false,mustMatch:sol[r][c]===sol[r+1][c]});
  }
  shuffle(cand,rand);
  const edges=[];
  for(const e of cand){if(edges.length>=count)break;edges.push(e);}
  const equals=edges.filter(e=>e.mustMatch);
  return edges.filter(e=>{
    if(e.mustMatch)return true;
    return !equals.some(eq=>eq.horizontal===e.horizontal && (
      eq.horizontal ? (eq.row===e.row && Math.abs(eq.col-e.col)===1)
                    : (eq.col===e.col && Math.abs(eq.row-e.row)===1)));
  });
}
function countSolutions(fixed,edges,limit){const g=fixed.map(r=>r.slice());const cells=[];for(let r=0;r<N;r++)for(let c=0;c<N;c++)if(g[r][c]===EMPTY)cells.push([r,c]);let n=0;function bt(i){if(n>=limit)return;if(i===cells.length){n++;return;}const[r,c]=cells[i];for(const v of[SUN,MOON]){g[r][c]=v;if(partialOk(g,edges))bt(i+1);g[r][c]=EMPTY;if(n>=limit)return;}}bt(0);return n;}
function makeUniqueGivens(sol,edges,rand){const given=Array.from({length:N},()=>Array(N).fill(true));const order=[];for(let r=0;r<N;r++)for(let c=0;c<N;c++)order.push([r,c]);shuffle(order,rand);for(const[r,c]of order){given[r][c]=false;const fixed=sol.map((row,rr)=>row.map((v,cc)=>given[rr][cc]?v:EMPTY));if(countSolutions(fixed,edges,2)!==1)given[r][c]=true;}return given;}
function buildLevel(index, clues, tier){
  const rand=mulberry32(fnv1a("bordy-campaign-v3-"+index));
  const sol=generateSolution(rand);
  const edges=pickEdges(sol,rand,6);
  const givens=makeUniqueGivens(sol,edges,rand);
  const hidden=[];for(let r=0;r<N;r++)for(let c=0;c<N;c++)if(!givens[r][c])hidden.push([r,c]);
  shuffle(hidden,rand);
  let have=givens.flat().filter(Boolean).length;
  for(const[r,c]of hidden){if(have>=clues)break;givens[r][c]=true;have++;}
  return {
    id:"campaign-"+String(index).padStart(2,"0"),
    index, tier, size:N,
    difficulty: 36 - givens.flat().filter(Boolean).length, // fewer clues -> higher
    solution: sol.flat(),
    givens: givens.flat(),
    edges: edges.map(e=>({row:e.row,col:e.col,horizontal:e.horizontal,mustMatch:e.mustMatch})),
  };
}
// 4 levels, increasing difficulty: clues 22 -> 12, tiers easy..hard
const plan=[{clues:22,tier:"easy"},{clues:18,tier:"easy"},{clues:15,tier:"medium"},{clues:12,tier:"hard"}];
const levels=plan.map((p,i)=>buildLevel(i+1,p.clues,p.tier));
const bundle={version:2,levels};
const out="/sessions/jolly-wizardly-noether/mnt/Bordy/Assets/Bordy/Resources/Bordy/campaign-levels.json";
fs.writeFileSync(out, JSON.stringify(bundle,null,2));
// verify
let ok=true;
for(const lv of levels){
  const g=[];for(let r=0;r<N;r++)g.push(lv.solution.slice(r*N,r*N+N));
  for(let r=0;r<N;r++){let s=g[r].filter(v=>v===0).length;if(s!==3)ok=false;for(let i=0;i<=N-3;i++)if(g[r][i]===g[r][i+1]&&g[r][i+1]===g[r][i+2])ok=false;}
  for(let c=0;c<N;c++){let s=0;for(let r=0;r<N;r++)if(g[r][c]===0)s++;if(s!==3)ok=false;}
  const clues=lv.givens.filter(Boolean).length;
  console.log(`${lv.id} index=${lv.index} tier=${lv.tier} clues=${clues}/36 difficulty=${lv.difficulty} edges=${lv.edges.length}`);
}
// distinctness check
const sigs=new Set(levels.map(l=>l.solution.join("")));
console.log(ok?"ALL VALID":"INVALID", "distinct solutions:", sigs.size, "/", levels.length);
