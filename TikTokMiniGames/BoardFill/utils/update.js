import {canvas} from "../canvas"
import {drawMainPage, 
    touched_start_button, 
    init_game_1, 
    touched_return_button,
    destroy_board} from "../game/game_state"

export var game_state = 0;


function update_main_page(x, y) {
    if (touched_start_button(x, y)) {
        game_state = 1;
        init_game_1();
    }
}

function update_game_page(x, y) {
    // some special case first: return / hint / how_to_play / revoke
    if (touched_return_button(x, y)) {
        game_state = 0;
        destroy_board();
        drawMainPage();
    }
}

export function update(touchInfo) {
    let x = touchInfo.clientX, y = touchInfo.clientY

    console.log("User point on", x, y, "with game state", game_state)

    switch (game_state) {
        case 0:
            update_main_page(x, y);
            break;
        case 1:
            update_game_page(x, y);
            break;
    }
}