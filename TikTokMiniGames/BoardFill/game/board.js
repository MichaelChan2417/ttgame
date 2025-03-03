import {Grid} from "./grid"

class Board {
    constructor() {
        this.grid = Array.from({ length: 6 }, () => 
            Array.from({ length: 6 }, () => new Grid())
        );

        for (let i=0; i<6; i++) {
            for (let j=0; j<6; j++) {
                this.grid[i][j].row = i;
                this.grid[i][j].col = j;
            }
        }

        this.timer = 1750;
        this.startTimer();
    }

    // TODO: generate a correct init state, make sure it has an answer
    randomGenerate() {
        
    }

    startTimer() {
        this.timeInterval = setInterval(() => {
            this.timer++;
            this.timer = Math.min(this.timer, 1800)
        }, 1000);
    }

    // ------------------------------ inner funcs ------------------------------
    getCurrentTime() {
        return this.timer;
    }
    stopTimer() {
        clearInterval(this.timeInterval);
    }
    printBoard() {
        console.log(this.grid.map(row => row.map(cell => cell.state).join(" ")).join("\n"));
    }
    destroy() {
        this.stopTimer();
        this.grid = null;
        this.timer = null;
        console.log("Board instance destroyed.");
    }
}

export let boardInstance = null


export function initBoard() {
    if (!boardInstance) {
        boardInstance = new Board();
    }

    // generate a random game result
    boardInstance.randomGenerate();
}

export function destroyBoard() {
    boardInstance.destroy();
    boardInstance = null
}