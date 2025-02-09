import {canvas} from "../canvas"
import {touched_start_button, init_game_1} from "../game/game_state"

export var game_state = 0;


function update_main_page(x, y) {
    if (touched_start_button(x, y)) {
        game_state = 1;
        init_game_1();
    }
}

function update_game_page(x, y) {

}

export function update(touchInfo) {
    let x = touchInfo.clientX, y = touchInfo.clientY

    console.log("User point on", x, y)

    switch (game_state) {
        case 0:
            update_main_page(x, y);
        case 1:
            update_game_page(x, y);
    }
}