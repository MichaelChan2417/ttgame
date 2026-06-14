using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Drives the header timer label. Fully static-driven so control calls (reset / stop /
    /// resume) always take effect regardless of instance/event timing. Counting pauses when
    /// the gameplay scene is left (e.g. back to home) and resumes on return; a solved board
    /// freezes it. Time is "in-game time" only — it does not advance while on the home page.
    ///
    /// 驱动头部计时。完全由静态状态驱动，重置/停止/续玩等控制调用一定生效，不依赖实例或事件时序。
    /// 离开游戏场景（如返回主页）时暂停，回来继续；通关则冻结。只统计“在棋盘里的时间”，
    /// 停留主页时不增长。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class BordyTimer : MonoBehaviour
    {
        private static float s_base;     // banked seconds from earlier segments / 之前各段累计的秒数
        private static float s_runStart; // realtime when the current segment started / 本段开始的实时刻
        private static bool s_running;   // actively counting in the current scene / 当前场景内正在计时
        private static bool s_frozen;    // solved → fully stopped / 通关后完全停止

        private Text _label;

        private static float Now => Time.realtimeSinceStartup;

        /// <summary>The current elapsed time in seconds (float). / 当前已用时间（秒，浮点）。</summary>
        private static float Current => (s_frozen || !s_running) ? s_base : s_base + (Now - s_runStart);

        /// <summary>Current shown time in whole seconds. / 当前显示的秒数（整数）。</summary>
        public static int ElapsedSeconds => (int)Current;

        private void Awake() => _label = GetComponent<Text>();

        private void OnEnable()
        {
            // Resume counting for this scene unless the board is solved (frozen).
            // 进入本场景就继续计时；若已通关（冻结）则不计。
            if (!s_frozen)
            {
                s_runStart = Now;
                s_running = true;
            }
        }

        private void OnDisable()
        {
            // Pause and bank elapsed time when leaving the scene (e.g. back to home).
            // 离开场景时（如返回主页）暂停并结算已用时间。
            if (s_running && !s_frozen)
            {
                s_base += Now - s_runStart;
                s_running = false;
            }
        }

        private void Update()
        {
            int t = (int)Current;
            _label.text = $"◷ {t / 60}:{t % 60:00}";
        }

        /// <summary>Reset to zero and start counting. / 清零并开始计时。</summary>
        public static void ResetClock()
        {
            s_base = 0f;
            s_runStart = Now;
            s_running = true;
            s_frozen = false;
        }

        /// <summary>
        /// Keep the clock running from its current value WITHOUT resetting it (used by the Reset
        /// button — board clears but time continues). Un-freezes a solved clock too.
        /// 让计时从当前值继续、但不清零（重置按钮用——清盘但计时继续）。也会解除通关冻结。
        /// </summary>
        public static void Continue()
        {
            if (!s_frozen && s_running)
                return; // already running normally / 已在正常计时
            // When frozen or paused, s_base already holds the current value — just resume from it.
            // 冻结或暂停时 s_base 即当前值——从它继续即可。
            s_runStart = Now;
            s_running = true;
            s_frozen = false;
        }

        /// <summary>Freeze the clock at its current value (e.g. on solving). / 在当前值冻结（如通关时）。</summary>
        public static void Stop()
        {
            if (s_frozen)
                return;
            if (s_running)
            {
                s_base += Now - s_runStart;
                s_running = false;
            }
            s_frozen = true;
        }

        /// <summary>Resume counting from a saved base time (e.g. daily resume). / 从保存的基准时间继续计时（如每日续玩）。</summary>
        public static void Resume(int baseSeconds)
        {
            s_base = baseSeconds;
            s_runStart = Now;
            s_running = true;
            s_frozen = false;
        }

        /// <summary>Display a fixed, frozen time (e.g. the saved daily result). / 显示一个固定的冻结时间（如每日成绩）。</summary>
        public static void ShowFrozen(int seconds)
        {
            s_base = seconds;
            s_running = false;
            s_frozen = true;
        }

        /// <summary>Format seconds as m:ss. / 把秒格式化为 m:ss。</summary>
        public static string Format(int seconds) => $"{seconds / 60}:{seconds % 60:00}";
    }
}
