using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Drives the header timer label. Pauses on the home page; resets to 0 when
    /// <see cref="ResetClock"/> is called (e.g. board reset).
    ///
    /// 驱动头部计时。在主页暂停；调用 <see cref="ResetClock"/>（如重置棋盘）时清零并重新计时。
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class BordyTimer : MonoBehaviour
    {
        private static float s_accumulated;
        private static event Action ClockReset;

        private Text _label;
        private float _sessionStart;

        private void Awake()
        {
            _label = GetComponent<Text>();
        }

        private void OnEnable()
        {
            ClockReset += HandleClockReset;
            _sessionStart = Time.realtimeSinceStartup;
        }

        private void OnDisable()
        {
            ClockReset -= HandleClockReset;
            s_accumulated += Time.realtimeSinceStartup - _sessionStart;
        }

        private void Update()
        {
            float total = s_accumulated + (Time.realtimeSinceStartup - _sessionStart);
            int minutes = (int)(total / 60f);
            int seconds = (int)(total % 60f);
            _label.text = $"◷ {minutes}:{seconds:00}";
        }

        /// <summary>Reset elapsed time and restart counting from zero. / 清零并重新开始计时。</summary>
        public static void ResetClock()
        {
            s_accumulated = 0f;
            ClockReset?.Invoke();
        }

        private void HandleClockReset()
        {
            _sessionStart = Time.realtimeSinceStartup;
        }
    }
}
