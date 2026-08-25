import {
    _decorator,
    BlockInputEvents,
    Button,
    Color,
    Component,
    director,
    EventTouch,
    find,
    Graphics,
    Label,
    Node,
    Tween,
    tween,
    UITransform,
    Vec3,
} from "cc";
import { BottleView } from "./BottleView";
import {
    Attempt,
    BOTTLE_PALETTE,
    ROUND_COUNT,
    SLOT_COUNT,
    countMatches,
    createSecret,
    createStartingGuess,
    isPerfect,
    swap,
} from "./core/Rules";
import { followTikTokLanguageAsync, getLang, initLocale, Lang, setLang } from "./core/Locale";
import { hostLine, t, tf } from "./core/I18n";

const { ccclass } = _decorator;

const LAYER_UI = 1 << 25;
const INK = new Color(36, 22, 28, 255);
const CREAM = new Color(255, 244, 228, 255);
const PINK = new Color(255, 112, 148, 255);
const TEAL = new Color(72, 196, 176, 255);
const MUTED = new Color(140, 118, 128, 255);
const GOLD = new Color(255, 204, 96, 255);

@ccclass("GameApp")
export class GameApp extends Component {
    private canvas: Node | null = null;
    private menuRoot: Node | null = null;
    private playRoot: Node | null = null;
    private resultRoot: Node | null = null;
    private toastLabel: Label | null = null;

    private hudLabel: Label | null = null;
    private bubbleLabel: Label | null = null;
    private bottleViews: BottleView[] = [];
    private matchViews: BottleView[] = [];
    private matchLid: Node | null = null;
    private historyRoot: Node | null = null;
    private settingsPanel: Node | null = null;

    private secret: number[] = [];
    private guess: number[] = [];
    private attempts: Attempt[] = [];
    private selected = -1;
    private round = 1;
    private totalAsks = 0;
    private busy = false;
    private revealed = false;

    onLoad(): void {
        this.canvas = this.findCanvas();
        if (!this.canvas) {
            console.error("[AlmostPerfect] Canvas not found");
            return;
        }
        this.node.layer = LAYER_UI;
        this.node.setParent(this.canvas);
        this.node.setPosition(Vec3.ZERO);
        this.node.addComponent(UITransform).setContentSize(750, 1624);
        this.drawBackdrop();
        initLocale();
        this.showMenu();
        followTikTokLanguageAsync(() => this.applyLocale());
    }

    private findCanvas(): Node | null {
        return (
            find("Canvas") ||
            this.node.scene?.getChildByName("Canvas") ||
            director.getScene()?.getChildByName("Canvas") ||
            null
        );
    }

    private showMenu(): void {
        this.clearPlay();
        if (this.resultRoot) {
            this.resultRoot.active = false;
        }
        if (this.menuRoot && this.menuRoot.isValid) {
            this.menuRoot.active = true;
            this.menuRoot.setSiblingIndex(-1);
            this.applyLocale();
            return;
        }
        const overlay = this.makeNode("Menu", this.canvas!, 0, 0, 750, 1624);
        this.menuRoot = overlay;
        const dimNode = this.makeNode("Dim", overlay, 0, 0, 750, 1624);
        const dim = dimNode.addComponent(Graphics);
        dim.fillColor = new Color(28, 14, 24, 210);
        dim.rect(-375, -812, 750, 1624);
        dim.fill();
        dimNode.addComponent(BlockInputEvents);

        const card = this.makeNode("Card", overlay, 0, 220, 680, 440);
        const cardG = card.addComponent(Graphics);
        cardG.fillColor = new Color(20, 10, 18, 230);
        cardG.roundRect(-340, -220, 680, 440, 28);
        cardG.fill();
        this.makeLabel("Brand", "ALMOST PERFECT", 48, CREAM, 0, 150, 620, 64, card);
        this.makeLabel("Tag", t("menuTag"), 24, new Color(230, 210, 200, 255), 0, 40, 600, 90, card);
        this.drawMiniBottles(card, 0, -130);

        this.makeButton("Solo", t("solo"), 0, -140, 360, 92, PINK, () => {
            overlay.active = false;
            this.startSolo();
        }, overlay);

        this.makeButton("Duo", t("duo"), 0, -270, 360, 100, MUTED, () => {
            this.flashToast(t("duoToast"));
        }, overlay, t("comingSoon"));

        this.toastLabel = this.makeLabel("Toast", "", 22, GOLD, 0, -400, 620, 40, overlay);

        this.makeButton("Settings", t("settings"), 250, -720, 200, 72, TEAL, () => {
            this.showSettings(true);
        }, overlay);

        this.buildSettingsPanel(overlay);
        this.applyLocale();
    }

