using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Hidden on-device debug reset. Tap the Home title area <see cref="TapsNeeded"/> times
    /// quickly to wipe all player data (profile + tutorial + daily) — the next run behaves like
    /// a brand-new user. Works on a real device where the Editor menu isn't available.
    ///
    /// IMPORTANT: set <see cref="Enabled"/> to false before a public release so players can't
    /// trigger it. (Or wrap this class in a BORDY_DEBUG #if to strip it from release builds.)
    ///
    /// 隐藏的真机调试重置：在主页标题区域快速连点 <see cref="TapsNeeded"/> 次，清空所有玩家数据
    /// （档案 + 教程 + 每日），下次进入即为全新用户。真机上无 Editor 菜单时用它。
    /// 重要：正式发布前把 <see cref="Enabled"/> 设为 false，避免玩家误触。
    /// </summary>
    public class BordyDebugReset : MonoBehaviour
    {
        /// <summary>Master switch — set false for public release. / 总开关，正式发布设为 false。</summary>
        public static readonly bool Enabled = true;

        private const int TapsNeeded = 5;
        private const float Window = 2f; // must finish the taps within this many seconds / 需在这么多秒内点完

        private int _taps;
        private float _firstTapTime;
        private Text _toast;

        private void Start()
        {
            if (!Enabled)
            {
                enabled = false;
                return;
            }
            BuildTapZone();
            BuildToast();
        }

        private Canvas ResolveCanvas() =>
            GetComponentInParent<Canvas>() ?? UnityEngine.Object.FindObjectOfType<Canvas>();

        private void BuildTapZone()
        {
            var canvas = ResolveCanvas();
            if (canvas == null)
                return;

            var go = new GameObject("DebugTapZone", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // fully transparent / 全透明
            img.raycastTarget = true;

            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(500f, 260f);
            rt.anchoredPosition = new Vector2(0f, -460f); // over the "Bordy" title / 覆盖 “Bordy” 标题

            go.AddComponent<BordyDebugTapZone>().OnTap = HandleTap;
        }

        private void BuildToast()
        {
            var canvas = ResolveCanvas();
            if (canvas == null)
                return;

            var go = new GameObject("DebugToast", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(canvas.transform, false);
            _toast = go.GetComponent<Text>();
            _toast.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _toast.fontSize = 32;
            _toast.alignment = TextAnchor.MiddleCenter;
            _toast.color = new Color(0.85f, 0.2f, 0.2f);
            _toast.raycastTarget = false;

            var rt = _toast.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(900f, 60f);
            rt.anchoredPosition = new Vector2(0f, 40f);
            _toast.text = "";
        }

        private void HandleTap()
        {
            float now = Time.unscaledTime;
            if (_taps == 0 || now - _firstTapTime > Window)
            {
                _taps = 0;
                _firstTapTime = now;
            }
            _taps++;

            if (_taps >= TapsNeeded)
            {
                _taps = 0;
                BordyUserService.ResetAll();
                Debug.Log("[BordyDebug] Player data reset via 5-tap gesture.");
                if (_toast != null) _toast.text = "[debug] player data reset ✓ — restart to see it";
            }
            else if (_toast != null)
            {
                _toast.text = $"[debug] tap {_taps}/{TapsNeeded}…";
            }
        }
    }

    /// <summary>Forwards UI clicks to a callback. / 把 UI 点击转发给回调。</summary>
    public class BordyDebugTapZone : MonoBehaviour, IPointerClickHandler
    {
        public Action OnTap;
        public void OnPointerClick(PointerEventData eventData) => OnTap?.Invoke();
    }
}
