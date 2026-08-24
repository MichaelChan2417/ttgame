import {
    _decorator,
    Button,
    Color,
    Component,
    director,
    find,
    Graphics,
    Label,
    Node,
    tween,
    UIOpacity,
    UITransform,
    Vec3,
    Widget,
} from "cc";
import { FishView } from "./FishView";
import {
    createMatchFish,
    judgeMatch,
    playerName,
    ToyFish,
    PlayerId,
} from "./core/Rules";

const { ccclass } = _decorator;

const LAYER_UI = 1 << 25;
const P1 = new Color(255, 122, 92, 255);
const P2 = new Color(80, 214, 196, 255);
const INK = new Color(18, 32, 44, 255);
const CREAM = new Color(247, 236, 210, 255);

@ccclass("GameApp")
export class GameApp extends Component {
    private canvas: Node | null = null;
    private hudLabel: Label | null = null;
    private hintLabel: Label | null = null;
    private resultRoot: Node | null = null;
    private resultLabel: Label | null = null;
    private fishViews = new Map<number, FishView>();
    private fish: ToyFish[] = [];
    private turn: PlayerId = 0;
    private busy = false;
    private started = false;
    private elapsed = 0;

    onLoad(): void {
        this.canvas = this.findCanvas();
        if (!this.canvas) {
            console.error("[FishOff] Canvas not found");
            return;
        }
        this.node.layer = LAYER_UI;
        this.node.setParent(this.canvas);
        this.node.setPosition(Vec3.ZERO);
        this.node.addComponent(UITransform).setContentSize(750, 1624);
        this.buildWorld();
        this.showTitle();
    }

    update(dt: number): void {
        if (!this.started) {
            return;
        }
        this.elapsed += dt;
        this.fishViews.forEach((view) => view.tickIdle(this.elapsed));
    }

    private findCanvas(): Node | null {
        return (
            find("Canvas") ||
            this.node.scene?.getChildByName("Canvas") ||
            director.getScene()?.getChildByName("Canvas") ||
            null
        );
    }

    private buildWorld(): void {
        this.drawBackdrop();
        this.drawPlayers();
        this.hudLabel = this.makeLabel("Hud", "", 34, CREAM, 0, 690, 700, 56);
        this.hintLabel = this.makeLabel(
            "Hint",
            "Tap a toy fish. Length stays hidden until you hook it.",
            22,
            new Color(210, 226, 232, 255),
            0,
            640,
            680,
            40,
        );
    }

    private showTitle(): void {
        const overlay = this.makeNode("Title", this.canvas!, 0, 0, 750, 1624);
        overlay.addComponent(UIOpacity).opacity = 255;
        const dim = overlay.addComponent(Graphics);
        dim.fillColor = new Color(8, 24, 38, 210);
        dim.rect(-375, -812, 750, 1624);
        dim.fill();

        this.makeLabel("Brand", "FISH OFF", 78, CREAM, 0, 180, 700, 90, overlay);
        this.makeLabel(
            "Tag",
            "Two players. One pile of toy fish.\nHook it. Stretch it. Longer wins.",
            26,
            new Color(196, 220, 228, 255),
            0,
            60,
            640,
            80,
            overlay,
        );
        this.makeButton("Play", "START MATCH", 0, -160, 320, 88, P1, () => {
            overlay.destroy();
            this.beginMatch();
        }, overlay);
    }

    private beginMatch(): void {
        this.started = true;
        this.busy = false;
        this.turn = 0;
        this.elapsed = 0;
        this.fish = createMatchFish();
        this.clearFish();
        this.spawnFishPile();
        this.refreshHud();
        if (this.resultRoot) {
            this.resultRoot.active = false;
        }
    }

    private spawnFishPile(): void {
        const originY = -70;
        this.fish.forEach((item, index) => {
            const col = index % 4;
            const row = Math.floor(index / 4);
            const x = -228 + col * 152;
            const y = originY - row * 118;
            const node = this.makeNode(`Fish-${item.id}`, this.canvas!, x, y, 200, 90);
            const view = node.addComponent(FishView);
            view.baseY = y;
            view.setup(item.id, item.hue, index * 0.7);
            view.onHook = (id) => this.tryCatch(id);
            this.fishViews.set(item.id, view);
        });
    }

    private tryCatch(id: number): void {
        if (this.busy || !this.started) {
            return;
        }
        const target = this.fish.find((f) => f.id === id);
        if (!target || target.caughtBy !== null) {
            return;
        }
        this.busy = true;
        target.caughtBy = this.turn;
        const view = this.fishViews.get(id);
        if (!view) {
            this.busy = false;
            return;
        }
        view.setHooked();
        const player = this.turn;
        const dest = player === 0 ? new Vec3(-230, 360, 0) : new Vec3(230, 360, 0);
        const slot = this.fish.filter((f) => f.caughtBy === player).length - 1;
        dest.y -= slot * 46;

        if (this.hintLabel) {
            this.hintLabel.string = `${playerName(player)} hooked one...`;
        }

        tween(view.node)
            .to(0.28, { position: dest }, { easing: "quadOut" })
            .call(() => {
                const stretch = 0.35 + (target.lengthCm - 18) / 70;
                const anim = { t: 1 };
                tween(anim)
                    .to(0.45, { t: stretch }, {
                        easing: "backOut",
                        onUpdate: () => view.setStretch(anim.t),
                    })
                    .call(() => {
                        if (this.hintLabel) {
                            this.hintLabel.string = `${target.lengthCm} cm`;
                        }
                        this.finishCatch();
                    })
                    .start();
            })
            .start();
    }

