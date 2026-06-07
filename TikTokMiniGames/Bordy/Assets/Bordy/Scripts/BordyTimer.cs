using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Drives the header timer label. The elapsed total is stored in static fields, so it
    /// survives scene loads: leaving to the home page and coming back resumes the same count
    /// instead of resetting. Timing starts on the FIRST entry into the game only; while you
    /// are on the home page the clock pauses (it only counts in-game time).
    ///
    /// 驱动头部的计时标签。累计时间存在静态字段里，因此能跨场景保留：返回主页再进来会从
    /// 原值继续，而不是清零。只有“第一次进入游戏”时才开始计时；停留在主页时计时暂停
    /// （只统计游戏内时间）。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class BordyTimer : MonoBehaviour
    {
        // Static → the value lives across scene loads within a single Play session.
        // 静态字段 → 数值在一次 Play 内跨场景存活。
        private static bool s_started;
        private static float s_accumulated;   // seconds banked from earlier visits / 之前各次累计的秒数

        private Text _label;
        private float _sessionStart;          // realtime when this visit began / 本次进入的实时刻

        private void Awake()
        {
            _label = GetComponent<Text>();
        }

        private void OnEnable()
        {
            // Start the clock only the first time the game is entered.
            // 只在首次进入游戏时启动计时。
            if (!s_started)
            {
                s_started = true;
                s_accumulated = 0f;
            }
            _sessionStart = Time.realtimeSinceStartup;
        }

        private void OnDisable()
        {
            // Bank this visit's elapsed time so a return trip resumes from here (not cleared).
            // 把本次进入的用时存起来，下次回来从这里继续（不清零）。
            s_accumulated += Time.realtimeSinceStartup - _sessionStart;
        }

        private void Update()
        {
            float total = s_accumulated + (Time.realtimeSinceStartup - _sessionStart);
            int minutes = (int)(total / 60f);
            int seconds = (int)(total % 60f);
            _label.text = $"◷ {minutes}:{seconds:00}";
        }

        /// <summary>Reset the clock to 0 for a brand-new puzzle. / 开新一局时把计时清零。</summary>
        public static void ResetClock()
        {
            s_started = false;
            s_accumulated = 0f;
        }
    }
}
