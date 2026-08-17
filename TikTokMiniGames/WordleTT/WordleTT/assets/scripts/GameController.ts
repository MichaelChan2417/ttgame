import {
    _decorator, Component, Node, Label, UITransform, Graphics, Color, Vec3,
    view, ResolutionPolicy, input, Input, KeyCode, EventKeyboard, EventTouch, Layers,
} from 'cc';
import { isValidGuess, randomAnswer } from './WordList';

const { ccclass, property } = _decorator;

/** Number of guesses allowed. */
const MAX_ROWS = 6;
/** Letters per word. */
const COLS = 5;

/** Tile / key evaluation states. */
enum LetterState {
    EMPTY = 0,
    ABSENT = 1,
    PRESENT = 2,
    CORRECT = 3,
}

// ---- Palette (light theme) -------------------------------------------------
const C_BG = new Color(255, 255, 255);
const C_BORDER_EMPTY = new Color(211, 214, 218);
const C_BORDER_FILLED = new Color(135, 138, 140);
const C_ABSENT = new Color(120, 124, 126);
const C_PRESENT = new Color(201, 180, 88);
const C_CORRECT = new Color(106, 170, 100);
const C_KEY = new Color(211, 214, 218);
const C_TEXT_DARK = new Color(30, 30, 30);
const C_TEXT_LIGHT = new Color(255, 255, 255);

interface Cell {
    node: Node;
    graphics: Graphics;
    label: Label;
    state: LetterState;
}

interface Key {
    node: Node;
    graphics: Graphics;
    label: Label;
    w: number;
    h: number;
    state: LetterState;
}

@ccclass('GameController')
export class GameController extends Component {
    private W = 720;
    private H = 1280;

    private cells: Cell[][] = [];
    private keys: Map<string, Key> = new Map();

    private target = '';
    private curRow = 0;
    private curCol = 0;
    private guess = '';
    private gameOver = false;

    private messageLabel: Label | null = null;
    private msgTimer = 0;

    private timerLabel: Label | null = null;
    private elapsed = 0;
    private timing = false;

    onLoad() {
        // Force a portrait design resolution (phone / TikTok mini-game), fitting
        // the width so the full board and keyboard are always on screen.
        view.setDesignResolutionSize(720, 1280, ResolutionPolicy.FIT_WIDTH);

        // Use the visible (logical) canvas size so the layout adapts to the
        // project's design resolution and to any phone aspect ratio.
        const vs = view.getVisibleSize();
        if (vs.width > 0 && vs.height > 0) {
            this.W = vs.width;
            this.H = vs.height;
        } else {
            const uit = this.getComponent(UITransform);
            if (uit && uit.width > 0) {
                this.W = uit.width;
                this.H = uit.height;
            }
        }
        this.buildUI();
        this.startNewGame();
    }

    update(dt: number) {
        if (this.timing && !this.gameOver) {
            this.elapsed += dt;
            this.refreshTimer();
        }
    }

    private refreshTimer() {
        if (!this.timerLabel) return;
        const total = Math.floor(this.elapsed);
        const mm = Math.floor(total / 60);
        const ss = total % 60;
        this.timerLabel.string = (mm < 10 ? '0' : '') + mm + ':' + (ss < 10 ? '0' : '') + ss;
    }

    onEnable() {
        input.on(Input.EventType.KEY_DOWN, this.onPhysicalKey, this);
        this.node.on(Node.EventType.TOUCH_END, this.onRootTap, this);
    }

    onDisable() {
        input.off(Input.EventType.KEY_DOWN, this.onPhysicalKey, this);
        this.node.off(Node.EventType.TOUCH_END, this.onRootTap, this);
    }

    // ---- UI construction ---------------------------------------------------

