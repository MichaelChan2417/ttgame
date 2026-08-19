using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>
    /// Playable sun/moon puzzle board. Puzzle layout comes from <see cref="BordyLevelCatalog"/>
    /// via the serialized <see cref="_levelId"/>.
    /// </summary>
    public class BordyBoardController : MonoBehaviour
    {
        private static readonly Color ColCell = Color.white;
        private static readonly Color ColGivenCell = new Color(0.94f, 0.93f, 0.90f);
        private static readonly Color ColErrorCell = new Color(1.00f, 0.86f, 0.86f);
        private static readonly Color ColGuideCell = new Color(1.00f, 0.97f, 0.78f);
        private static readonly Color ColHintCell = new Color(1.00f, 0.90f, 0.52f);
        private static readonly Color ColCheckArmed = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColCheckArmedLabel = Color.white;
        private static readonly Color ColPillLabel = new Color(0.45f, 0.45f, 0.48f);

        [SerializeField] private string _levelId = BordyLevelCatalog.Level1Id;

        /// <summary>
        /// Set by <see cref="BordyNav"/> before loading the shared 6×6 scene to pick which
        /// puzzle to load (Level 1 vs Daily, which share the same scene). Consumed once.
        /// 由 <see cref="BordyNav"/> 在加载共享 6×6 场景前设置，用于选择载入哪个谜题
        /// （第一关 / 每日挑战共用同一场景）。用后即清。
        /// </summary>
        public static string RequestedLevelId;

        private BordyPuzzleData _puzzle;
        private int _size;
        private int[,] _state = new int[0, 0];
        private BordyTokenView[,] _tokenViews = new BordyTokenView[0, 0];
        private Image[,] _cells = new Image[0, 0];
        private readonly Stack<MoveRecord> _undo = new Stack<MoveRecord>();

        private Text _statusLabel;
        private Transform _boardRoot;
        private bool _won;
        private bool _reviewMode; // read-only view of a finished daily / 每日挑战的只读结算视图
        private string _pinnedStatus;
        private string _pinnedStatusKey;
        private object[] _pinnedStatusArgs;
        private string _transientStatusKey = BordyStrings.Keys.StatusTap;
        private int _hintsUsedThisSession;
        /// <summary><c>-1</c> = unlimited free hints (non-campaign levels).</summary>
        private int _freeHintBudget = -1;
        private bool _hintAdInFlight;

        private int _checksUsedThisSession;
        private int _freeCheckBudget = 1;
        private int _adChecksGranted;
        private bool _checkPickMode;
        private bool _checkAdInFlight;
        private bool[,] _checkMarks;
        private bool[,] _hintMarks;
        private bool[,] _guideMarks;
        private Image _checkButtonImage;
        private Text _checkButtonLabel;
        private Button _checkButton;
        private Button _hintButton;
        private Color _checkButtonIdleColor;
        private Color _checkButtonIdleLabelColor = ColPillLabel;
        private bool _checkButtonIdleCaptured;

        public event Action BoardWon;
        public event Action BlockedTap;
        public event Action<int, int> CellChanged;
        public Func<int, int, bool> CanTapCell { get; set; }

        public BordyPuzzleData Puzzle => _puzzle;
        public int Size => _size;
        public bool IsWon => _won;
        public bool IsCheckPickMode => _checkPickMode;
        public Image GetCellImage(int row, int col) => _cells[row, col];

        public bool HasCheckMark(int row, int col)
        {
            if (_checkMarks == null || row < 0 || col < 0 || row >= _size || col >= _size)
                return false;
            return _checkMarks[row, col];
        }

        private void Start()
        {
            ResolveLevelIdFromScene();

            if (!TryLoadPuzzle(out _puzzle))
            {
                enabled = false;
                return;
            }

            _size = _puzzle.Size;
            _state = new int[_size, _size];
            _tokenViews = new BordyTokenView[_size, _size];
            _cells = new Image[_size, _size];
            _checkMarks = new bool[_size, _size];
            _hintMarks = new bool[_size, _size];
            _guideMarks = new bool[_size, _size];

            if (NeedsRuntimeBoard())
                BordyBoardViewBuilder.EnsureBoard(transform, _puzzle);

            if (!CacheBoardViews())
            {
                Debug.LogError("[BordyBoardController] Board cells missing — rebuild Play scene or run Full Setup.");
                enabled = false;
                return;
            }

            BuildEdgeSymbols();
            WireActionButtons();
            EnsureStatusLabel();
            ApplyHeaderTitle();
            WireBackButton();
            InitHintBudget();
            InitCheckBudget();
            RefreshToolCaps();

            if (_levelId == BordyLevelCatalog.DailyId && BordyDaily.CompletedToday)
            {
                EnterReviewMode();
            }
            else if (_levelId == BordyLevelCatalog.DailyId && BordyDaily.HasProgressToday)
            {
                ResetPuzzle();
                RestoreDailyProgress();
            }
            else if (BordyCampaignCatalog.IsCampaignId(_levelId) && BordyProgress.IsCampaignLevelCompleted(_levelId))
            {
                // A cleared campaign level stays cleared: show the solved board, read-only.
                // 已通关的关卡保持通关：显示解好的棋盘，只读。
                EnterCampaignReview();
            }
            else
            {
                BordyTimer.ResetClock();
                ResetPuzzle();
            }
        }

        private bool TryLoadPuzzle(out BordyPuzzleData puzzle)
        {
            puzzle = null;
            if (_levelId == BordyLevelCatalog.DailyId)
            {
                puzzle = BordyDailyService.GetTodayPuzzleOrNull()
                    ?? BordyDailyService.UseBuiltInFallback("board start without fetch");
                return puzzle != null;
            }

            if (BordyCampaignCatalog.IsCampaignId(_levelId) && BordyCampaignCatalog.TryGet(_levelId, out puzzle))
                return true;

            if (BordyLevelCatalog.TryGet(_levelId, out puzzle))
                return true;

            Debug.LogError($"[BordyBoardController] Unknown level id: {_levelId}");
            return false;
        }

        private bool NeedsRuntimeBoard()
        {
            string sceneName = gameObject.scene.name;
            return sceneName == BordyLevelCatalog.PlayScene
                || _levelId == BordyLevelCatalog.DailyId
                || BordyCampaignCatalog.IsCampaignId(_levelId);
        }

        private void OnDisable() => SaveDailyProgressIfNeeded();

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveDailyProgressIfNeeded();
        }

        /// <summary>Snapshot the in-progress daily so the player can resume later. / 快照进行中的每日挑战，便于之后续玩。</summary>
        private void SaveDailyProgressIfNeeded()
        {
            if (_puzzle == null || _size == 0)
                return;
            if (_levelId != BordyLevelCatalog.DailyId || _reviewMode || _won)
                return;
            BordyDaily.SaveProgress(BordyTimer.ElapsedSeconds, EncodeState());
        }

        /// <summary>Load today's saved in-progress board and resume the gameplay clock. / 载入今天的进行中盘面并续上计时。</summary>
        private void RestoreDailyProgress()
        {
            string board = BordyDaily.ProgressBoard;
            if (board.Length == _size * _size)
            {
                for (int r = 0; r < _size; r++)
                {
                    for (int c = 0; c < _size; c++)
                    {
                        if (_puzzle.IsGiven(r, c))
                            continue; // givens already set by ResetPuzzle / 给定格已由 ResetPuzzle 设好
                        char ch = board[r * _size + c];
                        _state[r, c] = ch == '1' ? BordyPuzzleData.Moon
                                     : ch == '0' ? BordyPuzzleData.Sun
                                     : BordyPuzzleData.Empty;
                        RefreshCell(r, c, animate: false);
                    }
                }
            }

            BordyTimer.Resume(BordyDaily.ProgressSeconds);
            EvaluateBoard();
        }

        private void ResolveLevelIdFromScene()
        {
            // An explicit request (e.g. Daily) wins over scene-name resolution. Consumed once.
            // 显式请求（如每日挑战）优先于按场景名解析，用后即清。
            if (!string.IsNullOrEmpty(RequestedLevelId))
            {
                _levelId = RequestedLevelId;
                RequestedLevelId = null;
                BordyNav.PendingPlayLevelId = _levelId;
                return;
            }

            // Fixed scenes ALWAYS resolve by their own name — never inherit a stale play level id
            // (otherwise entering the tutorial after the daily would load the daily board).
            // 固定场景一律按场景名解析，绝不继承残留的 play 关卡 id（否则玩过每日后进教程会加载成每日盘）。
            string sceneName = gameObject.scene.name;
            if (sceneName == BordyLevelCatalog.TutorialScene)
            {
                _levelId = BordyLevelCatalog.TutorialId;
                return;
            }
            if (sceneName == BordyLevelCatalog.Level1Scene)
            {
                _levelId = BordyLevelCatalog.Level1Id;
                return;
            }

            // Only the generic Play scene uses the pending id as a backup (if RequestedLevelId
            // was lost during scene load).
            // 只有通用 Play 场景才用 pending id 兜底（当 RequestedLevelId 在加载中丢失时）。
            if (sceneName == BordyLevelCatalog.PlayScene && !string.IsNullOrEmpty(BordyNav.PendingPlayLevelId))
            {
                _levelId = BordyNav.PendingPlayLevelId;
            }
        }

        private bool CacheBoardViews()
        {
            _boardRoot = transform.Find("Board");
            if (_boardRoot == null)
            {
                Debug.LogWarning("[BordyBoardController] Board not found.");
                return false;
            }

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    var cellTr = _boardRoot.Find($"Cell_{r}_{c}");
                    if (cellTr == null)
                        return false;

                    _cells[r, c] = cellTr.GetComponent<Image>();
                    var tokenTr = cellTr.Find($"Token_{r}_{c}");
                    if (tokenTr == null)
                        return false;

                    var view = tokenTr.GetComponent<BordyTokenView>();
                    if (view == null)
                        view = tokenTr.gameObject.AddComponent<BordyTokenView>();
                    _tokenViews[r, c] = view;

                    int row = r;
                    int col = c;
                    _cells[r, c].raycastTarget = true;

                    var tap = cellTr.GetComponent<BordyCellTap>();
                    if (tap == null)
                        tap = cellTr.gameObject.AddComponent<BordyCellTap>();
                    tap.Configure(row, col, OnCellTapped);
                }
            }

            return true;
        }

        private void WireActionButtons()
        {
            if (transform.Find("CheckButton") != null)
                WirePill("CheckButton", Check);
            else
                WirePill("UndoButton", Check);
            WirePill("HintButton", Hint);
            WirePill("ResetPill", OnResetPressed);
            CacheCheckButton();
            CacheHintButton();
        }

        private void CacheCheckButton()
        {
            var tr = transform.Find("CheckButton") ?? transform.Find("UndoButton");
            if (tr == null)
                return;

            _checkButtonImage = tr.GetComponent<Image>();
            _checkButtonLabel = tr.Find("Text")?.GetComponent<Text>();
            _checkButton = tr.GetComponent<Button>();
        }

        private void CacheHintButton()
        {
            var tr = transform.Find("HintButton");
            if (tr != null)
                _hintButton = tr.GetComponent<Button>();
        }

        private bool HintCapReached()
            => _freeHintBudget >= 0 && _hintsUsedThisSession >= BordyHintPolicy.MaxUsesPerLevel;

        private bool CheckCapReached()
            => _freeCheckBudget >= 0 && _checksUsedThisSession >= BordyCheckPolicy.MaxUsesPerLevel;

        private void RefreshToolCaps()
        {
            if (_hintButton != null)
                _hintButton.interactable = !HintCapReached();
            if (_checkButton != null)
                _checkButton.interactable = !CheckCapReached() || _checkPickMode;
        }

        /// <summary>
        /// Enter or leave check-pick mode and keep the Check button visually armed so the
        /// player can tell the first tap registered.
        /// 进入/退出选格模式，并让 Check 按钮保持点亮，避免玩家不确定有没有点到。
        /// </summary>
        private void SetCheckPickMode(bool on)
        {
            _checkPickMode = on;
            if (_checkButtonImage == null)
                CacheCheckButton();
            if (_checkButtonImage == null)
                return;

            if (on)
            {
                if (!_checkButtonIdleCaptured)
                {
                    _checkButtonIdleColor = _checkButtonImage.color;
                    _checkButtonIdleLabelColor = _checkButtonLabel != null
                        ? _checkButtonLabel.color
                        : ColPillLabel;
                    _checkButtonIdleCaptured = true;
                }

                _checkButtonImage.color = ColCheckArmed;
                if (_checkButtonLabel != null)
                    _checkButtonLabel.color = ColCheckArmedLabel;
            }
            else if (_checkButtonIdleCaptured)
            {
                _checkButtonImage.color = _checkButtonIdleColor;
                if (_checkButtonLabel != null)
                    _checkButtonLabel.color = _checkButtonIdleLabelColor;
                _checkButtonIdleCaptured = false;
            }
        }

        private void WirePill(string name, UnityEngine.Events.UnityAction action)
        {
            var tr = transform.Find(name);
            if (tr == null)
                return;

            var button = tr.GetComponent<Button>();
            if (button == null)
                button = tr.gameObject.AddComponent<Button>();

            var image = tr.GetComponent<Image>();
            if (image != null)
                button.targetGraphic = image;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /// <summary>
        /// Override the header title with the current puzzle's title at runtime, so the shared
        /// 6×6 scene shows "第一关" for Level 1 and "每日挑战" for the daily.
        /// 运行时用当前谜题标题覆盖头部标题，使共用的 6×6 场景在第一关显示“第一关”、
        /// 每日挑战显示“每日挑战”。
        /// </summary>
        private void ApplyHeaderTitle()
        {
            var titleLabel = transform.Find("Title")?.GetComponent<Text>();
            if (titleLabel == null || _puzzle == null)
                return;

            // English: keep partner's baked puzzle title (e.g. CDN daily). Chinese: localized label.
            titleLabel.text = BordyLocale.Current == BordyLanguage.En
                ? _puzzle.Title
                : BordyStrings.LevelTitle(_levelId);
        }

        private void WireBackButton()
        {
            var back = transform.Find("Back")?.GetComponent<Button>();
            var nav = GetComponent<BordyNav>();
            if (back == null || nav == null)
                return;

            back.onClick.RemoveAllListeners();
            if (_levelId == BordyLevelCatalog.DailyId)
                back.onClick.AddListener(nav.BackToLevelSelect);
            else if (BordyCampaignCatalog.IsCampaignId(_levelId))
                back.onClick.AddListener(nav.BackToCampaignSelect);
            else
                back.onClick.AddListener(nav.BackToLevelSelect);
        }

        /// <summary>Re-apply localized labels after a language change. / 切换语言后刷新文案。</summary>
        public void RefreshLocale()
        {
            if (_puzzle == null)
                return;

            ApplyHeaderTitle();
            BordyLocalization.ApplyGameplay(transform, _levelId == BordyLevelCatalog.TutorialId);

            if (_reviewMode)
            {
                if (_levelId == BordyLevelCatalog.DailyId)
                    PinStatusKey(BordyStrings.Keys.StatusDailyDone, BordyTimer.Format(BordyDaily.CompletedSeconds));
                else
                    SetStatus(BordyStrings.Get(BordyStrings.Keys.StatusWin));
                return;
            }

            if (!string.IsNullOrEmpty(_pinnedStatusKey))
            {
                PinStatusKey(_pinnedStatusKey, _pinnedStatusArgs);
                return;
            }

            if (_won)
            {
                if (_levelId == BordyLevelCatalog.DailyId)
                    PinStatusKey(BordyStrings.Keys.StatusDailyWin, BordyTimer.Format(BordyTimer.ElapsedSeconds));
                else
                    SetStatus(BordyStrings.Get(BordyStrings.Keys.StatusWin));
                return;
            }

            SetTransientStatusKey(_transientStatusKey);
        }

        /// <summary>
        /// Draw the = / × edge symbols from the puzzle data at runtime (removing any baked into
        /// the scene). This lets the shared 6×6 scene display server-driven dailies whose edges
        /// differ from the built-in Level 1.
        /// 运行时按题目数据绘制 = / × 边符号（并移除场景里烘焙的），使共用的 6×6 场景能显示
        /// 边约束各不相同的服务器每日题。
        /// </summary>
        private void BuildEdgeSymbols()
        {
            if (_boardRoot == null)
                return;

            // Remove baked edge labels.
            var stale = new List<GameObject>();
            foreach (Transform child in _boardRoot)
                if (child.name.StartsWith("Edge_"))
                    stale.Add(child.gameObject);
            foreach (var go in stale)
                Destroy(go);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (var e in _puzzle.Edges)
            {
                int r2 = e.Horizontal ? e.Row : e.Row + 1;
                int c2 = e.Horizontal ? e.Col + 1 : e.Col;
                if (e.Row < 0 || e.Col < 0 || r2 >= _size || c2 >= _size)
                    continue;

                Vector2 mid = (_cells[e.Row, e.Col].rectTransform.anchoredPosition +
                               _cells[r2, c2].rectTransform.anchoredPosition) * 0.5f;
                string symbol = e.MustMatch ? "=" : "×";

                var go = new GameObject($"Edge_{e.Row}_{e.Col}_{(e.Horizontal ? "H" : "V")}",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                go.transform.SetParent(_boardRoot, false);
                var t = go.GetComponent<Text>();
                t.text = symbol;
                t.font = font;
                t.fontSize = symbol == "=" ? 44 : 40;
                t.fontStyle = FontStyle.Bold;
                t.alignment = TextAnchor.MiddleCenter;
                t.color = new Color(0.16f, 0.16f, 0.18f);
                t.raycastTarget = false;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;

                var rt = t.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(48, 48);
                rt.anchoredPosition = mid;
                BordyEdgeRules.OrientSymbol(rt, e);
            }
        }

        private void EnsureStatusLabel()
        {
            var existing = transform.Find("StatusBanner");
            if (existing != null)
            {
                _statusLabel = existing.GetComponent<Text>();
                return;
            }

            var go = new GameObject("StatusBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(900f, 56f);
            rt.anchoredPosition = new Vector2(0f, -310f);

            _statusLabel = go.GetComponent<Text>();
            _statusLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _statusLabel.fontSize = 34;
            _statusLabel.alignment = TextAnchor.MiddleCenter;
            _statusLabel.color = new Color(0.16f, 0.55f, 0.28f);
            _statusLabel.raycastTarget = false;
            _statusLabel.text = BordyStrings.Get(BordyStrings.Keys.StatusTap);
        }

        /// <summary>
        /// The Reset button: clears this board back to its givens (and, for the daily, drops the
        /// saved in-progress snapshot so it truly restarts — it does NOT exit or forfeit the day).
        /// 重置按钮：把本盘清回给定格（每日挑战还会丢弃进行中存档以真正重开）——不会退出、也不算放弃当天。
        /// </summary>
        public void OnResetPressed()
        {
            if (_reviewMode)
                return;
            if (_levelId == BordyLevelCatalog.TutorialId && _won)
                return;

            ResetPuzzle();
            // Daily: persist the cleared board with the UNCHANGED time, so resuming keeps the timer.
            // 每日挑战：把清空后的盘面与“不变的时间”一起存档，续玩时计时不丢。
            if (_levelId == BordyLevelCatalog.DailyId)
                SaveDailyProgressIfNeeded();

            if (_levelId == BordyLevelCatalog.TutorialId)
                GetComponent<BordyTutorialGuide>()?.OnPuzzleReset();
        }

        public void ResetPuzzle()
        {
            if (_reviewMode) // a finished daily is read-only / 已完成的每日挑战只读
                return;

            _undo.Clear();
            _won = false;
            SetCheckPickMode(false);
            ClearCheckMarks();
            ClearHintHighlights();

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (_puzzle.IsGiven(r, c))
                        _state[r, c] = _puzzle.GivenValue(r, c);
                    else
                        _state[r, c] = BordyPuzzleData.Empty;

                    RefreshCell(r, c, animate: false);
                }
            }

            // Reset only clears the board — the timer keeps running (it is NOT reset here).
            // 重置只清棋盘——计时继续，不在这里清零。
            BordyTimer.Continue();
            SetTransientStatusKey(BordyStrings.Keys.StatusTap);
        }

        /// <summary>
        /// Show the saved daily result: load the finished board, freeze the recorded time, and
        /// lock all interaction so the player can only look and go back.
        /// 显示保存的每日成绩：载入完成时的盘面、冻结记录的用时，并锁定所有交互，玩家只能看和返回。
        /// </summary>
        private void EnterReviewMode()
        {
            _reviewMode = true;
            _won = true; // blocks taps / undo / hint
            _undo.Clear();

            string board = BordyDaily.CompletedBoard;
            bool valid = board != null && board.Length == _size * _size;

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (valid)
                        _state[r, c] = board[r * _size + c] == '1' ? BordyPuzzleData.Moon : BordyPuzzleData.Sun;
                    else
                        _state[r, c] = _puzzle.Solution[r, c]; // fallback / 兜底
                    RefreshCell(r, c, animate: false);
                }
            }

            BordyTimer.ShowFrozen(BordyDaily.CompletedSeconds);
            PinStatusKey(BordyStrings.Keys.StatusDailyDone, BordyTimer.Format(BordyDaily.CompletedSeconds));

            // Re-show the result popup on re-entry (same as the moment of solving).
            // 再次进入已完成的每日时，也弹出结算弹窗（与当场解出时一致）。
            BordyDailyResultPopup.Show(transform, BordyDaily.CompletedSeconds);
        }

        /// <summary>
        /// Read-only view of a cleared campaign level: fill the solved board and lock input, so
        /// reopening a completed level stays completed instead of resetting to a blank board.
        /// 已通关关卡的只读视图：填入解答并锁定，重开时保持通关而非清空。
        /// </summary>
        private void EnterCampaignReview()
        {
            _reviewMode = true;
            _won = true;
            _undo.Clear();

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    _state[r, c] = _puzzle.Solution[r, c];
                    RefreshCell(r, c, animate: false);
                }
            }

            BordyTimer.Stop();
            SetStatus(BordyStrings.Get(BordyStrings.Keys.StatusWin));
        }

        /// <summary>Encode the board row-major: '0'=sun, '1'=moon, '2'=empty. / 把盘面编码：'0'太阳 '1'月亮 '2'空。</summary>
        private string EncodeState()
        {
            var sb = new StringBuilder(_size * _size);
            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                    sb.Append(_state[r, c] == BordyPuzzleData.Moon ? '1'
                            : _state[r, c] == BordyPuzzleData.Sun ? '0' : '2');
            return sb.ToString();
        }

        public void SetGuideHighlight(int row, int col, bool on)
        {
            if (row < 0 || col < 0 || row >= _size || col >= _size)
                return;

            if (_puzzle.IsGiven(row, col))
                return;

            if (_guideMarks != null)
                _guideMarks[row, col] = on;
            ApplyCellBackground(row, col);
        }

        public void ClearGuideHighlights()
        {
            if (_guideMarks == null)
                return;

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (!_guideMarks[r, c])
                        continue;
                    _guideMarks[r, c] = false;
                    ApplyCellBackground(r, c);
                }
            }
        }

        private void OnCellTapped(int row, int col)
        {
            if (_won || _reviewMode)
                return;

            ClearHintHighlights();

            if (_checkPickMode)
            {
                if (CanTapCell != null && !CanTapCell(row, col))
                {
                    BlockedTap?.Invoke();
                    return;
                }
                ApplyCheckAt(row, col);
                return;
            }

            if (_puzzle.IsGiven(row, col))
            {
                if (CanTapCell != null)
                    BlockedTap?.Invoke();
                return;
            }

            if (CanTapCell != null && !CanTapCell(row, col))
            {
                BlockedTap?.Invoke();
                return;
            }

            int previous = _state[row, col];
            int next = CycleToken(previous);
            _state[row, col] = next;
            _undo.Push(new MoveRecord(row, col, previous));
            RefreshCell(row, col, animate: true);
            EvaluateBoard();
            CellChanged?.Invoke(row, col);
            SaveDailyProgressIfNeeded();
        }

        private static int CycleToken(int value)
        {
            if (value == BordyPuzzleData.Empty)
                return BordyPuzzleData.Sun;
            if (value == BordyPuzzleData.Sun)
                return BordyPuzzleData.Moon;
            return BordyPuzzleData.Empty;
        }

        public void Undo()
        {
            // Kept for baked scene listeners; gameplay uses Check instead.
            // 场景里可能还绑着旧监听，玩法已改为 Check。
        }

        /// <summary>
        /// Start a row/column check: tap Check, then tap a cell. Wrong filled cells in that
        /// row and column stay marked until the player corrects them.
        /// 点 Check 再点一格：标出该行列里填错的格子，改对前一直留着。
        /// </summary>
        public void Check()
        {
            if (_won || _reviewMode)
                return;

            ClearHintHighlights();

            if (_checkPickMode)
            {
                SetCheckPickMode(false);
                SetTransientStatusKey(BordyStrings.Keys.StatusTap);
                return;
            }

            if (CheckCapReached())
            {
                SetTransientStatusKey(BordyStrings.Keys.StatusCheckCap, BordyCheckPolicy.MaxUsesPerLevel);
                RefreshToolCaps();
                return;
            }

            if (NeedsRewardedAdForCheck())
            {
                RequestCheckViaAd();
                return;
            }

            BeginCheckPick();
        }

        private void InitCheckBudget()
        {
            _checksUsedThisSession = 0;
            _adChecksGranted = 0;
            SetCheckPickMode(false);
            _checkAdInFlight = false;
            _freeCheckBudget = BordyCheckPolicy.ResolveBudget(_levelId);
        }

        private bool NeedsRewardedAdForCheck()
            => _freeCheckBudget >= 0
               && _checksUsedThisSession >= _freeCheckBudget + _adChecksGranted
               && !CheckCapReached();

        private void BeginCheckPick()
        {
            SetCheckPickMode(true);
            SetTransientStatusKey(BordyStrings.Keys.StatusCheckPick);
        }

        private void RequestCheckViaAd()
        {
            if (_checkAdInFlight)
                return;

            _checkAdInFlight = true;
            SetTransientStatusKey(BordyStrings.Keys.StatusCheckWatchAd);
            BordyAdsService.ShowRewarded(
                () =>
                {
                    _checkAdInFlight = false;
                    _adChecksGranted++;
                    BeginCheckPick();
                },
                reason =>
                {
                    _checkAdInFlight = false;
                    SetTransientStatusKey(MapAdFailReason(reason));
                });
        }

        private void ApplyCheckAt(int row, int col)
        {
            SetCheckPickMode(false);
            _checksUsedThisSession++;

            int marked = 0;
            for (int c = 0; c < _size; c++)
            {
                if (MarkIfWrong(row, c))
                    marked++;
            }

            for (int r = 0; r < _size; r++)
            {
                if (r == row)
                    continue;
                if (MarkIfWrong(r, col))
                    marked++;
            }

            RefreshCheckMarks();
            RefreshToolCaps();
            if (CheckCapReached())
                SetTransientStatusKey(BordyStrings.Keys.StatusCheckCap, BordyCheckPolicy.MaxUsesPerLevel);
            else if (marked > 0)
                SetTransientStatusKey(BordyStrings.Keys.StatusCheckMarked);
            else
                SetTransientStatusKey(BordyStrings.Keys.StatusCheckNone);
        }

        private bool MarkIfWrong(int row, int col)
        {
            if (_puzzle.IsGiven(row, col))
                return false;
            if (_state[row, col] == BordyPuzzleData.Empty)
                return false;
            if (_state[row, col] == _puzzle.Solution[row, col])
                return false;
            _checkMarks[row, col] = true;
            return true;
        }

        private bool IsCheckWrong(int row, int col)
        {
            if (_puzzle.IsGiven(row, col))
                return false;
            if (_state[row, col] == BordyPuzzleData.Empty)
                return false;
            return _state[row, col] != _puzzle.Solution[row, col];
        }

        private void ClearCheckMarks()
        {
            if (_checkMarks == null)
                return;
            for (int r = 0; r < _size; r++)
                for (int c = 0; c < _size; c++)
                    _checkMarks[r, c] = false;
        }

        private void MarkHintCell(int row, int col)
        {
            if (_hintMarks == null)
                return;
            _hintMarks[row, col] = true;
        }

        private void ClearHintHighlights()
        {
            if (_hintMarks == null || _cells == null || _cells.Length == 0)
                return;

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (!_hintMarks[r, c])
                        continue;

                    _hintMarks[r, c] = false;
                    ApplyCellBackground(r, c);
                }
            }
        }

        private void RefreshCheckMarks()
        {
            if (_checkMarks == null || _cells == null || _cells.Length == 0)
                return;

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (!_checkMarks[r, c])
                        continue;
                    if (!IsCheckWrong(r, c))
                    {
                        _checkMarks[r, c] = false;
                        ApplyCellBackground(r, c);
                        continue;
                    }

                    ApplyCellBackground(r, c);
                }
            }
        }

        public void Hint()
        {
            if (_won || _reviewMode)
                return;

            if (!HasHintableCell())
            {
                SetTransientStatusKey(BordyStrings.Keys.StatusNoHint);
                return;
            }

            if (HintCapReached())
            {
                SetTransientStatusKey(BordyStrings.Keys.StatusHintCap, BordyHintPolicy.MaxUsesPerLevel);
                RefreshToolCaps();
                return;
            }

            if (NeedsRewardedAdForHint())
            {
                RequestHintViaAd();
                return;
            }

            if (ApplyHintInternal())
            {
                _hintsUsedThisSession++;
                UpdateHintStatus();
            }
        }

        private void InitHintBudget()
        {
            _hintsUsedThisSession = 0;
            if (BordyCampaignCatalog.TryGetEntry(_levelId, out var entry))
                _freeHintBudget = BordyHintPolicy.ResolveBudget(_levelId, entry.Tier);
            else
                _freeHintBudget = BordyHintPolicy.ResolveBudget(_levelId, null);
        }

        private bool NeedsRewardedAdForHint()
            => _freeHintBudget >= 0 && _hintsUsedThisSession >= _freeHintBudget && !HintCapReached();

        private void RequestHintViaAd()
        {
            if (_hintAdInFlight)
                return;

            _hintAdInFlight = true;
            SetTransientStatusKey(BordyStrings.Keys.StatusHintLoadingAd);
            BordyAdsService.ShowRewarded(
                () =>
                {
                    _hintAdInFlight = false;
                    if (ApplyHintInternal())
                    {
                        _hintsUsedThisSession++;
                        UpdateHintStatus();
                    }
                },
                reason =>
                {
                    _hintAdInFlight = false;
                    SetTransientStatusKey(MapAdFailReason(reason));
                });
        }

        private static string MapAdFailReason(string reason)
        {
            switch (reason)
            {
                case "editor_no_sim":
                    return BordyStrings.Keys.StatusHintEditorBlocked;
                case "sdk_not_ready":
                    return BordyStrings.Keys.StatusHintSdkNotReady;
                case "not_configured":
                    return BordyStrings.Keys.StatusHintAdNotConfigured;
                default:
                    return BordyStrings.Keys.StatusHintAdFailed;
            }
        }

        private void UpdateHintStatus()
        {
            RefreshToolCaps();
            if (_won || _freeHintBudget < 0)
                return;

            if (HintCapReached())
            {
                SetTransientStatusKey(BordyStrings.Keys.StatusHintCap, BordyHintPolicy.MaxUsesPerLevel);
                return;
            }

            int remaining = Mathf.Max(0, _freeHintBudget - _hintsUsedThisSession);
            if (remaining > 0)
                SetTransientStatusKey(BordyStrings.Keys.StatusHintFreeLeft, remaining);
            else
                SetTransientStatusKey(BordyStrings.Keys.StatusHintWatchAd);
        }

        private bool HasHintableCell()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (!_puzzle.IsGiven(r, c) && _state[r, c] != _puzzle.Solution[r, c])
                        return true;
                }
            }

            return false;
        }

        private bool ApplyHintInternal()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (_puzzle.IsGiven(r, c) || _state[r, c] == _puzzle.Solution[r, c])
                        continue;

                    int previous = _state[r, c];
                    int answer = _puzzle.Solution[r, c];
                    _state[r, c] = answer;
                    _undo.Push(new MoveRecord(r, c, previous));
                    MarkHintCell(r, c);
                    RefreshCell(r, c, animate: true);
                    EvaluateBoard();
                    if (_won)
                        ClearHintHighlights();
                    return true;
                }
            }

            SetTransientStatusKey(BordyStrings.Keys.StatusNoHint);
            return false;
        }

        public int GetCellState(int row, int col) => _state[row, col];

        private void RefreshCell(int row, int col, bool animate)
        {
            var token = _tokenViews[row, col];
            int value = _state[row, col];
            if (animate)
                token.SetKind(value, true);
            else
                token.ShowStatic(value);

            ApplyCellBackground(row, col);
        }

        private void ApplyCellBackground(int row, int col)
        {
            if (_cells == null || _puzzle == null)
                return;

            if (_puzzle.IsGiven(row, col))
            {
                _cells[row, col].color = ColGivenCell;
                return;
            }

            if (_checkMarks != null && _checkMarks[row, col] && IsCheckWrong(row, col))
                _cells[row, col].color = ColErrorCell;
            else if (_hintMarks != null && _hintMarks[row, col])
                _cells[row, col].color = ColHintCell;
            else if (_guideMarks != null && _guideMarks[row, col])
                _cells[row, col].color = ColGuideCell;
            else
                _cells[row, col].color = ColCell;
        }

        private void EvaluateBoard()
        {
            ClearErrorHighlights();
            RefreshCheckMarks();

            if (!IsBoardComplete())
            {
                SetTransientStatusKey(BordyStrings.Keys.StatusTap);
                return;
            }

            if (!IsBoardValid())
            {
                HighlightViolations();
                SetTransientStatusKey(BordyStrings.Keys.StatusErrors);
                return;
            }

            _won = true;
            BordyTimer.Stop(); // freeze the timer on solve / 通关即停表
            ClearHintHighlights();

            if (_levelId == BordyLevelCatalog.DailyId)
            {
                // Record today's result and lock the board into the read-only result view.
                // 记录今天的成绩，并把棋盘锁定为只读结算视图。
                int seconds = BordyTimer.ElapsedSeconds;
                BordyDaily.SaveResult(seconds, EncodeState());
                BordyDaily.ClearProgress(); // solved → no in-progress snapshot needed / 已解出，无需进行中存档
                BordyFriendCloud.UploadDailyTime(BordyDaily.TodayKey, seconds); // publish to friends / 上传成绩供好友查看
                _reviewMode = true;
                PinStatusKey(BordyStrings.Keys.StatusDailyWin, BordyTimer.Format(seconds));

                // Order: on the first Daily solve, prompt "add to home screen" FIRST, then the
                // result popup (time + congrats + friends/invite). Otherwise go straight to result.
                // 顺序：首次完成每日先弹「加桌」，关掉后再弹结算弹窗（含好友/邀请）；否则直接结算。
                var canvasT = transform;
                int solvedSeconds = seconds;
                if (BordyShortcut.ShouldPrompt)
                {
                    BordyShortcut.Prompted = true;
                    BordyShortcutPopup.Show(canvasT, () => BordyDailyResultPopup.Show(canvasT, solvedSeconds));
                }
                else
                {
                    BordyDailyResultPopup.Show(canvasT, solvedSeconds);
                }
            }
            else if (BordyCampaignCatalog.IsCampaignId(_levelId))
            {
                if (BordyCampaignCatalog.TryGetEntry(_levelId, out var entry))
                {
                    BordyProgress.CompleteCampaignLevel(_levelId, entry.Index);
                    if (entry.Tier == "brutal")
                        BordyAdsService.TryShowInterstitial();
                }
                SetStatus(BordyStrings.Get(BordyStrings.Keys.StatusWin));

                // Cleared the whole campaign → "more levels coming soon".
                // 全部闯关通关 → 弹「更多关卡敬请期待」。
                if (BordyProgress.AllCampaignCompleted())
                    BordyMessagePopup.Show(transform,
                        "All levels cleared! 🎉",
                        "You've beaten every Bordy level. More challenges are coming soon — thanks for playing!");
            }
            else
            {
                SetStatus(BordyStrings.Get(BordyStrings.Keys.StatusWin));
            }

            BoardWon?.Invoke();
        }

        private void ClearErrorHighlights()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                    ApplyCellBackground(r, c);
            }
        }

        private void HighlightViolations()
        {
            var bad = new bool[_size, _size];

            MarkRunViolations(bad);
            MarkBalanceViolations(bad);
            MarkEdgeViolations(bad);

            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (bad[r, c] && !_puzzle.IsGiven(r, c))
                        _cells[r, c].color = ColErrorCell;
                }
            }
        }

        private void MarkRunViolations(bool[,] bad)
        {
            for (int r = 0; r < _size; r++)
                MarkLineRunViolations(bad, r, true);
            for (int c = 0; c < _size; c++)
                MarkLineRunViolations(bad, c, false);
        }

        private void MarkLineRunViolations(bool[,] bad, int index, bool horizontal)
        {
            for (int i = 0; i <= _size - 3; i++)
            {
                int a = ReadState(index, i, horizontal);
                int b = ReadState(index, i + 1, horizontal);
                int d = ReadState(index, i + 2, horizontal);
                if (a == BordyPuzzleData.Empty || b == BordyPuzzleData.Empty || d == BordyPuzzleData.Empty)
                    continue;
                if (a == b && b == d)
                {
                    Mark(index, i, bad, horizontal);
                    Mark(index, i + 1, bad, horizontal);
                    Mark(index, i + 2, bad, horizontal);
                }
            }
        }

        private void MarkBalanceViolations(bool[,] bad)
        {
            for (int r = 0; r < _size; r++)
                MarkBalanceLine(bad, r, true);
            for (int c = 0; c < _size; c++)
                MarkBalanceLine(bad, c, false);
        }

        private void MarkBalanceLine(bool[,] bad, int index, bool horizontal)
        {
            int sun = 0;
            int moon = 0;
            for (int i = 0; i < _size; i++)
            {
                int value = ReadState(index, i, horizontal);
                if (value == BordyPuzzleData.Sun) sun++;
                else if (value == BordyPuzzleData.Moon) moon++;
            }

            if (sun != moon)
            {
                for (int i = 0; i < _size; i++)
                    Mark(index, i, bad, horizontal);
            }
        }

        private void MarkEdgeViolations(bool[,] bad)
        {
            foreach (var edge in _puzzle.Edges)
            {
                int aRow = edge.Row;
                int aCol = edge.Col;
                int bRow = edge.Horizontal ? edge.Row : edge.Row + 1;
                int bCol = edge.Horizontal ? edge.Col + 1 : edge.Col;

                int a = _state[aRow, aCol];
                int b = _state[bRow, bCol];
                if (a == BordyPuzzleData.Empty || b == BordyPuzzleData.Empty)
                    continue;

                bool ok = edge.MustMatch ? a == b : a != b;
                if (!ok)
                {
                    bad[aRow, aCol] = true;
                    bad[bRow, bCol] = true;
                }
            }
        }

        private int ReadState(int index, int offset, bool horizontal) =>
            horizontal ? _state[index, offset] : _state[offset, index];

        private static void Mark(int index, int offset, bool[,] bad, bool horizontal)
        {
            if (horizontal)
                bad[index, offset] = true;
            else
                bad[offset, index] = true;
        }

        private bool IsBoardComplete()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (_state[r, c] == BordyPuzzleData.Empty)
                        return false;
                }
            }
            return true;
        }

        private bool IsBoardValid()
        {
            for (int r = 0; r < _size; r++)
            {
                if (!LineValid(r, true))
                    return false;
            }

            for (int c = 0; c < _size; c++)
            {
                if (!LineValid(c, false))
                    return false;
            }

            foreach (var edge in _puzzle.Edges)
            {
                int aRow = edge.Row;
                int aCol = edge.Col;
                int bRow = edge.Horizontal ? edge.Row : edge.Row + 1;
                int bCol = edge.Horizontal ? edge.Col + 1 : edge.Col;

                int a = _state[aRow, aCol];
                int b = _state[bRow, bCol];
                if (a == BordyPuzzleData.Empty || b == BordyPuzzleData.Empty)
                    return false;

                if (edge.MustMatch && a != b)
                    return false;
                if (!edge.MustMatch && a == b)
                    return false;
            }

            return true;
        }

        private bool LineValid(int index, bool horizontal)
        {
            int sun = 0;
            int moon = 0;
            for (int i = 0; i < _size; i++)
            {
                int value = ReadState(index, i, horizontal);
                if (value == BordyPuzzleData.Sun) sun++;
                else if (value == BordyPuzzleData.Moon) moon++;
            }

            if (sun != moon)
                return false;

            for (int i = 0; i <= _size - 3; i++)
            {
                int a = ReadState(index, i, horizontal);
                int b = ReadState(index, i + 1, horizontal);
                int d = ReadState(index, i + 2, horizontal);
                if (a != BordyPuzzleData.Empty && a == b && b == d)
                    return false;
            }

            return true;
        }

        /// <summary>Set the status text directly (one-off). / 直接设置状态文字（一次性）。</summary>
        public void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
        }

        /// <summary>
        /// Pin a status message so the board's own evaluation messages won't overwrite it.
        /// Used by the tutorial guide to keep its hint visible while the player taps.
        /// 钉住一条状态文字，棋盘自身的校验提示不会再覆盖它；新手引导用它在玩家点击时保持提示常驻。
        /// </summary>
        public void PinStatus(string message)
        {
            _pinnedStatusKey = null;
            _pinnedStatusArgs = null;
            _pinnedStatus = message;
            SetStatus(message);
        }

        public void PinStatusKey(string key, params object[] args)
        {
            _pinnedStatusKey = key;
            _pinnedStatusArgs = args;
            _pinnedStatus = BordyStrings.Format(key, args);
            SetStatus(_pinnedStatus);
        }

        /// <summary>Release the pinned status so evaluation messages show again. / 取消钉住。</summary>
        public void ClearStatusPin()
        {
            _pinnedStatus = null;
            _pinnedStatusKey = null;
            _pinnedStatusArgs = null;
        }

        private void SetTransientStatusKey(string key)
        {
            _transientStatusKey = key;
            if (!string.IsNullOrEmpty(_pinnedStatus))
                return;
            SetStatus(BordyStrings.Get(key));
        }

        private void SetTransientStatusKey(string key, params object[] args)
        {
            _transientStatusKey = key;
            if (!string.IsNullOrEmpty(_pinnedStatus))
                return;
            SetStatus(BordyStrings.Format(key, args));
        }

        private readonly struct MoveRecord
        {
            public readonly int Row;
            public readonly int Col;
            public readonly int Previous;

            public MoveRecord(int row, int col, int previous)
            {
                Row = row;
                Col = col;
                Previous = previous;
            }
        }
    }

    internal sealed class BordyCellTap : MonoBehaviour, IPointerClickHandler
    {
        private int _row;
        private int _col;
        private Action<int, int> _onTap;

        public void Configure(int row, int col, Action<int, int> onTap)
        {
            _row = row;
            _col = col;
            _onTap = onTap;
        }

        public void OnPointerClick(PointerEventData eventData) => _onTap?.Invoke(_row, _col);
    }
}
