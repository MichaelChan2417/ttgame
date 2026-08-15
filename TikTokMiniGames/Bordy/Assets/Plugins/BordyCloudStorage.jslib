// Bordy ↔ Douyin/TikTok relationship-chain bridge (NATIVE scheme).
// Per the official "开放数据域-好友排行榜接入指南", in the native scheme the open-data
// methods are called directly on `tt` and return via callbacks — no separate open-data
// JS sub-bundle / sharedCanvas is required, so we can render the leaderboard in our own UI.
//
//  BordyDouyinSetUserCloudStorage(arrJson) — arrJson = [{"key":"daily_YYYYMMDD","value":"<json>"}]
//  BordyDouyinFetchFriendDaily(dateKey)    — authorize → getFriendCloudStorage, then push the
//        result to Unity: SendMessage("BordyFriendCloud","OnFriendDaily", {"items":[{name,seconds}]})
//  BordyDouyinShareInvite(title)           — tt.shareAppMessage invite card
mergeInto(LibraryManager.library, {
  BordyDouyinSetUserCloudStorage: function (arrJsonPtr) {
    try {
      var list = JSON.parse(UTF8ToString(arrJsonPtr));
      var g = (typeof tt !== 'undefined') ? tt : (typeof window !== 'undefined' ? window.tt : null);
      if (!g || !g.setUserCloudStorage) { console.warn('[Bordy] setUserCloudStorage unavailable'); return; }
      g.setUserCloudStorage({
        data: list,
        success: function () { console.log('[Bordy] setUserCloudStorage ok'); },
        fail: function (e) { console.warn('[Bordy] setUserCloudStorage fail', e); }
      });
    } catch (e) { console.warn('[Bordy] setUserCloudStorage exception', e); }
  },

  BordyDouyinFetchFriendDaily: function (dateKeyPtr) {
    try {
      var dateKey = UTF8ToString(dateKeyPtr);
      var key = 'daily_' + dateKey;
      var g = (typeof tt !== 'undefined') ? tt : (typeof window !== 'undefined' ? window.tt : null);
      if (!g) { console.warn('[Bordy] tt unavailable'); return; }

      function unity() {
        return (typeof window !== 'undefined' &&
          (window.bordyUnity || window.unityInstance || window.myGameInstance || window.gameInstance)) || null;
      }
      function send(json) {
        var u = unity();
        try { if (u && u.SendMessage) u.SendMessage('BordyFriendCloud', 'OnFriendDaily', json); }
        catch (e) { console.warn('[Bordy] SendMessage fail', e); }
      }

      function fetch() {
        if (!g.getFriendCloudStorage) { console.warn('[Bordy] getFriendCloudStorage unavailable'); return; }
        g.getFriendCloudStorage({
          keyList: [key],
          success: function (res) {
            var items = [];
            try {
              var arr = (res && res.data) ? res.data : [];
              for (var i = 0; i < arr.length; i++) {
                var f = arr[i];
                var name = f.display_name || f.nickname || '';
                var kv = f.data || f.KVDataList || [];
                var secs = -1;
                for (var j = 0; j < kv.length; j++) {
                  if (kv[j].key === key) {
                    try { var v = JSON.parse(kv[j].value); secs = (v && typeof v.seconds === 'number') ? v.seconds : parseInt(kv[j].value, 10); }
                    catch (e2) { secs = parseInt(kv[j].value, 10); }
                  }
                }
                if (secs >= 0) items.push({ name: name, seconds: secs });
              }
            } catch (e) { console.warn('[Bordy] parse friend data', e); }
            send(JSON.stringify({ items: items }));
          },
          fail: function (e) { console.warn('[Bordy] getFriendCloudStorage fail', e); send(JSON.stringify({ items: [] })); }
        });
      }

      // Consent for avatar/nickname + friend relationship is required before reading friends.
      if (g.authorizeOpenContext) {
        g.authorizeOpenContext({
          get_status_only: false,
          success: function () { fetch(); },
          fail: function (e) { console.warn('[Bordy] authorizeOpenContext fail', e); },
          complete: function () { }
        });
      } else {
        fetch();
      }
    } catch (e) { console.warn('[Bordy] fetchFriendDaily exception', e); }
  },

  BordyDouyinShareInvite: function (titlePtr) {
    try {
      var title = UTF8ToString(titlePtr);
      var g = (typeof tt !== 'undefined') ? tt : (typeof window !== 'undefined' ? window.tt : null);
      if (!g || !g.shareAppMessage) { console.warn('[Bordy] shareAppMessage unavailable'); return; }
      g.shareAppMessage({
        title: title,
        success: function () { console.log('[Bordy] share ok'); },
        fail: function (e) { console.warn('[Bordy] share fail', e); }
      });
    } catch (e) { console.warn('[Bordy] shareInvite exception', e); }
  }
});
