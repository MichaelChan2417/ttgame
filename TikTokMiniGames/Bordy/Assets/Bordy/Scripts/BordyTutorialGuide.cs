using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Step-by-step onboarding for the 4×4 tutorial. Always uses classic sun / moon art
    /// so the spoken rules match the board even if the player equipped another skin.
    /// 4×4 新手关逐步教学。固定经典太阳/月亮，避免换肤后文案对不上。
    /// </summary>
    public class BordyTutorialGuide : MonoBehaviour
    {
        private static readonly Color ColOverlay = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color ColGuidePill = new Color(1.00f, 0.97f, 0.78f);
        private static readonly Color ColPill = new Color(0.92f, 0.91f, 0.88f);
        private static readonly Color ColNudge = new Color(1.00f, 0.52f, 0.08f);
        private static readonly Color ColNudgeInk = Color.white;

        private const int Sun = BordyPuzzleData.Sun;
        private const int Moon = BordyPuzzleData.Moon;
        private const int CheckPlantStep = 9;
        private const int CheckUseStep = 10;
        private const int CheckFixStep = 11;
        private const int HintUseStep = 12;
        private const int LastCellStep = 13;
        private const int CompleteStep = 14;

        private const int ToolRow = 2;
        private const int ToolCol = 2;
        private const int HintRow = 3;
        private const int HintCol = 1;

        private const float IdleNudgeSeconds = 5f;

        private BordyBoardController _board;
        private BordyNav _nav;
        private int _step;
        private CellGoal[] _goals;
        private float _lastProgressTime;
        private bool _idleNudged;

        private GameObject _overlayRoot;
        private Image _dimmer;
        private RectTransform _cardRt;
        private RectTransform _messageRt;
        private Text _message;
        private GameObject _actionGo;
        private Button _actionButton;
        private Text _actionLabel;
        private GameObject _nudgeGo;
        private RectTransform _nudgeRt;
        private Text _nudgeText;
        private Coroutine _nudgeHide;

        private struct CellGoal
        {
            public int Row;
            public int Col;
            public int Value;

            public CellGoal(int row, int col, int value)
            {
                Row = row;
                Col = col;
                Value = value;
            }
        }

        private IEnumerator Start()
        {
            _board = GetComponent<BordyBoardController>();
            _nav = GetComponent<BordyNav>();
            if (_board == null)
            {
                enabled = false;
                yield break;
            }

            BuildOverlay();
            _board.BoardWon += OnBoardWon;
            _board.BlockedTap += OnBlockedTap;
            _board.CellChanged += OnCellChanged;
            EnterStep(0);
            yield return null;
            HideStatusBanner();
        }

        private void OnDestroy()
        {
            if (_board != null)
            {
                _board.BoardWon -= OnBoardWon;
                _board.BlockedTap -= OnBlockedTap;
                _board.CellChanged -= OnCellChanged;
            }
        }

        private void Update()
        {
            if (_board == null || _step >= CompleteStep)
                return;

            RefreshGoalHighlights();

            if (_step == CheckUseStep)
            {
                MaybeIdleNudge();
                if (_board.HasCheckMark(ToolRow, ToolCol))
                    EnterStep(CheckFixStep);
                return;
            }

            if (_step == HintUseStep)
            {
                MaybeIdleNudge();
                if (_board.GetCellState(HintRow, HintCol) == Sun)
                    EnterStep(LastCellStep);
                return;
            }

            if (_goals == null || _goals.Length == 0)
            {
                if (!IsBlockingStep())
                    MaybeIdleNudge();
                return;
            }

            MaybeIdleNudge();
            if (GoalsMet())
                EnterStep(_step + 1);
        }

        /// <summary>Reset button on the tutorial board restarts the guided steps. / 教程里点重置会从头带一遍。</summary>
        public void OnPuzzleReset()
        {
            if (_step >= CompleteStep)
                return;
            EnterStep(_step <= 0 ? 0 : 1);
        }

        private void BuildOverlay()
        {
            _overlayRoot = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _overlayRoot.transform.SetParent(transform, false);
            _dimmer = _overlayRoot.GetComponent<Image>();
            _dimmer.color = ColOverlay;
            _dimmer.raycastTarget = true;
            Stretch(_overlayRoot.GetComponent<RectTransform>());

            var card = new GameObject("TutorialCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(_overlayRoot.transform, false);
            var cardImg = card.GetComponent<Image>();
            cardImg.color = Color.white;
            cardImg.raycastTarget = true;
            _cardRt = card.GetComponent<RectTransform>();
            _cardRt.anchorMin = new Vector2(0.5f, 0f);
            _cardRt.anchorMax = new Vector2(0.5f, 0f);
            _cardRt.pivot = new Vector2(0.5f, 0f);
            _cardRt.sizeDelta = new Vector2(960f, 360f);
            _cardRt.anchoredPosition = new Vector2(0f, 120f);

            var msgGo = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            msgGo.transform.SetParent(card.transform, false);
            _message = msgGo.GetComponent<Text>();
            _message.font = BordyFonts.Ui;
            _message.fontSize = 30;
            _message.color = new Color(0.16f, 0.16f, 0.18f);
            _message.alignment = TextAnchor.UpperLeft;
            _message.horizontalOverflow = HorizontalWrapMode.Wrap;
            _message.verticalOverflow = VerticalWrapMode.Overflow;
            _message.raycastTarget = false;
            _messageRt = _message.rectTransform;
            _messageRt.anchorMin = new Vector2(0f, 0f);
            _messageRt.anchorMax = new Vector2(1f, 1f);
            _messageRt.offsetMin = new Vector2(36f, 100f);
            _messageRt.offsetMax = new Vector2(-36f, -36f);

            _actionGo = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            _actionGo.transform.SetParent(card.transform, false);
            var btnImg = _actionGo.GetComponent<Image>();
            btnImg.color = new Color(1f, 0.66f, 0.10f);
            btnImg.sprite = BordyUi.Rounded();
            btnImg.type = Image.Type.Sliced;
            _actionButton = _actionGo.GetComponent<Button>();
            _actionButton.targetGraphic = btnImg;
            var btnRt = _actionGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.sizeDelta = new Vector2(360f, 72f);
            btnRt.anchoredPosition = new Vector2(0f, 28f);

            var labelGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelGo.transform.SetParent(_actionGo.transform, false);
            _actionLabel = labelGo.GetComponent<Text>();
            _actionLabel.font = BordyFonts.Ui;
            _actionLabel.fontSize = 30;
            _actionLabel.fontStyle = FontStyle.Bold;
            _actionLabel.color = Color.white;
            _actionLabel.alignment = TextAnchor.MiddleCenter;
            _actionLabel.raycastTarget = false;
            Stretch(labelGo.GetComponent<RectTransform>());

            var nudge = new GameObject("TutorialNudge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Shadow));
            nudge.transform.SetParent(_overlayRoot.transform, false);
            var nudgeImg = nudge.GetComponent<Image>();
            nudgeImg.color = ColNudge;
            nudgeImg.sprite = BordyUi.Rounded();
            nudgeImg.type = Image.Type.Sliced;
            nudgeImg.raycastTarget = false;
            var shadow = nudge.GetComponent<Shadow>();
            shadow.effectColor = new Color(0.35f, 0.12f, 0.02f, 0.55f);
            shadow.effectDistance = new Vector2(0f, -8f);
            _nudgeRt = nudge.GetComponent<RectTransform>();
            _nudgeRt.anchorMin = new Vector2(0.5f, 0f);
            _nudgeRt.anchorMax = new Vector2(0.5f, 0f);
            _nudgeRt.pivot = new Vector2(0.5f, 0f);
            _nudgeRt.sizeDelta = new Vector2(960f, 120f);
            _nudgeRt.anchoredPosition = new Vector2(0f, 308f);

            var nudgeMsg = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            nudgeMsg.transform.SetParent(nudge.transform, false);
            _nudgeText = nudgeMsg.GetComponent<Text>();
            _nudgeText.font = BordyFonts.Ui;
            _nudgeText.fontSize = 36;
            _nudgeText.fontStyle = FontStyle.Bold;
            _nudgeText.color = ColNudgeInk;
            _nudgeText.alignment = TextAnchor.MiddleCenter;
            _nudgeText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _nudgeText.verticalOverflow = VerticalWrapMode.Overflow;
            _nudgeText.raycastTarget = false;
            Stretch(nudgeMsg.GetComponent<RectTransform>(), 24f);
            _nudgeGo = nudge;
            _nudgeGo.SetActive(false);
        }

        private void EnterStep(int step)
        {
            _step = step;
            _goals = null;
            _idleNudged = false;
            _lastProgressTime = Time.unscaledTime;
            HideNudge();
            _board.ClearGuideHighlights();
            _board.ClearStatusPin();
            _board.CanTapCell = null;

            switch (step)
            {
                case 0:
                    ShowBlocking(
                        BordyStrings.Keys.TutorialWelcome,
                        BordyStrings.Keys.TutorialStart,
                        () => EnterStep(1));
                    break;

                case 1:
                    GuideCells(BordyStrings.Keys.TutorialGuideSun, new CellGoal(0, 2, Sun));
                    break;

                case 2:
                    GuideCells(BordyStrings.Keys.TutorialGuideMoon, new CellGoal(0, 3, Moon));
                    break;

                case 3:
                    ShowBlocking(
                        BordyStrings.Keys.TutorialSymbols,
                        BordyStrings.Keys.TutorialContinue,
                        () => EnterStep(4));
                    break;

                case 4:
                    GuideCells(
                        BordyStrings.Keys.TutorialEquals,
                        new CellGoal(1, 1, Moon),
                        new CellGoal(1, 2, Moon));
                    break;

                case 5:
                    GuideCells(
                        BordyStrings.Keys.TutorialCross,
                        new CellGoal(2, 0, Sun),
                        new CellGoal(3, 0, Moon));
                    break;

                case 6:
                    GuideCells(BordyStrings.Keys.TutorialRowCount, new CellGoal(1, 3, Sun));
                    break;

                case 7:
                    GuideCells(BordyStrings.Keys.TutorialColCount, new CellGoal(3, 3, Sun));
                    break;

                case 8:
                    GuideCells(BordyStrings.Keys.TutorialAvoidThree, new CellGoal(2, 1, Moon));
                    break;

                case CheckPlantStep:
                    GuideCells(BordyStrings.Keys.TutorialCheckPlant, new CellGoal(ToolRow, ToolCol, Moon));
                    break;

                case CheckUseStep:
                    BeginCheckLesson();
                    break;

                case CheckFixStep:
                    BeginCheckFixLesson();
                    break;

                case HintUseStep:
                    BeginHintLesson();
                    break;

                case LastCellStep:
                    GuideCells(BordyStrings.Keys.TutorialLastMoon, new CellGoal(3, 2, Moon));
                    break;

                case CompleteStep:
                    ShowBlocking(
                        BordyStrings.Keys.TutorialComplete,
                        BordyStrings.Keys.TutorialToLevelSelect,
                        () => _nav.BackToLevelSelect());
                    break;
            }

            RefreshToolPills();
        }

        private void BeginCheckLesson()
        {
            _goals = null;
            LayoutCard(play: true);
            _message.text = BordyStrings.Get(BordyStrings.Keys.TutorialCheckUse);
            BordyFonts.Apply(_message);
            _board.SetGuideHighlight(ToolRow, ToolCol, true);
            _board.CanTapCell = (r, c) =>
                _board.IsCheckPickMode && r == ToolRow && c == ToolCol;
        }

        private void BeginHintLesson()
        {
            _goals = null;
            LayoutCard(play: true);
            _message.text = BordyStrings.Get(BordyStrings.Keys.TutorialHintUse);
            BordyFonts.Apply(_message);
            _board.CanTapCell = (r, c) => false;
        }

        private void BeginCheckFixLesson()
        {
            _goals = new[] { new CellGoal(ToolRow, ToolCol, Sun) };
            LayoutCard(play: true);
            _message.text = BordyStrings.Get(BordyStrings.Keys.TutorialCheckFix);
            BordyFonts.Apply(_message);
            _board.CanTapCell = (r, c) => r == ToolRow && c == ToolCol;
            _board.SetGuideHighlight(ToolRow, ToolCol, true);
        }

        private void GuideCells(string messageKey, params CellGoal[] goals)
        {
            _goals = goals;
            LayoutCard(play: true);
            _message.text = BordyStrings.Get(messageKey);
            BordyFonts.Apply(_message);

            _board.CanTapCell = (r, c) =>
            {
                for (int i = 0; i < goals.Length; i++)
                {
                    if (goals[i].Row == r && goals[i].Col == c)
                        return true;
                }
                return false;
            };

            for (int i = 0; i < goals.Length; i++)
                _board.SetGuideHighlight(goals[i].Row, goals[i].Col, true);
        }

        private void ShowBlocking(string messageKey, string buttonKey, UnityEngine.Events.UnityAction onClick)
        {
            LayoutCard(play: false);
            _message.text = BordyStrings.Get(messageKey);
            _actionLabel.text = BordyStrings.Get(buttonKey);
            BordyFonts.Apply(_message);
            BordyFonts.Apply(_actionLabel);
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(onClick);
        }

        private bool GoalsMet()
        {
            for (int i = 0; i < _goals.Length; i++)
            {
                var g = _goals[i];
                if (_board.GetCellState(g.Row, g.Col) != g.Value)
                    return false;
            }
            return true;
        }

        private void RefreshGoalHighlights()
        {
            if (_goals == null)
                return;

            for (int i = 0; i < _goals.Length; i++)
            {
                var g = _goals[i];
                bool done = _board.GetCellState(g.Row, g.Col) == g.Value;
                _board.SetGuideHighlight(g.Row, g.Col, !done);
            }
        }

        private void OnBlockedTap()
        {
            NoteProgress();
            if (_step == CheckUseStep && !_board.IsCheckPickMode)
                ShowNudge(BordyStrings.Keys.TutorialNudgeCheck);
            else if (_step == HintUseStep)
                ShowNudge(BordyStrings.Keys.TutorialNudgeHint);
            else
                ShowNudge(BordyStrings.Keys.TutorialNudgeCell);
        }

        private void OnCellChanged(int row, int col)
        {
            NoteProgress();
            if (_goals == null)
                return;

            for (int i = 0; i < _goals.Length; i++)
            {
                var g = _goals[i];
                if (g.Row != row || g.Col != col)
                    continue;
                if (_board.GetCellState(row, col) != g.Value)
                    ShowNudge(BordyStrings.Keys.TutorialNudgeAgain);
                return;
            }
        }

        private void MaybeIdleNudge()
        {
            if (_idleNudged || IsBlockingStep())
                return;
            if (Time.unscaledTime - _lastProgressTime < IdleNudgeSeconds)
                return;

            _idleNudged = true;
            if (_step == CheckUseStep && !_board.IsCheckPickMode)
                ShowNudge(BordyStrings.Keys.TutorialNudgeCheck);
            else if (_step == HintUseStep)
                ShowNudge(BordyStrings.Keys.TutorialNudgeHint);
            else
                ShowNudge(BordyStrings.Keys.TutorialNudgeIdle);
        }

        private void NoteProgress()
        {
            _lastProgressTime = Time.unscaledTime;
            _idleNudged = false;
        }

        private bool IsBlockingStep()
            => _step == 0 || _step == 3 || _step >= CompleteStep;

        private void ShowNudge(string key)
        {
            if (_nudgeGo == null || IsBlockingStep())
                return;

            _nudgeText.text = BordyStrings.Get(key);
            BordyFonts.Apply(_nudgeText);
            _nudgeGo.SetActive(true);
            _nudgeGo.transform.SetAsLastSibling();
            _nudgeRt.localScale = Vector3.one;

            if (_nudgeHide != null)
                StopCoroutine(_nudgeHide);
            _nudgeHide = StartCoroutine(PopAndHideNudge(2.6f));
        }

        private IEnumerator PopAndHideNudge(float seconds)
        {
            float pop = 0.16f;
            float t = 0f;
            while (t < pop && _nudgeRt != null)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / pop);
                float s = Mathf.LerpUnclamped(0.86f, 1.06f, k);
                _nudgeRt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            t = 0f;
            const float settle = 0.08f;
            while (t < settle && _nudgeRt != null)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / settle);
                float s = Mathf.Lerp(1.06f, 1f, k);
                _nudgeRt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            if (_nudgeRt != null)
                _nudgeRt.localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(seconds);
            HideNudge();
        }

        private void HideNudge()
        {
            if (_nudgeHide != null)
            {
                StopCoroutine(_nudgeHide);
                _nudgeHide = null;
            }

            if (_nudgeGo != null)
            {
                _nudgeGo.SetActive(false);
                if (_nudgeRt != null)
                    _nudgeRt.localScale = Vector3.one;
            }
        }

        private static void Stretch(RectTransform rt, float pad)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        /// <summary>Re-apply tutorial copy after language change. / 切换语言后刷新引导文案。</summary>
        public void RefreshLocale() => EnterStep(_step);

        private void OnBoardWon()
        {
            BordyProgress.TutorialCompleted = true;
            EnterStep(CompleteStep);
        }

        private void LayoutCard(bool play)
        {
            if (_overlayRoot != null)
            {
                _overlayRoot.SetActive(true);
                _overlayRoot.transform.SetAsLastSibling();
            }

            if (play)
            {
                _dimmer.color = Color.clear;
                _dimmer.raycastTarget = false;
                _cardRt.sizeDelta = new Vector2(960f, 260f);
                _cardRt.anchoredPosition = new Vector2(0f, 36f);
                _messageRt.offsetMin = new Vector2(36f, 28f);
                _actionGo.SetActive(false);
            }
            else
            {
                _dimmer.color = ColOverlay;
                _dimmer.raycastTarget = true;
                _cardRt.sizeDelta = new Vector2(960f, 380f);
                _cardRt.anchoredPosition = new Vector2(0f, 80f);
                _messageRt.offsetMin = new Vector2(36f, 100f);
                _actionGo.SetActive(true);
            }
        }

        private void HideStatusBanner()
        {
            var banner = transform.Find("StatusBanner");
            if (banner != null)
                banner.gameObject.SetActive(false);
        }

        private void RefreshToolPills()
        {
            bool showCheck = _step >= CheckUseStep && _step < CompleteStep;
            bool showHint = _step >= HintUseStep && _step < CompleteStep;
            SetPillVisible("UndoButton", showCheck);
            SetPillVisible("CheckButton", showCheck);
            SetPillVisible("HintButton", showHint);
            SetPillHighlight("UndoButton", _step == CheckUseStep);
            SetPillHighlight("CheckButton", _step == CheckUseStep);
            SetPillHighlight("HintButton", _step == HintUseStep);
            if (showCheck)
            {
                BringPillForward("UndoButton");
                BringPillForward("CheckButton");
            }

            if (showHint)
                BringPillForward("HintButton");
        }

        private void SetPillVisible(string name, bool visible)
        {
            var pill = transform.Find(name);
            if (pill != null)
                pill.gameObject.SetActive(visible);
        }

        private void SetPillHighlight(string name, bool on)
        {
            var pill = transform.Find(name);
            if (pill == null)
                return;
            var image = pill.GetComponent<Image>();
            if (image != null)
                image.color = on ? ColGuidePill : ColPill;
        }

        private void BringPillForward(string name)
        {
            var pill = transform.Find(name);
            if (pill != null)
                pill.SetAsLastSibling();
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
