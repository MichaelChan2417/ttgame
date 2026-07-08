using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Step-by-step onboarding for the 4×4 tutorial level.
    /// 新手引导关的分步教学。
    /// </summary>
    public class BordyTutorialGuide : MonoBehaviour
    {
        private static readonly Color ColOverlay = new Color(0f, 0f, 0f, 0.45f);

        private BordyBoardController _board;
        private BordyNav _nav;
        private int _step;
        private GameObject _overlayRoot;
        private Text _message;
        private Button _actionButton;
        private Text _actionLabel;

        private void Start()
        {
            _board = GetComponent<BordyBoardController>();
            _nav = GetComponent<BordyNav>();
            if (_board == null)
            {
                enabled = false;
                return;
            }

            BuildOverlay();
            _board.BoardWon += OnBoardWon;
            EnterStep(0);
        }

        private void OnDestroy()
        {
            if (_board != null)
                _board.BoardWon -= OnBoardWon;
        }

        private void Update()
        {
            if (_board == null)
                return;

            const int Sun = BordyPuzzleData.Sun;
            const int Moon = BordyPuzzleData.Moon;

            switch (_step)
            {
                case 1: // tap (0,2) until Sun / 把第三格点成太阳
                    if (_board.GetCellState(0, 2) == Sun)
                        EnterStep(2);
                    break;
                case 2: // tap (0,3) until Moon / 把第四格点成月亮
                    if (_board.GetCellState(0, 3) == Moon)
                        EnterStep(3);
                    break;
                case 4: // "=" lesson: the two cells must MATCH / “=”教学：两格必须相同
                    if (_board.GetCellState(1, 1) == Moon && _board.GetCellState(1, 2) == Moon)
                        EnterStep(5);
                    break;
                case 5: // "×" lesson: the two cells must DIFFER / “×”教学：两格必须不同
                    if (_board.GetCellState(2, 0) == Sun && _board.GetCellState(3, 0) == Moon)
                        EnterStep(6);
                    break;
            }
        }

        private void BuildOverlay()
        {
            _overlayRoot = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _overlayRoot.transform.SetParent(transform, false);
            var bg = _overlayRoot.GetComponent<Image>();
            bg.color = ColOverlay;
            bg.raycastTarget = true;
            Stretch(_overlayRoot.GetComponent<RectTransform>());

            var card = new GameObject("TutorialCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(_overlayRoot.transform, false);
            var cardImg = card.GetComponent<Image>();
            cardImg.color = Color.white;
            cardImg.raycastTarget = true;
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0f);
            cardRt.anchorMax = new Vector2(0.5f, 0f);
            cardRt.pivot = new Vector2(0.5f, 0f);
            cardRt.sizeDelta = new Vector2(960f, 360f);
            cardRt.anchoredPosition = new Vector2(0f, 120f);

            var msgGo = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            msgGo.transform.SetParent(card.transform, false);
            _message = msgGo.GetComponent<Text>();
            _message.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _message.fontSize = 30;
            _message.color = new Color(0.16f, 0.16f, 0.18f);
            _message.alignment = TextAnchor.UpperLeft;
            _message.horizontalOverflow = HorizontalWrapMode.Wrap;
            _message.verticalOverflow = VerticalWrapMode.Overflow;
            _message.raycastTarget = false;
            var msgRt = _message.rectTransform;
            msgRt.anchorMin = new Vector2(0f, 0f);
            msgRt.anchorMax = new Vector2(1f, 1f);
            msgRt.offsetMin = new Vector2(36f, 100f);
            msgRt.offsetMax = new Vector2(-36f, -36f);

            var btnGo = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(card.transform, false);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(1f, 0.66f, 0.10f);
            btnImg.sprite = BordyUi.Rounded(); // runtime sprite — builtin UISprite fails on device / 运行时生成，避免内置资源真机报错
            btnImg.type = Image.Type.Sliced;
            _actionButton = btnGo.GetComponent<Button>();
            _actionButton.targetGraphic = btnImg;
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.sizeDelta = new Vector2(360f, 72f);
            btnRt.anchoredPosition = new Vector2(0f, 28f);

            var labelGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelGo.transform.SetParent(btnGo.transform, false);
            _actionLabel = labelGo.GetComponent<Text>();
            _actionLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _actionLabel.fontSize = 30;
            _actionLabel.fontStyle = FontStyle.Bold;
            _actionLabel.color = Color.white;
            _actionLabel.alignment = TextAnchor.MiddleCenter;
            _actionLabel.raycastTarget = false;
            Stretch(labelGo.GetComponent<RectTransform>());
        }

        private void EnterStep(int step)
        {
            _step = step;
            _board.ClearGuideHighlights();
            _board.ClearStatusPin();
            _board.CanTapCell = null;

            switch (step)
            {
                case 0:
                    ShowOverlay(true);
                    _message.text = BordyStrings.Get(BordyStrings.Keys.TutorialWelcome);
                    _actionLabel.text = BordyStrings.Get(BordyStrings.Keys.TutorialStart);
                    _actionButton.onClick.RemoveAllListeners();
                    _actionButton.onClick.AddListener(() => EnterStep(1));
                    break;

                case 1:
                    ShowOverlay(false);
                    _board.SetGuideHighlight(0, 2, true);
                    _board.CanTapCell = (r, c) => r == 0 && c == 2;
                    _board.PinStatusKey(BordyStrings.Keys.TutorialGuideSun);
                    break;

                case 2:
                    ShowOverlay(false);
                    _board.SetGuideHighlight(0, 3, true);
                    _board.CanTapCell = (r, c) => r == 0 && c == 3;
                    _board.PinStatusKey(BordyStrings.Keys.TutorialGuideMoon);
                    break;

                case 3:
                    ShowOverlay(true);
                    _message.text = BordyStrings.Get(BordyStrings.Keys.TutorialSymbols);
                    _actionLabel.text = BordyStrings.Get(BordyStrings.Keys.TutorialContinue);
                    _actionButton.onClick.RemoveAllListeners();
                    _actionButton.onClick.AddListener(() => EnterStep(4));
                    break;

                case 4:
                    ShowOverlay(false);
                    _board.SetGuideHighlight(1, 1, true);
                    _board.SetGuideHighlight(1, 2, true);
                    _board.CanTapCell = (r, c) => r == 1 && (c == 1 || c == 2);
                    _board.PinStatusKey(BordyStrings.Keys.TutorialEquals);
                    break;

                case 5:
                    ShowOverlay(false);
                    _board.SetGuideHighlight(2, 0, true);
                    _board.SetGuideHighlight(3, 0, true);
                    _board.CanTapCell = (r, c) => c == 0 && (r == 2 || r == 3);
                    _board.PinStatusKey(BordyStrings.Keys.TutorialCross);
                    break;

                case 6:
                    ShowOverlay(false);
                    _board.CanTapCell = null;
                    _board.SetStatus(BordyStrings.Get(BordyStrings.Keys.TutorialFinishRest));
                    break;

                case 7:
                    ShowOverlay(true);
                    _message.text = BordyStrings.Get(BordyStrings.Keys.TutorialComplete);
                    _actionLabel.text = BordyStrings.Get(BordyStrings.Keys.TutorialToLevelSelect);
                    _actionButton.onClick.RemoveAllListeners();
                    _actionButton.onClick.AddListener(() => _nav.BackToLevelSelect());
                    break;
            }
        }

        /// <summary>Re-apply tutorial copy after language change. / 切换语言后刷新引导文案。</summary>
        public void RefreshLocale() => EnterStep(_step);

        private void OnBoardWon()
        {
            BordyProgress.TutorialCompleted = true;
            EnterStep(7);
        }

        private void ShowOverlay(bool show)
        {
            if (_overlayRoot != null)
                _overlayRoot.SetActive(show);
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
