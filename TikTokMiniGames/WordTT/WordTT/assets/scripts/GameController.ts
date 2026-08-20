import {
    _decorator, Component, Node, Label, LabelOutline, UITransform, Graphics, Color, Vec3,
    view, ResolutionPolicy, input, Input, KeyCode, EventKeyboard, EventTouch, Layers, sys,
    Sprite, SpriteFrame, Mask,
} from 'cc';
import { isValidGuess } from './WordList';
import { dailyWord, puzzleId } from './Daily';
import {
    isTikTok, showRewardedAd, shareAppMessage, canShare,
    authorizeOpenContext, setUserCloudStorage, postToOpenData, navigateToSidebar,
} from './Platform';

const { ccclass, property } = _decorator;

const COLS = 5;
const BASE_ROWS = 6;   // normal guesses
const MAX_ROWS = 7;    // hard cap once the +1-row booster is used
const DESIGN_W = 720;
const DESIGN_H = 1280;

/** Rewarded-ad slot id — configure in the TikTok dev portal before release. */
const AD_UNIT = 'YOUR_REWARDED_AD_UNIT_ID';
/** Cloud-storage key for the friend leaderboard. */
const DAILY_KEY = 'wordtt_daily';
/** Local-storage key for saving today's in-progress game. */
const SAVE_KEY = 'wordtt_save_v2';

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
const C_BG = new Color(255, 255, 255);
const C_BORDER_EMPTY = new Color(214, 217, 222);
const C_BORDER_FILLED = new Color(150, 154, 158);
const C_ABSENT = new Color(120, 124, 126);
const C_PRESENT = new Color(224, 176, 76);
const C_CORRECT = new Color(96, 176, 108);
const C_KEY = new Color(224, 227, 231);
const C_TEXT_DARK = new Color(38, 40, 46);
const C_TEXT_LIGHT = new Color(255, 255, 255);
const C_HALO = new Color(255, 255, 255);
const C_ACCENT = new Color(99, 102, 241);
const C_ACCENT_DARK = new Color(67, 56, 202);
const C_ADD = new Color(240, 150, 60);       // add-to-desktop (warm) accent
const C_DIM = new Color(20, 22, 30);
const C_SHADOW = new Color(28, 32, 40, 40);
const C_SHADOW_SOFT = new Color(28, 32, 40, 18);
const C_CARD = new Color(232, 235, 240);

const FONT_FAMILY = 'Poppins, Nunito, "Trebuchet MS", Verdana, sans-serif';

interface Cell { node: Node; graphics: Graphics; label: Label; state: LetterState; }
interface Key { node: Node; graphics: Graphics; label: Label; w: number; h: number; state: LetterState; }

@ccclass('GameController')
export class GameController extends Component {
    @property(SpriteFrame)
    hintIcon: SpriteFrame | null = null;
    @property(SpriteFrame)
    revealIcon: SpriteFrame | null = null;
    @property(SpriteFrame)
    addRowIcon: SpriteFrame | null = null;

    private W = DESIGN_W;
    private H = DESIGN_H;
    private topInset = 0;      // safe-area inset (notch / Dynamic Island)
    private bottomInset = 0;   // safe-area inset (home indicator)
    private uiBuilt = false;
    private stage: Node | null = null;
    private boardRoot: Node | null = null;
    private kbRoot: Node | null = null;

