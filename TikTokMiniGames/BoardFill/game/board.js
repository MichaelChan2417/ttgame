import { canvas, ctx, systemInfo } from "../canvas"
import {Grid} from "./grid"
import { return_button_y, return_button_height } from './game_state';

const COLORS = ['#FF5252', '#4CAF50', '#2196F3', '#FFC107', '#9C27B0'];

const MODIFY_COLOR = '#EEEEEE'
const UNCHANGABLE_COLOR = '#999999'

class Board {
    constructor() {
        this.grid = Array.from({ length: 6 }, () => 
            Array.from({ length: 6 }, () => new Grid())
        );

        // Init grids
        for (let i=0; i<6; i++) {
            for (let j=0; j<6; j++) {
                this.grid[i][j].row = i;
                this.grid[i][j].col = j;
                this.grid[i][j].canModify = true;
            }
        }

        this.timer = 0;
        this.cellSize = 0;
        this.startPos = { x: 0, y: 0 };
        this.startTimer();
    }

    randomGenerate() {

        // use this to generate one correct answer
        this.boardSetUp();

        // randomly select several grid to be shown
        this.showRandomStart();

        // randomly pick some relation, not between 2 Start
        this.showRandomRelation();


    }

    draw(ctx) {
        const safeArea = {
            startY: return_button_y + return_button_height + 60,
            availableHeight: systemInfo.windowHeight - return_button_y - return_button_height - 40
        };
        // 动态计算格子尺寸
        this.cellSize = Math.min(
            Math.floor((systemInfo.windowWidth - 7 * 10) / 6), // 横向最大尺寸
            Math.floor(safeArea.availableHeight / 6)          // 纵向最大尺寸
        );
    
        // 居中布局
        this.startPos = {
            x: (systemInfo.windowWidth - (6 * this.cellSize + 5 * 10)) / 2, // 水平居中
            y: safeArea.startY
        };
    
        // 绘制棋盘
        for (let i = 0; i < 6; i++) {
            for (let j = 0; j < 6; j++) {
                const x = this.startPos.x + i * (this.cellSize + 10);
                const y = this.startPos.y + j * (this.cellSize + 10);
                
                // 绘制格子
                if (this.grid[i][j].canModify) {
                    ctx.fillStyle = MODIFY_COLOR;
                } else {
                    ctx.fillStyle = UNCHANGABLE_COLOR;
                }
                ctx.fillRect(x, y, this.cellSize, this.cellSize);
                
                // 绘制边框
                ctx.strokeStyle = '#333';
                ctx.lineWidth = 2;
                ctx.strokeRect(x, y, this.cellSize, this.cellSize);
            }
        }
    }

    startTimer() {
        this.timeInterval = setInterval(() => {
            this.timer++;
            this.timer = Math.min(this.timer, 1800)
        }, 1000);
    }

    touch_and_update_grid(input_x, input_y) {
        let dx = input_x - this.startPos.x; let dy = input_y - this.startPos.y;
        if ((dx < 0) || (dy < 0)) {
            return false
        }
        let extended_size = this.cellSize + 10;
        let x = Math.trunc(dx / extended_size);
        let y = Math.trunc(dy / extended_size);
        if ((x > 5) || (y > 5)) {
            return false;
        }
        if ((dx % extended_size) > this.cellSize || (dy % extended_size) > this.cellSize) {
            return false;
        }
        
        // make logical update
        if (!this.grid[x][y].canModify) {
            return true;
        }
        this.grid[x][y].state += 1;
        this.grid[x][y].state %= 3;

        // draw the updated grid
        this.grid[x][y].draw();

        return true;
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

    boardSetUp() {
        // TODO: make it generalized

    }

    showRandomStart() {
        let selectedCells = new Set();
        let directions = [
            [-1, 0], [1, 0], [0, -1], [0, 1]
        ];

        function getRandomCell() {
            let row = Math.floor(Math.random() * 6);
            let col = Math.floor(Math.random() * 6);
            return `${row},${col}`;
        }

        function addGroup(size) {
            let start = getRandomCell();
            while (selectedCells.has(start)) {
                start = getRandomCell();
            }
            selectedCells.add(start);

            let [r, c] = start.split(',').map(Number);
            for (let i = 1; i < size; i++) {
                let neighbors = directions
                    .map(([dr, dc]) => [r + dr, c + dc])
                    .filter(([nr, nc]) => nr >= 0 && nr < 6 && nc >= 0 && nc < 6)
                    .map(([nr, nc]) => `${nr},${nc}`)
                    .filter(nc => !selectedCells.has(nc));

                if (neighbors.length === 0) break;

                let newCell = neighbors[Math.floor(Math.random() * neighbors.length)];
                selectedCells.add(newCell);
                [r, c] = newCell.split(',').map(Number);
            }
        }

        // 生成 1-2 组（每组 2-3 个连在一起）
        let numGroups = Math.floor(Math.random() * 2) + 1; // 1 or 2 groups
        for (let i = 0; i < numGroups; i++) {
            let groupSize = Math.floor(Math.random() * 2) + 2; // 2 or 3
            addGroup(groupSize);
        }

        // 生成 3-5 个独立格子
        let numSingles = Math.floor(Math.random() * 3) + 3; // 3 to 5
        while (selectedCells.size < numSingles + numGroups * 2) {
            let single = getRandomCell();
            if (!selectedCells.has(single)) {
                selectedCells.add(single);
            }
        }

        for (let cell of selectedCells) {
            let [r, c] = cell.split(',').map(Number);
            this.grid[r][c].canModify = false;  // shown cases, cannot modify
        }
    }

    showRandomRelation() {

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