    private startSolo(): void {
        this.round = 1;
        this.totalAsks = 0;
        if (this.resultRoot) {
            this.resultRoot.active = false;
        }
        this.ensurePlayfield();
        this.applyLocale();
        this.beginRound();
    }

    private ensurePlayfield(): void {
        if (this.playRoot && this.playRoot.isValid) {
            this.playRoot.active = true;
            return;
        }
        const root = this.makeNode("Play", this.canvas!, 0, 0, 750, 1624);
        this.playRoot = root;

        this.drawHost(root);
        this.hudLabel = this.makeLabel("Hud", "", 26, CREAM, 0, 690, 700, 40, root);
        this.bubbleLabel = this.makeLabel(
            "Bubble",
            t("bubbleAsk"),
            28,
            INK,
            70,
            470,
            420,
            80,
            root,
        );

        this.makeLabel("TableTitle", t("dragHint"), 20, new Color(255, 220, 200, 200), 0, 250, 520, 28, root);
        this.spawnPlayBottles(root);

        this.makeButton("Ask", t("askHost"), 0, 20, 300, 80, GOLD, () => {
            this.askHost();
        }, root);

        this.makeLabel("MatchTitle", t("matchTitle"), 20, new Color(255, 220, 200, 200), 0, -70, 620, 28, root);
        this.spawnMatchRow(root);
        this.historyRoot = this.makeNode("History", root, 0, -430, 700, 360);

        this.makeButton("Back", t("menuBack"), -250, -720, 180, 64, MUTED, () => {
            this.showMenu();
        }, root);
    }

    private beginRound(): void {
        this.secret = createSecret();
        this.guess = createStartingGuess(this.secret);
        this.attempts = [];
        this.selected = -1;
        this.busy = false;
        this.revealed = false;
        this.bottleViews.forEach((view) => {
            Tween.stopAllByTarget(view.node);
            view.snapHome();
        });
        this.refreshBottles();
        this.refreshMatchRow(false);
        this.renderHistory();
        this.setBubble(t("bubbleAsk"));
        this.refreshHud();
    }

    private spawnPlayBottles(parent: Node): void {
        this.bottleViews = [];
        const gap = 128;
        const startX = -gap * 2;
        for (let i = 0; i < SLOT_COUNT; i++) {
            const x = startX + i * gap;
            const node = this.makeNode(`Bottle-${i}`, parent, x, 150, 110, 170);
            const view = node.addComponent(BottleView);
            view.setup(i, 0, 1);
            view.setTappable(true);
            view.onDragStart = (slot) => this.onBottleDragStart(slot);
            view.onDragMove = (slot, pos) => this.onBottleDragMove(slot, pos);
            view.onDragEnd = (slot, pos) => this.onBottleDragEnd(slot, pos);
            this.bottleViews.push(view);
        }
    }

    private spawnMatchRow(parent: Node): void {
        this.matchViews = [];
        const gap = 108;
        const startX = -gap * 2;
        const board = this.makeNode("MatchBoard", parent, 0, -165, 640, 130);
        const g = board.addComponent(Graphics);
        g.fillColor = new Color(28, 16, 24, 220);
        g.roundRect(-320, -60, 640, 120, 24);
        g.fill();
        for (let i = 0; i < SLOT_COUNT; i++) {
            const x = startX + i * gap;
            const node = this.makeNode(`Match-${i}`, board, x, 0, 90, 120);
            const view = node.addComponent(BottleView);
            view.setup(i, 0, 0.72);
            view.setTappable(false);
            view.node.active = false;
            this.matchViews.push(view);
        }
        const lid = this.makeNode("Lid", board, 0, 0, 600, 100);
        this.matchLid = lid;
        const lidG = lid.addComponent(Graphics);
        lidG.fillColor = new Color(58, 36, 48, 255);
        lidG.roundRect(-280, -40, 560, 80, 16);
        lidG.fill();
        this.makeLabel("LidText", t("lidCovered"), 26, new Color(210, 190, 180, 230), 0, 0, 400, 40, lid);
    }

