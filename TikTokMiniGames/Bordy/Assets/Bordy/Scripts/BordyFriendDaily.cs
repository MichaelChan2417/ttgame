using System.Collections.Generic;

namespace Bordy
{
    /// <summary>
    /// Holds friends' daily-challenge times for today, so the result popup can show a ranking.
    /// Filled by <see cref="BordyFriendCloudReceiver"/> after authorize + getFriendCloudStorage.
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