    private buildUI() {
        const W = this.W;
        const H = this.H;

        // Title
        const titleY = H * 0.5 - 58;
        this.makeText('WORDLE TT', 0, titleY, 44, C_TEXT_DARK, true, this.node);

        // Timer (top)
        const timerY = titleY - 52;
        this.timerLabel = this.makeText('00:00', 0, timerY, 34, C_ABSENT, true, this.node);

        // Message / toast label
        const messageY = timerY - 44;
        this.messageLabel = this.makeText('', 0, messageY, 28, C_CORRECT, true, this.node);

        // ---- Board geometry ----
        const colGap = 12;               // horizontal gap between tiles
        const rowGap = 20;               // vertical gap between rows (larger)
        const tile = Math.min(100, (W * 0.88 - (COLS - 1) * colGap) / COLS);
        const boardW = COLS * tile + (COLS - 1) * colGap;
        const boardH = MAX_ROWS * tile + (MAX_ROWS - 1) * rowGap;
        const startX = -boardW / 2 + tile / 2;

        // ---- Keyboard geometry (compute first so we can center the board) ----
        const rows = ['QWERTYUIOP', 'ASDFGHJKL', '<ZXCVBNM>']; // < = ENTER, > = DEL
        const kGap = 8;
        const keyW = (W * 0.96 - (10 - 1) * kGap) / 10;
        const keyH = Math.min(keyW * 1.5, 96);
        const wideW = keyW * 1.5 + kGap / 2;
        const kbRows = rows.length;
        const kbBottomMargin = H * 0.03;
        const kbBottomRowY = -H * 0.5 + kbBottomMargin + keyH / 2; // center y of bottom row
        const kbTopEdge = kbBottomRowY + (kbRows - 1) * (keyH + kGap) + keyH / 2;

        // ---- Vertically center the board between the message and the keyboard ----
        const regionTop = messageY - 34;
        const regionBottom = kbTopEdge + 24;
        const boardCenterY = (regionTop + regionBottom) / 2;
        const firstRowY = boardCenterY + boardH / 2 - tile / 2; // center y of the first row

        for (let r = 0; r < MAX_ROWS; r++) {
            const row: Cell[] = [];
            const cy = firstRowY - r * (tile + rowGap);
            for (let c = 0; c < COLS; c++) {
                const cx = startX + c * (tile + colGap);
                row.push(this.makeCell(cx, cy, tile));
            }
            this.cells.push(row);
        }

        // ---- Keyboard ----
        for (let r = 0; r < rows.length; r++) {
            const chars = rows[r].split('');
            const ky = kbBottomRowY + (rows.length - 1 - r) * (keyH + kGap);
            // compute total row width
            let total = 0;
            const widths: number[] = [];
            for (const ch of chars) {
                const w = (ch === '<' || ch === '>') ? wideW : keyW;
                widths.push(w);
                total += w;
            }
            total += (chars.length - 1) * kGap;
            let x = -total / 2;
            for (let i = 0; i < chars.length; i++) {
                const w = widths[i];
                const cx = x + w / 2;
                const ch = chars[i];
                const face = ch === '<' ? 'ENTER' : ch === '>' ? 'DEL' : ch;
                this.makeKey(ch, face, cx, ky, w, keyH);
                x += w + kGap;
            }
        }
    }

