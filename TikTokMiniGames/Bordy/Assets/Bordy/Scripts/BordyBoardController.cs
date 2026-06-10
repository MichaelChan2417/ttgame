using System;
using System.Collections.Generic;
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

        private BordyPuzzleData _puzzle;
        private int _size;
        private int[,] _state = new int[0, 0];
        private BordyTokenView[,] _tokenViews = new BordyTokenView[0, 0];
        private Image[,] _cells = new Image[0, 0];
        private readonly Stack<MoveRecord> _undo = new Stack<MoveRecord>();

        private Text _statusLabel;
        private Transform _boardRoot;
        private bool _won;

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
            ResetPuzzle();
        }

        private void ResolveLevelIdFromScene()
        {
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
            WirePill("ResetPill", ResetPuzzle);
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
            _statusLabel.text = "点击空格填入太阳或月亮";
        }

        public void ResetPuzzle()
        {
            _undo.Clear();
            _won = false;
            BordyTimer.ResetClock();

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

            SetStatus("点击空格填入太阳或月亮");
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

            SetStatus("没有可提示的格子了");
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
                SetStatus("点击空格填入太阳或月亮");
                return;
            }

            if (!IsBoardValid())
            {
                HighlightViolations();
                SetStatus("还有规则未满足，请检查标红的格子");
                return;
            }

            _won = true;
            SetStatus("恭喜通关！");
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

        public void SetStatus(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
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
