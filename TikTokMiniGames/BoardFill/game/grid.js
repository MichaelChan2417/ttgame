
export class Grid {
    constructor(row = null, col = null, canModify = null, state = null) {
        this.row = row;
        this.col = col;
        this.canModify = canModify;
        this.state = state;
    }
}