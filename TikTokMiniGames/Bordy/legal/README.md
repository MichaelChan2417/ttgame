# Bordy 法律页与四国上架

TikTok 审核会核对：**业务运营地区、用户所在地区、服务提供地区** 是否与隐私政策 / 服务条款一致。上一次拒审就是政策只像中国产品，却按印尼 Minis 提交。

当前约定：

| 项 | 内容 |
|---|---|
| 运营主体 | Shanghai Quanjie Technology Co., Ltd. |
| 上架地区 | **日本、印度尼西亚、菲律宾、马来西亚** |
| 渠道 | TikTok Mini Games（不是国内抖音/微信小游戏） |
| 隐私 | `privacy.html` |
| 条款 | `terms.html` |

游戏是太阳/月亮逻辑填格，不含 Content Requirements 里禁止的暴力组织、自残、色情、仇恨、血腥、骚扰、管制品、赌博、诈骗等内容。名称、图标、简介也必须保持同样干净。

## 你要在 TikTok 后台做的

1. 把本目录 HTML **发布**到后台填的 Privacy / Terms URL（与 `docs/` 线上页保持同一内容）。
2. 打开线上链接，确认页头写的是四国，不是只写印尼。
3. **Basic information → 上架 / 服务地区** 勾选且仅勾选：Japan、Indonesia、Philippines、Malaysia。不要勾未写进政策的国家。
4. 应用名、图标、英文/当地简介与游戏一致：益智填格，无违规暗示。
5. 重新提交审核。

若以后加减国家：先改这两份 HTML 并上线，再改后台勾选，最后提审。
