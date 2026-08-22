import {
    _decorator, Component, Node, Label, UITransform, Graphics, Color, Vec3,
    view, ResolutionPolicy, input, Input, KeyCode, EventKeyboard, EventTouch, Layers, sys, tween,
    Sprite, SpriteFrame, resources, UIOpacity,
} from 'cc';
import { isValidGuess } from './WordList';
import { dailyWord, puzzleId } from './Daily';
import {
    isTikTok, showRewardedAd, shareAppMessage, canShare,
    authorizeOpenContext, setUserCloudStorage, postToOpenData, navigateToSidebar,
} from './Platform';

const { ccclass } = _decorator;

const COLS = 5;
const BASE_ROWS = 6;   // normal guesses
const MAX_ROWS = 7;    // hard cap once the +1-row booster is used

/** Rewarded-ad slot id — configure in the TikTok dev portal before release. */
const AD_UNIT = 'YOUR_REWARDED_AD_UNIT_ID';
/** Cloud-storage key for the friend leaderboard. */
const DAILY_KEY = 'wordtt_daily';
/** Local-storage key for saving today's in-progress game. */
const SAVE_KEY = 'wordtt_save_v1';
/** Set false for release: hides the on-screen debug "reset" button. */
const DEBUG = false;

interface SaveState {
    day: number;
    guesses: string[];
    revealed: string[];
    hints: string[];
    addRowUsed: boolean;
    maxRows: number;
    elapsed: number;
    finished: boolean;
    won: boolean;
    pending: string[];
}

enum LetterState { EMPTY = 0, ABSENT = 1, PRESENT = 2, CORRECT = 3 }

// ---- Palette ---------------------------------------------------------------
const C_BG = new Color(255, 255, 255);           // empty tile fill
const C_BORDER_EMPTY = new Color(219, 213, 200);
const C_BORDER_FILLED = new Color(176, 168, 148);
const C_ABSENT = new Color(122, 114, 102);       // warm gray
const C_PRESENT = new Color(228, 151, 43);       // icon amber
const C_CORRECT = new Color(84, 185, 106);       // icon green
const C_KEY = new Color(228, 223, 208);          // warm light key
const C_TEXT_DARK = new Color(43, 36, 29);       // warm dark ink
const C_TEXT_LIGHT = new Color(243, 236, 221);   // cream
const C_HALO = new Color(255, 255, 255);
const C_LTR_CORRECT = new Color(18, 64, 31);     // dark green letter on green tile
const C_LTR_PRESENT = new Color(110, 67, 16);    // dark amber letter on amber tile
const C_ACCENT = new Color(228, 151, 43);        // brand amber
const C_ACCENT_DARK = new Color(138, 90, 18);
const C_ADD = new Color(84, 185, 106);           // sidebar accent → brand green
const C_DIM = new Color(36, 28, 22);

const FONT_FAMILY = 'Poppins, Nunito, "Trebuchet MS", Verdana, sans-serif';

interface Cell { node: Node; graphics: Graphics; label: Label; state: LetterState; }
interface Key { node: Node; graphics: Graphics; label: Label; w: number; h: number; state: LetterState; }

@ccclass('GameController')
export class GameController extends Component {
    private W = 720;
    private H = 1280;
    private topInset = 0;      // safe-area inset (notch / Dynamic Island)
    private bottomInset = 0;   // safe-area inset (home indicator)

    private menuRoot: Node | null = null;
    private gameRoot: Node | null = null;
    private propsRoot: Node | null = null;
    private shareBtn: Node | null = null;
    private addRowBtn: Node | null = null;
    private propSprites: { [k: string]: Sprite } = {};
    private propFrames: { [k: string]: SpriteFrame } = {};
    private mockRankRoot: Node | null = null;
    private sidebarPopupRoot: Node | null = null;
    private menuMsg: Label | null = null;

    private cells: Cell[][] = [];        // MAX_ROWS x COLS (extra row hidden until earned)
    private keys: Map<string, Key> = new Map();

    private target = '';
    private puzzleNo = 0;
    private curRow = 0;
    private maxRows = BASE_ROWS;
    private gameOver = false;

    private revealed: string[] = ['', '', '', '', ''];
    private history: LetterState[][] = [];
    private hintLetters: string[] = [];
    private addRowUsed = false;
    private adBusy = false;
    private animating = false;
    private saveAccum = 0;

    private lastWon = false;
    private lastRows = 0;

    private messageLabel: Label | null = null;
    private timerLabel: Label | null = null;
    private elapsed = 0;
    private timing = false;

    // board layout region (set in buildGame, used by layoutBoard)
    private boardRegionTop = 0;
    private boardRegionBottom = 0;
    private boardColGap = 12;
    private boardRowGap = 20;

    onLoad() {
        view.setDesignResolutionSize(720, 1280, ResolutionPolicy.FIT_WIDTH);
        const vs = view.getVisibleSize();
        if (vs.width > 0 && vs.height > 0) { this.W = vs.width; this.H = vs.height; }
        else {
            const uit = this.getComponent(UITransform);
            if (uit && uit.width > 0) { this.W = uit.width; this.H = uit.height; }
        }
        this.computeSafeArea();
        this.buildUI();
        this.loadPropIcons();
        this.showMenu();

        if (DEBUG) (globalThis as any).wordttReset = () => this.clearSave();
    }

    private computeSafeArea() {
        this.topInset = 0;
        this.bottomInset = 0;
        try {
            const sa = (sys as any).getSafeAreaRect ? (sys as any).getSafeAreaRect() : null;
            if (sa && sa.height > 0) {
                // rect is in the same logical space as the visible size
                this.topInset = Math.min(Math.max(0, this.H - (sa.y + sa.height)), this.H * 0.14);
                this.bottomInset = Math.min(Math.max(0, sa.y), this.H * 0.10);
            }
        } catch (e) { /* not supported → no inset */ }
    }

    update(dt: number) {
        if (this.timing && !this.gameOver) {
            this.elapsed += dt;
            this.refreshTimer();
            this.saveAccum += dt;
            if (this.saveAccum >= 5) { this.saveAccum = 0; this.saveState(); }
        }
    }

    onEnable() {
        input.on(Input.EventType.KEY_DOWN, this.onPhysicalKey, this);
        this.node.on(Node.EventType.TOUCH_END, this.onRootTap, this);
    }
    onDisable() {
        input.off(Input.EventType.KEY_DOWN, this.onPhysicalKey, this);
        this.node.off(Node.EventType.TOUCH_END, this.onRootTap, this);
    }

