import {update} from "./update"

export function initTouchCallback() {
    function touchCallback({
        touches,
        changedTouches,
        timeStamp
        }) {
        // console.log('开始触摸回调', touches, changedTouches, timeStamp)
        update(touches[0])
    };
    tt.onTouchStart(touchCallback);
}