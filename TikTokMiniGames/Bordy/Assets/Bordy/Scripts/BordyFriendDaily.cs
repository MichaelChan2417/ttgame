using System.Collections.Generic;

namespace Bordy
{
    /// <summary>
    /// Holds friends' daily-challenge times for today, so the result popup can show a ranking.
    ///
    /// SOURCE IS NOT WIRED YET. TikTok/Douyin does not hand friend openIDs to your server; the
    /// platform way is the relationship-chain "open data domain" (tt.setUserCloudStorage /
    /// tt.getFriendCloudStorage), which the Unity C# SDK does not expose. Once we pick an
    /// approach, that layer calls <see cref="SetFriends"/> with the fetched data. For now the
    /// list is empty, so the popup shows the "invite friends" empty state.
    ///
    /// 好友今日成绩的容器,供结算弹窗做排名。数据源尚未接入:TikTok/抖音不会把好友 openID 给服务器,
    /// 官方走关系链"开放数据域",而 Unity C# SDK 未暴露相关 API。方案定了之后由那层调用
    /// <see cref="SetFriends"/> 灌数据;当前为空,弹窗显示"邀请好友"空状态。
    /// </summary>
    public static class BordyFriendDaily
    {
        public sealed class Entry
        {
            public string Name;
            public int Seconds;
            public bool IsSelf;
        }

        private static readonly List<Entry> _friends = new List<Entry>();

        /// <summary>Friends (excluding self) who finished today. May be empty. / 今日完成的好友（不含自己），可能为空。</summary>
        public static IReadOnlyList<Entry> Friends => _friends;

        public static bool HasFriendData => _friends.Count > 0;

        /// <summary>Called by the data layer once friend times are fetched. / 数据层拿到好友成绩后调用。</summary>
        public static void SetFriends(IEnumerable<Entry> entries)
        {
            _friends.Clear();
            if (entries != null)
                _friends.AddRange(entries);
        }

        /// <summary>Full ranking including self, sorted fastest-first. / 含自己的完整排名，用时升序。</summary>
        public static List<Entry> RankingWithSelf(int selfSeconds)
        {
            var list = new List<Entry>(_friends);
            list.Add(new Entry { Name = "You", Seconds = selfSeconds, IsSelf = true });
            list.Sort((a, b) => a.Seconds.CompareTo(b.Seconds));
            return list;
        }

        /// <summary>Your 1-based rank among friends + you. / 你在“好友+你”中的名次（从 1 起）。</summary>
        public static int SelfRank(int selfSeconds)
        {
            int rank = 1;
            foreach (var f in _friends)
                if (f.Seconds < selfSeconds)
                    rank++;
            return rank;
        }
    }
}
