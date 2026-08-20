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
        // 6×6 tutorial step machine.
        private const int ClearPlantStep = 8;  // place a wrong moon
        private const int ClearEmptyStep = 9;  // tap again → back to empty
        private const int ClearFixStep = 10;   // place the correct sun
        private const int HintUseStep = 11;    // hint fills the last cell → win
        private const int CompleteStep = 12;

        // Cell used for the "clear back to empty" lesson (solution = Sun).
        private const int ClearRow = 4;
        private const int ClearCol = 3;

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

        // "Coach" popup: read the tip (with inline sun/moon icons), tap to dismiss, then act.
        private GameObject _coachGo;
        private RectTransform _coachContent;
        private RectTransform _coachCardRt;
        private Text _coachFooter;
        private Button _coachButton;
        private const float CoachContentWidth = 800f;

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

            if (_step == HintUseStep)
            {
                // Waiting for the player to press Hint, which fills the last cell and wins
                // (OnBoardWon then advances to the completion step).
                MaybeIdleNudge();
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

            // Nudge banner removed — the red-bordered highlighted cell + the guide card already
            // tell the player what to do, so the orange "not done yet" banner is unnecessary.
            // Leaving _nudgeGo null makes ShowNudge()/HideNudge() no-ops via their null guards.
            _nudgeGo = null;
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
                    ShowCoach(BordyStrings.Keys.TutorialWelcome, () => EnterStep(1));
                    break;

                case 1: // place a sun
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialGuideSun, new CellGoal(3, 1, Sun));
                    break;

                case 2: // place a moon (+ goal)
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialGuideMoon, new CellGoal(3, 4, Moon));
                    break;

                case 3: // = / × intro
                    ShowCoach(BordyStrings.Keys.TutorialSymbols, () => EnterStep(4));
                    break;

                case 4: // = edge: both suns
                    PreHighlightThenCoach(
                        BordyStrings.Keys.TutorialEquals,
                        new CellGoal(0, 4, Sun),
                        new CellGoal(1, 4, Sun));
                    break;

                case 5: // × edge: top moon, bottom sun
                    PreHighlightThenCoach(
                        BordyStrings.Keys.TutorialCross,
                        new CellGoal(2, 2, Moon),
                        new CellGoal(3, 2, Sun));
                    break;

                case 6: // KEY: no 3 in a row — two given moons → must be sun
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialAvoidThree, new CellGoal(0, 2, Sun));
                    break;

                case 7: // count rule — row already has three moons → must be sun
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialRowCount, new CellGoal(5, 2, Sun));
                    break;

                case ClearPlantStep: // place a wrong moon
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialCheckPlant, new CellGoal(ClearRow, ClearCol, Moon));
                    break;

                case ClearEmptyStep: // tap again → back to empty
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialCheckUse,
                        new CellGoal(ClearRow, ClearCol, BordyPuzzleData.Empty));
                    break;

                case ClearFixStep: // place the correct sun
                    PreHighlightThenCoach(BordyStrings.Keys.TutorialCheckFix,
                        new CellGoal(ClearRow, ClearCol, Sun));
                    break;

                case HintUseStep: // hint fills the last empty cell → win
                    ShowCoach(BordyStrings.Keys.TutorialHintUse, BeginHintLesson);
                    break;

                case CompleteStep:
                    ShowCoach(BordyStrings.Keys.TutorialComplete, () => _nav.BackToLevelSelect());
                    break;
            }

            RefreshToolPills();
        }

        private void BeginHintLesson()
        {
            _goals = null;
            HideCard();
            _board.CanTapCell = (r, c) => false;
        }

        private void GuideCells(string messageKey, params CellGoal[] goals)
            => GuideCellsImpl(messageKey, true, goals);

        private void GuideCells(string messageKey, bool showCard, params CellGoal[] goals)
            => GuideCellsImpl(messageKey, showCard, goals);

        private void GuideCellsImpl(string messageKey, bool showCard, CellGoal[] goals)
        {
            _goals = goals;
            if (showCard)
            {
                LayoutCard(play: true);
                _message.text = BordyStrings.Get(messageKey);
                BordyFonts.Apply(_message);
            }
            else
            {
                HideCard();
            }

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

        /// <summary>
        /// Half-screen coach popup: show a tip, dim the board, and let the player tap anywhere to
        /// dismiss — then <paramref name="onDismiss"/> sets up the actual board interaction.
        /// 半屏教练弹窗：显示提示并遮住棋盘，点任意处关闭后再进行棋盘操作。
        /// </summary>
        /// <summary>
        /// Red-border the target cell(s) immediately, then show the centered coach popup. On
        /// dismiss, allow tapping those cells. So the player sees WHERE (red border, above the
        /// card) while reading the tip, taps to continue, then taps the cell.
        /// 先给目标格加红框，再弹居中提示；关闭后才允许点击。玩家读提示时已看到红框目标。
        /// </summary>
        private void PreHighlightThenCoach(string messageKey, params CellGoal[] goals)
        {
            _goals = goals;
            HideCard();
            for (int i = 0; i < goals.Length; i++)
                _board.SetGuideHighlight(goals[i].Row, goals[i].Col, true);

            var captured = goals;
            ShowCoach(messageKey, () =>
            {
                _board.CanTapCell = (r, c) =>
                {
                    for (int i = 0; i < captured.Length; i++)
                        if (captured[i].Row == r && captured[i].Col == c)
                            return true;
                    return false;
                };
            });
        }

        private void ShowCoach(string messageKey, System.Action onDismiss)
        {
            if (_coachGo == null)
                BuildCoach();

            HideCard(); // no bottom card while the coach is up
            _coachGo.SetActive(true); // activate BEFORE measuring so layout resolves
            _coachGo.transform.SetAsLastSibling();

            BuildCoachContent(BordyStrings.Get(messageKey));
            _coachFooter.text = BordyStrings.Get(BordyStrings.Keys.TutorialCoachTap);
            BordyFonts.Apply(_coachFooter);

            _coachButton.onClick.RemoveAllListeners();
            _coachButton.onClick.AddListener(() =>
            {
                _coachGo.SetActive(false);
                onDismiss?.Invoke();
            });
        }

        private void BuildCoach()
        {
            _coachGo = new GameObject("TutorialCoach", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            _coachGo.transform.SetParent(transform, false);
            // Light dim only — keep the board clearly visible while the coach is up.
            var dim = _coachGo.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.22f);
            dim.raycastTarget = true;
            Stretch(_coachGo.GetComponent<RectTransform>());
            _coachButton = _coachGo.GetComponent<Button>();
            _coachButton.transition = Selectable.Transition.None;

            // Centered card. The board's top rows (where the guided cell lives) stay visible
            // above it, so the red-highlighted target reads clearly through the light dim.
            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(_coachGo.transform, false);
            var cardImg = card.GetComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = BordyUi.Rounded();
            cardImg.type = Image.Type.Sliced;
            cardImg.raycastTarget = false;
            var crt = card.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(900f, 420f); // height is auto-fitted per message
            crt.anchoredPosition = Vector2.zero;
            _coachCardRt = crt;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(card.transform, false);
            _coachContent = contentGo.GetComponent<RectTransform>();
            _coachContent.anchorMin = new Vector2(0f, 0f);
            _coachContent.anchorMax = new Vector2(1f, 1f);
            _coachContent.offsetMin = new Vector2(44f, 96f);
            _coachContent.offsetMax = new Vector2(-44f, -44f);
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.spacing = 6f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var footGo = new GameObject("Footer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            footGo.transform.SetParent(card.transform, false);
            _coachFooter = footGo.GetComponent<Text>();
            _coachFooter.font = BordyFonts.Ui;
            _coachFooter.fontSize = 26;
            _coachFooter.fontStyle = FontStyle.Bold;
            _coachFooter.color = new Color(1f, 0.66f, 0.10f);
            _coachFooter.alignment = TextAnchor.MiddleCenter;
            _coachFooter.raycastTarget = false;
            var frt = _coachFooter.rectTransform;
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(1f, 0f);
            frt.pivot = new Vector2(0.5f, 0f);
            frt.sizeDelta = new Vector2(-48f, 56f);
            frt.anchoredPosition = new Vector2(0f, 30f);

            _coachGo.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Coach content: renders a message with inline {sun}/{moon} icons.
        // Lines split on '\n'. A line with a token becomes a centered icon row (no wrap, keep it
        // short); a plain line wraps as normal text. / 解析 {sun}/{moon} 为图标，'\n' 换行。
        // -----------------------------------------------------------------
        private void BuildCoachContent(string message)
        {
            for (int i = _coachContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(_coachContent.GetChild(i).gameObject);

            var lines = message.Split('\n');
            foreach (var raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    AddSpacer(14f);
                    continue;
                }

                if (line.Contains("{sun}") || line.Contains("{moon}"))
                    AddIconLine(line);
                else
                    AddWrapText(line);
            }

            // Auto-fit the card height to the content so nothing overlaps the footer.
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_coachContent);
            float contentH = LayoutUtility.GetPreferredHeight(_coachContent);
            if (_coachCardRt != null)
            {
                float cardH = Mathf.Clamp(contentH + 140f, 300f, 1200f); // 96 (footer) + 44 (top)
                _coachCardRt.sizeDelta = new Vector2(_coachCardRt.sizeDelta.x, cardH);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_coachContent); // re-lay in the new size
            }
        }

        private void AddSpacer(float h)
        {
            var go = new GameObject("Spacer", typeof(RectTransform));
            go.transform.SetParent(_coachContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = h;
            le.preferredWidth = 4f;
        }

        private void AddWrapText(string text)
        {
            var go = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(_coachContent, false);
            var t = go.GetComponent<Text>();
            t.font = BordyFonts.Ui;
            t.fontSize = 32;
            t.color = new Color(0.16f, 0.16f, 0.18f);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.text = text;
            // Fixed width (so it wraps); the VLG reads the Text's preferred height for that width.
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = CoachContentWidth;
            BordyFonts.Apply(t);
        }

        private void AddIconLine(string line)
        {
            var row = new GameObject("IconLine", typeof(RectTransform));
            row.transform.SetParent(_coachContent, false);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var le = row.AddComponent<LayoutElement>();
            le.preferredWidth = CoachContentWidth;
            le.preferredHeight = 62f;

            int i = 0;
            while (i < line.Length)
            {
                int open = line.IndexOf('{', i);
                if (open < 0)
                {
                    AddCoachTextChunk(row.transform, line.Substring(i));
                    break;
                }
                if (open > i)
                    AddCoachTextChunk(row.transform, line.Substring(i, open - i));

                int close = line.IndexOf('}', open);
                if (close < 0)
                {
                    AddCoachTextChunk(row.transform, line.Substring(open));
                    break;
                }

                string tok = line.Substring(open + 1, close - open - 1);
                if (tok == "sun")
                    AddCoachIcon(row.transform, BordyTokenSprites.Sun);
                else if (tok == "moon")
                    AddCoachIcon(row.transform, BordyTokenSprites.Moon);
                else
                    AddCoachTextChunk(row.transform, "{" + tok + "}");
                i = close + 1;
            }
        }

        private void AddCoachTextChunk(Transform parent, string text)
        {
            text = text.Trim();
            if (text.Length == 0)
                return;
            var go = new GameObject("t", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = BordyFonts.Ui;
            t.fontSize = 32;
            t.color = new Color(0.16f, 0.16f, 0.18f);
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.text = text;
            BordyFonts.Apply(t);
        }

        private void AddCoachIcon(Transform parent, Sprite sprite)
        {
            var go = new GameObject("icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 52f;
            le.preferredHeight = 52f;
        }

        private void HideCard()
        {
            if (_overlayRoot != null)
            {
                _overlayRoot.SetActive(true);
                _overlayRoot.transform.SetAsLastSibling();
            }
            if (_dimmer != null)
            {
                _dimmer.color = Color.clear;
                _dimmer.raycastTarget = false;
            }
            if (_cardRt != null)
                _cardRt.gameObject.SetActive(false);
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
            if (_step == ClearEmptyStep)
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
            if (_step == ClearEmptyStep)
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

            if (_cardRt != null)
                _cardRt.gameObject.SetActive(true); // re-show if HideCard() turned it off

            if (play)
            {
                // Small guide card pinned near the bottom, so it never covers the board.
                // 交互步骤：底部小引导卡，不遮挡棋盘。
                _dimmer.color = Color.clear;
                _dimmer.raycastTarget = false;
                _cardRt.anchorMin = _cardRt.anchorMax = new Vector2(0.5f, 0f);
                _cardRt.pivot = new Vector2(0.5f, 0f);
                _cardRt.sizeDelta = new Vector2(960f, 260f);
                _cardRt.anchoredPosition = new Vector2(0f, 36f);
                _messageRt.offsetMin = new Vector2(36f, 28f);
                _messageRt.offsetMax = new Vector2(-36f, -28f);
                _message.alignment = TextAnchor.UpperLeft;
                _actionGo.SetActive(false);
            }
            else
            {
                // Blocking steps (welcome / symbols / complete): centered modal.
                // 阻塞步骤（欢迎 / 符号 / 完成）：居中弹窗。
                _dimmer.color = ColOverlay;
                _dimmer.raycastTarget = true;
                _cardRt.anchorMin = _cardRt.anchorMax = new Vector2(0.5f, 0.5f);
                _cardRt.pivot = new Vector2(0.5f, 0.5f);
                _cardRt.sizeDelta = new Vector2(860f, 480f);
                _cardRt.anchoredPosition = Vector2.zero;
                _messageRt.offsetMin = new Vector2(48f, 132f);
                _messageRt.offsetMax = new Vector2(-48f, -56f);
                _message.alignment = TextAnchor.UpperCenter;
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
            // Check tool removed from the tutorial. Only the Hint pill is shown (for its lesson).
            bool showHint = _step >= HintUseStep && _step < CompleteStep;
            SetPillVisible("UndoButton", false);
            SetPillVisible("CheckButton", false);
            SetPillVisible("HintButton", showHint);
            SetPillHighlight("HintButton", _step == HintUseStep);
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