    private onBottleDragStart(slot: number): boolean {
        if (this.busy || this.revealed) {
            return false;
        }
        this.selected = slot;
        this.refreshSelection();
        return true;
    }

    private onBottleDragMove(from: number, pos: Vec3): void {
        const over = this.slotNear(pos, from);
        for (let i = 0; i < this.bottleViews.length; i++) {
            if (i === from) {
                continue;
            }
            this.bottleViews[i].setSelected(i === over);
        }
    }

    private onBottleDragEnd(from: number, pos: Vec3): void {
        const target = this.slotNear(pos, -1);
        const fromView = this.bottleViews[from];
        if (target < 0 || target === from) {
            this.tweenHome(fromView);
            this.selected = -1;
            this.refreshSelection();
            return;
        }
        this.busy = true;
        const toView = this.bottleViews[target];
        const fromRest = fromView.restPos.clone();
        const toRest = toView.restPos.clone();
        swap(this.guess, from, target);
        this.bottleViews[from] = toView;
        this.bottleViews[target] = fromView;
        toView.slotIndex = from;
        fromView.slotIndex = target;
        toView.restPos.set(fromRest);
        fromView.restPos.set(toRest);
        let done = 0;
        const finish = () => {
            done += 1;
            if (done < 2) {
                return;
            }
            fromView.snapHome();
            toView.snapHome();
            this.selected = -1;
            this.busy = false;
            this.refreshBottles();
            this.setBubble(t("bubbleSwapped"));
        };
        tween(fromView.node)
            .to(0.16, { position: toRest }, { easing: "quadOut" })
            .call(finish)
            .start();
        tween(toView.node)
            .to(0.16, { position: fromRest }, { easing: "quadOut" })
            .call(finish)
            .start();
    }