    private finishCatch(): void {
        const remaining = this.fish.some((f) => f.caughtBy === null);
        if (!remaining) {
            this.showResult();
            this.busy = false;
            return;
        }
        this.turn = this.turn === 0 ? 1 : 0;
        this.refreshHud();
        this.busy = false;
    }

    private showResult(): void {
        const result = judgeMatch(this.fish);
        if (!this.resultRoot) {
            this.resultRoot = this.makeNode("Result", this.canvas!, 0, 0, 750, 1624);
            const g = this.resultRoot.addComponent(Graphics);
            g.fillColor = new Color(8, 20, 32, 200);
            g.rect(-375, -812, 750, 1624);
            g.fill();
            this.resultLabel = this.makeLabel(
                "ResultText",
                "",
                36,
                CREAM,
                0,
                80,
                640,
                280,
                this.resultRoot,
            );
            this.makeButton("Again", "PLAY AGAIN", 0, -220, 300, 84, P2, () => {
                this.beginMatch();
            }, this.resultRoot);
        }
        this.resultRoot.active = true;
        this.resultRoot.setSiblingIndex(-1);
        let headline = "TIE — same longest fish";
        if (result.winner === 0) headline = "PLAYER 1 WINS";
        if (result.winner === 1) headline = "PLAYER 2 WINS";
        if (this.resultLabel) {
            this.resultLabel.string =
                `${headline}\n\n` +
                `P1 longest ${result.p1Longest} cm · pile ${result.p1Total} cm\n` +
                `P2 longest ${result.p2Longest} cm · pile ${result.p2Total} cm\n\n` +
                `Winner is whoever hooked the longest fish.`;
        }
        if (this.hudLabel) {
            this.hudLabel.string = headline;
        }
        if (this.hintLabel) {
            this.hintLabel.string = "Match over";
        }
    }

    private refreshHud(): void {
        if (this.hudLabel) {
            this.hudLabel.color = this.turn === 0 ? P1 : P2;
            this.hudLabel.string = `${playerName(this.turn)}  ·  pick a fish`;
        }
        if (this.hintLabel) {
            this.hintLabel.string = "They all look the same until they stretch.";
        }
    }

    private clearFish(): void {
        this.fishViews.forEach((view) => {
            if (view.node && view.node.isValid) {
                view.node.destroy();
            }
        });
        this.fishViews.clear();
    }

    private drawBackdrop(): void {
        const bg = this.makeNode("Backdrop", this.canvas!, 0, 0, 750, 1624);
        const g = bg.addComponent(Graphics);
        g.fillColor = new Color(18, 56, 82, 255);
        g.rect(-375, -812, 750, 1624);
        g.fill();
        g.fillColor = new Color(36, 110, 128, 255);
        g.rect(-375, -812, 750, 720);
        g.fill();
        g.fillColor = new Color(214, 184, 122, 255);
        g.rect(-375, -140, 750, 90);
        g.fill();
        g.fillColor = new Color(12, 78, 96, 180);
        g.roundRect(-300, -40, 600, 220, 40);
        g.fill();
        this.makeLabel("PondTitle", "TOY POND", 20, new Color(190, 230, 230, 200), 0, 150, 200, 30, bg);
    }

    private drawPlayers(): void {
        this.drawPerson(-250, 470, P1, "P1");
        this.drawPerson(250, 470, P2, "P2");
    }

    private drawPerson(x: number, y: number, color: Color, tag: string): void {
        const node = this.makeNode(tag, this.canvas!, x, y, 140, 180);
        const g = node.addComponent(Graphics);
        g.fillColor = color;
        g.circle(0, 48, 22);
        g.fill();
        g.roundRect(-22, -10, 44, 58, 12);
        g.fill();
        g.roundRect(-28, -58, 18, 48, 8);
        g.fill();
        g.roundRect(10, -58, 18, 48, 8);
        g.fill();
        g.fillColor = INK;
        g.roundRect(-6, 20, 50, 8, 3);
        g.fill();
        g.roundRect(40, -20, 8, 70, 3);
        g.fill();
        this.makeLabel(`${tag}-name`, tag, 22, CREAM, 0, -88, 80, 28, node);
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
        label.lineHeight = Math.round(size * 1.2);
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
    ): void {
        const node = this.makeNode(name, parent || this.canvas!, x, y, w, h);
        const g = node.addComponent(Graphics);
        g.fillColor = color;
        g.roundRect(-w * 0.5, -h * 0.5, w, h, 18);
        g.fill();
        const label = node.addComponent(Label);
        label.string = text;
        label.fontSize = 28;
        label.lineHeight = 32;
        label.color = INK;
        label.horizontalAlign = Label.HorizontalAlign.CENTER;
        label.verticalAlign = Label.VerticalAlign.CENTER;
        label.useSystemFont = true;
        label.fontFamily = "Arial";
        const button = node.addComponent(Button);
        button.transition = Button.Transition.SCALE;
        button.zoomScale = 0.96;
        node.on(Button.EventType.CLICK, onClick, this);
        const widget = node.addComponent(Widget);
        widget.isAlignHorizontalCenter = false;
        void widget;
    }
}
