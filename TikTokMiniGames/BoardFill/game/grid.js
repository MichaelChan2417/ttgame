import { canvas, ctx, systemInfo } from "../canvas"

const MODIFY_COLOR = '#EEEEEE'
const UNCHANGABLE_COLOR = '#999999'
const SIGN_WIDTH = 12


const state1Image = tt.createImage();
state1Image.src = './resources/state1.png'
const state2Image = tt.createImage();
state2Image.src = './resources/state2.png'

const equalImage = tt.createImage();
equalImage.src = './resources/equal_sign.png'
const diffImage = tt.createImage();
diffImage.src = './resources/cross_sign.png'

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

    draw_relation(is_draw_right, is_equal) {
        if (is_equal) {
            if (is_draw_right) {
                ctx.drawImage(
                    equalImage,
                    0, 0, equalImage.width, equalImage.height,
                    this.start_x + this.cellSize + 2, this.start_y + this.cellSize / 2 - SIGN_WIDTH / 2,
                    SIGN_WIDTH, SIGN_WIDTH
                )
            } else {
                ctx.drawImage(
                    equalImage,
                    0, 0, equalImage.width, equalImage.height,
                    this.start_x + this.cellSize / 2 - SIGN_WIDTH / 2, this.start_y + this.cellSize + 2,
                    SIGN_WIDTH, SIGN_WIDTH
                )
            }
        } else {
            if (is_draw_right) {
                ctx.drawImage(
                    diffImage,
                    0, 0, diffImage.width, diffImage.height,
                    this.start_x + this.cellSize + 2, this.start_y + this.cellSize / 2 - SIGN_WIDTH / 2,
                    SIGN_WIDTH, SIGN_WIDTH
                )
            } else {
                ctx.drawImage(
                    diffImage,
                    0, 0, diffImage.width, diffImage.height,
                    this.start_x + this.cellSize / 2 - SIGN_WIDTH / 2, this.start_y + this.cellSize + 2,
                    SIGN_WIDTH, SIGN_WIDTH
                )
            }
        }
    }
}
