// Bordy ↔ Douyin/TikTok relationship-chain bridge (NATIVE scheme).
// Per the official "开放数据域-好友排行榜接入指南", in the native scheme the open-data
// methods are called directly on `tt` and return via callbacks — no separate open-data
// JS sub-bundle / sharedCanvas is required, so we can render the leaderboard in our own UI.
//
//  BordyDouyinSetUserCloudStorage(arrJson) — arrJson = [{"key":"daily_YYYYMMDD","value":"<json>"}]
//  BordyDouyinFetchFriendDaily(dateKey)    — authorize → getFriendCloudStorage, then push the
//        result to Unity: SendMessage("BordyFriendCloud","OnFriendDaily", {"items":[{name,seconds}]})
//  BordyDouyinShareInvite(title, subtitle) — tt.shareAppMessage DM card (title = 留言)
mergeInto(LibraryManager.library, {
  $BordyTt: {
    sdk: function () {
      try {
        if (typeof TTMinis !== 'undefined' && TTMinis && TTMinis.game) return TTMinis.game;
      } catch (e) {}
      try {
        if (typeof tt !== 'undefined' && tt) return tt;
      } catch (e2) {}
      var root = typeof globalThis !== 'undefined' ? globalThis
        : (typeof window !== 'undefined' ? window : null);
      if (!root) return null;
      if (root.TTMinis && root.TTMinis.game) return root.TTMinis.game;
      if (root.tt) return root.tt;
      return null;
    },
    send: function (method, payload) {
      try {
        var root = typeof globalThis !== 'undefined' ? globalThis
          : (typeof window !== 'undefined' ? window : null);
        var u = root && (root.bordyUnity || root.unityInstance || root.myGameInstance || root.gameInstance || root.Module);
        if (u && typeof u.SendMessage === 'function') {
          u.SendMessage('BordyPlatformHost', method, payload);
          return;
        }
        if (typeof SendMessage === 'function') {
          SendMessage('BordyPlatformHost', method, payload);
        }
      } catch (e) {
        console.warn('[Bordy] SendMessage fail', method, e);
      }
    }
  },

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

  BordyDouyinShareInvite__deps: ['$BordyTt'],
  BordyDouyinShareInvite: function (titlePtr, subtitlePtr) {
    try {
      var title = titlePtr ? UTF8ToString(titlePtr) : '';
      var subtitle = subtitlePtr ? UTF8ToString(subtitlePtr) : '';
      var root = typeof globalThis !== 'undefined' ? globalThis
        : (typeof window !== 'undefined' ? window : {});
      var gMini = (root.TTMinis && root.TTMinis.game) ? root.TTMinis.game : null;
      var gTt = root.tt || null;
      var g = BordyTt.sdk();

      var dump = function (label, obj) {
        var s = '';
        try { s = JSON.stringify(obj); } catch (e) { s = String(obj); }
        console.log('[Bordy][share]', label, obj, s);
      };

      console.log('[Bordy][share] -------- shareAppMessage --------');
      console.log('[Bordy][share] title   =', JSON.stringify(title));
      console.log('[Bordy][share] subtitle=', JSON.stringify(subtitle));
      console.log('[Bordy][share] desc    =', JSON.stringify(title));
      console.log('[Bordy][share] TTMinis.game?', !!gMini,
        'shareAppMessage=', !!(gMini && typeof gMini.shareAppMessage === 'function'),
        'canIUse=', gMini && typeof gMini.canIUse === 'function' ? gMini.canIUse('shareAppMessage') : 'n/a');
      console.log('[Bordy][share] tt?', !!gTt,
        'shareAppMessage=', !!(gTt && typeof gTt.shareAppMessage === 'function'),
        'canIUse=', gTt && typeof gTt.canIUse === 'function' ? gTt.canIUse('shareAppMessage') : 'n/a');
      console.log('[Bordy][share] sdk picked=', g === gMini ? 'TTMinis.game' : (g === gTt ? 'tt' : String(g)),
        'sameObject=', gMini === gTt);

      if (!g || typeof g.shareAppMessage !== 'function') {
        console.warn('[Bordy][share] shareAppMessage UNAVAILABLE — not calling native share');
        return;
      }

      // TikTok Global card uses title + subtitle; Douyin-style hosts put 分享文案 in `desc`.
      // The portal "Description" is the fallback when `desc` is omitted — pass the taunt in all three.
      var payload = {
        title: title,
        subtitle: subtitle,
        desc: title,
        templateType: 1,
        success: function (res) {
          console.log('[Bordy][share] SUCCESS');
          dump('success res', res);
        },
        fail: function (e) {
          console.warn('[Bordy][share] FAIL');
          dump('fail err', e);
          if (e) console.warn('[Bordy][share] fail errMsg=', e.errMsg || e.error || e.message);
        },
        complete: function (res) {
          console.log('[Bordy][share] COMPLETE');
          dump('complete res', res);
        }
      };
      root.__bordyLastShare = { title: title, subtitle: subtitle, desc: title, templateType: 1 };
      console.log('[Bordy][share] calling shareAppMessage with', JSON.stringify(root.__bordyLastShare));
      g.shareAppMessage(payload);
    } catch (e) {
      console.warn('[Bordy][share] exception', e && e.message ? e.message : e, e);
    }
  },

  BordyNavigateToSidebar__deps: ['$BordyTt'],
  BordyNavigateToSidebar: function () {
    // All-in-One guide §3.2: canIUse then startEntranceMission (profile sidebar revisit).
    try {
      var g = BordyTt.sdk();
      if (!g) {
        console.warn('[Bordy] no TTMinis.game/tt for startEntranceMission');
        BordyTt.send('OnSidebarResult', '0');
        return;
      }
      if (typeof g.canIUse === 'function' && !g.canIUse('startEntranceMission')) {
        console.warn('[Bordy] canIUse startEntranceMission = false');
        BordyTt.send('OnSidebarResult', '0');
        return;
      }
      if (typeof g.startEntranceMission !== 'function') {
        console.warn('[Bordy] startEntranceMission unavailable');
        BordyTt.send('OnSidebarResult', '0');
        return;
      }
      g.startEntranceMission({
        success: function () {
          console.log('[Bordy] startEntranceMission ok');
          BordyTt.send('OnSidebarResult', '1');
        },
        fail: function (e) {
          console.warn('[Bordy] startEntranceMission fail', e);
          BordyTt.send('OnSidebarResult', '0');
        },
        complete: function () {}
      });
    } catch (e) {
      console.warn('[Bordy] startEntranceMission exception', e);
      BordyTt.send('OnSidebarResult', '0');
    }
  },

  BordyOpenLink__deps: ['$BordyTt'],
  BordyOpenLink: function (urlPtr) {
    try {
      var url = UTF8ToString(urlPtr);
      var g = BordyTt.sdk();
      console.log('[Bordy] open url', url, 'openSchema', !!(g && g.openSchema), 'openLink', !!(g && g.openLink));

      function opened() { BordyTt.send('OnOpenLinkResult', '1'); }
      function failed() { BordyTt.send('OnOpenLinkResult', '0'); }

      // TikTok Native has openSchema (in-app / system browser). tt.openLink is Douyin-only
      // and may exist as a no-op stub, so do not return early on it.
      if (g && typeof g.openSchema === 'function') {
        g.openSchema({
          schema: url,
          success: function () { console.log('[Bordy] openSchema ok'); opened(); },
          fail: function (e) {
            console.warn('[Bordy] openSchema https fail', e);
            g.openSchema({
              schema: 'sslocal://webview?url=' + encodeURIComponent(url),
              success: function () { console.log('[Bordy] openSchema webview ok'); opened(); },
              fail: function (e2) {
                console.warn('[Bordy] openSchema webview fail', e2);
                if (typeof g.openLink === 'function') {
                  g.openLink({
                    url: url,
                    success: function () { opened(); },
                    fail: function (e3) {
                      console.warn('[Bordy] openLink fail', e3);
                      failed();
                    }
                  });
                } else {
                  failed();
                }
              }
            });
          }
        });
        return;
      }

      if (g && typeof g.openLink === 'function') {
        g.openLink({
          url: url,
          success: function () { opened(); },
          fail: function (e) {
            console.warn('[Bordy] openLink fail', e);
            failed();
          }
        });
        return;
      }

      if (typeof window !== 'undefined' && window.open) {
        var w = window.open(url, '_blank');
        if (w) { opened(); return; }
      }
      failed();
    } catch (e) {
      console.warn('[Bordy] openLink exception', e);
      BordyTt.send('OnOpenLinkResult', '0');
    }
  }
});