    // ===== UI construction ==================================================

    private buildUI() {
        this.menuRoot = new Node('menu');
        this.menuRoot.layer = Layers.Enum.UI_2D;
        this.menuRoot.setParent(this.node);

        this.gameRoot = new Node('game');
        this.gameRoot.layer = Layers.Enum.UI_2D;
        this.gameRoot.setParent(this.node);

        this.buildMenu();
        this.buildGame();
        this.buildMockLeaderboard();
        this.buildSidebarPopup();
    }

    private buildMenu() {
        const W = this.W, H = this.H, parent = this.menuRoot!;
        this.makeTiledTitle(parent, 'WORDTT', 0, H * 0.18, 84, 9);
        this.makeText('Daily Challenge  ·  #' + puzzleId(), 0, H * 0.18 - 78, 30, C_ABSENT, false, parent);
        this.makeButton(parent, 'PLAY', 0, -H * 0.02, W * 0.52, 104, C_ACCENT, 26, () => this.startGame());
        this.makeText('Same word for everyone, every day', 0, -H * 0.02 - 84, 24, C_ABSENT, false, parent);

        // Small side button (lower-right) → opens the "Add to Sidebar" popup
        this.makeButton(parent, 'SIDEBAR', W * 0.5 - 84, -H * 0.20, 132, 70, C_ADD, 18, () => this.showSidebarPopup());

        // Debug-only: clear the local save for testing
        if (DEBUG) this.makeButton(parent, 'reset', -W * 0.5 + 66, -H * 0.40, 100, 50, C_ABSENT, 14, () => this.clearSave());
    }

    private clearSave() {
        try { sys.localStorage.removeItem(SAVE_KEY); } catch (e) { /* ignore */ }
        // Drop in-memory progress too — showMenu() persists whatever is still in RAM.
        this.target = '';
        this.puzzleNo = 0;
        this.curRow = 0;
        this.maxRows = BASE_ROWS;
        this.gameOver = false;
        this.elapsed = 0;
        this.timing = false;
        this.revealed = ['', '', '', '', ''];
        this.history = [];
        this.hintLetters = [];
        this.addRowUsed = false;
        this.lastWon = false;
        this.lastRows = 0;
        this.showMenu();
        console.log('[WordTT] local save cleared — press PLAY for a fresh game');
    }

    private buildSidebarPopup() {
        const W = this.W, H = this.H;
        const root = new Node('sidebarPopup');
        root.layer = Layers.Enum.UI_2D;
        root.setParent(this.node);

        const dim = new Node('dim');
        dim.layer = Layers.Enum.UI_2D;
        dim.setParent(root);
        const du = dim.addComponent(UITransform);
        du.setContentSize(W, H); du.setAnchorPoint(0.5, 0.5); dim.setPosition(Vec3.ZERO);
        const dg = dim.addComponent(Graphics);
        dg.roundRect(-W / 2, -H / 2, W, H, 0);
        dg.fillColor = new Color(C_DIM.r, C_DIM.g, C_DIM.b, 190); dg.fill();
        dim.on(Node.EventType.TOUCH_END, (e: EventTouch) => { e.propagationStopped = true; root.active = false; }, this);

        const pw = W * 0.82, ph = H * 0.42;
        const panel = new Node('panel');
        panel.layer = Layers.Enum.UI_2D;
        panel.setParent(root);
        const pu = panel.addComponent(UITransform);
        pu.setContentSize(pw, ph); pu.setAnchorPoint(0.5, 0.5); panel.setPosition(Vec3.ZERO);
        const pg = panel.addComponent(Graphics);
        pg.roundRect(-pw / 2, -ph / 2, pw, ph, 28); pg.fillColor = C_BG; pg.fill();

        const t = this.makeText('Add to Sidebar', 0, ph / 2 - 54, 40, C_ADD, true, panel);
        this.addOutline(t, new Color(200, 120, 40), 3);
        this.makeText('Pin WordTT to your TikTok sidebar\nfor one-tap daily access.', 0, ph * 0.12, 26, C_TEXT_DARK, false, panel);
        this.menuMsg = this.makeText('', 0, -ph * 0.12, 22, C_ABSENT, false, panel);
        this.makeButton(panel, 'ADD TO SIDEBAR', 0, -ph / 2 + 122, pw * 0.74, 86, C_ADD, 20, () => this.onSidebarConfirm());
        this.makeButton(panel, 'Not now', 0, -ph / 2 + 46, pw * 0.5, 54, C_ABSENT, 16, () => { root.active = false; });

        root.active = false;
        this.sidebarPopupRoot = root;
    }

    private showSidebarPopup() {
        if (this.menuMsg) this.menuMsg.string = '';
        if (this.sidebarPopupRoot) this.sidebarPopupRoot.active = true;
    }

    private onSidebarConfirm() {
        if (!isTikTok()) {
            if (this.menuMsg) { this.menuMsg.string = 'Preview only · works on TikTok'; this.menuMsg.color = C_ABSENT; }
            return;
        }
        navigateToSidebar().then(ok => {
            if (ok) { if (this.sidebarPopupRoot) this.sidebarPopupRoot.active = false; }
            else if (this.menuMsg) { this.menuMsg.string = 'Sidebar not available'; this.menuMsg.color = C_ABSENT; }
        });
    }

