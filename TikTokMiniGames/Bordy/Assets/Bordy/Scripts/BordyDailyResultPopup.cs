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
        private static readonly Color ColAccentDark = new Color(0.82f, 0.52f, 0.03f);
        private static readonly Color ColBox = new Color(0.95f, 0.95f, 0.97f);
        private static readonly Color ColRow = new Color(0.93f, 0.93f, 0.95f);
        private static readonly Color ColSelfRow = new Color(1f, 0.92f, 0.76f);

        private RectTransform _statsRoot;
        private float _nextStatY;
        private int _seconds;
        private System.Action _onClosed;
        private static BordyDailyResultPopup s_current;

        /// <summary>Run a callback after this popup closes (e.g. chain the add-to-home prompt). / 关闭后回调。</summary>
        public void SetOnClosed(System.Action onClosed) => _onClosed = onClosed;

        /// <summary>Build and show the popup under the given canvas. / 在指定 Canvas 下创建并显示弹窗。</summary>
        public static BordyDailyResultPopup Show(Transform canvas, int seconds)
        {
            if (canvas == null)
                return null;

            var go = new GameObject("DailyResultPopup", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            var popup = go.AddComponent<BordyDailyResultPopup>();
            popup.Build(seconds);

            // Kick off the async friend fetch; results arrive via BordyFriendCloudReceiver
            // and call RefreshOpenFriends() to repopulate the box.
            // 异步拉取好友成绩;结果回来后刷新方块。
            BordyFriendCloud.RequestFriendDaily(BordyDaily.TodayKey);
            return popup;
        }

        /// <summary>Rebuild the friends box of the open popup once friend data arrives. / 好友数据到达后刷新。</summary>
        public static void RefreshOpenFriends()
        {
            if (s_current != null)
                s_current.RebuildFriends();
        }

        private void RebuildFriends()
        {
            var card = transform.Find("Card");
            if (card == null)
                return;
            var old = card.Find("FriendsBox");
            if (old != null)
                Destroy(old.gameObject);
            BuildFriendsBox(card, _seconds);
        }

        private void OnDestroy()
        {
            if (s_current == this)
                s_current = null;
        }

        private void Build(int seconds)
        {
            _seconds = seconds;
            s_current = this;

            // Full-screen dim overlay that blocks input to the board behind it.
            // Tapping the dim area (outside the card) closes the popup.
            // 点击卡片外的暗色区域关闭弹窗。
            var overlay = gameObject.AddComponent<Image>();
            overlay.color = ColDim;
            overlay.raycastTarget = true;
            Stretch((RectTransform)transform);
            var overlayBtn = gameObject.AddComponent<Button>();
            overlayBtn.transition = Selectable.Transition.None; // don't tint the dim on press
            overlayBtn.onClick.AddListener(Close);

            // Card.
            var card = CreatePanel("Card", transform, ColCard);
            var crt = card.rectTransform;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(760, 648);
            crt.anchoredPosition = Vector2.zero;

            var title = CreateText("Title", card.transform, "🎉  Daily Challenge Complete!", 42, FontStyle.Bold, ColInk);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            title.rectTransform.sizeDelta = new Vector2(-48, 92);
            title.rectTransform.anchoredPosition = new Vector2(0, -28);
            title.alignment = TextAnchor.MiddleCenter;
            title.horizontalOverflow = HorizontalWrapMode.Wrap;

            var subtitle = CreateText("Subtitle", card.transform, "Come back tomorrow for a new puzzle!", 26, FontStyle.Normal, ColMuted);
            Anchor(subtitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            subtitle.rectTransform.sizeDelta = new Vector2(-56, 36);
            subtitle.rectTransform.anchoredPosition = new Vector2(0, -118);
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;

            // Compact one-line "Your time  MM:SS" (time coloured via rich text).
            var yourTime = CreateText("YourTime", card.transform,
                $"Your time   <b><color=#FFA81A>{BordyTimer.Format(seconds)}</color></b>", 38, FontStyle.Normal, ColInk);
            Anchor(yourTime.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            yourTime.rectTransform.sizeDelta = new Vector2(-56, 56);
            yourTime.rectTransform.anchoredPosition = new Vector2(0, -168);
            yourTime.alignment = TextAnchor.MiddleCenter;

            // Friends box.
            BuildFriendsBox(card.transform, seconds);
        }

        /// <summary>Friends ranking box, or an invite-friends empty state. / 好友排名方块，或邀请好友空状态。</summary>
        private void BuildFriendsBox(Transform card, int seconds)
        {
            var box = CreatePanel("FriendsBox", card, ColBox);
            box.raycastTarget = false;
            var rt = box.rectTransform;
            Anchor(rt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            rt.sizeDelta = new Vector2(-80, 372);
            rt.anchoredPosition = new Vector2(0, -232);

            var header = CreateText("FriendsHeader", box.transform, "Friends today", 30, FontStyle.Bold, ColInk);
            Anchor(header.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.rectTransform.sizeDelta = new Vector2(-32, 48);
            header.rectTransform.anchoredPosition = new Vector2(0, -14);
            header.alignment = TextAnchor.MiddleCenter;

            if (!BordyFriendDaily.HasFriendData)
            {
                var empty = CreateText("Empty", box.transform,
                    "No friends have finished today.\n\nInvite friends to compare times!", 27, FontStyle.Normal, ColMuted);
                Anchor(empty.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
                empty.rectTransform.offsetMin = new Vector2(30, 110);
                empty.rectTransform.offsetMax = new Vector2(-30, -70);
                empty.alignment = TextAnchor.MiddleCenter;
                empty.horizontalOverflow = HorizontalWrapMode.Wrap;
                empty.verticalOverflow = VerticalWrapMode.Overflow;

                var invite = CreatePanel("InviteButton", box.transform, ColAccent);
                var irt = invite.rectTransform;
                Anchor(irt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
                irt.sizeDelta = new Vector2(340, 80);
                irt.anchoredPosition = new Vector2(0, 28);
                var ibtn = invite.gameObject.AddComponent<Button>();
                ibtn.targetGraphic = invite;
                ibtn.onClick.AddListener(() =>
                    BordyFriendCloud.ShareInvite("Can you beat my Bordy time?"));
                var ilabel = CreateText("Text", invite.transform, "Invite friends", 30, FontStyle.Bold, Color.white);
                Stretch(ilabel.rectTransform);
                ilabel.alignment = TextAnchor.MiddleCenter;
                return;
            }

            var ranking = BordyFriendDaily.RankingWithSelf(seconds);
            float y = -74f;
            int shown = 0;
            foreach (var e in ranking)
            {
                if (shown >= 5)
                    break;
                BuildFriendRow(box.transform, shown + 1, e, y);
                y -= 56f;
                shown++;
            }
        }

        private void BuildFriendRow(Transform box, int rank, BordyFriendDaily.Entry e, float y)
        {
            var row = CreatePanel("Row", box, e.IsSelf ? ColSelfRow : ColRow);
            row.raycastTarget = false;
            var rt = row.rectTransform;
            Anchor(rt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            rt.sizeDelta = new Vector2(-28, 48);
            rt.anchoredPosition = new Vector2(0, y);

            Color txt = e.IsSelf ? ColAccentDark : ColInk;
            FontStyle fs = e.IsSelf ? FontStyle.Bold : FontStyle.Normal;

            var left = CreateText("L", row.transform, $"#{rank}   {e.Name}", 26, fs, txt);
            Anchor(left.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f));
            left.rectTransform.offsetMin = new Vector2(22, 0);
            left.rectTransform.offsetMax = new Vector2(-120, 0);
            left.alignment = TextAnchor.MiddleLeft;

            var right = CreateText("R", row.transform, BordyTimer.Format(e.Seconds), 26, fs, txt);
            Anchor(right.rectTransform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
            right.rectTransform.offsetMin = new Vector2(-118, 0);
            right.rectTransform.offsetMax = new Vector2(-22, 0);
            right.alignment = TextAnchor.MiddleRight;
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
