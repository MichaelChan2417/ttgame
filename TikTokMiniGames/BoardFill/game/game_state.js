import { canvas, ctx, systemInfo } from "../canvas"
import { boardInstance, initBoard, destroyBoard } from "./board"
import { logger  } from "./log"

const window_width = systemInfo.windowWidth
const window_height = systemInfo.windowHeight

export const return_button_y = 80;
export const return_button_height = 40;

let expected_button_width = Math.floor(0.45 * window_width)
let expected_button_height = Math.floor(0.12 * window_height)
let button_x = (window_width - expected_button_width) / 2;
let button_y = window_height * 0.5;

export function drawMainPage() {

    // background color fill
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, systemInfo.windowWidth, systemInfo.windowHeight);

    // draw button
    function load_buttons() {
        const buttonImage = tt.createImage();
        buttonImage.src = './resources/yellow_button.png'
        buttonImage.onload = () => {
            ctx.drawImage(
                buttonImage,
                0, 0,
                buttonImage.width, buttonImage.height,
                button_x, button_y,
                expected_button_width, expected_button_height,
            );
            
            ctx.fillStyle = '#000000';
            const fontSize = parseInt(expected_button_width / 6);
            ctx.font = `${fontSize}px Arial`;
            ctx.fillText('开始游戏', button_x+expected_button_width/6, button_y+expected_button_height/2 + fontSize * 0.4);
        };
    }

    load_buttons();
  
}


let return_button_x = 40;
let return_button_y = 80;
let return_button_width = 40;
let return_button_height = 40;

export function init_game_1() {
    logger.log("init_game_1")
    ctx.fillStyle = '#FFFFFF';
    ctx.fillRect(0, 0, systemInfo.windowWidth, systemInfo.windowHeight);

    // draw return button
    function load_return_button() {
        const returnButtonImage = tt.createImage();
        returnButtonImage.src = './resources/return_button.png'
        returnButtonImage.onload = () => {
            ctx.drawImage(
                returnButtonImage,
                0, 0,
                returnButtonImage.width, returnButtonImage.height,
                return_button_x, return_button_y,
                return_button_width, return_button_height,
            );
        };
    }
    load_return_button();
    logger.log('This is a test message');

    // board initialize
    initBoard();
    boardInstance.draw(ctx);

    // draw game timer
    drawGameTimer();
}






// ====================================== helper func ======================================

export function touched_start_button(x, y) {
    return x >= button_x && x <= button_x + expected_button_width &&
            y >= button_y && y <= button_y + expected_button_height;
}

export function touched_return_button(x, y) {
    return x >= return_button_x && x <= return_button_x + return_button_width &&
            y >= return_button_y && y <= return_button_y + return_button_height;
}

export function destroy_board() {
    destroyBoard();
}



// ====================================== inner func ======================================
let time_background_width = 120;
let time_background_height = 40;
let time_background_x = (window_width - time_background_width) / 2;
let time_background_y = return_button_y;            // same height with return button
function drawGameTimer() {
    // draw timebackground
    function load_time_background() {
        const timeBackgroundImage = tt.createImage();
        timeBackgroundImage.src = './resources/time_background.png'
        timeBackgroundImage.onload = () => {
            ctx.drawImage(
                timeBackgroundImage,
                0, 0,
                timeBackgroundImage.width, timeBackgroundImage.height,
                time_background_x, time_background_y,
                time_background_width, time_background_height,
            );
            
            // draw text
            ctx.fillStyle = '#000000';
            const fontSize = parseInt(time_background_width / 6);
            ctx.font = `${fontSize}px Arial`;
            let seconds = boardInstance.getCurrentTime();
            let minutes = Math.floor(seconds / 60);
            seconds = seconds % 60;
            let minutesStr = String(minutes).padStart(2, '0');
            let secondsStr = String(seconds).padStart(2, '0');

            ctx.fillText(`用时 ${minutesStr}:${secondsStr}`, time_background_x + 10, time_background_y + time_background_height / 2 + fontSize * 0.35);

        };
    }
    load_time_background();
    
    const timerUpdate = setInterval(() => {
        if (boardInstance == null) {
            clearInterval(timerUpdate);
            logger.log("Timer Stopped");
            return;
        }
        load_time_background();
    }, 1000);
}