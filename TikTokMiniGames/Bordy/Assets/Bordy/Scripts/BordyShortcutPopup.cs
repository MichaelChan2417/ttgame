using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// "Add to Home Screen" prompt shown once after the first Daily solve (if not already added).
    /// Tap the dim area to dismiss; "Add" triggers the platform shortcut flow.
    /// 首次完成每日后弹出的加桌提示(未加桌才弹)。点外部关闭,点 Add 触发平台加桌。
    /// </summary>
    public class BordyShortcutPopup : MonoBehaviour
    {
        private static readonly Color ColDim = new Color(0f, 0f, 0f, 0.5f);
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColAccent = new Color(1f, 0.66f, 0.10f);

        private System.Action _onClosed;

        public static void Show(Transform canvas, System.Action onClosed = null)
        {
            if (canvas == null)
            {
                onClosed?.Invoke();
                return;
            }
            var go = new GameObject("ShortcutPopup", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            go.transform.SetAsLastSibling();
            var popup = go.AddComponent<BordyShortcutPopup>();
            popup._onClosed = onClosed;
            popup.Build();
        }

        private void Build()
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
            crt.sizeDelta = new Vector2(760, 460);
            crt.anchoredPosition = Vector2.zero;

            var title = CreateText("Title", card.transform, "Add Bordy to your Home Screen", 40, FontStyle.Bold, ColInk);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(-56, 130);
            title.rectTransform.anchoredPosition = new Vector2(0, -30);
            title.alignment = TextAnchor.MiddleCenter;
            title.horizontalOverflow = HorizontalWrapMode.Wrap;

            var body = CreateText("Body", card.transform, "Jump straight into the Daily Challenge — one tap from your home screen, no searching.", 28, FontStyle.Normal, ColMuted);
            Anchor(body.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            body.rectTransform.sizeDelta = new Vector2(-72, 140);
            body.rectTransform.anchoredPosition = new Vector2(0, -178);
            body.alignment = TextAnchor.UpperCenter;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;

            var add = CreatePanel("AddButton", card.transform, ColAccent);
            var art = add.rectTransform;
            Anchor(art, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            art.sizeDelta = new Vector2(420, 96);
            art.anchoredPosition = new Vector2(0, 44);
            var addBtn = add.gameObject.AddComponent<Button>();
            addBtn.targetGraphic = add;
            addBtn.onClick.AddListener(() => BordyShortcut.Add(_ => Close()));

            var label = CreateText("Text", add.transform, "Add to Home Screen", 34, FontStyle.Bold, Color.white);
            Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
        }

        public void Close()
        {
            var cb = _onClosed;
            _onClosed = null;
            Destroy(gameObject);
            cb?.Invoke();
        }

        // ---- helpers ----
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
