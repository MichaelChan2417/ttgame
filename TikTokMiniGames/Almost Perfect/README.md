# Almost Perfect

Cocos Creator 3.8 水瓶对位。玩法对齐 TikTok 上那套线下双人挑战：桌上摆一排水瓶，下面有一份盖住的 MATCH，主持人只报「对了几个」，**没全对就继续摆，全对才换人**。

## 打开

工程根目录就是这个文件夹（里面有 `package.json` 和 `assets`）：

```
C:\Users\MSI_NB\Desktop\ttgame\TikTokMiniGames\Almost Perfect
```

Cocos Dashboard → Add → 选上面这个目录。编辑器请用 **3.8.8**。

1. 打开 `assets/scenes/main.scene`
2. 点预览。竖屏 750×1624

场景几乎是空的。`GameApp` 会在运行时搭目录、主持人和瓶子。

## 现在能玩

- 目录：**单人模式**可以进；**双人模式**先占位（即将开放）
- 点两瓶交换位置
- **问主持人**：只告诉你对了几个，不说哪几个
- 全对揭开 MATCH，进入下一局；一共 3 局，比问的次数

## 工程结构

```
Almost Perfect/
  assets/scenes/main.scene
  assets/scripts/GameApp.ts       目录、单人局、主持人
  assets/scripts/BottleView.ts    水瓶绘制和点击
  assets/scripts/core/Rules.ts    洗牌、对位数
```

脚本用系统字体 + `Graphics`，没有外部贴图。
