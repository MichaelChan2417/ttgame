using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Centered modal shown the first time the daily challenge is solved: congratulations + time.
    /// Designed to be extended with rank rows later via <see cref="AddStatRow"/>
    /// (regional rank, friends rank, …).
    ///
    /// 每日挑战首次通关时弹出的居中弹窗：祝贺 + 用时。之后可用 <see cref="AddStatRow"/> 追加排名行
    /// （地区排名、好友排名……）。
    /// </summary>
    public class BordyDailyResultPopup : MonoBehaviour
    {
        private static readonly Color ColDim = new Color(0f, 0f, 0f, 0.5f);
        private static readonly Color ColCard = Color.white;
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColAccent = new Color(1f, 0.66f, 0.10f);

        private RectTransform _statsRoot;
        private float _nextStatY;

        /// <summary>Build and show the popup under the given canvas. / 在指定 Canvas 下创建并显示弹窗。</summary>
        public static BordyDailyResultPopup Show(Transform canvas, int seconds)
        {
            if (canvas == null)
                return null;

            var go = new GameObject("DailyResultPopup", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            var popup = go.AddComponent<BordyDailyResultPopup>();
            popup.Build(seconds);
            return popup;
        }

        private void Build(int seconds)
        {
            // Full-screen dim overlay that blocks input to the board behind it.
            var overlay = gameObject.AddComponent<Image>();
            overlay.color = ColDim;
            overlay.raycastTarget = true;
            Stretch((RectTransform)transform);

            // Card.
            var card = CreatePanel("Card", transform, ColCard);
            var crt = card.rectTransform;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(760, 560);
            crt.anchoredPosition = Vector2.zero;

            var title = CreateText("Title", card.transform, "🎉  Daily Challenge Complete!", 42, FontStyle.Bold, ColInk);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(-48, 92);
            title.rectTransform.anchoredPosition = new Vector2(0, -30);
            title.alignment = TextAnchor.MiddleCenter;
            title.horizontalOverflow = HorizontalWrapMode.Wrap;

            var subtitle = CreateText("Subtitle", card.transform, "Come back tomorrow for a new puzzle!", 28, FontStyle.Normal, ColMuted);
            Anchor(subtitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            subtitle.rectTransform.sizeDelta = new Vector2(-56, 40);
            subtitle.rectTransform.anchoredPosition = new Vector2(0, -128);
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;

            var timeLabel = CreateText("TimeLabel", card.transform, "Your time", 30, FontStyle.Normal, ColMuted);
            Anchor(timeLabel.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            timeLabel.rectTransform.sizeDelta = new Vector2(600, 48);
            timeLabel.rectTransform.anchoredPosition = new Vector2(0, -186);
            timeLabel.alignment = TextAnchor.MiddleCenter;

            var time = CreateText("Time", card.transform, BordyTimer.Format(seconds), 88, FontStyle.Bold, ColAccent);
            Anchor(time.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            time.rectTransform.sizeDelta = new Vector2(600, 120);
            time.rectTransform.anchoredPosition = new Vector2(0, -226);
            time.alignment = TextAnchor.MiddleCenter;

            // Stats container — future rank rows go here.
            var statsGo = new GameObject("Stats", typeof(RectTransform));
            statsGo.transform.SetParent(card.transform, false);
            _statsRoot = (RectTransform)statsGo.transform;
            Anchor(_statsRoot, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            _statsRoot.sizeDelta = new Vector2(-80, 130);
            _statsRoot.anchoredPosition = new Vector2(0, -348);

            // Continue button (closes the popup, revealing the read-only solved board).
            var btn = CreatePanel("ContinueButton", card.transform, ColAccent);
            var brt = btn.rectTransform;
            Anchor(brt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            brt.sizeDelta = new Vector2(380, 96);
            brt.anchoredPosition = new Vector2(0, 40);
            var button = btn.gameObject.AddComponent<Button>();
            button.targetGraphic = btn;
            button.onClick.AddListener(Close);

            var blabel = CreateText("Text", btn.transform, "Continue", 40, FontStyle.Bold, Color.white);
            Stretch(blabel.rectTransform);
            blabel.alignment = TextAnchor.MiddleCenter;
            blabel.raycastTarget = false;
        }

        /// <summary>
        /// Append a stat line, e.g. <c>AddStatRow("Regional rank", "#123")</c>. Call after
        /// <see cref="Show"/> once you have the rank data (regional / friends).
        /// 追加一行统计，如 <c>AddStatRow("Regional rank", "#123")</c>。拿到排名后调用。
        /// </summary>
        public void AddStatRow(string label, string value)
        {
            if (_statsRoot == null)
                return;

            var row = CreateText("Stat", _statsRoot, $"{label}:  {value}", 30, FontStyle.Normal, ColInk);
            Anchor(row.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            row.rectTransform.sizeDelta = new Vector2(-24, 44);
            row.rectTransform.anchoredPosition = new Vector2(0, _nextStatY);
            row.alignment = TextAnchor.MiddleCenter;
            _nextStatY -= 48f;
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        // ---- helpers ----
        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            BordyUi.ApplySliced(img); // rounded 9-slice, runtime-safe
            return img;
        }

        private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }
    }
}
