import { canvas, ctx, systemInfo } from "../canvas"
import {Grid} from "./grid"
import { return_button_y, return_button_height } from './game_state';
import {bd} from '../resources/example'
import {relation_vec} from './relation'

const space_width = 16;

const passImage = tt.createImage();
passImage.src = './resources/pass.png'

class Board {
    constructor() {
        this.grid = Array.from({ length: 6 }, () => 
            Array.from({ length: 6 }, () => new Grid())
        );
        this.safeArea = {
            startY: return_button_y + return_button_height + 60,
            availableHeight: systemInfo.windowHeight - return_button_y - return_button_height - 40
        };
        // 动态计算格子尺寸
        this.cellSize = Math.min(
            Math.floor((systemInfo.windowWidth - 7 * space_width) / 6), // 横向最大尺寸
            Math.floor(this.safeArea.availableHeight / 6)          // 纵向最大尺寸
        );
        this.startPos = {
            x: (systemInfo.windowWidth - (6 * this.cellSize + 5 * space_width)) / 2, // 水平居中
            y: this.safeArea.startY
        };
        
        // Init grids
        for (let i=0; i<6; i++) {
            for (let j=0; j<6; j++) {
                this.grid[i][j].row = i;
                this.grid[i][j].col = j;
                this.grid[i][j].canModify = true;
                this.grid[i][j].cellSize = this.cellSize;
                this.grid[i][j].state = 0;
            }
        }

        this.timer = 0;
        this.startTimer();
    }

    randomGenerate() {

        // randomly select several grid to be shown
        this.showRandomStart();

        // use this to generate one correct answer (has to be after RandomStart)
        this.boardSetUp();

        // randomly pick some relation, not between 2 Start
        this.showRandomRelation();


    }

    // this is the first time draw
    draw() {
        // 绘制棋盘
        for (let i = 0; i < 6; i++) {
            for (let j = 0; j < 6; j++) {
                this.grid[i][j].draw()
            }
        }
    }

    startTimer() {
        this.timeInterval = setInterval(() => {
            this.timer++;
            this.timer = Math.min(this.timer, 1800)
        }, 1000);
    }
    stopTimer() {
        clearInterval(this.timeInterval);
    }

    touch_and_update_grid(input_x, input_y) {
        let dx = input_x - this.startPos.x; let dy = input_y - this.startPos.y;
        if ((dx < 0) || (dy < 0)) {
            return false
        }
        let extended_size = this.cellSize + space_width;
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

    check_valid() {
        if (!this.checkGreaterThan3()) {
            return false;
        }
        if (!this.checkCont3()) {
            return false;
        }
        // TODO: 添加对 =/x 的逻辑检查
        return true;
    }

    check_end() {
        for (let i=0; i<6; i++) {
            for (let j=0; j<6; j++) {
                if (this.grid[i][j].state == 0) {
                    return false;
                }
            }
        }
        return true;
    }

    ShowPass() {
        ctx.drawImage(
            passImage,
            0, 0, passImage.width, passImage.height,
            120, 640,
            200, 200
        )
    }

    // ------------------------------ inner funcs ------------------------------
    getCurrentTime() {
        return this.timer;
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
        let idx = 0;
        for (let i=0; i<6; i++) {
            for (let j=0; j<6; j++) {
                const x = this.startPos.x + i * (this.cellSize + space_width);
                const y = this.startPos.y + j * (this.cellSize + space_width);
                this.grid[i][j].start_x = x;
                this.grid[i][j].start_y = y;
                if (!this.grid[i][j].canModify) {
                    this.grid[i][j].state = bd[idx];
                }
                ++idx;
            }
        }
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
        // 5 - 10 relations
        let relation_cnt = Math.floor(Math.random() * (10 - 5 + 1)) + 5;
    
        // 获取所有有效的关系索引（两个grid不同时为不可修改）
        const validIndices = [];
        for (let i = 0; i < relation_vec.length; i++) {
            const [x1, y1, x2, y2] = relation_vec[i];
            // 假设通过grid访问每个格子的canModify属性
            if (this.grid[x1][y1].canModify || this.grid[x2][y2].canModify) {
                validIndices.push(i);
            }
        }
        
        // 调整数量不超过有效索引的数量
        relation_cnt = Math.min(relation_cnt, validIndices.length);
        if (relation_cnt === 0) return; // 无有效关系则退出
        
        // 随机打乱有效索引数组
        for (let i = validIndices.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [validIndices[i], validIndices[j]] = [validIndices[j], validIndices[i]];
        }
        
        // 选取前relation_cnt个索引
        const selectedIndices = validIndices.slice(0, relation_cnt);

        // detect draw right/down
        for (const idx of selectedIndices) {
            let is_draw_right = true;
            if (idx > relation_vec.length / 2) {
                is_draw_right = false;
            }

            const cx = relation_vec[idx][0], cy = relation_vec[idx][1]
            const nx = relation_vec[idx][2], ny = relation_vec[idx][3]
            let is_equal = true;
            if (bd[nx*6+ny] != bd[cx*6+cy]) {
                is_equal = false;
            }

            this.grid[cx][cy].draw_relation(is_draw_right, is_equal)
        }
    }

    checkGreaterThan3() {
        // row checks
        for (let i=0; i<6; i++) {
            let cnt1 = 0, cnt2 = 0;
            for (let j=0; j<6; j++) {
                if (this.grid[i][j].state == 1) {
                    cnt1++;
                }
                if (this.grid[i][j].state == 2) {
                    cnt2++;
                }

                if (cnt1 > 3 || cnt2 > 3) {
                    return false;
                }
            }
        }
        // col checks
        for (let i=0; i<6; i++) {
            let cnt1 = 0, cnt2 = 0;
            for (let j=0; j<6; j++) {
                if (this.grid[j][i].state == 1) {
                    cnt1++;
                }
                if (this.grid[j][i].state == 2) {
                    cnt2++;
                }

                if (cnt1 > 3 || cnt2 > 3) {
                    return false;
                }
            }
        }
        return true;
    }

    checkCont3() {
        for (let i=0; i<6; i++) {
            for (let j=0; j<=3; j++) {
                if (this.grid[i][j].state == 1 && this.grid[i][j+1].state == 1 && this.grid[i][j+2].state == 1) {
                    return false;
                }
                if (this.grid[i][j].state == 2 && this.grid[i][j+1].state == 2 && this.grid[i][j+2].state == 2) {
                    return false;
                }
            }
        }
        for (let i=0; i<6; i++) {
            for (let j=0; j<=3; j++) {
                if (this.grid[j][i].state == 1 && this.grid[j+1][i].state == 1 && this.grid[j+2][i].state == 1) {
                    return false;
                }
                if (this.grid[j][i].state == 2 && this.grid[j+1][i].state == 2 && this.grid[j+2][i].state == 2) {
                    return false;
                }
            }
        }
        return true;
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