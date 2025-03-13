import { canvas, ctx, systemInfo } from "../canvas"
import {Grid} from "./grid"
import { return_button_y, return_button_height } from './game_state';

const COLORS = ['#FF5252', '#4CAF50', '#2196F3', '#FFC107', '#9C27B0'];

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
        for (let i = 0; i < 6; i++) {
            for (let j = 0; j < 6; j++) {
                this.grid[i][j].state = Math.floor(Math.random() * COLORS.length);
            }
        }
    }


    draw(ctx) {
        // 计算安全区域（考虑返回按钮高度）
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
                const x = this.startPos.x + j * (this.cellSize + 10);
                const y = this.startPos.y + i * (this.cellSize + 10);
                
                // 绘制格子
                ctx.fillStyle = COLORS[this.grid[i][j].state];
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