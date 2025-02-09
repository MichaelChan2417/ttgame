import {canvas, ctx, systemInfo} from "../canvas"


const window_width = systemInfo.windowWidth
const window_height = systemInfo.windowHeight


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

export function init_game_1() {
    console.log("init_game_1")
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, systemInfo.windowWidth, systemInfo.windowHeight);
}





export function touched_start_button(x, y) {
    return x >= button_x && x <= button_x + expected_button_width &&
            y >= button_y && y <= button_y + expected_button_height
}