    private makeCell(x: number, y: number, size: number): Cell {
        const node = new Node('cell');
        node.layer = Layers.Enum.UI_2D;
        node.setParent(this.node);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(size, size);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));
        const g = node.addComponent(Graphics);

        const lblNode = new Node('lbl');
        lblNode.layer = Layers.Enum.UI_2D;
        lblNode.setParent(node);
        const lu = lblNode.addComponent(UITransform);
        lu.setContentSize(size, size);
        lu.setAnchorPoint(0.5, 0.5);
        lblNode.setPosition(Vec3.ZERO);
        const label = lblNode.addComponent(Label);
        label.string = '';
        label.fontSize = Math.floor(size * 0.52);
        label.lineHeight = Math.floor(size * 0.52);
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = C_TEXT_DARK;
        label.isBold = true;

        const cell: Cell = { node, graphics: g, label, state: LetterState.EMPTY };
        this.drawTile(cell, false);
        return cell;
    }

    private makeKey(id: string, face: string, x: number, y: number, w: number, h: number): Key {
        const node = new Node('key_' + id);
        node.layer = Layers.Enum.UI_2D;
        node.setParent(this.node);
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
        label.fontSize = face.length > 1 ? Math.floor(h * 0.26) : Math.floor(h * 0.42);
        label.lineHeight = label.fontSize;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = C_TEXT_DARK;
        label.isBold = true;

        const key: Key = { node, graphics: g, label, w, h, state: LetterState.EMPTY };
        this.drawKey(key);

        node.on(Node.EventType.TOUCH_END, (e: EventTouch) => {
            e.propagationStopped = true;
            this.handleInput(id);
        }, this);

        this.keys.set(id, key);
        return key;
    }

    private makeText(str: string, x: number, y: number, size: number, color: Color, bold: boolean, parent: Node): Label {
        const node = new Node('text');
        node.layer = Layers.Enum.UI_2D;
        node.setParent(parent);
        const uit = node.addComponent(UITransform);
        uit.setContentSize(this.W * 0.9, size * 1.6);
        uit.setAnchorPoint(0.5, 0.5);
        node.setPosition(new Vec3(x, y, 0));
        const label = node.addComponent(Label);
        label.string = str;
        label.fontSize = size;
        label.lineHeight = size * 1.1;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.color = color;
        label.isBold = bold;
        return label;
    }

    // ---- Drawing -----------------------------------------------------------

    private drawTile(cell: Cell, filled: boolean) {
        const g = cell.graphics;
        const s = cell.node.getComponent(UITransform)!.width;
        const half = s / 2;
        g.clear();
        let fill = C_BG;
        let border = filled ? C_BORDER_FILLED : C_BORDER_EMPTY;
        let stroke = true;
        switch (cell.state) {
            case LetterState.CORRECT: fill = C_CORRECT; stroke = false; break;
            case LetterState.PRESENT: fill = C_PRESENT; stroke = false; break;
            case LetterState.ABSENT: fill = C_ABSENT; stroke = false; break;
        }
        g.roundRect(-half, -half, s, s, 6);
        g.fillColor = fill;
        g.fill();
        if (stroke) {
            g.lineWidth = 3;
            g.strokeColor = border;
            g.roundRect(-half, -half, s, s, 6);
            g.stroke();
        }
    }

    private drawKey(key: Key) {
        const g = key.graphics;
        const hw = key.w / 2;
        const hh = key.h / 2;
        g.clear();
        let fill = C_KEY;
        switch (key.state) {
            case LetterState.CORRECT: fill = C_CORRECT; break;
            case LetterState.PRESENT: fill = C_PRESENT; break;
            case LetterState.ABSENT: fill = C_ABSENT; break;
        }
        g.roundRect(-hw, -hh, key.w, key.h, 8);
        g.fillColor = fill;
        g.fill();
        key.label.color = key.state === LetterState.EMPTY ? C_TEXT_DARK : C_TEXT_LIGHT;
    }

    // ---- Input handling ----------------------------------------------------

    private onPhysicalKey(e: EventKeyboard) {
        const code = e.keyCode;
        if (code >= KeyCode.KEY_A && code <= KeyCode.KEY_Z) {
            this.handleInput(String.fromCharCode(code));
        } else if (code === KeyCode.ENTER || code === KeyCode.NUM_ENTER) {
            this.handleInput('<');
        } else if (code === KeyCode.BACKSPACE || code === KeyCode.DELETE) {
            this.handleInput('>');
        }
    }

    private onRootTap() {
        if (this.gameOver) {
            this.startNewGame();
        }
    }

    private handleInput(id: string) {
        if (this.gameOver) {
            this.startNewGame();
            return;
        }
        if (id === '<') {
            this.submitGuess();
        } else if (id === '>') {
            this.deleteLetter();
        } else if (/^[A-Za-z]$/.test(id)) {
            this.typeLetter(id.toUpperCase());
        }
    }

    private typeLetter(ch: string) {
        if (this.curCol >= COLS) return;
        const cell = this.cells[this.curRow][this.curCol];
        cell.label.string = ch;
        this.guess += ch.toLowerCase();
        this.drawTile(cell, true);
        this.curCol++;
    }

    private deleteLetter() {
        if (this.curCol <= 0) return;
        this.curCol--;
        const cell = this.cells[this.curRow][this.curCol];
        cell.label.string = '';
        this.guess = this.guess.slice(0, -1);
        this.drawTile(cell, false);
    }

    private submitGuess() {
        if (this.curCol < COLS) {
            this.showMessage('Not enough letters', C_ABSENT);
            return;
        }
        if (!isValidGuess(this.guess)) {
            this.showMessage('Not in word list', C_ABSENT);
            return;
        }
        this.evaluateRow();
    }

    private evaluateRow() {
        const target = this.target;
        const guess = this.guess;
        const result: LetterState[] = new Array(COLS).fill(LetterState.ABSENT);
        const counts: Record<string, number> = {};
        for (const c of target) counts[c] = (counts[c] || 0) + 1;

        for (let i = 0; i < COLS; i++) {
            if (guess[i] === target[i]) {
                result[i] = LetterState.CORRECT;
                counts[guess[i]]--;
            }
        }
        for (let i = 0; i < COLS; i++) {
            if (result[i] !== LetterState.CORRECT && counts[guess[i]] > 0) {
                result[i] = LetterState.PRESENT;
                counts[guess[i]]--;
            }
        }

        const row = this.cells[this.curRow];
        for (let i = 0; i < COLS; i++) {
            row[i].state = result[i];
            this.drawTile(row[i], true);
            this.updateKeyState(guess[i].toUpperCase(), result[i]);
        }

        const won = result.every(s => s === LetterState.CORRECT);
        if (won) {
            this.gameOver = true;
            this.showMessage('You got it! Tap to play again', C_CORRECT, 0);
            return;
        }

        this.curRow++;
        this.curCol = 0;
        this.guess = '';
        if (this.curRow >= MAX_ROWS) {
            this.gameOver = true;
            this.showMessage(this.target.toUpperCase() + ' — Tap to play again', C_TEXT_DARK, 0);
        }
    }

    private updateKeyState(letter: string, state: LetterState) {
        const key = this.keys.get(letter);
        if (!key) return;
        if (state > key.state) {          // upgrade priority: EMPTY<ABSENT<PRESENT<CORRECT
            key.state = state;
            this.drawKey(key);
        }
    }

    // ---- Messages ----------------------------------------------------------

    private showMessage(str: string, color: Color, autoHide = 1.4) {
        if (!this.messageLabel) return;
        this.messageLabel.string = str;
        this.messageLabel.color = color;
        this.unschedule(this.hideMessage);
        if (autoHide > 0) {
            this.scheduleOnce(this.hideMessage, autoHide);
        }
    }

    private hideMessage = () => {
        if (this.messageLabel) this.messageLabel.string = '';
    };

    // ---- Game lifecycle ----------------------------------------------------

    private startNewGame() {
        this.target = randomAnswer();
        this.curRow = 0;
        this.curCol = 0;
        this.guess = '';
        this.gameOver = false;
        this.elapsed = 0;
        this.timing = true;
        this.refreshTimer();
        this.hideMessage();

        for (const row of this.cells) {
            for (const cell of row) {
                cell.state = LetterState.EMPTY;
                cell.label.string = '';
                this.drawTile(cell, false);
            }
        }
        this.keys.forEach(key => {
            key.state = LetterState.EMPTY;
            this.drawKey(key);
        });
        // console.log('[WordleTT] answer:', this.target);
    }
}
