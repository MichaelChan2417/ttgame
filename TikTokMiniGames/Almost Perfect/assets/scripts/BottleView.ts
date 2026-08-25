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
import { BOTTLE_PALETTE } from "./core/Rules";

const { ccclass } = _decorator;

@ccclass("BottleView")
export class BottleView extends Component {
    public slotIndex = -1;
    public colorId = -1;
    public restPos = new Vec3();
    public canDrag = false;
    public onDragStart: ((slot: number) => boolean) | null = null;
    public onDragMove: ((slot: number, pos: Vec3) => void) | null = null;
    public onDragEnd: ((slot: number, pos: Vec3) => void) | null = null;

    private graphics: Graphics | null = null;
    private selected = false;
    private scale = 1;
    private hidden = false;
    private dragging = false;

    public setup(slot: number, colorId: number, scale = 1): void {
        this.slotIndex = slot;
        this.colorId = colorId;
        this.scale = scale;
        this.restPos.set(this.node.position);
        this.ensureGraphics();
        this.redraw();
        this.bindInput();
    }

    public setColor(colorId: number): void {
        this.colorId = colorId;
        this.hidden = false;
        this.redraw();
    }

    public setSelected(on: boolean): void {
        this.selected = on;
        this.redraw();
        if (!this.dragging) {
            this.node.setScale(on ? 1.08 : 1, on ? 1.08 : 1, 1);
        }
    }

    public setHidden(on: boolean): void {
        this.hidden = on;
        this.redraw();
    }

    public setTappable(on: boolean): void {
        this.canDrag = on;
        this.bindInput();
    }

    public snapHome(): void {
        this.dragging = false;
        this.node.setPosition(this.restPos);
        this.node.setScale(1, 1, 1);
        this.setSelected(false);
    }

    public isDragging(): boolean {
        return this.dragging;
    }

    private bindInput(): void {
        this.node.off(Node.EventType.TOUCH_START, this.handleStart, this, true);
        this.node.off(Node.EventType.TOUCH_MOVE, this.handleMove, this, true);
        this.node.off(Node.EventType.TOUCH_END, this.handleEnd, this, true);
        this.node.off(Node.EventType.TOUCH_CANCEL, this.handleEnd, this, true);
        if (!this.canDrag) {
            return;
        }
        this.node.on(Node.EventType.TOUCH_START, this.handleStart, this, true);
        this.node.on(Node.EventType.TOUCH_MOVE, this.handleMove, this, true);
        this.node.on(Node.EventType.TOUCH_END, this.handleEnd, this, true);
        this.node.on(Node.EventType.TOUCH_CANCEL, this.handleEnd, this, true);
    }

    private handleStart(event: EventTouch): void {
        if (!this.canDrag) {
            return;
        }
        if (this.onDragStart && !this.onDragStart(this.slotIndex)) {
            return;
        }
        this.dragging = true;
        this.node.setScale(1.12, 1.12, 1);
        this.setSelected(true);
        const parent = this.node.parent;
        if (parent) {
            this.node.setSiblingIndex(parent.children.length - 1);
        }
        event.propagationStopped = true;
    }

    private handleMove(event: EventTouch): void {
        if (!this.dragging) {
            return;
        }
        const delta = event.getUIDelta();
        const pos = this.node.position;
        this.node.setPosition(pos.x + delta.x, pos.y + delta.y, pos.z);
        if (this.onDragMove) {
            this.onDragMove(this.slotIndex, this.node.position);
        }
        event.propagationStopped = true;
    }

    private handleEnd(event: EventTouch): void {
        if (!this.dragging) {
            return;
        }
        this.dragging = false;
        this.node.setScale(1, 1, 1);
        if (this.onDragEnd) {
            this.onDragEnd(this.slotIndex, this.node.position);
        }
        event.propagationStopped = true;
    }

    private ensureGraphics(): void {
        if (this.graphics) {
            return;
        }
        const gNode = new Node("BottleArt");
        gNode.layer = this.node.layer;
        gNode.addComponent(UITransform).setContentSize(120, 180);
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
        const s = this.scale;
        const outline = new Color(32, 22, 28, 255);
        if (this.hidden) {
            g.fillColor = new Color(70, 48, 62, 255);
            g.roundRect(-22 * s, -48 * s, 44 * s, 96 * s, 10 * s);
            g.fill();
            g.fillColor = new Color(48, 32, 42, 255);
            g.roundRect(-10 * s, 48 * s, 20 * s, 18 * s, 4 * s);
            g.fill();
            g.fillColor = new Color(210, 190, 170, 220);
            g.roundRect(-8 * s, -8 * s, 16 * s, 28 * s, 4 * s);
            g.fill();
            return;
        }
        const spec = BOTTLE_PALETTE[this.colorId] || BOTTLE_PALETTE[0];
        const liquid = new Color(spec.r, spec.g, spec.b, 255);
        const glass = new Color(
            Math.min(255, spec.r + 70),
            Math.min(255, spec.g + 70),
            Math.min(255, spec.b + 50),
            90,
        );
        const cap = new Color(
            Math.max(0, spec.r - 40),
            Math.max(0, spec.g - 50),
            Math.max(0, spec.b - 40),
            255,
        );

        if (this.selected) {
            g.fillColor = new Color(255, 255, 255, 70);
            g.circle(0, -4 * s, 46 * s);
            g.fill();
        }

        g.fillColor = cap;
        g.roundRect(-12 * s, 50 * s, 24 * s, 16 * s, 4 * s);
        g.fill();

        g.fillColor = new Color(220, 230, 236, 255);
        g.roundRect(-10 * s, 38 * s, 20 * s, 16 * s, 5 * s);
        g.fill();

        g.fillColor = glass;
        g.roundRect(-24 * s, -52 * s, 48 * s, 92 * s, 14 * s);
        g.fill();

        g.fillColor = liquid;
        g.roundRect(-20 * s, -48 * s, 40 * s, 70 * s, 12 * s);
        g.fill();

        g.fillColor = new Color(255, 255, 255, 80);
        g.roundRect(-16 * s, -10 * s, 8 * s, 36 * s, 4 * s);
        g.fill();

        g.strokeColor = outline;
        g.lineWidth = Math.max(2, 2 * s);
        g.roundRect(-24 * s, -52 * s, 48 * s, 92 * s, 14 * s);
        g.stroke();
    }
}
