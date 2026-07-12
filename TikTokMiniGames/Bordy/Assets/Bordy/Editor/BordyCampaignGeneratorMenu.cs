using System;
using System.IO;
using System.Text;
using Bordy;
using UnityEditor;
using UnityEngine;

namespace Bordy.EditorTools
{
    public static class BordyCampaignGeneratorMenu
    {
        private const string OutputPath = "Assets/Bordy/Resources/Bordy/campaign-levels.json";
        private const int DefaultCount = 20;

        [MenuItem("Bordy/Dev Tools/Generate Campaign Levels (Editor, slow)")]
        public static void GenerateMenu() => Generate(showDialog: true);

        public static void GenerateIfMissing() 
        {
            if (!File.Exists(OutputPath))
                Generate(showDialog: false);
        }

        /// <summary>CI / command-line entry (no dialog). / 命令行入口。</summary>
        public static void GenerateBatch() => Generate(showDialog: false);

        public static void Generate(bool showDialog)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);

            try
            {
                var batch = BordyPuzzleGenerator.GenerateCampaignBatch(DefaultCount, onProgress: (i, total, msg) =>
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Bordy — Generate Campaign Levels", msg, (float)i / total))
                        throw new OperationCanceledException("Cancelled by user.");
                });

                var bundle = new BordyCampaignBundleDto { version = 1, levels = new BordyCampaignLevelDto[batch.Count] };

                for (int i = 0; i < batch.Count; i++)
                {
                    var p = batch[i];
                    string id = $"{BordyCampaignCatalog.IdPrefix}{i + 1:D2}";
                    bundle.levels[i] = BordyCampaignLevelDto.FromPuzzle(
                        id,
                        i + 1,
                        (float)p.DifficultyScore,
                        p.Solution,
                        p.Givens,
                        p.Edges);
                }

                string json = ToJson(bundle);
                File.WriteAllText(OutputPath, json, Encoding.UTF8);
                AssetDatabase.Refresh();
                BordyCampaignCatalog.Reload();
                Debug.Log($"[BordyCampaignGenerator] Wrote {batch.Count} levels → {OutputPath}");
                if (showDialog && !Application.isBatchMode)
                    EditorUtility.DisplayDialog("Bordy", $"Generated {batch.Count} campaign levels.\n{OutputPath}", "OK");
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[BordyCampaignGenerator] Cancelled.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // JsonUtility cannot serialize top-level arrays well for nested structures in all cases;
        // hand-roll minimal JSON for our DTO shape.
        private static string ToJson(BordyCampaignBundleDto bundle)
        {
            var sb = new StringBuilder();
            sb.Append("{\n  \"version\": ").Append(bundle.version).Append(",\n  \"levels\": [\n");
            for (int i = 0; i < bundle.levels.Length; i++)
            {
                var lv = bundle.levels[i];
                sb.Append("    {\n");
                sb.Append("      \"id\": \"").Append(lv.id).Append("\",\n");
                sb.Append("      \"index\": ").Append(lv.index).Append(",\n");
                sb.Append("      \"size\": ").Append(lv.size).Append(",\n");
                sb.Append("      \"difficulty\": ").Append(lv.difficulty.ToString("F3")).Append(",\n");
                sb.Append("      \"solution\": ").Append(IntArray(lv.solution)).Append(",\n");
                sb.Append("      \"givens\": ").Append(BoolArray(lv.givens)).Append(",\n");
                sb.Append("      \"edges\": ").Append(EdgesArray(lv.edges)).Append("\n");
                sb.Append("    }");
                if (i < bundle.levels.Length - 1)
                    sb.Append(',');
                sb.Append('\n');
            }

            sb.Append("  ]\n}");
            return sb.ToString();
        }

        private static string IntArray(int[] a)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < a.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(a[i]);
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static string BoolArray(bool[] a)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < a.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(a[i] ? "true" : "false");
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static string EdgesArray(BordyEdgeDto[] edges)
        {
            if (edges == null || edges.Length == 0)
                return "[]";

            var sb = new StringBuilder("[");
            for (int i = 0; i < edges.Length; i++)
            {
                var e = edges[i];
                if (i > 0) sb.Append(',');
                sb.Append("{\"row\":").Append(e.row)
                    .Append(",\"col\":").Append(e.col)
                    .Append(",\"horizontal\":").Append(e.horizontal ? "true" : "false")
                    .Append(",\"mustMatch\":").Append(e.mustMatch ? "true" : "false")
                    .Append('}');
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