    private slotNear(pos: Vec3, ignore: number): number {
        let best = -1;
        let bestDist = 88;
        for (let i = 0; i < this.bottleViews.length; i++) {
            if (i === ignore) {
                continue;
            }
            const rest = this.bottleViews[i].restPos;
            const dist = Math.hypot(pos.x - rest.x, pos.y - rest.y);
            if (dist < bestDist) {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }

    private tweenHome(view: BottleView): void {
        tween(view.node)
            .to(0.14, { position: view.restPos.clone() }, { easing: "quadOut" })
            .call(() => view.snapHome())
            .start();
    }

    private askHost(): void {
        if (this.busy || this.revealed) {
            return;
        }
        this.busy = true;
        const correct = countMatches(this.secret, this.guess);
        this.attempts.push({ guess: [...this.guess], correct });
        this.totalAsks += 1;
        this.refreshHud();
        this.renderHistory();
        this.setBubble(hostLine(correct));

        if (isPerfect(this.secret, this.guess)) {
            this.revealed = true;
            this.refreshMatchRow(true);
            this.flashHostWin(() => {
                this.busy = false;
                this.onRoundCleared();
            });
            return;
        }
        this.busy = false;
    }

    private onRoundCleared(): void {
        if (this.round >= ROUND_COUNT) {
            this.showSoloResult();
            return;
        }
        this.round += 1;
        this.beginRound();
        this.setBubble(t("bubbleNext"));
    }

    private showSoloResult(): void {
        if (this.playRoot) {
            this.playRoot.active = false;
        }
        if (!this.resultRoot || !this.resultRoot.isValid) {
            const overlay = this.makeNode("Result", this.canvas!, 0, 0, 750, 1624);
            this.resultRoot = overlay;
            const g = overlay.addComponent(Graphics);
            g.fillColor = new Color(20, 10, 18, 220);
            g.rect(-375, -812, 750, 1624);
            g.fill();
            this.makeLabel("WinTitle", t("winTitle"), 56, GOLD, 0, 180, 600, 70, overlay);
            this.makeLabel("WinBody", "", 28, CREAM, 0, 40, 640, 160, overlay);
            this.makeButton("Again", t("playAgain"), 0, -180, 300, 84, PINK, () => {
                overlay.active = false;
                this.startSolo();
            }, overlay);
            this.makeButton("Menu", t("backMenu"), 0, -290, 300, 84, TEAL, () => {
                this.showMenu();
            }, overlay);
        }
        this.resultRoot.active = true;
        this.resultRoot.setSiblingIndex(-1);
        const body = this.resultRoot.getChildByName("WinBody")?.getComponent(Label);
        if (body) {
            body.string = tf("winBody", this.totalAsks, ROUND_COUNT);
        }
    }

    private flashHostWin(done: () => void): void {
        const host = this.playRoot?.getChildByName("Host");
        if (!host) {
            done();
            return;
        }
        tween(host)
            .to(0.12, { scale: new Vec3(1.12, 1.12, 1) })
            .to(0.18, { scale: new Vec3(1, 1, 1) })
            .delay(1.15)
            .call(done)
            .start();
    }

    private refreshBottles(): void {
        for (let i = 0; i < this.bottleViews.length; i++) {
            this.bottleViews[i].setColor(this.guess[i]);
        }
        this.refreshSelection();
    }

    private refreshSelection(): void {
        for (let i = 0; i < this.bottleViews.length; i++) {
            this.bottleViews[i].setSelected(i === this.selected);
        }
    }

    private refreshMatchRow(show: boolean): void {
        if (this.matchLid && this.matchLid.isValid) {
            this.matchLid.active = !show;
        }
        for (let i = 0; i < this.matchViews.length; i++) {
            this.matchViews[i].node.active = show;
            if (show) {
                this.matchViews[i].setHidden(false);
                this.matchViews[i].setColor(this.secret[i]);
            }
        }
    }

    private renderHistory(): void {
        if (!this.historyRoot) {
            return;
        }
        this.historyRoot.removeAllChildren();
        const recent = this.attempts.slice(-3).reverse();
        recent.forEach((attempt, row) => {
            const y = 120 - row * 100;
            const line = this.makeNode(`H-${row}`, this.historyRoot!, 0, y, 680, 88);
            const g = line.addComponent(Graphics);
            g.fillColor = new Color(40, 24, 34, 200);
            g.roundRect(-330, -40, 660, 80, 16);
            g.fill();
            const gap = 78;
            const startX = -230;
            for (let i = 0; i < attempt.guess.length; i++) {
                const node = this.makeNode(`hb-${i}`, line, startX + i * gap, 0, 70, 80);
                const view = node.addComponent(BottleView);
                view.setup(i, attempt.guess[i], 0.42);
                view.setTappable(false);
            }
            const color = attempt.correct === SLOT_COUNT ? GOLD : CREAM;
            this.makeLabel(
                "n",
                hostLine(attempt.correct),
                22,
                color,
                230,
                0,
                180,
                40,
                line,
            );
        });
        if (this.attempts.length === 0) {
            this.makeLabel(
                "Empty",
                t("historyEmpty"),
                22,
                new Color(210, 190, 180, 200),
                0,
                80,
                600,
                40,
                this.historyRoot,
            );
        }
    }

    private refreshHud(): void {
        if (this.hudLabel) {
            this.hudLabel.string = tf("hud", this.round, ROUND_COUNT, this.totalAsks);
        }
    }

    private setBubble(text: string): void {
        if (this.bubbleLabel) {
            this.bubbleLabel.string = text;
        }
    }

    private flashToast(text: string): void {
        if (this.toastLabel) {
            this.toastLabel.string = text;
        }
    }

    private clearPlay(): void {
        if (this.playRoot && this.playRoot.isValid) {
            this.playRoot.active = false;
        }
    }

    private drawBackdrop(): void {
        const bg = this.makeNode("Backdrop", this.canvas!, 0, 0, 750, 1624);
        const g = bg.addComponent(Graphics);
        g.fillColor = new Color(48, 28, 42, 255);
        g.rect(-375, -812, 750, 1624);
        g.fill();
        g.fillColor = new Color(92, 44, 58, 255);
        g.rect(-375, -812, 750, 520);
        g.fill();
        g.fillColor = new Color(160, 96, 72, 255);
        g.roundRect(-340, 70, 680, 220, 28);
        g.fill();
        g.fillColor = new Color(120, 70, 54, 255);
        g.rect(-340, 70, 680, 24);
        g.fill();
    }

    private drawHost(parent: Node): void {
        const node = this.makeNode("Host", parent, -220, 500, 160, 200);
        const g = node.addComponent(Graphics);
        g.fillColor = PINK;
        g.circle(0, 48, 26);
        g.fill();
        g.roundRect(-28, -8, 56, 64, 16);
        g.fill();
        g.fillColor = INK;
        g.circle(-8, 52, 4);
        g.fill();
        g.circle(10, 52, 4);
        g.fill();
        g.fillColor = CREAM;
        g.roundRect(40, 20, 70, 56, 16);
        g.fill();
        this.makeLabel("HostTag", t("hostName"), 18, CREAM, 0, -78, 100, 24, node);
    }

    private drawMiniBottles(parent: Node, x: number, y: number): void {
        BOTTLE_PALETTE.forEach((color, i) => {
            const node = this.makeNode(`mini-${i}`, parent, x + (i - 2) * 72, y, 70, 110);
            const view = node.addComponent(BottleView);
            view.setup(i, color.id, 0.7);
            view.setTappable(false);
        });
    }

    private showSettings(on: boolean): void {
        if (!this.settingsPanel || !this.settingsPanel.isValid) {
            return;
        }
        this.settingsPanel.active = on;
        if (on) {
            this.settingsPanel.setSiblingIndex(-1);
            this.refreshLangButtons();
        }
    }

    private buildSettingsPanel(parent: Node): void {
        const panel = this.makeNode("SettingsPanel", parent, 0, 0, 750, 1624);
        this.settingsPanel = panel;
        panel.active = false;
        const dim = this.makeNode("Dim", panel, 0, 0, 750, 1624);
        const dimG = dim.addComponent(Graphics);
        dimG.fillColor = new Color(12, 6, 10, 200);
        dimG.rect(-375, -812, 750, 1624);
        dimG.fill();
        dim.addComponent(BlockInputEvents);
        dim.on(Node.EventType.TOUCH_END, () => this.showSettings(false), this);

        const card = this.makeNode("Card", panel, 0, 40, 560, 540);
        const cardG = card.addComponent(Graphics);
        cardG.fillColor = new Color(36, 20, 30, 255);
        cardG.roundRect(-280, -270, 560, 540, 28);
        cardG.fill();
        this.makeLabel("Title", t("language"), 36, CREAM, 0, 200, 480, 48, card);
        this.makeButton("LangEn", "English", 0, 90, 360, 80, MUTED, () => this.pickLang("en"), card);
        this.makeButton("LangZh", "中文", 0, -10, 360, 80, MUTED, () => this.pickLang("zh"), card);
        this.makeButton("LangJa", "日本語", 0, -110, 360, 80, MUTED, () => this.pickLang("ja"), card);
        this.makeButton("Close", t("close"), 0, -220, 240, 72, GOLD, () => {
            this.showSettings(false);
        }, card);
    }

    private pickLang(lang: Lang): void {
        setLang(lang);
        this.applyLocale();
    }

    private refreshLangButtons(): void {
        const card = this.settingsPanel?.getChildByName("Card");
        if (!card) {
            return;
        }
        const cur = getLang();
        const rows: Array<[string, Lang]> = [
            ["LangEn", "en"],
            ["LangZh", "zh"],
            ["LangJa", "ja"],
        ];
        rows.forEach(([name, lang]) => {
            const node = card.getChildByName(name);
            const g = node?.getComponent(Graphics);
            if (!node || !g) {
                return;
            }
            g.clear();
            g.fillColor = lang === cur ? GOLD : MUTED;
            g.roundRect(-180, -40, 360, 80, 18);
            g.fill();
        });
    }

    private applyLocale(): void {
        const menu = this.menuRoot;
        if (menu && menu.isValid) {
            this.setNamedLabel(menu.getChildByName("Card"), "Tag", t("menuTag"));
            this.setButtonCopy(menu.getChildByName("Solo"), t("solo"));
            this.setButtonCopy(menu.getChildByName("Duo"), t("duo"), t("comingSoon"));
            this.setButtonCopy(menu.getChildByName("Settings"), t("settings"));
            const settingsCard = this.settingsPanel?.getChildByName("Card");
            this.setNamedLabel(settingsCard, "Title", t("language"));
            this.setButtonCopy(settingsCard?.getChildByName("Close") || null, t("close"));
            this.refreshLangButtons();
        }
        const play = this.playRoot;
        if (play && play.isValid) {
            this.setNamedLabel(play, "TableTitle", t("dragHint"));
            this.setNamedLabel(play, "MatchTitle", t("matchTitle"));
            this.setButtonCopy(play.getChildByName("Ask"), t("askHost"));
            this.setButtonCopy(play.getChildByName("Back"), t("menuBack"));
            this.setNamedLabel(play.getChildByName("Host"), "HostTag", t("hostName"));
            this.setNamedLabel(this.matchLid, "LidText", t("lidCovered"));
            this.refreshHud();
            this.renderHistory();
        }
        const result = this.resultRoot;
        if (result && result.isValid) {
            this.setNamedLabel(result, "WinTitle", t("winTitle"));
            this.setNamedLabel(result, "WinBody", tf("winBody", this.totalAsks, ROUND_COUNT));
            this.setButtonCopy(result.getChildByName("Again"), t("playAgain"));
            this.setButtonCopy(result.getChildByName("Menu"), t("backMenu"));
        }
    }

    private setButtonCopy(node: Node | null, text: string, sub?: string): void {
        if (!node) {
            return;
        }
        this.setNamedLabel(node, "Text", text);
        if (sub) {
            this.setNamedLabel(node, "Sub", sub);
        }
    }

    private setNamedLabel(parent: Node | null, name: string, text: string): void {
        const label = parent?.getChildByName(name)?.getComponent(Label);
        if (label) {
            label.string = text;
        }
    }

    private makeNode(
        name: string,
        parent: Node,
        x: number,
        y: number,
        w: number,
        h: number,
    ): Node {
        const node = new Node(name);
        node.layer = LAYER_UI;
        const transform = node.addComponent(UITransform);
        transform.setContentSize(w, h);
        transform.setAnchorPoint(0.5, 0.5);
        parent.addChild(node);
        node.setPosition(x, y, 0);
        return node;
    }

    private makeLabel(
        name: string,
        text: string,
        size: number,
        color: Color,
        x: number,
        y: number,
        w: number,
        h: number,
        parent?: Node,
    ): Label {
        const node = this.makeNode(name, parent || this.canvas!, x, y, w, h);
        const label = node.addComponent(Label);
        label.string = text;
        label.fontSize = size;
        label.lineHeight = Math.round(size * 1.25);
        label.color = color;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.overflow = Label.Overflow.SHRINK;
        label.enableWrapText = true;
        label.useSystemFont = true;
        label.fontFamily = "Arial";
        return label;
    }

    private makeButton(
        name: string,
        text: string,
        x: number,
        y: number,
        w: number,
        h: number,
        color: Color,
        onClick: () => void,
        parent?: Node,
        subText?: string,
    ): Node {
        const node = this.makeNode(name, parent || this.canvas!, x, y, w, h);
        const g = node.addComponent(Graphics);
        g.fillColor = color;
        g.roundRect(-w * 0.5, -h * 0.5, w, h, 18);
        g.fill();

        const titleY = subText ? 14 : 0;
        this.makeLabel("Text", text, 30, INK, 0, titleY, w - 24, 40, node);
        if (subText) {
            this.makeLabel("Sub", subText, 18, CREAM, 0, -22, w - 24, 28, node);
        }

        const button = node.addComponent(Button);
        button.transition = Button.Transition.SCALE;
        button.zoomScale = 0.96;
        button.interactable = true;
        button.target = node;
        node.on(Node.EventType.TOUCH_END, (ev: EventTouch) => {
            ev.propagationStopped = true;
            onClick();
        }, this, true);
        return node;
    }
}
