import {canvas} from "../canvas"
import {drawMainPage, 
    touched_start_button, 
    init_game_1, 
    touched_return_button,
    destroy_board} from "../game/game_state"

import {boardInstance} from "../game/board"

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
    } else if (boardInstance.touch_and_update_grid(x, y)) {
        let valid_result = boardInstance.check_valid();
        boardInstance.ShowPass()
        if (!valid_result) {
            console.log("===========================")
            // TODO: 这个之后是要加error showing的 比如在做完一次操作后3秒， 如果有问题 ping出来
        } else {
            let result = boardInstance.check_end();
            // if pass valid check, then if no zero, its an end
            if (result) {
                boardInstance.stopTimer()
                boardInstance.ShowPass()
            }
        }
    }
}

export function update(touchInfo) {
    let x = touchInfo.clientX, y = touchInfo.clientY

    switch (game_state) {
        case 0:
            update_main_page(x, y);
            break;
        case 1:
            update_game_page(x, y);
            break;
    }
}