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

        public event Action BoardWon;
        public Func<int, int, bool> CanTapCell { get; set; }

        public BordyPuzzleData Puzzle => _puzzle;
        public int Size => _size;
        public bool IsWon => _won;
        public Image GetCellImage(int row, int col) => _cells[row, col];

        private void Start()
        {
            ResolveLevelIdFromScene();
            if (!BordyLevelCatalog.TryGet(_levelId, out _puzzle))
            {
                Debug.LogError($"[BordyBoardController] Unknown level id: {_levelId}");
                enabled = false;
                return;
            }

            _size = _puzzle.Size;
            _state = new int[_size, _size];
            _tokenViews = new BordyTokenView[_size, _size];
            _cells = new Image[_size, _size];

            if (!CacheBoardViews())
            {
                enabled = false;
                return;
            }

            WireActionButtons();
            EnsureStatusLabel();
            ApplyHeaderTitle();

            // Daily already solved today → show the saved result, read-only.
            // 今天已解出每日挑战 → 显示保存的成绩，只读。
            if (_levelId == BordyLevelCatalog.DailyId && BordyDaily.CompletedToday)
            {
                EnterReviewMode();
            }
            else if (_levelId == BordyLevelCatalog.DailyId && BordyDaily.HasProgressToday)
            {
                // Daily, not yet solved, has today's snapshot → resume board + clock.
                // 每日挑战未解出且有今天存档 → 续上盘面与计时。
                ResetPuzzle();
                RestoreDailyProgress();
            }
            else
            {
                // Fresh attempt → start the clock from zero, then lay out the board.
                // 全新一局 → 计时从 0 开始，再铺好棋盘。
                BordyTimer.ResetClock();
                ResetPuzzle();
            }
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
                return;
            }

            string sceneName = gameObject.scene.name;
            if (sceneName == BordyLevelCatalog.TutorialScene)
                _levelId = BordyLevelCatalog.TutorialId;
            else if (sceneName == BordyLevelCatalog.Level1Scene)
                _levelId = BordyLevelCatalog.Level1Id;
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
            WirePill("UndoButton", Undo);
            WirePill("HintButton", Hint);
            WirePill("ResetPill", OnResetPressed);
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
            if (titleLabel != null)
                titleLabel.text = _puzzle.Title;
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
            _statusLabel.text = "Tap an empty cell to place a sun or moon";
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
            ResetPuzzle();
            // Daily: persist the cleared board with the UNCHANGED time, so resuming keeps the timer.
            // 每日挑战：把清空后的盘面与“不变的时间”一起存档，续玩时计时不丢。
            if (_levelId == BordyLevelCatalog.DailyId)
                SaveDailyProgressIfNeeded();
        }

        public void ResetPuzzle()
        {
            if (_reviewMode) // a finished daily is read-only / 已完成的每日挑战只读
                return;

            _undo.Clear();
            _won = false;

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
            SetTransientStatus("Tap an empty cell to place a sun or moon");
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
            PinStatus($"Done today · Time {BordyTimer.Format(BordyDaily.CompletedSeconds)} (view only — come back tomorrow)");
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

            _cells[row, col].color = on ? ColGuideCell : ColCell;
        }

        public void ClearGuideHighlights()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (!_puzzle.IsGiven(r, c) && _cells[r, c].color == ColGuideCell)
                        _cells[r, c].color = ColCell;
                }
            }
        }

        private void OnCellTapped(int row, int col)
        {
            if (_won || _puzzle.IsGiven(row, col))
                return;

            if (CanTapCell != null && !CanTapCell(row, col))
                return;

            int previous = _state[row, col];
            int next = CycleToken(previous);
            _state[row, col] = next;
            _undo.Push(new MoveRecord(row, col, previous));
            RefreshCell(row, col, animate: true);
            EvaluateBoard();
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
            if (_won || _undo.Count == 0)
                return;

            var move = _undo.Pop();
            if (_puzzle.IsGiven(move.Row, move.Col))
                return;

            _state[move.Row, move.Col] = move.Previous;
            RefreshCell(move.Row, move.Col, animate: true);
            EvaluateBoard();
            SaveDailyProgressIfNeeded();
        }

        public void Hint()
        {
            if (_won)
                return;

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
                    RefreshCell(r, c, animate: true);
                    EvaluateBoard();
                    return;
                }
            }

            SetStatus("No cells left to hint");
        }

        public int GetCellState(int row, int col) => _state[row, col];

        private void RefreshCell(int row, int col, bool animate)
        {
            var token = _tokenViews[row, col];
            var cell = _cells[row, col];
            bool given = _puzzle.IsGiven(row, col);
            cell.color = given ? ColGivenCell : ColCell;

            int value = _state[row, col];
            if (animate)
                token.SetKind(value, true);
            else
                token.ShowStatic(value);
        }

        private void EvaluateBoard()
        {
            ClearErrorHighlights();

            if (!IsBoardComplete())
            {
                SetTransientStatus("Tap an empty cell to place a sun or moon");
                return;
            }

            if (!IsBoardValid())
            {
                HighlightViolations();
                SetTransientStatus("Some rules aren't satisfied — check the cells in red");
                return;
            }

            _won = true;
            BordyTimer.Stop(); // freeze the timer on solve / 通关即停表

            if (_levelId == BordyLevelCatalog.DailyId)
            {
                // Record today's result and lock the board into the read-only result view.
                // 记录今天的成绩，并把棋盘锁定为只读结算视图。
                int seconds = BordyTimer.ElapsedSeconds;
                BordyDaily.SaveResult(seconds, EncodeState());
                BordyDaily.ClearProgress(); // solved → no in-progress snapshot needed / 已解出，无需进行中存档
                _reviewMode = true;
                PinStatus($"Daily Challenge complete! Time {BordyTimer.Format(seconds)} (view only)");
            }
            else
            {
                SetStatus("Puzzle solved!");
            }

            BoardWon?.Invoke();
        }

        private void ClearErrorHighlights()
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (!_puzzle.IsGiven(r, c) && _cells[r, c].color == ColErrorCell)
                        _cells[r, c].color = ColCell;
                }
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
            _pinnedStatus = message;
            SetStatus(message);
        }

        /// <summary>Release the pinned status so evaluation messages show again. / 取消钉住。</summary>
        public void ClearStatusPin() => _pinnedStatus = null;

        /// <summary>Status from board evaluation — suppressed while a status is pinned. / 校验提示，在钉住期间被抑制。</summary>
        private void SetTransientStatus(string message)
        {
            if (!string.IsNullOrEmpty(_pinnedStatus))
                return;
            SetStatus(message);
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
