using System;

namespace Bordy
{
    /// <summary>
    /// The persisted record of "who is playing". Stored as JSON in <see cref="BordyStore"/>.
    /// In the test phase <see cref="UserId"/> is a locally generated GUID; once a backend is
    /// wired up it is replaced by the real Douyin/TikTok <c>openid</c> (see
    /// <see cref="BordyUserService"/>). Plain public fields only, so JsonUtility can serialize it.
    ///
    /// “是谁在玩”的持久化档案，以 JSON 存进 <see cref="BordyStore"/>。测试阶段 <see cref="UserId"/>
    /// 是本地生成的 GUID；接入后端后会被真实的抖音/TikTok <c>openid</c> 替换（见
    /// <see cref="BordyUserService"/>）。只用公开字段，方便 JsonUtility 序列化。
    /// </summary>
    [Serializable]
    public class BordyUserProfile
    {
        /// <summary>Bumped if the stored shape changes, for future migrations. / 结构变更时自增，便于迁移。</summary>
        public int schemaVersion = 1;

        /// <summary>Local GUID now; real openid once a backend exchanges the login code. / 现为本地 GUID；接后端后为 openid。</summary>
        public string userId = "";

        /// <summary>True until the login code has been exchanged for a real openid. / 在用 code 换到 openid 之前为 true。</summary>
        public bool isAnonymous = true;

        /// <summary>Display name (empty unless authorize/user.info is fetched). / 昵称（未授权拿用户信息时为空）。</summary>
        public string displayName = "";

        public long firstSeenUnix;
        public long lastSeenUnix;

        /// <summary>How many times this user has entered the game. / 该用户进入游戏的次数。</summary>
        public int playCount;
    }
}
