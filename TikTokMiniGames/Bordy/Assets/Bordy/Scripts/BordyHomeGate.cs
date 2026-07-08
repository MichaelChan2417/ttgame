using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Home scene gate: waits for cloud login, shows loading/error, auto-starts tutorial for new users.
    /// 主页门控：等待云端登录，显示加载/错误，新用户自动进入新手教程。
    /// </summary>
    public class BordyHomeGate : MonoBehaviour
    {
        private BordyNav _nav;
        private GameObject _overlay;
        private Text _message;
        private Button _startButton;
        private Button _retryButton;
        private bool _autoStarted;

        public static void EnsureOn(Transform canvasRoot)
        {
            if (canvasRoot.GetComponent<BordyHomeGate>() != null)
                return;
            canvasRoot.gameObject.AddComponent<BordyHomeGate>();
        }

        private void Awake()
        {
            _nav = GetComponent<BordyNav>();
            _startButton = transform.Find("StartButton")?.GetComponent<Button>();
            BuildOverlay();
            BordyUserService.ReadyChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            BordyUserService.ReadyChanged -= Refresh;
        }

        private void BuildOverlay()
        {
            _overlay = new GameObject("LoginOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _overlay.transform.SetParent(transform, false);
            var bg = _overlay.GetComponent<Image>();
            bg.color = new Color(0.96f, 0.95f, 0.92f, 0.92f);
            bg.raycastTarget = true;
            Stretch(_overlay.GetComponent<RectTransform>());

            var msgGo = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            msgGo.transform.SetParent(_overlay.transform, false);
            _message = msgGo.GetComponent<Text>();
            _message.font = BordyFonts.Ui;
            _message.fontSize = 36;
            _message.alignment = TextAnchor.MiddleCenter;
            _message.color = new Color(0.16f, 0.16f, 0.18f);
            _message.horizontalOverflow = HorizontalWrapMode.Wrap;
            _message.verticalOverflow = VerticalWrapMode.Overflow;
            var msgRt = msgGo.GetComponent<RectTransform>();
            msgRt.anchorMin = new Vector2(0.5f, 0.55f);
            msgRt.anchorMax = new Vector2(0.5f, 0.55f);
            msgRt.pivot = new Vector2(0.5f, 0.5f);
            msgRt.sizeDelta = new Vector2(800, 200);
            msgRt.anchoredPosition = Vector2.zero;

            var retryGo = new GameObject("RetryButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            retryGo.transform.SetParent(_overlay.transform, false);
            var retryImg = retryGo.GetComponent<Image>();
            BordyUi.ApplySliced(retryImg);
            retryImg.color = new Color(1f, 0.66f, 0.1f);
            var retryRt = retryGo.GetComponent<RectTransform>();
            retryRt.anchorMin = new Vector2(0.5f, 0.42f);
            retryRt.anchorMax = new Vector2(0.5f, 0.42f);
            retryRt.pivot = new Vector2(0.5f, 0.5f);
            retryRt.sizeDelta = new Vector2(360, 100);
            retryRt.anchoredPosition = Vector2.zero;
            _retryButton = retryGo.GetComponent<Button>();
            _retryButton.targetGraphic = retryImg;
            _retryButton.onClick.AddListener(() => BordyUserService.RetryCloudLogin());

            var retryLabelGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            retryLabelGo.transform.SetParent(retryGo.transform, false);
            var retryLabel = retryLabelGo.GetComponent<Text>();
            retryLabel.font = BordyFonts.Ui;
            retryLabel.fontSize = 36;
            retryLabel.fontStyle = FontStyle.Bold;
            retryLabel.alignment = TextAnchor.MiddleCenter;
            retryLabel.color = Color.white;
            retryLabel.text = "Retry";
            Stretch(retryLabelGo.GetComponent<RectTransform>());
        }

        private void Refresh()
        {
            if (!BordyUserService.CloudEnabled)
            {
                _overlay.SetActive(false);
                SetStartInteractable(true);
                return;
            }

            if (!BordyUserService.IsReady)
            {
                _overlay.SetActive(true);
                _retryButton.gameObject.SetActive(false);
                _message.text = BordyStrings.Get(BordyStrings.Keys.HomeLoginLoading);
                SetStartInteractable(false);
                return;
            }

            if (BordyUserService.CloudLoginFailed)
            {
                _overlay.SetActive(true);
                _retryButton.gameObject.SetActive(true);
                _retryButton.GetComponentInChildren<Text>().text = BordyStrings.Get(BordyStrings.Keys.HomeLoginRetry);
                _message.text = BordyStrings.Get(BordyStrings.Keys.HomeLoginFailed);
                SetStartInteractable(false);
                return;
            }

            _overlay.SetActive(false);
            SetStartInteractable(true);

            if (!_autoStarted && BordyUserService.CloudLoggedIn && BordyUserService.IsFirstTimePlayer)
                StartCoroutine(AutoStartTutorial());
        }

        private IEnumerator AutoStartTutorial()
        {
            _autoStarted = true;
            yield return new WaitForSecondsRealtime(0.6f);
            if (_nav != null && BordyUserService.IsReady && !BordyUserService.CloudLoginFailed)
                _nav.StartGame();
        }

        private void SetStartInteractable(bool on)
        {
            if (_startButton != null)
                _startButton.interactable = on;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