    private buildGame() {
        const W = this.W, H = this.H, parent = this.gameRoot!;

        // top button row (Menu / Rank) — pushed below the notch / Dynamic Island
        const buttonsY = H * 0.5 - this.topInset - 58;
        this.makeButton(parent, 'Menu', -W * 0.5 + 72, buttonsY, 104, 58, C_ABSENT, 18, () => this.showMenu());
        this.makeButton(parent, 'Rank', W * 0.5 - 72, buttonsY, 104, 58, C_ACCENT, 18, () => this.onRank());

        // title sits fully BELOW the button row
        const titleY = H * 0.5 - this.topInset - 132;
        this.makeTiledTitle(parent, 'WORDTT', 0, titleY, 46, 6);

        const timerY = titleY - 50;
        this.timerLabel = this.makeText('00:00', 0, timerY, 34, C_TEXT_DARK, true, parent);
        const messageY = timerY - 42;
        this.messageLabel = this.makeText('', 0, messageY, 27, C_CORRECT, true, parent);

        // ---- keyboard geometry ----
        const rows = ['QWERTYUIOP', 'ASDFGHJKL', '<ZXCVBNM>'];
        const kGap = 8;
        const keyW = (W * 0.96 - 9 * kGap) / 10;
        const keyH = Math.min(keyW * 1.5, 92);
        const wideW = keyW * 1.5 + kGap / 2;
        const kbRows = rows.length;
        const kbBottomMargin = H * 0.028 + this.bottomInset;
        const kbBottomRowY = -H * 0.5 + kbBottomMargin + keyH / 2;
        const kbTopEdge = kbBottomRowY + (kbRows - 1) * (keyH + kGap) + keyH / 2;

        // ---- booster (prop) pill buttons + share button, just above keyboard ----
        const propH = 84;
        const propY = kbTopEdge + 18 + propH / 2;
        const pgap = 26;
        const propW = (W * 0.94 - 2 * pgap) / 3;
        this.propsRoot = new Node('props');
        this.propsRoot.layer = Layers.Enum.UI_2D;
        this.propsRoot.setParent(parent);
        this.makePropPill(this.propsRoot, 'hint', 'Hint', -(propW + pgap), propY, propW, propH, () => this.onProp('hint'));
        this.makePropPill(this.propsRoot, 'reveal', 'Reveal', 0, propY, propW, propH, () => this.onProp('reveal'));
        this.addRowBtn = this.makePropPill(this.propsRoot, 'addrow', '+1 Row', propW + pgap, propY, propW, propH, () => this.onProp('addrow'));

        const share = this.makeButton(parent, 'SHARE  ▸  CHALLENGE', 0, propY, W * 0.94, propH, C_CORRECT, 18, () => this.onShare());
        this.shareBtn = share.node;
        this.shareBtn.active = false;

        // ---- board region: between the message and the booster row ----
        this.boardRegionTop = messageY - 30;
        this.boardRegionBottom = propY + propH / 2 + 16;

        // create MAX_ROWS rows of cells (extra row hidden until earned), then lay out for BASE_ROWS
        for (let r = 0; r < MAX_ROWS; r++) {
            const row: Cell[] = [];
            for (let c = 0; c < COLS; c++) row.push(this.makeCell());
            this.cells.push(row);
        }

        // ---- keyboard ----
        for (let r = 0; r < rows.length; r++) {
            const chars = rows[r].split('');
            const ky = kbBottomRowY + (rows.length - 1 - r) * (keyH + kGap);
            let total = 0;
            const widths: number[] = [];
            for (const ch of chars) { const w = (ch === '<' || ch === '>') ? wideW : keyW; widths.push(w); total += w; }
            total += (chars.length - 1) * kGap;
            let x = -total / 2;
            for (let i = 0; i < chars.length; i++) {
                const w = widths[i];
                const face = chars[i] === '<' ? 'ENTER' : chars[i] === '>' ? 'DEL' : chars[i];
                this.makeKey(chars[i], face, x + w / 2, ky, w, keyH);
                x += w + kGap;
            }
        }

        this.layoutBoard(BASE_ROWS);
    }

    // ===== element factories ================================================

    private makeCell(): Cell {
        const node = new Node('cell');
        node.layer = Layers.Enum.UI_2D;
        node.setParent(this.gameRoot!);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(80, 80);
        uit.setAnchorPoint(0.5, 0.5);
        const g = node.addComponent(Graphics);

        const lblNode = new Node('lbl');
        lblNode.layer = Layers.Enum.UI_2D;
        lblNode.setParent(node);
        const lu = lblNode.addComponent(UITransform);
        lu.setContentSize(80, 80);
        lu.setAnchorPoint(0.5, 0.5);
        lblNode.setPosition(Vec3.ZERO);
        const label = lblNode.addComponent(Label);
        label.string = '';
        label.fontFamily = FONT_FAMILY;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = C_TEXT_DARK;
        label.isBold = true;
        this.addOutline(label, C_HALO, 1.6);

        const cell: Cell = { node, graphics: g, label, state: LetterState.EMPTY };
        return cell;
    }

    private positionCell(cell: Cell, x: number, y: number, size: number) {
        cell.node.setScale(1, 1, 1);
        cell.node.getComponent(UITransform)!.setContentSize(size, size);
        cell.node.setPosition(new Vec3(x, y, 0));
        cell.label.node.getComponent(UITransform)!.setContentSize(size, size);
        cell.label.fontSize = Math.floor(size * 0.5);
        cell.label.lineHeight = Math.floor(size * 0.5);
        const filled = cell.state === LetterState.EMPTY && cell.label.string !== '';
        this.drawTile(cell, filled);
    }

