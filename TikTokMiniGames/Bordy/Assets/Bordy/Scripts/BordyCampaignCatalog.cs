using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bordy
{
    /// <summary>Runtime registry for procedurally authored campaign levels. / 闯关关卡运行时目录。</summary>
    public static class BordyCampaignCatalog
    {
        public const string PlayScene = "Play";
        public const string CampaignSelectScene = "CampaignSelect";
        public const string IdPrefix = "campaign-";

        private static readonly List<BordyCampaignEntry> Entries = new List<BordyCampaignEntry>();
        private static bool _loaded;

        public static IReadOnlyList<BordyCampaignEntry> Levels
        {
            get
            {
                EnsureLoaded();
                return Entries;
            }
        }

        public static int Count
        {
            get
            {
                EnsureLoaded();
                return Entries.Count;
            }
        }

        public static bool IsCampaignId(string id)
            => !string.IsNullOrEmpty(id) && id.StartsWith(IdPrefix, StringComparison.Ordinal);

        public static bool TryGet(string id, out BordyPuzzleData puzzle)
        {
            EnsureLoaded();
            foreach (var e in Entries)
            {
                if (e.Id == id)
                {
                    puzzle = e.Puzzle;
                    return true;
                }
            }

            puzzle = null;
            return false;
        }

        public static bool TryGetByIndex(int index, out BordyCampaignEntry entry)
        {
            EnsureLoaded();
            foreach (var e in Entries)
            {
                if (e.Index == index)
                {
                    entry = e;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public static void Reload()
        {
            _loaded = false;
            Entries.Clear();
            EnsureLoaded();
        }

        public static bool TryGetEntry(string id, out BordyCampaignEntry entry)
        {
            EnsureLoaded();
            foreach (var e in Entries)
            {
                if (e.Id == id)
                {
                    entry = e;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;
            Entries.Clear();

            var asset = Resources.Load<TextAsset>("Bordy/campaign-levels");
            if (asset == null)
            {
                Debug.LogError("[BordyCampaignCatalog] Missing Resources/Bordy/campaign-levels.json — run Bordy → Generate Campaign Levels.");
                return;
            }

            var bundle = JsonUtility.FromJson<BordyCampaignBundleDto>(asset.text);
            if (bundle?.levels == null || bundle.levels.Length == 0)
            {
                Debug.LogError("[BordyCampaignCatalog] campaign-levels.json is empty or invalid.");
                return;
            }

            Array.Sort(bundle.levels, (a, b) => a.index.CompareTo(b.index));
            foreach (var dto in bundle.levels)
            {
                if (!dto.IsValid())
                {
                    Debug.LogWarning($"[BordyCampaignCatalog] Skip invalid level: {dto?.id}");
                    continue;
                }

                Entries.Add(new BordyCampaignEntry
                {
                    Id = dto.id,
                    Index = dto.index,
                    Tier = dto.tier ?? "",
                    Size = dto.size,
                    Difficulty = dto.difficulty,
                    Puzzle = dto.ToPuzzle(),
                });
            }

            Debug.Log($"[BordyCampaignCatalog] Loaded {Entries.Count} campaign levels.");
        }
    }

    public struct BordyCampaignEntry
    {
        public string Id;
        public int Index;
        public string Tier;
        public int Size;
        public float Difficulty;
        public BordyPuzzleData Puzzle;
    }
}
