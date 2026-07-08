using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Bordy
{
    /// <summary>
    /// Cloudflare Workers backend: exchanges TT.Login code for open_id and syncs saves.
    /// 对接 Cloudflare Workers：用 TT.Login 的 code 换 open_id 并同步存档。
    /// </summary>
    public sealed class BordyCloudBackend : IBordyAuthBackend
    {
        private readonly string _baseUrl;

        public string SessionToken { get; private set; }
        public string OpenId { get; private set; }

        public BordyCloudBackend(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>Legacy hook — use <see cref="LoginWithCode"/> for full cloud login. / 旧接口，请用 LoginWithCode。</summary>
        public void ExchangeCodeForOpenId(string code, Action<string> onOpenId, Action<string> onError)
        {
            LoginWithCode(code,
                res =>
                {
                    onOpenId?.Invoke(res.openId);
                },
                onError);
        }

        public void LoginWithCode(string code, Action<BordyLoginResponse> onOk, Action<string> onError)
        {
            BordyHttpRunner.Run(LoginCoroutine(code, onOk, onError));
        }

        public void PushSave(Action onOk, Action<string> onError)
        {
            if (string.IsNullOrEmpty(SessionToken) || string.IsNullOrEmpty(OpenId))
            {
                onError?.Invoke("Not logged in");
                return;
            }

            var body = BordyCloudSave.CaptureFromLocal(OpenId);
            BordyHttpRunner.Run(PutSaveCoroutine(body, onOk, onError));
        }

        private IEnumerator LoginCoroutine(string code, Action<BordyLoginResponse> onOk, Action<string> onError)
        {
            var payload = "{\"code\":\"" + EscapeJson(code) + "\"}";
            using var req = BuildJsonPost($"{_baseUrl}/api/auth/login", payload, null);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"HTTP {req.responseCode}: {req.error}");
                yield break;
            }

            var wrapper = JsonUtility.FromJson<BordyLoginResponseWrapper>(req.downloadHandler.text);
            if (wrapper == null)
            {
                onError?.Invoke("Invalid login response");
                yield break;
            }

            if (!string.IsNullOrEmpty(wrapper.error))
            {
                onError?.Invoke(wrapper.error);
                yield break;
            }

            if (string.IsNullOrEmpty(wrapper.openId) || string.IsNullOrEmpty(wrapper.sessionToken))
            {
                onError?.Invoke("Login response missing openId or sessionToken");
                yield break;
            }

            OpenId = wrapper.openId;
            SessionToken = wrapper.sessionToken;

            onOk?.Invoke(new BordyLoginResponse
            {
                openId = wrapper.openId,
                sessionToken = wrapper.sessionToken,
                save = wrapper.save,
                isNewUser = wrapper.isNewUser,
            });
        }

        private IEnumerator PutSaveCoroutine(BordyCloudSave body, Action onOk, Action<string> onError)
        {
            var json = JsonUtility.ToJson(body);
            using var req = BuildJsonPut($"{_baseUrl}/api/save", json, SessionToken);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"HTTP {req.responseCode}: {req.error}");
                yield break;
            }

            var res = JsonUtility.FromJson<BordySavePutResponse>(req.downloadHandler.text);
            if (res != null && !string.IsNullOrEmpty(res.error))
            {
                onError?.Invoke(res.error);
                yield break;
            }

            onOk?.Invoke();
        }

        private static UnityWebRequest BuildJsonPost(string url, string json, string bearer)
        {
            var req = new UnityWebRequest(url, "POST");
            var bytes = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(bearer))
                req.SetRequestHeader("Authorization", "Bearer " + bearer);
            return req;
        }

        private static UnityWebRequest BuildJsonPut(string url, string json, string bearer)
        {
            var req = new UnityWebRequest(url, "PUT");
            var bytes = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(bearer))
                req.SetRequestHeader("Authorization", "Bearer " + bearer);
            return req;
        }

        private static string EscapeJson(string s)
            => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
