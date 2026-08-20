# WordTT — TikTok 接入说明（道具 / 每日挑战 / 分享 / 好友排行榜）

这些能力只有在**真机 TikTok 小游戏环境**才生效；Cocos 编辑器预览里全部走模拟（看广告秒发、排行榜显示 mock 面板、分享打印到 Console），逻辑可以照常测。

## 1. 激励广告（道具）
- 在 TikTok Dev Portal 创建**激励视频广告位**，拿到 adUnitId。
- 打开 `assets/scripts/GameController.ts`，把常量 `AD_UNIT` 改成你的广告位 id。
- 预览环境没有广告接口，`Platform.showRewardedAd` 会直接模拟“看完”。

## 2. 分享（IM）
- 用 `TTMinis.game.shareAppMessage`（原生 `tt.shareAppMessage`），需客户端 ≥ 40.3.0，代码里已用 `canIUse` 判断。
- 分享文案在 `GameController.onShare()`，可自行改标题/副标题/图片。

## 3. 好友排行榜（开放数据域）
文档：TikTok 排行榜接入指南；Cocos 官方：
https://docs.cocos.com/creator/3.8/manual/zh/editor/publish/build-open-data-context.html

步骤：
1. 本目录 `openDataContext/index.js` 就是开放数据域入口（纯 JS，跑在隔离子域）。
2. 在 **game.json** 增加配置（构建后生成，或在构建模板里配置）：
   ```json
   {
     "dev": { "port": 9527 },
     "openDataContext": "openDataContext",
     "app_id": "你的 client key"
   }
   ```
3. Cocos 构建 TikTok/字节小游戏时，把「开放数据域」根目录指向本 `openDataContext` 文件夹（Cocos 构建面板里的 Open Data Context 选项）。
4. 主域侧已接好：
   - 通关后 `setUserCloudStorage` 写入自己的成绩，key = `wordtt_daily`，value = `{"d":期号,"r":行数,"t":秒}`。
   - 点 Rank：`authorizeOpenContext` 授权好友关系 → `getOpenDataContext().postMessage({type:'show', day})` 让子域拉取好友数据并渲染到 sharedCanvas。
5. 注意：好友的 `getFriendCloudStorage` 返回字段实际是 `displayName` / `avatarUrl`（驼峰），且好友必须在本游戏登录认证过才有头像昵称——`index.js` 已兼容。

## 4. 每日挑战
- `assets/scripts/Daily.ts`：按 **UTC 日期 hash** 生成当天单词，全球所有人一致，无需服务器。
- 期号 = 距 2024-01-01 的天数，可在 `Daily.ts` 改 `BASE_UTC`。
