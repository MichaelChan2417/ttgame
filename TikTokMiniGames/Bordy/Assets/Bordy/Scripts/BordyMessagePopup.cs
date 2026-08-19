using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Simple centered message popup (title + body). Tap the dim area to dismiss.
    /// Used e.g. for "all campaign levels cleared — more coming soon".
    /// 简单居中消息弹窗(标题 + 正文),点外部关闭。
    /// </summary>
    public class BordyMessagePopup : MonoBehaviour
    {
        private static readonly Color ColDim = new Color(0f, 0f, 0f, 0.5f);
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);

        public static void Show(Transform canvas, string title, string body)
        {
            if (canvas == null)
                return;
            var go = new GameObject("MessagePopup", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            go.transform.SetAsLastSibling();
            go.AddComponent<BordyMessagePopup>().Build(title, body);
        }

        private void Build(string title, string body)
        {
            var overlay = gameObject.AddComponent<Image>();
            overlay.color = ColDim;
            overlay.raycastTarget = true;
            Stretch((RectTransform)transform);
            var overlayBtn = gameObject.AddComponent<Button>();
            overlayBtn.transition = Selectable.Transition.None;
            overlayBtn.onClick.AddListener(Close);

            var card = CreatePanel("Card", transform, Color.white);
            var crt = card.rectTransform;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(760, 440);
            crt.anchoredPosition = Vector2.zero;

            var t = CreateText("Title", card.transform, title, 42, FontStyle.Bold, ColInk);
            Anchor(t.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            t.rectTransform.sizeDelta = new Vector2(-56, 150);
            t.rectTransform.anchoredPosition = new Vector2(0, -40);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;

            var b = CreateText("Body", card.transform, body, 30, FontStyle.Normal, ColMuted);
            Anchor(b.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            b.rectTransform.offsetMin = new Vector2(48, 60);
            b.rectTransform.offsetMax = new Vector2(-48, -170);
            b.alignment = TextAnchor.UpperCenter;
            b.horizontalOverflow = HorizontalWrapMode.Wrap;
            b.verticalOverflow = VerticalWrapMode.Overflow;

            var hint = CreateText("Hint", card.transform, "Tap anywhere to close", 24, FontStyle.Normal, ColMuted);
            Anchor(hint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            hint.rectTransform.sizeDelta = new Vector2(600, 40);
            hint.rectTransform.anchoredPosition = new Vector2(0, 34);
            hint.alignment = TextAnchor.MiddleCenter;
        }

        public void Close() => Destroy(gameObject);

        private static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            BordyUi.ApplySliced(img);
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
