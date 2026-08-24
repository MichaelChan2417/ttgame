import {
    _decorator,
    Color,
    Component,
    EventTouch,
    Graphics,
    Node,
    UITransform,
    Vec3,
} from "cc";

const { ccclass } = _decorator;

@ccclass("FishView")
export class FishView extends Component {
    public fishId = -1;
    public onHook: ((id: number) => void) | null = null;

    private graphics: Graphics | null = null;
    private bodyColor = new Color(255, 168, 72, 255);
    private stretch = 1;
    private bobPhase = 0;
    private hooked = false;

    public setup(id: number, hue: number, phase: number): void {
        this.fishId = id;
        this.bobPhase = phase;
        this.bodyColor = this.colorFromHue(hue);
        this.ensureGraphics();
        this.redraw();
        this.node.on(Node.EventType.TOUCH_END, this.handleTap, this);
    }

    public setHooked(): void {
        this.hooked = true;
        this.node.off(Node.EventType.TOUCH_END, this.handleTap, this);
    }

    public setStretch(value: number): void {
        this.stretch = value;
        this.redraw();
    }

    public tickIdle(time: number): void {
        if (this.hooked) {
            return;
        }
        const bob = Math.sin(time * 2.1 + this.bobPhase) * 6;
        const pos = this.node.position;
        this.node.setPosition(pos.x, this.baseY + bob, pos.z);
    }

    public baseY = 0;

    private handleTap(_event: EventTouch): void {
        if (this.hooked || !this.onHook) {
            return;
        }
        this.onHook(this.fishId);
    }

    private ensureGraphics(): void {
        if (this.graphics) {
            return;
        }
        const gNode = new Node("FishArt");
        gNode.layer = this.node.layer;
        gNode.addComponent(UITransform).setContentSize(220, 90);
        this.graphics = gNode.addComponent(Graphics);
        this.node.addChild(gNode);
        gNode.setPosition(new Vec3(0, 0, 0));
    }

    private redraw(): void {
        const g = this.graphics;
        if (!g) {
            return;
        }
        g.clear();
        const w = 54 + 86 * this.stretch;
        const h = 28;
        const body = this.bodyColor;
        const belly = new Color(
            Math.min(255, body.r + 40),
            Math.min(255, body.g + 30),
            Math.min(255, body.b + 20),
            255,
        );
        const outline = new Color(28, 36, 48, 255);

        g.fillColor = body;
        g.rect(-w * 0.5, -h * 0.5, w, h);
        g.fill();
        g.roundRect(-w * 0.5, -h * 0.5, w, h, 16);
        g.fill();

        g.fillColor = belly;
        g.roundRect(-w * 0.28, -h * 0.42, w * 0.55, h * 0.38, 10);
        g.fill();

        g.fillColor = body;
        g.moveTo(w * 0.5 - 4, 0);
        g.lineTo(w * 0.5 + 22, 18);
        g.lineTo(w * 0.5 + 22, -18);
        g.close();
        g.fill();

        g.fillColor = Color.WHITE;
        g.circle(-w * 0.28, 6, 6);
        g.fill();
        g.fillColor = outline;
        g.circle(-w * 0.26, 6, 3);
        g.fill();

        g.strokeColor = new Color(255, 255, 255, 90);
        g.lineWidth = 2;
        g.moveTo(-w * 0.05, 4);
        g.lineTo(w * 0.22, 4);
        g.stroke();
    }

    private colorFromHue(hue: number): Color {
        const s = 0.62;
        const l = 0.58;
        const c = (1 - Math.abs(2 * l - 1)) * s;
        const hp = hue / 60;
        const x = c * (1 - Math.abs((hp % 2) - 1));
        let r = 0;
        let g = 0;
        let b = 0;
        if (hp < 1) {
            r = c;
            g = x;
        } else if (hp < 2) {
            r = x;
            g = c;
        } else if (hp < 3) {
            g = c;
            b = x;
        } else if (hp < 4) {
            g = x;
            b = c;
        } else if (hp < 5) {
            r = x;
            b = c;
        } else {
            r = c;
            b = x;
        }
        const m = l - c / 2;
        return new Color(
            Math.round((r + m) * 255),
            Math.round((g + m) * 255),
            Math.round((b + m) * 255),
            255,
        );
    }
}
