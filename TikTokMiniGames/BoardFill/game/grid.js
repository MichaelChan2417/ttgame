import { canvas, ctx, systemInfo } from "../canvas"

const MODIFY_COLOR = '#EEEEEE'
const UNCHANGABLE_COLOR = '#999999'


const state1Image = tt.createImage();
state1Image.src = './resources/state1.png'
const state2Image = tt.createImage();
state2Image.src = './resources/state2.png'

export class Grid {
    constructor(row = null, col = null, canModify = null, state = null, cellSize = null,
        start_x = null, start_y = null) {
        this.row = row;
        this.col = col;
        this.canModify = canModify;
        this.state = state;         // 0 -> unset; 1 -> class1; 2 -> class2
        this.cellSize = cellSize;
        this.start_x = start_x;
        this.start_y = start_y;
    }

    draw() {
        // always draw the back ground
        if (this.canModify) {
            ctx.fillStyle = MODIFY_COLOR;
        } else {
            ctx.fillStyle = UNCHANGABLE_COLOR;
        }
        ctx.fillRect(this.start_x,this.start_y, this.cellSize, this.cellSize);
        // draw border
        ctx.strokeStyle = '#333';
        ctx.lineWidth = 2;
        ctx.strokeRect(this.start_x, this.start_y, this.cellSize, this.cellSize);

        // draw content
        if (this.state == 1) {
            ctx.drawImage(
                state1Image,
                0, 0,
                state1Image.width, state1Image.height,
                this.start_x, this.start_y,
                this.cellSize, this.cellSize,
            );
        } else if (this.state == 2) {
            ctx.drawImage(
                state2Image,
                0, 0,
                state2Image.width, state2Image.height,
                this.start_x, this.start_y,
                this.cellSize, this.cellSize,
            );
        }
    }
}