    private menuRoot: Node | null = null;
    private gameRoot: Node | null = null;
    private propsRoot: Node | null = null;
    private shareBtn: Node | null = null;
    private addRowBtnSprite: Sprite | null = null;
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
        this.W = DESIGN_W;
        this.H = DESIGN_H;
        view.setDesignResolutionSize(DESIGN_W, DESIGN_H, ResolutionPolicy.SHOW_ALL);
        view.on('canvas-resize', this.onCanvasResize, this);
        this.buildOnce();
    }

    start() { this.fitStage(); }

    onDestroy() {
        view.off('canvas-resize', this.onCanvasResize, this);
    }

    private onCanvasResize = () => this.fitStage();

    private buildOnce() {
        if (this.uiBuilt) return;
        this.uiBuilt = true;
        this.setupStage();
        this.computeSafeArea();
        this.buildUI();
        this.showMenu();
        this.fitStage();
    }

    /** All UI lives on a 720×1280 stage, uniformly scaled to fit the real canvas. */
    private setupStage() {
        this.stage = this.makeUiLayer('stage', this.node);
        this.fitStage();
    }

    private fitStage() {
        if (!this.stage) return;
        const uit = this.getComponent(UITransform);
        const vs = view.getVisibleSize();
        const cw = (uit && uit.width > 1) ? uit.width : vs.width;
        const ch = (uit && uit.height > 1) ? uit.height : vs.height;
        if (cw <= 1 || ch <= 1) return;
        const s = Math.min(cw / DESIGN_W, ch / DESIGN_H);
        this.stage.setScale(s, s, 1);
        this.stage.setPosition(0, 0, 0);
    }

    private makeUiLayer(name: string, parent: Node): Node {
        const n = new Node(name);
        n.layer = Layers.Enum.UI_2D;
        n.setParent(parent);
        const uit = n.addComponent(UITransform);
        uit.setContentSize(this.W, this.H);
        uit.setAnchorPoint(0.5, 0.5);
        n.setPosition(Vec3.ZERO);
        return n;
    }

    private computeSafeArea() {
        this.topInset = 0;
        this.bottomInset = 0;
        try {
            const vs = view.getVisibleSize();
            // Editor / landscape previews are not phones — don't steal layout with a bogus inset.
            if (!vs || vs.height < vs.width) return;
            const sa = (sys as any).getSafeAreaRect ? (sys as any).getSafeAreaRect() : null;
            if (sa && sa.height > 0) {
                this.topInset = Math.min(Math.max(0, this.H - (sa.y + sa.height) * (this.H / vs.height)), this.H * 0.10);
                this.bottomInset = Math.min(Math.max(0, sa.y * (this.H / vs.height)), this.H * 0.08);
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
        const root = this.stage!;
        this.menuRoot = this.makeUiLayer('menu', root);
        this.gameRoot = this.makeUiLayer('game', root);

        this.buildMenu();
        this.buildGame();
        this.buildMockLeaderboard();
        this.buildSidebarPopup();
    }

    private buildMenu() {
        const W = this.W, H = this.H, parent = this.menuRoot!;
        const title = this.makeText('WORD TT', 0, H * 0.18, 76, C_ACCENT, true, parent);
        this.addOutline(title, C_ACCENT_DARK, 5);
        this.makeText('Daily Challenge  ·  #' + puzzleId(), 0, H * 0.18 - 70, 30, C_ABSENT, false, parent);
        this.makeButton(parent, 'PLAY', 0, -H * 0.02, W * 0.52, 104, C_ACCENT, 26, () => this.startGame());
        this.makeText('Same word for everyone, every day', 0, -H * 0.02 - 84, 24, C_ABSENT, false, parent);

        // Small side button (lower-right) → opens the "Add to Sidebar" popup
        this.makeButton(parent, 'SIDEBAR', W * 0.5 - 84, -H * 0.20, 132, 70, C_ADD, 18, () => this.showSidebarPopup());
    }

    private buildSidebarPopup() {
        const W = this.W, H = this.H;
        const root = new Node('sidebarPopup');
        root.layer = Layers.Enum.UI_2D;
        root.setParent(this.stage!);
        const ru = root.addComponent(UITransform);
        ru.setContentSize(W, H);
        ru.setAnchorPoint(0.5, 0.5);

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
        this.boardRoot = this.makeUiLayer('board', parent);
        this.kbRoot = this.makeUiLayer('keyboard', parent);
        this.propsRoot = this.makeUiLayer('props', parent);

        // top button row (Menu / Rank) — pushed below the notch / Dynamic Island
        const buttonsY = H * 0.5 - this.topInset - 58;
        this.makeButton(parent, 'Menu', -W * 0.5 + 72, buttonsY, 104, 58, C_ABSENT, 18, () => this.showMenu());
        this.makeButton(parent, 'Rank', W * 0.5 - 72, buttonsY, 104, 58, C_ACCENT, 18, () => this.onRank());

        // title sits fully BELOW the button row
        const titleY = H * 0.5 - this.topInset - 132;
        const gTitle = this.makeText('WORD TT', 0, titleY, 40, C_ACCENT, true, parent);
        this.addOutline(gTitle, C_ACCENT_DARK, 3);

        const timerY = titleY - 50;
        this.timerLabel = this.makeText('00:00', 0, timerY, 34, C_TEXT_DARK, true, parent);
        const messageY = timerY - 42;
        this.messageLabel = this.makeText('', 0, messageY, 27, C_CORRECT, true, parent);

        // ---- keyboard geometry ----
        const rows = ['QWERTYUIOP', 'ASDFGHJKL', '<ZXCVBNM>'];
        const kGap = 8;
        const keyW = (W * 0.96 - 9 * kGap) / 10;
        const keyH = Math.min(keyW * 1.35, 76);
        const wideW = keyW * 1.5 + kGap / 2;
        const kbRows = rows.length;
        const kbBottomMargin = H * 0.028 + this.bottomInset;
        const kbBottomRowY = -H * 0.5 + kbBottomMargin + keyH / 2;
        const kbTopEdge = kbBottomRowY + (kbRows - 1) * (keyH + kGap) + keyH / 2;

        // ---- booster (prop) icon buttons + share button, just above keyboard ----
        const propSize = 110;
        const propGap = 20;
        const propY = kbTopEdge + 14 + propSize / 2;
        this.makeIconButton(this.propsRoot, this.hintIcon, 'HINT', -(propSize + propGap), propY, propSize, () => this.onProp('hint'));
        this.makeIconButton(this.propsRoot, this.revealIcon, 'REVEAL', 0, propY, propSize, () => this.onProp('reveal'));
        this.addRowBtnSprite = this.makeIconButton(
            this.propsRoot, this.addRowIcon, '+ROW', (propSize + propGap), propY, propSize, () => this.onProp('addrow'),
        ).sprite;

        const share = this.makeButton(parent, 'SHARE  ▸  CHALLENGE', 0, propY, W * 0.9, 64, C_CORRECT, 16, () => this.onShare());
        this.shareBtn = share.node;
        this.shareBtn.active = false;

        // ---- board region: between the message and the booster row ----
        this.boardRegionTop = messageY - 24;
        this.boardRegionBottom = propY + propSize / 2 + 12;

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
        this.boardRoot.setSiblingIndex(0);
        this.propsRoot.setSiblingIndex(this.gameRoot!.children.length - 1);
        this.kbRoot.setSiblingIndex(this.gameRoot!.children.length - 1);
    }

    // ===== element factories ================================================

    private makeCell(): Cell {
        const node = new Node('cell');
        node.layer = Layers.Enum.UI_2D;
        node.setParent(this.boardRoot!);
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
        node.setParent(this.kbRoot!);
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
        this.paintButtonChrome(g, w, h, radius, bg, false);

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

    private makeIconButton(parent: Node, frame: SpriteFrame | null, fallback: string,
                           x: number, y: number, size: number, onClick: () => void): { node: Node; sprite: Sprite | null } {
        if (!frame) {
            const btn = this.makeButton(parent, fallback, x, y, size * 1.4, Math.max(64, size * 0.55), C_ACCENT, 16, onClick);
            return { node: btn.node, sprite: null };
        }
        const radius = Math.round(size * 0.22);
        const node = new Node('iconBtn_' + fallback);
        node.layer = Layers.Enum.UI_2D;
        node.setParent(parent);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(size, size);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));

        const chrome = new Node('chrome');
        chrome.layer = Layers.Enum.UI_2D;
        chrome.setParent(node);
        const cu = chrome.addComponent(UITransform);
        cu.setContentSize(size, size);
        cu.setAnchorPoint(0.5, 0.5);
        chrome.setPosition(Vec3.ZERO);
        this.paintButtonChrome(chrome.addComponent(Graphics), size, size, radius, C_CARD, false);

        const maskNode = new Node('mask');
        maskNode.layer = Layers.Enum.UI_2D;
        maskNode.setParent(node);
        const mu = maskNode.addComponent(UITransform);
        mu.setContentSize(size, size);
        mu.setAnchorPoint(0.5, 0.5);
        maskNode.setPosition(Vec3.ZERO);
        const mask = maskNode.addComponent(Mask);
        mask.type = Mask.Type.GRAPHICS_STENCIL;
        const stencil = maskNode.getComponent(Graphics) || maskNode.addComponent(Graphics);
        stencil.clear();
        stencil.roundRect(-size / 2, -size / 2, size, size, radius);
        stencil.fillColor = Color.WHITE;
        stencil.fill();

        const icon = new Node('icon');
        icon.layer = Layers.Enum.UI_2D;
        icon.setParent(maskNode);
        const iu = icon.addComponent(UITransform);
        iu.setContentSize(size, size);
        iu.setAnchorPoint(0.5, 0.5);
        icon.setPosition(Vec3.ZERO);
        const sprite = icon.addComponent(Sprite);
        sprite.sizeMode = Sprite.SizeMode.CUSTOM;
        sprite.trim = false;
        sprite.type = Sprite.Type.SIMPLE;
        sprite.spriteFrame = frame;

        const rim = new Node('rim');
        rim.layer = Layers.Enum.UI_2D;
        rim.setParent(node);
        const ru = rim.addComponent(UITransform);
        ru.setContentSize(size, size);
        ru.setAnchorPoint(0.5, 0.5);
        rim.setPosition(Vec3.ZERO);
        const rg = rim.addComponent(Graphics);
        rg.clear();
        rg.lineWidth = 2;
        rg.strokeColor = C_BORDER_EMPTY;
        rg.roundRect(-size / 2, -size / 2, size, size, radius);
        rg.stroke();

        node.on(Node.EventType.TOUCH_END, (e: EventTouch) => { e.propagationStopped = true; onClick(); }, this);
        return { node, sprite };
    }

    /** Bottom-weighted drop shadow + optional hairline so white plates lift off the game bg. */
    private paintButtonChrome(g: Graphics, w: number, h: number, radius: number, fill: Color, bordered: boolean) {
        g.clear();
        const drop = Math.max(5, Math.round(h * 0.07));
        g.roundRect(-w / 2, -h / 2 - drop, w, h, radius);
        g.fillColor = C_SHADOW_SOFT;
        g.fill();
        g.roundRect(-w / 2, -h / 2 - drop * 0.55, w, h, radius);
        g.fillColor = C_SHADOW;
        g.fill();
        g.roundRect(-w / 2, -h / 2, w, h, radius);
        g.fillColor = fill;
        g.fill();
        if (bordered) {
            g.lineWidth = 2;
            g.strokeColor = C_BORDER_EMPTY;
            g.roundRect(-w / 2, -h / 2, w, h, radius);
            g.stroke();
        }
    }

    private setAddRowUsedVisual(used: boolean) {
        if (!this.addRowBtnSprite) return;
        this.addRowBtnSprite.color = used ? new Color(255, 255, 255, 120) : new Color(255, 255, 255, 255);
    }

    private addOutline(label: Label, color: Color, width: number) {
        const ol = label.node.addComponent(LabelOutline);
        ol.color = color;
        ol.width = width;
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
        const heightFit = regionH > 0 ? (regionH - (visibleRows - 1) * rowGap) / visibleRows : 24;
        const tile = Math.max(24, Math.min(96, widthFit, heightFit));
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
        if (this.gameOver) return;   // ignore input after the game is over
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
        if (!isValidGuess(guess)) { this.showMessage('Not in word list', C_ABSENT); return; }
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
        const row = this.cells[this.curRow];
        for (let i = 0; i < COLS; i++) {
            row[i].state = result[i];
            this.drawTile(row[i], true);
            this.updateKeyState(guess[i].toUpperCase(), result[i]);
        }
        this.history.push(result);

        if (result.every(s => s === LetterState.CORRECT)) {
            this.gameOver = true; this.timing = false;
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
        this.applyRevealedToRow(this.maxRows - 1); // in case a row was newly shown
        this.setAddRowUsedVisual(true);
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
        this.refreshTimer();
        this.hideMessage();

        for (const row of this.cells) for (const cell of row) {
            cell.state = LetterState.EMPTY;
            cell.label.string = '';
        }
        this.keys.forEach(k => { k.state = LetterState.EMPTY; this.drawKey(k); });

        this.setAddRowUsedVisual(false);
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
            sys.localStorage.removeItem('wordtt_save_v1');
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

        this.setAddRowUsedVisual(this.addRowUsed);
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
        root.setParent(this.stage!);
        const ru = root.addComponent(UITransform);
        ru.setContentSize(W, H);
        ru.setAnchorPoint(0.5, 0.5);

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