    private makeKey(id: string, face: string, x: number, y: number, w: number, h: number): Key {
        const node = new Node('key_' + id);
        node.layer = Layers.Enum.UI_2D;
        node.setParent(this.gameRoot!);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(w, h);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));
        const g = node.addComponent(Graphics);

        const lblNode = new Node('lbl');
        lblNode.layer = Layers.Enum.UI_2D;
        lblNode.setParent(node);
        const lu = lblNode.addComponent(UITransform);
        lu.setContentSize(w, h);
        lu.setAnchorPoint(0.5, 0.5);
        lblNode.setPosition(Vec3.ZERO);
        const label = lblNode.addComponent(Label);
        label.string = face;
        label.fontFamily = FONT_FAMILY;
        label.fontSize = face.length > 1 ? Math.floor(h * 0.26) : Math.floor(h * 0.42);
        label.lineHeight = label.fontSize;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = C_TEXT_DARK;
        label.isBold = true;

        const key: Key = { node, graphics: g, label, w, h, state: LetterState.EMPTY };
        this.drawKey(key);
        node.on(Node.EventType.TOUCH_END, (e: EventTouch) => { e.propagationStopped = true; this.handleInput(id); }, this);
        this.keys.set(id, key);
        return key;
    }

    private makeText(str: string, x: number, y: number, size: number, color: Color, bold: boolean, parent: Node): Label {
        const node = new Node('text');
        node.layer = Layers.Enum.UI_2D;
        node.setParent(parent);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(this.W * 0.92, size * 1.6);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));
        const label = node.addComponent(Label);
        label.string = str;
        label.fontFamily = FONT_FAMILY;
        label.fontSize = size;
        label.lineHeight = size * 1.15;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = color;
        label.isBold = bold;
        return label;
    }

    /** Brand-style title: each letter on its own tilted rounded tile (icon look). */
    private makeTiledTitle(parent: Node, text: string, cx: number, cy: number, tile: number, gap: number): Node {
        const n = text.length;
        tile = Math.min(tile, (this.W * 0.9 - (n - 1) * gap) / n);
        const totalW = n * tile + (n - 1) * gap;
        const th = tile * 1.12;
        const tileColors = [C_PRESENT, C_DIM, C_CORRECT];               // amber / dark / green
        const letterColors = [C_LTR_PRESENT, C_TEXT_LIGHT, C_LTR_CORRECT];

        const container = new Node('title');
        container.layer = Layers.Enum.UI_2D;
        container.setParent(parent);
        const cu = container.addComponent(UITransform);
        cu.setContentSize(totalW, th);
        cu.setAnchorPoint(0.5, 0.5);
        container.setPosition(new Vec3(cx, cy, 0));

        const startX = -totalW / 2 + tile / 2;
        for (let i = 0; i < n; i++) {
            const ci = i % 3;
            const t = new Node('t');
            t.layer = Layers.Enum.UI_2D;
            t.setParent(container);
            const tu = t.addComponent(UITransform);
            tu.setContentSize(tile, th);
            tu.setAnchorPoint(0.5, 0.5);
            t.setPosition(new Vec3(startX + i * (tile + gap), 0, 0));
            t.setRotationFromEuler(0, 0, i % 2 === 0 ? 4 : -4);
            const g = t.addComponent(Graphics);
            g.roundRect(-tile / 2, -th / 2, tile, th, tile * 0.22);
            g.fillColor = tileColors[ci];
            g.fill();

            const ln = new Node('l');
            ln.layer = Layers.Enum.UI_2D;
            ln.setParent(t);
            const lu = ln.addComponent(UITransform);
            lu.setContentSize(tile, th);
            lu.setAnchorPoint(0.5, 0.5);
            ln.setPosition(Vec3.ZERO);
            const lab = ln.addComponent(Label);
            lab.string = text[i];
            lab.fontFamily = FONT_FAMILY;
            lab.fontSize = Math.floor(tile * 0.6);
            lab.lineHeight = lab.fontSize;
            lab.horizontalAlign = Label.HorizontalAlign.CENTER;
            lab.verticalAlign = Label.VerticalAlign.CENTER;
            lab.color = letterColors[ci];
            lab.isBold = true;
        }
        return container;
    }

    private makeButton(parent: Node, text: string, x: number, y: number, w: number, h: number,
                       bg: Color, radius: number, onClick: () => void): { node: Node; label: Label } {
        const node = new Node('button');
        node.layer = Layers.Enum.UI_2D;
        node.setParent(parent);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(w, h);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));
        const g = node.addComponent(Graphics);
        g.roundRect(-w / 2, -h / 2, w, h, radius);
        g.fillColor = bg;
        g.fill();

        const lblNode = new Node('lbl');
        lblNode.layer = Layers.Enum.UI_2D;
        lblNode.setParent(node);
        const lu = lblNode.addComponent(UITransform);
        lu.setContentSize(w, h);
        lu.setAnchorPoint(0.5, 0.5);
        lblNode.setPosition(Vec3.ZERO);
        const label = lblNode.addComponent(Label);
        label.string = text;
        label.fontFamily = FONT_FAMILY;
        label.fontSize = Math.floor(h * 0.4);
        label.lineHeight = label.fontSize;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = C_TEXT_LIGHT;
        label.isBold = true;

        node.on(Node.EventType.TOUCH_END, (e: EventTouch) => { e.propagationStopped = true; onClick(); }, this);
        return { node, label };
    }

    /** Wide booster pill: white rounded card + square icon (left) + caption (right). */
    private makePropPill(parent: Node, key: string, caption: string, x: number, y: number, w: number, h: number, onClick: () => void): Node {
        const node = new Node('prop_' + key);
        node.layer = Layers.Enum.UI_2D;
        node.setParent(parent);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(w, h);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));
        const g = node.addComponent(Graphics);
        g.roundRect(-w / 2, -h / 2, w, h, 20);
        g.fillColor = C_BG;
        g.fill();
        g.lineWidth = 2;
        g.strokeColor = C_BORDER_EMPTY;
        g.roundRect(-w / 2, -h / 2, w, h, 20);
        g.stroke();

        const iconSize = h * 0.72;
        const iconX = -w / 2 + 10 + iconSize / 2;
        const iconNode = new Node('icon');
        iconNode.layer = Layers.Enum.UI_2D;
        iconNode.setParent(node);
        const iu = iconNode.addComponent(UITransform);
        iu.setContentSize(iconSize, iconSize);
        iu.setAnchorPoint(0.5, 0.5);
        iconNode.setPosition(new Vec3(iconX, 0, 0));
        const sp = iconNode.addComponent(Sprite);
        sp.type = Sprite.Type.SIMPLE;
        sp.sizeMode = Sprite.SizeMode.CUSTOM;
        if (this.propFrames[key]) sp.spriteFrame = this.propFrames[key];
        this.propSprites[key] = sp;

        const labLeft = iconX + iconSize / 2 + 4;
        const labRight = w / 2 - 8;
        const cap = new Node('cap');
        cap.layer = Layers.Enum.UI_2D;
        cap.setParent(node);
        const lu = cap.addComponent(UITransform);
        lu.setContentSize(labRight - labLeft, h);
        lu.setAnchorPoint(0.5, 0.5);
        cap.setPosition(new Vec3((labLeft + labRight) / 2, 0, 0));
        const lab = cap.addComponent(Label);
        lab.string = caption;
        lab.fontFamily = FONT_FAMILY;
        lab.fontSize = Math.floor(h * 0.28);
        lab.lineHeight = lab.fontSize;
        lab.horizontalAlign = Label.HorizontalAlign.CENTER;
        lab.verticalAlign = Label.VerticalAlign.CENTER;
        lab.color = C_TEXT_DARK;
        lab.isBold = true;

        node.on(Node.EventType.TOUCH_END, (e: EventTouch) => { e.propagationStopped = true; onClick(); }, this);
        return node;
    }

    private loadPropIcons() {
        for (const k of ['hint', 'reveal', 'addrow']) {
            resources.load('ui/' + k + '/spriteFrame', SpriteFrame, (err, sf) => {
                if (err || !sf) { console.warn('[WordTT] booster icon load failed:', k, err); return; }
                this.propFrames[k] = sf as SpriteFrame;
                if (this.propSprites[k]) this.propSprites[k].spriteFrame = sf as SpriteFrame;
            });
        }
    }

    private setAddRowDim(dim: boolean) {
        if (!this.addRowBtn) return;
        let op = this.addRowBtn.getComponent(UIOpacity);
        if (!op) op = this.addRowBtn.addComponent(UIOpacity);
        op.opacity = dim ? 110 : 255;
    }

    private addOutline(label: Label, color: Color, width: number) {
        const l = label as any;
        l.enableOutline = true;
        l.outlineColor = color;
        l.outlineWidth = width;
    }

    // ===== drawing ==========================================================

    private drawTile(cell: Cell, filled: boolean) {
        const g = cell.graphics;
        const s = cell.node.getComponent(UITransform)!.width;
        const half = s / 2;
        const radius = Math.min(20, s * 0.2);
        g.clear();
        let fill = C_BG;
        let border = filled ? C_BORDER_FILLED : C_BORDER_EMPTY;
        let stroke = true;
        switch (cell.state) {
            case LetterState.CORRECT: fill = C_CORRECT; stroke = false; break;
            case LetterState.PRESENT: fill = C_PRESENT; stroke = false; break;
            case LetterState.ABSENT: fill = C_ABSENT; stroke = false; break;
        }
        g.roundRect(-half, -half, s, s, radius);
        g.fillColor = fill;
        g.fill();
        if (stroke) {
            g.lineWidth = 3;
            g.strokeColor = border;
            g.roundRect(-half, -half, s, s, radius);
            g.stroke();
        }
        cell.label.color = cell.state === LetterState.CORRECT ? C_LTR_CORRECT
            : cell.state === LetterState.PRESENT ? C_LTR_PRESENT
            : cell.state === LetterState.ABSENT ? C_TEXT_LIGHT : C_TEXT_DARK;
    }

    private drawKey(key: Key) {
        const g = key.graphics;
        const hw = key.w / 2, hh = key.h / 2;
        const radius = Math.min(18, key.h * 0.22);
        g.clear();
        let fill = C_KEY;
        switch (key.state) {
            case LetterState.CORRECT: fill = C_CORRECT; break;
            case LetterState.PRESENT: fill = C_PRESENT; break;
            case LetterState.ABSENT: fill = C_ABSENT; break;
        }
        g.roundRect(-hw, -hh, key.w, key.h, radius);
        g.fillColor = fill;
        g.fill();
        key.label.color = key.state === LetterState.EMPTY ? C_TEXT_DARK : C_TEXT_LIGHT;
    }

    // ===== board layout =====================================================

    private layoutBoard(visibleRows: number) {
        this.maxRows = visibleRows;
        const colGap = this.boardColGap, rowGap = this.boardRowGap;
        const widthFit = (this.W * 0.88 - (COLS - 1) * colGap) / COLS;
        const regionH = this.boardRegionTop - this.boardRegionBottom;
        const heightFit = (regionH - (visibleRows - 1) * rowGap) / visibleRows;
        const tile = Math.max(40, Math.min(100, widthFit, heightFit));
        const boardW = COLS * tile + (COLS - 1) * colGap;
        const boardH = visibleRows * tile + (visibleRows - 1) * rowGap;
        const startX = -boardW / 2 + tile / 2;
        const boardCenterY = (this.boardRegionTop + this.boardRegionBottom) / 2;
        const firstRowY = boardCenterY + boardH / 2 - tile / 2;

        for (let r = 0; r < this.cells.length; r++) {
            const active = r < visibleRows;
            for (let c = 0; c < COLS; c++) {
                const cell = this.cells[r][c];
                cell.node.active = active;
                if (active) this.positionCell(cell, startX + c * (tile + colGap), firstRowY - r * (tile + rowGap), tile);
            }
        }
    }

    // ===== input ============================================================

    private onPhysicalKey(e: EventKeyboard) {
        if (!this.gameRoot || !this.gameRoot.active) return;
        const code = e.keyCode;
        if (code >= KeyCode.KEY_A && code <= KeyCode.KEY_Z) this.handleInput(String.fromCharCode(code));
        else if (code === KeyCode.ENTER || code === KeyCode.NUM_ENTER) this.handleInput('<');
        else if (code === KeyCode.BACKSPACE || code === KeyCode.DELETE) this.handleInput('>');
    }

    private onRootTap() {
        // Intentionally no-op: finishing a game must NOT restart on a blank tap.
    }

    private handleInput(id: string) {
        if (this.gameOver || this.animating) return;   // ignore input after the game is over or mid-animation
        if (id === '<') this.submitGuess();
        else if (id === '>') this.deleteLetter();
        else if (/^[A-Za-z]$/.test(id)) this.typeLetter(id.toUpperCase());
    }

    private typeLetter(ch: string) {
        const row = this.cells[this.curRow];
        for (let c = 0; c < COLS; c++) {
            if (row[c].label.string === '') {
                row[c].label.string = ch;
                this.drawTile(row[c], true);
                this.popTile(row[c]);
                this.saveState();
                return;
            }
        }
    }

    private deleteLetter() {
        const row = this.cells[this.curRow];
        for (let c = COLS - 1; c >= 0; c--) {
            if (row[c].label.string !== '' && this.revealed[c] === '') {
                row[c].label.string = '';
                this.drawTile(row[c], false);
                this.saveState();
                return;
            }
        }
    }

    private submitGuess() {
        const row = this.cells[this.curRow];
        for (let c = 0; c < COLS; c++) {
            if (row[c].label.string === '') { this.showMessage('Not enough letters', C_ABSENT); return; }
        }
        const guess = row.map(cell => cell.label.string.toLowerCase()).join('');
        if (!isValidGuess(guess)) { this.showMessage('Not in word list', C_ABSENT); this.shakeRow(this.curRow); return; }
        this.evaluateRow(guess);
    }

    private computeResult(guess: string): LetterState[] {
        const target = this.target;
        const result: LetterState[] = new Array(COLS).fill(LetterState.ABSENT);
        const counts: Record<string, number> = {};
        for (const c of target) counts[c] = (counts[c] || 0) + 1;
        for (let i = 0; i < COLS; i++) if (guess[i] === target[i]) { result[i] = LetterState.CORRECT; counts[guess[i]]--; }
        for (let i = 0; i < COLS; i++) if (result[i] !== LetterState.CORRECT && counts[guess[i]] > 0) { result[i] = LetterState.PRESENT; counts[guess[i]]--; }
        return result;
    }

    private evaluateRow(guess: string) {
        const result = this.computeResult(guess);
        this.history.push(result);
        const won = result.every(s => s === LetterState.CORRECT);
        if (won) this.timing = false;               // freeze the clock at the solve moment

        this.animating = true;
        this.revealRow(this.curRow, result, guess, () => {
            this.animating = false;
            this.afterRow(result);
        });
    }

    /** Sequential flip reveal: each tile flips shut, swaps to its color, flips open. */
    private revealRow(r: number, result: LetterState[], guess: string, done: () => void) {
        const row = this.cells[r];
        for (let c = 0; c < COLS; c++) {
            const cell = row[c];
            this.scheduleOnce(() => {
                tween(cell.node)
                    .to(0.12, { scale: new Vec3(1, 0.02, 1) })
                    .call(() => {
                        cell.state = result[c];
                        this.drawTile(cell, true);
                        this.updateKeyState(guess[c].toUpperCase(), result[c]);
                    })
                    .to(0.12, { scale: new Vec3(1, 1, 1) })
                    .start();
            }, c * 0.16);
        }
        this.scheduleOnce(done, COLS * 0.16 + 0.3);
    }

    private afterRow(result: LetterState[]) {
        if (result.every(s => s === LetterState.CORRECT)) {
            this.gameOver = true; this.timing = false;
            this.bounceRow(this.curRow);
            this.showMessage('Solved in ' + this.timerLabel!.string + '!', C_CORRECT, 0);
            this.onFinish(true, this.curRow + 1);
            this.saveState();
            return;
        }
        this.curRow++;
        if (this.curRow >= this.maxRows) {
            this.gameOver = true; this.timing = false;
            this.showMessage('Answer: ' + this.target.toUpperCase(), C_TEXT_DARK, 0);
            this.onFinish(false, this.maxRows);
        } else {
            this.applyRevealedToRow(this.curRow);
        }
        this.saveState();
    }

    private popTile(cell: Cell) {
        cell.node.setScale(1, 1, 1);
        tween(cell.node).to(0.05, { scale: new Vec3(1.1, 1.1, 1) }).to(0.06, { scale: new Vec3(1, 1, 1) }).start();
    }

    private shakeRow(r: number) {
        const row = this.cells[r];
        for (const cell of row) {
            tween(cell.node)
                .by(0.05, { position: new Vec3(-8, 0, 0) })
                .by(0.05, { position: new Vec3(16, 0, 0) })
                .by(0.05, { position: new Vec3(-16, 0, 0) })
                .by(0.05, { position: new Vec3(8, 0, 0) })
                .start();
        }
    }

    private bounceRow(r: number) {
        const row = this.cells[r];
        for (let c = 0; c < COLS; c++) {
            this.scheduleOnce(() => {
                tween(row[c].node)
                    .by(0.12, { position: new Vec3(0, 16, 0) })
                    .by(0.14, { position: new Vec3(0, -16, 0) })
                    .start();
            }, c * 0.08);
        }
    }

    private updateKeyState(letter: string, state: LetterState) {
        const key = this.keys.get(letter);
        if (key && state > key.state) { key.state = state; this.drawKey(key); }
    }

    private applyRevealedToRow(r: number) {
        const row = this.cells[r];
        for (let c = 0; c < COLS; c++) {
            if (this.revealed[c] !== '') {
                row[c].label.string = this.revealed[c];
                row[c].state = LetterState.CORRECT;
                this.drawTile(row[c], true);
            }
        }
    }

    // ===== boosters (props) =================================================

    private onProp(kind: string) {
        if (this.gameOver || this.adBusy) return;

        // pre-checks that must not burn an ad
        if (kind === 'hint' && this.hintCandidates().length === 0) {
            this.showMessage('No new letters to reveal', C_ABSENT); return;
        }
        if (kind === 'reveal' && this.revealCandidates().length === 0) {
            this.showMessage('All positions already known', C_ABSENT); return;
        }
        if (kind === 'addrow' && (this.addRowUsed || this.maxRows >= MAX_ROWS)) {
            this.showMessage('Extra row already used', C_ABSENT); return;
        }

        this.adBusy = true;
        this.showMessage('Loading ad…', C_ABSENT, 0);
        showRewardedAd(AD_UNIT).then(ok => {
            this.adBusy = false;
            if (!ok) { this.showMessage('Ad not completed', C_ABSENT); return; }
            if (kind === 'hint') this.applyHint();
            else if (kind === 'reveal') this.applyReveal();
            else if (kind === 'addrow') this.applyAddRow();
        });
    }

    /** Letters that are in the answer but not yet discovered (yellow/green) or hinted. */
    private hintCandidates(): string[] {
        const out: string[] = [];
        const seen: Record<string, boolean> = {};
        for (const ch of this.target) {
            const L = ch.toUpperCase();
            if (seen[L]) continue; seen[L] = true;
            const k = this.keys.get(L);
            if (k && k.state === LetterState.EMPTY) out.push(L);
        }
        return out;
    }

    private applyHint() {
        const cands = this.hintCandidates();
        if (cands.length === 0) { this.showMessage('No new letters to reveal', C_ABSENT); return; }
        const L = cands[Math.floor(Math.random() * cands.length)];
        const k = this.keys.get(L);
        if (k) { k.state = LetterState.PRESENT; this.drawKey(k); }
        this.hintLetters.push(L);
        this.showMessage('Hint: "' + L + '" is in the word', C_ACCENT);
        this.saveState();
    }

    /** Columns not yet solved (green) via guesses or reveals. */
    private revealCandidates(): number[] {
        const solved: boolean[] = new Array(COLS).fill(false);
        for (let c = 0; c < COLS; c++) if (this.revealed[c] !== '') solved[c] = true;
        for (const rowRes of this.history) for (let c = 0; c < COLS; c++) if (rowRes[c] === LetterState.CORRECT) solved[c] = true;
        const out: number[] = [];
        for (let c = 0; c < COLS; c++) if (!solved[c]) out.push(c);
        return out;
    }

    private applyReveal() {
        const cands = this.revealCandidates();
        if (cands.length === 0) { this.showMessage('All positions already known', C_ABSENT); return; }
        const c = cands[Math.floor(Math.random() * cands.length)];
        const L = this.target[c].toUpperCase();
        this.revealed[c] = L;
        const cell = this.cells[this.curRow][c];
        cell.label.string = L;
        cell.state = LetterState.CORRECT;
        this.drawTile(cell, true);
        const k = this.keys.get(L);
        if (k) { k.state = LetterState.CORRECT; this.drawKey(k); }
        this.showMessage('Revealed letter ' + (c + 1), C_ACCENT);
        this.saveState();
    }

    private applyAddRow() {
        if (this.addRowUsed || this.maxRows >= MAX_ROWS) return;
        this.addRowUsed = true;
        this.layoutBoard(this.maxRows + 1);
        const extra = this.cells[this.maxRows - 1];
        for (const cell of extra) {
            cell.state = LetterState.EMPTY;
            cell.label.string = '';
            this.drawTile(cell, false);
        }
        this.setAddRowDim(true);
        this.showMessage('+1 extra guess added!', C_ACCENT);
        this.saveState();
    }

    // ===== messages / timer =================================================

    private showMessage(str: string, color: Color, autoHide = 1.6) {
        if (!this.messageLabel) return;
        this.messageLabel.string = str;
        this.messageLabel.color = color;
        this.unschedule(this.hideMessage);
        if (autoHide > 0) this.scheduleOnce(this.hideMessage, autoHide);
    }
    private hideMessage = () => { if (this.messageLabel) this.messageLabel.string = ''; };

    private refreshTimer() { if (this.timerLabel) this.timerLabel.string = this.fmtTime(this.elapsed); }
    private fmtTime(sec: number): string {
        const t = Math.floor(sec), mm = Math.floor(t / 60), ss = t % 60;
        return (mm < 10 ? '0' : '') + mm + ':' + (ss < 10 ? '0' : '') + ss;
    }

    // ===== screens / lifecycle ==============================================

    private showMenu() {
        this.saveState();               // persist progress before leaving
        this.timing = false;
        if (this.mockRankRoot) this.mockRankRoot.active = false;
        if (this.gameRoot) this.gameRoot.active = false;
        if (this.menuRoot) this.menuRoot.active = true;
    }

    private startGame() {
        if (this.sidebarPopupRoot) this.sidebarPopupRoot.active = false;
        if (this.menuRoot) this.menuRoot.active = false;
        if (this.gameRoot) this.gameRoot.active = true;
        const saved = this.loadSave();
        if (saved && saved.day === puzzleId()) this.restoreState(saved);
        else this.startNewGame();
    }

    private startNewGame() {
        this.target = dailyWord();
        this.puzzleNo = puzzleId();
        this.curRow = 0;
        this.gameOver = false;
        this.elapsed = 0;
        this.timing = true;
        this.revealed = ['', '', '', '', ''];
        this.history = [];
        this.hintLetters = [];
        this.addRowUsed = false;
        this.adBusy = false;
        this.animating = false;
        this.refreshTimer();
        this.hideMessage();

        for (const row of this.cells) for (const cell of row) {
            cell.state = LetterState.EMPTY;
            cell.label.string = '';
        }
        this.keys.forEach(k => { k.state = LetterState.EMPTY; this.drawKey(k); });

        this.setAddRowDim(false);
        if (this.propsRoot) this.propsRoot.active = true;
        if (this.shareBtn) this.shareBtn.active = false;
        if (this.mockRankRoot) this.mockRankRoot.active = false;

        this.layoutBoard(BASE_ROWS); // resets tile sizes + redraws empty
        this.saveState();
        // console.log('[WordTT] answer:', this.target);
    }

    // ===== local save / restore =============================================

    private loadSave(): SaveState | null {
        try {
            const raw = sys.localStorage.getItem(SAVE_KEY);
            if (!raw) return null;
            return JSON.parse(raw) as SaveState;
        } catch (e) { return null; }
    }

    private saveState() {
        if (!this.target) return;
        const pending: string[] = [];
        if (!this.gameOver && this.curRow < this.maxRows) {
            const row = this.cells[this.curRow];
            for (let c = 0; c < COLS; c++) pending.push(row[c].label.string);
        }
        const guesses: string[] = [];
        for (let i = 0; i < this.history.length; i++) {
            guesses.push(this.cells[i].map(cell => cell.label.string.toLowerCase()).join(''));
        }
        const s: SaveState = {
            day: this.puzzleNo,
            guesses,
            revealed: this.revealed.slice(),
            hints: this.hintLetters.slice(),
            addRowUsed: this.addRowUsed,
            maxRows: this.maxRows,
            elapsed: this.elapsed,
            finished: this.gameOver,
            won: this.lastWon,
            pending,
        };
        try { sys.localStorage.setItem(SAVE_KEY, JSON.stringify(s)); } catch (e) { /* ignore */ }
    }

    private restoreState(s: SaveState) {
        this.target = dailyWord();
        this.puzzleNo = s.day;
        this.maxRows = s.maxRows || BASE_ROWS;
        this.addRowUsed = !!s.addRowUsed;
        this.revealed = (s.revealed && s.revealed.length === COLS) ? s.revealed.slice() : ['', '', '', '', ''];
        this.hintLetters = s.hints ? s.hints.slice() : [];
        this.elapsed = s.elapsed || 0;
        this.adBusy = false;
        this.animating = false;
        this.saveAccum = 0;

        // clear visuals
        for (const row of this.cells) for (const cell of row) { cell.state = LetterState.EMPTY; cell.label.string = ''; }
        this.keys.forEach(k => { k.state = LetterState.EMPTY; });
        this.layoutBoard(this.maxRows);

        // replay submitted guesses
        this.history = [];
        const guesses = s.guesses || [];
        for (let i = 0; i < guesses.length && i < this.maxRows; i++) {
            const g = guesses[i];
            const res = this.computeResult(g);
            const row = this.cells[i];
            for (let c = 0; c < COLS; c++) {
                row[c].label.string = (g[c] || '').toUpperCase();
                row[c].state = res[c];
                this.drawTile(row[c], true);
                this.updateKeyState((g[c] || '').toUpperCase(), res[c]);
            }
            this.history.push(res);
        }
        this.curRow = this.history.length;

        // re-apply hints (yellow) then revealed (green) on the keyboard
        for (const L of this.hintLetters) { const k = this.keys.get(L); if (k && k.state === LetterState.EMPTY) k.state = LetterState.PRESENT; }
        for (let c = 0; c < COLS; c++) { if (this.revealed[c]) { const k = this.keys.get(this.revealed[c]); if (k) k.state = LetterState.CORRECT; } }
        this.keys.forEach(k => this.drawKey(k));

        this.gameOver = !!s.finished;
        this.lastWon = !!s.won;
        this.lastRows = this.lastWon ? this.history.length : this.maxRows;

        if (!this.gameOver && this.curRow < this.maxRows) {
            this.applyRevealedToRow(this.curRow);
            const pending = s.pending || [];
            const row = this.cells[this.curRow];
            for (let c = 0; c < COLS; c++) {
                if (pending[c]) {
                    row[c].label.string = pending[c];
                    if (row[c].state === LetterState.EMPTY) this.drawTile(row[c], true);
                }
            }
        }

        this.setAddRowDim(this.addRowUsed);
        if (this.mockRankRoot) this.mockRankRoot.active = false;

        if (this.gameOver) {
            this.timing = false;
            if (this.lastWon) this.showMessage('Solved in ' + this.fmtTime(this.elapsed) + '!', C_CORRECT, 0);
            else this.showMessage('Answer: ' + this.target.toUpperCase(), C_TEXT_DARK, 0);
            if (this.propsRoot) this.propsRoot.active = false;
            if (this.shareBtn) this.shareBtn.active = true;
        } else {
            this.timing = true;
            this.hideMessage();
            if (this.propsRoot) this.propsRoot.active = true;
            if (this.shareBtn) this.shareBtn.active = false;
        }
        this.refreshTimer();
    }

    private onFinish(won: boolean, rows: number) {
        this.lastWon = won;
        this.lastRows = rows;
        if (this.propsRoot) this.propsRoot.active = false;
        if (this.shareBtn) this.shareBtn.active = true;
        if (won) {
            const value = JSON.stringify({ d: this.puzzleNo, r: rows, t: Math.floor(this.elapsed) });
            setUserCloudStorage([{ key: DAILY_KEY, value }]);
        }
    }

    // ===== share ============================================================

    private buildShareGrid(): string {
        const em = ['⬜', '⬜', '🟨', '🟩']; // absent/empty=⬜ present=🟨 correct=🟩
        return this.history.map(r => r.map(s => em[s]).join('')).join('\n');
    }

    private onShare() {
        const rowsStr = this.lastRows + '/' + this.maxRows;
        const time = this.fmtTime(this.elapsed);
        const title = this.lastWon
            ? 'I cracked WordTT #' + this.puzzleNo + ' in ' + rowsStr + ' (' + time + ')!'
            : 'WordTT #' + this.puzzleNo + ' beat me today…';
        const subtitle = this.lastWon ? 'Fewer rows = smarter. Can you beat me?' : 'Think you can solve it?';
        if (!canShare()) { this.showMessage('Sharing needs a newer app version', C_ABSENT); return; }
        console.log('[WordTT] share grid:\n' + this.buildShareGrid());
        if (!isTikTok()) { this.showMessage('Preview only · real share opens on TikTok', C_ABSENT); return; }
        shareAppMessage({ title, subtitle, query: 'day=' + this.puzzleNo, templateType: 1 })
            .then(ok => this.showMessage(ok ? 'Shared!' : 'Share cancelled', ok ? C_ACCENT : C_ABSENT));
    }

    // ===== leaderboard ======================================================

    private onRank() {
        authorizeOpenContext().then(ok => {
            if (!ok) { this.showMessage('Authorize friends to see the leaderboard', C_ABSENT); return; }
            const posted = postToOpenData({ type: 'show', day: this.puzzleNo, key: DAILY_KEY });
            if (!posted) this.showMockLeaderboard();  // editor preview / not on TikTok
        });
    }

    private buildMockLeaderboard() {
        const W = this.W, H = this.H;
        const root = new Node('mockRank');
        root.layer = Layers.Enum.UI_2D;
        root.setParent(this.node);

        // dim background
        const dim = new Node('dim');
        dim.layer = Layers.Enum.UI_2D;
        dim.setParent(root);
        const du = dim.addComponent(UITransform);
        du.setContentSize(W, H);
        du.setAnchorPoint(0.5, 0.5);
        dim.setPosition(Vec3.ZERO);
        const dg = dim.addComponent(Graphics);
        dg.roundRect(-W / 2, -H / 2, W, H, 0);
        dg.fillColor = new Color(C_DIM.r, C_DIM.g, C_DIM.b, 190);
        dg.fill();
        dim.on(Node.EventType.TOUCH_END, (e: EventTouch) => { e.propagationStopped = true; root.active = false; }, this);

        // panel
        const pw = W * 0.86, ph = H * 0.6;
        const panel = new Node('panel');
        panel.layer = Layers.Enum.UI_2D;
        panel.setParent(root);
        const pu = panel.addComponent(UITransform);
        pu.setContentSize(pw, ph);
        pu.setAnchorPoint(0.5, 0.5);
        panel.setPosition(Vec3.ZERO);
        const pg = panel.addComponent(Graphics);
        pg.roundRect(-pw / 2, -ph / 2, pw, ph, 28);
        pg.fillColor = C_BG;
        pg.fill();

        this.makeText('Friends  ·  Today', 0, ph / 2 - 46, 34, C_ACCENT, true, panel);

        const fake = [
            { n: 'You', r: 3, t: '01:12' },
            { n: 'Alex', r: 3, t: '01:45' },
            { n: 'Mia', r: 4, t: '02:03' },
            { n: 'Sam', r: 5, t: '01:38' },
            { n: 'Kai', r: 6, t: '03:20' },
        ];
        let y = ph / 2 - 110;
        for (let i = 0; i < fake.length; i++) {
            const f = fake[i];
            const line = (i + 1) + '.  ' + f.n + '    ' + f.r + ' rows · ' + f.t;
            this.makeText(line, 0, y, 26, i === 0 ? C_CORRECT : C_TEXT_DARK, i === 0, panel);
            y -= 52;
        }
        this.makeText('(preview mock — real friends show on TikTok)', 0, -ph / 2 + 78, 18, C_ABSENT, false, panel);
        this.makeButton(panel, 'Close', 0, -ph / 2 + 40, pw * 0.4, 52, C_ACCENT, 16, () => { root.active = false; });

        root.active = false;
        this.mockRankRoot = root;
    }

    private showMockLeaderboard() {
        if (this.mockRankRoot) this.mockRankRoot.active = true;
    }
}
