using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bordy
{
    /// <summary>Campaign level picker — adaptive grid of square tiles. / 闯关选关网格。</summary>
    public class BordyCampaignLevelSelectController : MonoBehaviour
    {
        private static readonly Color ColInk = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color ColMuted = new Color(0.45f, 0.45f, 0.48f);
        private static readonly Color ColTileOpen = new Color(0.96f, 0.95f, 0.92f);
        private static readonly Color ColInnerOpen = new Color(1.00f, 0.66f, 0.10f);
        private static readonly Color ColInnerDone = new Color(0.35f, 0.72f, 0.45f);
        private static readonly Color ColInnerLocked = new Color(0.78f, 0.78f, 0.80f);

        private Transform _gridRoot;
        private RectTransform _viewportRt;
        private ScrollRect _scroll;
        private GridLayoutGroup _grid;
        private Text _hint;
        private readonly List<GameObject> _tiles = new List<GameObject>();

        private void OnEnable()
        {
            if (_gridRoot == null)
                _gridRoot = transform.Find("ScrollViewport/LevelGrid");
            if (_scroll == null)
                _scroll = transform.Find("ScrollViewport")?.GetComponent<ScrollRect>();
            if (_viewportRt == null)
                _viewportRt = transform.Find("ScrollViewport") as RectTransform;

            EnsureScrollViewport();
            EnsureGridLayout();
            StopAllCoroutines();
            StartCoroutine(RefreshDeferred());
        }

        private IEnumerator RefreshDeferred()
        {
            yield return null;
            Refresh();
            yield return null;

            ConfigureGrid();

            if (_gridRoot is RectTransform gridRt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(gridRt);
                Canvas.ForceUpdateCanvases();
            }

            if (_scroll != null)
                _scroll.verticalNormalizedPosition = 1f;
        }

        private void EnsureScrollViewport()
        {
            var viewport = transform.Find("ScrollViewport");
            if (viewport == null)
                return;

            var legacyMask = viewport.GetComponent<Mask>();
            if (legacyMask != null)
                Destroy(legacyMask);

            if (viewport.GetComponent<RectMask2D>() == null)
                viewport.gameObject.AddComponent<RectMask2D>();
        }

        private void EnsureGridLayout()
        {
            var viewport = transform.Find("ScrollViewport");
            if (_gridRoot == null && viewport != null)
            {
                _gridRoot = viewport.Find("LevelGrid");
                if (_gridRoot == null)
                {
                    var legacyList = viewport.Find("LevelList");
                    if (legacyList != null)
                        legacyList.name = "LevelGrid";
                    else
                    {
                        var gridGo = new GameObject("LevelGrid", typeof(RectTransform));
                        gridGo.transform.SetParent(viewport, false);
                    }

                    _gridRoot = viewport.Find("LevelGrid");
                }
            }

            if (_gridRoot == null)
                return;

            _grid = _gridRoot.GetComponent<GridLayoutGroup>();
            if (_grid == null)
                _grid = _gridRoot.gameObject.AddComponent<GridLayoutGroup>();

            var legacyVlg = _gridRoot.GetComponent<VerticalLayoutGroup>();
            if (legacyVlg != null)
                Destroy(legacyVlg);

            var gridRt = _gridRoot as RectTransform;
            if (gridRt != null)
            {
                gridRt.anchorMin = new Vector2(0, 1);
                gridRt.anchorMax = new Vector2(1, 1);
                gridRt.pivot = new Vector2(0.5f, 1);
                gridRt.anchoredPosition = Vector2.zero;
            }

            if (_scroll != null && gridRt != null)
                _scroll.content = gridRt;

            var fitter = _gridRoot.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = _gridRoot.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _grid.childAlignment = TextAnchor.UpperCenter;
        }

        public void Refresh()
        {
            ClearTiles();
            ConfigureGrid();

            var levels = BordyCampaignCatalog.Levels;
            if (levels.Count == 0)
            {
                if (_hint != null)
                    _hint.text = BordyStrings.Get(BordyStrings.Keys.CampaignEmpty);
                return;
            }

            if (_hint == null)
                _hint = transform.Find("HintBanner")?.GetComponent<Text>();

            if (_hint != null)
            {
                _hint.text = BordyProgress.TutorialCompleted
                    ? BordyStrings.Get(BordyStrings.Keys.CampaignHint)
                    : BordyStrings.Get(BordyStrings.Keys.LevelSelectHintLocked);
            }

            var nav = GetComponent<BordyNav>();
            foreach (var entry in levels)
            {
                bool unlocked = BordyProgress.IsCampaignLevelUnlocked(entry.Index);
                bool done = BordyProgress.IsCampaignLevelCompleted(entry.Id);
                CreateTile(entry, unlocked, done, nav);
            }

            if (_gridRoot is RectTransform gridRt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(gridRt);
        }

        private void ConfigureGrid()
        {
            if (_grid == null || _viewportRt == null)
                return;

            float width = Mathf.Max(320f, _viewportRt.rect.width);
            const float spacing = 16f;
            const float padding = 12f;
            const float minCell = 140f;
            const float maxCell = 220f;

            int columns = Mathf.Clamp(Mathf.FloorToInt((width + spacing) / (minCell + spacing)), 3, 5);
            float cell = (width - padding * 2f - spacing * (columns - 1)) / columns;
            cell = Mathf.Clamp(cell, minCell, maxCell);

            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = columns;
            _grid.cellSize = new Vector2(cell, cell);
            _grid.spacing = new Vector2(spacing, spacing);
            _grid.padding = new RectOffset((int)padding, (int)padding, 8, 24);
        }

        private void ClearTiles()
        {
            foreach (var tile in _tiles)
            {
                if (tile != null)
                    Destroy(tile);
            }

            _tiles.Clear();
        }

        private void CreateTile(BordyCampaignEntry entry, bool unlocked, bool done, BordyNav nav)
        {
            var tile = new GameObject($"LevelTile_{entry.Index:D2}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tile.transform.SetParent(_gridRoot, false);
            _tiles.Add(tile);

            var tileImg = tile.GetComponent<Image>();
            BordyUi.ApplySliced(tileImg);
            tileImg.color = ColTileOpen;
            tileImg.raycastTarget = unlocked;

            var inner = new GameObject("Inner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            inner.transform.SetParent(tile.transform, false);
            var innerRt = inner.GetComponent<RectTransform>();
            innerRt.anchorMin = new Vector2(0f, 0f);
            innerRt.anchorMax = new Vector2(1f, 1f);
            innerRt.offsetMin = new Vector2(18f, 34f);
            innerRt.offsetMax = new Vector2(-18f, -34f);

            var innerImg = inner.GetComponent<Image>();
            BordyUi.ApplySliced(innerImg);
            innerImg.color = done ? ColInnerDone : unlocked ? ColInnerOpen : ColInnerLocked;
            innerImg.raycastTarget = false;

            string numText = entry.Index.ToString();
            var num = CreateText("Number", inner.transform, numText, 44, FontStyle.Bold);
            Stretch(num.rectTransform);
            num.alignment = TextAnchor.MiddleCenter;
            num.color = unlocked || done ? Color.white : ColInk;

            string tier = BordyStrings.CampaignTierLabel(entry.Tier);
            if (!string.IsNullOrEmpty(tier))
            {
                var tierLabel = CreateText("Tier", tile.transform, tier, 22, FontStyle.Normal);
                var tierRt = tierLabel.rectTransform;
                tierRt.anchorMin = new Vector2(0f, 0f);
                tierRt.anchorMax = new Vector2(1f, 0f);
                tierRt.pivot = new Vector2(0.5f, 0f);
                tierRt.offsetMin = new Vector2(6f, 8f);
                tierRt.offsetMax = new Vector2(-6f, 30f);
                tierLabel.alignment = TextAnchor.MiddleCenter;
                tierLabel.color = unlocked || done ? ColInk : ColMuted;
            }

            if (unlocked)
            {
                var button = tile.AddComponent<Button>();
                button.targetGraphic = tileImg;
                string levelId = entry.Id;
                button.onClick.AddListener(() => nav.OpenCampaignLevel(levelId));
            }
        }

        private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.color = ColInk;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            BordyFonts.Apply(t);
            return t;
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
