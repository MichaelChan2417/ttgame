/*
 * openDataContext/index.js
 * TikTok mini-game "open data domain" entry for the WordTT friend leaderboard.
 *
 * This file runs in the ISOLATED open data context (not the main game). It can
 * read friends' cloud-storage scores via getFriendCloudStorage and draw them to
 * the shared canvas that overlays the game. The main game triggers it with
 *   tt.getOpenDataContext().postMessage({ type: 'show', day, key })
 *
 * Build: set the Cocos "Open Data Context" root to this folder and add
 *   "openDataContext": "openDataContext"
 * to game.json (see openDataContext/README.md).
 *
 * Docs: TikTok 排行榜接入指南 (open data domain). Note: friend objects actually
 * return `displayName` / `avatarUrl` (camelCase), not the snake_case in the doc.
 */
/* global GameGlobal */

var G = (typeof GameGlobal !== 'undefined') ? GameGlobal : this;
var tt = G.tt;

var DAILY_KEY = 'wordtt_daily';
var sharedCanvas = tt.getSharedCanvas();
var ctx = sharedCanvas.getContext('2d');

function layout() {
    sharedCanvas.width = 800;
    sharedCanvas.height = 1040;
    if (sharedCanvas.style) {
        sharedCanvas.style.width = '86vw';
        sharedCanvas.style.height = '62vh';
        sharedCanvas.style.top = '16vh';
        sharedCanvas.style.left = '7vw';
    }
}

function clear() {
    ctx.clearRect(0, 0, sharedCanvas.width, sharedCanvas.height);
}

function roundRect(x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
}

function drawHeader(day) {
    ctx.fillStyle = '#6366F1';
    ctx.font = 'bold 48px sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('Friends  ·  WordTT #' + day, sharedCanvas.width / 2, 60);
}

function drawEmpty() {
    ctx.fillStyle = '#787C7E';
    ctx.font = '32px sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText('No friends have played today yet.', sharedCanvas.width / 2, 240);
    ctx.fillText('Share the challenge to get them in!', sharedCanvas.width / 2, 290);
}

function drawRow(rank, entry, y) {
    var W = sharedCanvas.width;
    var isYou = entry.you;
    // card
    ctx.fillStyle = isYou ? '#EEF0FF' : '#F6F7F8';
    roundRect(40, y, W - 80, 96, 20);
    ctx.fill();

    // rank
    ctx.fillStyle = rank <= 3 ? '#6AAA64' : '#26282E';
    ctx.font = 'bold 40px sans-serif';
    ctx.textAlign = 'left';
    ctx.fillText('' + rank, 70, y + 48);

    // avatar (async-safe: draw placeholder circle first)
    ctx.fillStyle = '#D3D6DA';
    ctx.beginPath();
    ctx.arc(170, y + 48, 34, 0, Math.PI * 2);
    ctx.fill();
    if (entry.avatar) {
        try {
            var img = tt.createImage();
            img.onload = function () {
                ctx.save();
                ctx.beginPath();
                ctx.arc(170, y + 48, 34, 0, Math.PI * 2);
                ctx.clip();
                ctx.drawImage(img, 136, y + 14, 68, 68);
                ctx.restore();
            };
            img.src = entry.avatar;
        } catch (e) { /* ignore */ }
    }

    // name
    ctx.fillStyle = '#26282E';
    ctx.font = (isYou ? 'bold ' : '') + '34px sans-serif';
    ctx.textAlign = 'left';
    ctx.fillText(entry.name, 224, y + 48);

    // rows / time (right-aligned)
    ctx.fillStyle = '#5A5C63';
    ctx.font = '30px sans-serif';
    ctx.textAlign = 'right';
    ctx.fillText(entry.rows + ' rows · ' + fmt(entry.time), W - 70, y + 48);
}

function fmt(sec) {
    var t = Math.floor(sec || 0), mm = Math.floor(t / 60), ss = t % 60;
    return (mm < 10 ? '0' : '') + mm + ':' + (ss < 10 ? '0' : '') + ss;
}

function draw(list, day) {
    clear();
    drawHeader(day);
    if (!list.length) { drawEmpty(); return; }
    var y = 120;
    for (var i = 0; i < list.length && i < 8; i++) {
        drawRow(i + 1, list[i], y);
        y += 112;
    }
}

function parseEntry(friend, day) {
    var data = friend.data || friend.KVDataList || [];
    var kv = null;
    for (var i = 0; i < data.length; i++) { if (data[i].key === DAILY_KEY) { kv = data[i]; break; } }
    if (!kv) return null;
    try {
        var v = JSON.parse(kv.value);
        if (v.d !== day) return null; // only today's puzzle
        return {
            name: friend.displayName || friend.nickname || friend.display_name || '—',
            avatar: friend.avatarUrl || friend.avatar_url || '',
            rows: v.r, time: v.t,
        };
    } catch (e) { return null; }
}

function show(day) {
    tt.getFriendCloudStorage({
        keyList: [DAILY_KEY],
        success: function (res) {
            var arr = (res && res.data) ? res.data : [];
            var list = [];
            for (var i = 0; i < arr.length; i++) {
                var e = parseEntry(arr[i], day);
                if (e) list.push(e);
            }
            list.sort(function (a, b) { return (a.rows - b.rows) || (a.time - b.time); });
            draw(list, day);
        },
        fail: function () { draw([], day); },
    });
}

tt.onMessage(function (msg) {
    if (!msg) return;
    if (msg.type === 'show') { layout(); show(msg.day); }
    else if (msg.type === 'hide') { clear(); }
});
