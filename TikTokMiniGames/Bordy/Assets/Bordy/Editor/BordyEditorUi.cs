using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bordy.EditorTools
{
    /// <summary>Shared UI helpers for scene builders. / 场景构建共用 UI 工具。</summary>
    internal static class BordyEditorUi
    {
        internal static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        internal static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        internal static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);

        /// <summary>Visible back pill — avoids Unicode ← which LegacyRuntime/WebGL often skips.</summary>
        internal static Button CreateBackButton(Transform parent, UnityAction onClick, float topY = 200f)
        {
            var pill = CreateClickablePill("Back", parent, "< Back", ColPill, ColInk);
            var rt = pill.rectTransform;
            Anchor(rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            rt.sizeDelta = new Vector2(200, 76);
            rt.anchoredPosition = new Vector2(32, -topY);
            UnityEventTools.AddPersistentListener(pill.GetComponent<Button>().onClick, onClick);
            return pill.GetComponent<Button>();
        }

        internal static Image CreateClickablePill(string name, Transform parent, string label, Color fill, Color textColor)
        {
            var img = CreatePill(name, parent, label, fill, textColor);
            var button = img.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            return img;
        }

        internal static Image CreatePill(string name, Transform parent, string label, Color fill, Color textColor)
        {
            var img = CreatePanel(name, parent, fill);
            BordyUi.ApplySliced(img);
            var t = CreateText("Text", img.transform, label, 30, FontStyle.Bold);
            t.alignment = TextAnchor.MiddleCenter;
            t.color = textColor;
            Stretch(t.rectTransform);
            return img;
        }

        internal static Image CreatePanel(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        internal static Text CreateText(string name, Transform parent, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.color = ColInk;
            t.alignment = TextAnchor.MiddleLeft;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        internal static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        internal static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
        }
    }
}
