import button from '../utils/button_api'
import show from '../utils/showContent'

let optArr = null;
let str = null
const createCanvas = () => {
  const offScreenCanvas = tt.createCanvas();
  const onScreenCanvas = canvas
  offScreenCanvas.height = onScreenCanvas.height
  offScreenCanvas.width = onScreenCanvas.width

  if(typeof canvas == 'object'){
    const offScreenContext = offScreenCanvas.getContext('2d')
    const onScreenCanvasContext = onScreenCanvas.getContext('2d')

    const centerX = offScreenCanvas.width / 2; // 圆心的 X 坐标
    const centerY = offScreenCanvas.height / 2; // 圆心的 Y 坐标
    const radius = 100; // 圆的半径

    // 绘制圆形
    offScreenContext.beginPath();
    offScreenContext.arc(centerX, centerY, radius, 0, Math.PI * 2); // 绘制圆弧 (完整圆: 0 到 2π)
    offScreenContext.fillStyle = 'blue'; // 填充颜色
    offScreenContext.fill(); // 填充圆形
    offScreenContext.strokeStyle = 'black'; // 边框颜色
    offScreenContext.lineWidth = 2; // 边框宽度
    offScreenContext.stroke(); // 绘制边框

    str = ['离屏画布创建成功，并将其内容绘制到屏上canvas']

    onScreenCanvasContext.drawImage(offScreenCanvas, 0, 0);
    show(str)
  }
}
export default () => {
  optArr = [
    {
      name: 'tt.createCanvas',
      callback: createCanvas
    }
  ]
  button(optArr);
}