# Fish Off

Cocos Creator 3.8 双人钓鱼比长度。玩法对齐 TikTok 上那套玩具鱼挑战：一堆看起来一样的鱼，钓上来才拉开比谁更长。

## 打开

工程根目录是**内层**这个文件夹（里面有 `package.json` 和 `assets`）：

```
C:\Users\MSI_NB\Desktop\ttgame\TikTokMiniGames\FishOff\FishOff
```

不要选外层的 `TikTokMiniGames\FishOff`。用 Cocos Dashboard → Add → 选上面这个内层目录。编辑器请用 **3.8.8**（和 WordTT 同一套）。

1. 打开 `assets/scenes/main.scene`
2. 点预览。竖屏 750×1624

场景里几乎是空的。`GameApp` 会在运行时搭完整桌面：两个小人、一塘玩具鱼、轮流点选、拉伸揭晓、最长鱼获胜。

## 怎么玩

- 点 **START MATCH**
- P1 / P2 轮流点一条鱼
- 鱼飞到自己这边并拉长，显示厘米
- 8 条钓完，**单条最长的赢**（两边总长度只作参考）
- **PLAY AGAIN** 重开，长度会重新洗

## 工程结构

```
FishOff/
  assets/scenes/main.scene
  assets/scripts/GameApp.ts      开局、回合、结算
  assets/scripts/FishView.ts     玩具鱼绘制和点击
  assets/scripts/core/Rules.ts   洗牌和胜负
```

脚本用系统字体 + `Graphics`，没有外部贴图，打开就能跑。
