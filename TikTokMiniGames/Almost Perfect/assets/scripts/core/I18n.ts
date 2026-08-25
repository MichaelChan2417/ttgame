import { SLOT_COUNT } from "./Rules";
import { getLang, Lang } from "./Locale";

type Pack = {
    menuTag: string;
    solo: string;
    duo: string;
    comingSoon: string;
    duoToast: string;
    settings: string;
    language: string;
    close: string;
    dragHint: string;
    askHost: string;
    matchTitle: string;
    menuBack: string;
    lidCovered: string;
    hostNone: string;
    hostAll: string;
    hostSome: string;
    bubbleAsk: string;
    bubbleSwapped: string;
    bubbleNext: string;
    hud: string;
    winTitle: string;
    winBody: string;
    playAgain: string;
    backMenu: string;
    hostName: string;
    historyEmpty: string;
};

const EN: Pack = {
    menuTag: "Line up the bottles. The host only says how many are right.\nKeep going until every bottle matches.",
    solo: "Solo",
    duo: "Two Players",
    comingSoon: "Coming soon",
    duoToast: "Two-player is still in the works. Try solo first.",
    settings: "Settings",
    language: "Language",
    close: "Close",
    dragHint: "Drag a bottle onto another to swap",
    askHost: "Ask the host",
    matchTitle: "MATCH  ·  only told how many are right",
    menuBack: "Menu",
    lidCovered: "Answer hidden",
    hostNone: "None right!",
    hostAll: "Perfect!",
    hostSome: "{0} right.",
    bubbleAsk: "Drag bottles, then ask me.",
    bubbleSwapped: "Swapped. Ask me again?",
    bubbleNext: "Next round. I'll keep counting.",
    hud: "Solo  ·  Round {0} / {1}  ·  Asked {2}",
    winTitle: "Perfect",
    winBody: "{1} rounds, {0} asks total.\n\nFewer asks is better. Two-player will race this.",
    playAgain: "Play again",
    backMenu: "Menu",
    hostName: "Host",
    historyEmpty: "Your asks will show up here.",
};

const ZH: Pack = {
    menuTag: "摆好一排水瓶，主持人只说对了几个。\n没全对就继续摆。全对才换人。",
    solo: "单人模式",
    duo: "双人模式",
    comingSoon: "即将开放",
    duoToast: "双人模式还在做，先玩单人吧",
    settings: "设置",
    language: "语言",
    close: "关闭",
    dragHint: "拖瓶子到另一格交换",
    askHost: "问主持人",
    matchTitle: "MATCH  ·  每次只告诉你对了几个",
    menuBack: "目录",
    lidCovered: "答案盖着",
    hostNone: "一个都不对！",
    hostAll: "全对！",
    hostSome: "对了 {0} 个。",
    bubbleAsk: "拖瓶子换位置，摆好了问我。",
    bubbleSwapped: "换好了？再问我一次。",
    bubbleNext: "下一局。还是我来报数。",
    hud: "单人  ·  第 {0} / {1} 局  ·  已问 {2} 次",
    winTitle: "全对了",
    winBody: "{1} 局里一共问了 {0} 次\n\n问得越少越好。双人模式会按这个比谁更快全对。",
    playAgain: "再来一局",
    backMenu: "回目录",
    hostName: "主持人",
    historyEmpty: "问过之后，记录会列在这里。",
};

const JA: Pack = {
    menuTag: "ボトルを並べて。司会は当たった数だけ言う。\n全部当たるまで続ける。全部当たったら交代。",
    solo: "ひとりで遊ぶ",
    duo: "ふたりで遊ぶ",
    comingSoon: "近日公開",
    duoToast: "ふたりプレイは準備中。まずはひとりでどうぞ。",
    settings: "設定",
    language: "言語",
    close: "閉じる",
    dragHint: "ボトルを別の位置へドラッグして入れ替え",
    askHost: "司会に聞く",
    matchTitle: "MATCH  ·  当たった数だけ教えてくれる",
    menuBack: "メニュー",
    lidCovered: "答えは隠してある",
    hostNone: "ひとつも当たってない！",
    hostAll: "全部当たり！",
    hostSome: "{0} つ当たり。",
    bubbleAsk: "ドラッグで並べて、聞いて。",
    bubbleSwapped: "入れ替えたね。もう一回聞く？",
    bubbleNext: "次のラウンド。数はこっちで数える。",
    hud: "ひとり  ·  {0} / {1} ラウンド  ·  {2} 回聞いた",
    winTitle: "パーフェクト",
    winBody: "{1} ラウンドで合計 {0} 回聞いた。\n\n少ないほどよい。ふたりプレイではこれを競う。",
    playAgain: "もう一度",
    backMenu: "メニューへ",
    hostName: "司会",
    historyEmpty: "聞いた記録がここに並ぶ。",
};

const PACKS: Record<Lang, Pack> = { en: EN, zh: ZH, ja: JA };

export function t(key: keyof Pack): string {
    const pack = PACKS[getLang()] || EN;
    return pack[key] || EN[key];
}

export function tf(key: keyof Pack, ...values: Array<string | number>): string {
    let s = t(key);
    for (let i = 0; i < values.length; i++) {
        s = s.replace(`{${i}}`, String(values[i]));
    }
    return s;
}

export function hostLine(correct: number): string {
    if (correct <= 0) {
        return t("hostNone");
    }
    if (correct >= SLOT_COUNT) {
        return t("hostAll");
    }
    return tf("hostSome", correct);
}
