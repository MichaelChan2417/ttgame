
const state1Image = tt.createImage();
state1Image.src = '../resources/stage1.png'
const stage2Image = tt.createImage();
stage2Image.src = '../resources/stage2.png'

export class Grid {
    constructor(row = null, col = null, canModify = null, state = null) {
        this.row = row;
        this.col = col;
        this.canModify = canModify;
        this.state = state;         // 0 -> unset; 1 -> class1; 2 -> class2
    }

    draw(ctx, start_x, start_y, grid_size) {
        console.log("Draw on", this.row, this.col, "with state", this.state);
        function grid_draw(idx) {
        }

        grid_draw(this.state);
    }
}