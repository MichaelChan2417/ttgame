import {canvas, ctx, systemInfo} from "./canvas"
import {initTouchCallback} from "./utils/touch_callback"
import { drawMainPage } from "./game/game_state";

console.log('使用抖音开发者工具开发过程中可以参考以下文档:');
console.log(
  'https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/guide/minigame/introduction',
);

canvas.width = systemInfo.windowWidth;
canvas.height = systemInfo.windowHeight;

console.log(canvas.width, canvas.height)

// touch handler
initTouchCallback();

drawMainPage();

