using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Exports the built-in daily template as a <see cref="BordyDailyDto"/> JSON file, matching the
    /// format the game fetches from the server. Use it to produce a real file you can host on your
    /// CDN (rename to "YYYYMMDD.json") and to verify the schema.
    /// 把内置每日模板导出为 <see cref="BordyDailyDto"/> JSON（与游戏从服务器拉取的格式一致），
    /// 可作为托管到 CDN 的真实文件（改名为 "YYYYMMDD.json"），也用于核对 schema。
    /// </summary>
    public static class BordyDailyExport
    {
        [MenuItem("Bordy/Export Daily Template JSON")]
        public static void Export()
        {
            var p = BordyLevelCatalog.Get(BordyLevelCatalog.DailyId);
            int n = p.Size;

            var dto = new BordyDailyDto
            {
                date = System.DateTime.UtcNow.ToString("yyyyMMdd"),
                size = n,
                solution = new int[n * n],
                givens = new bool[n * n],
                edges = new BordyEdgeDto[p.Edges.Length],
            };

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    dto.solution[r * n + c] = p.Solution[r, c];
                    dto.givens[r * n + c] = p.Givens[r, c];
                }

            for (int k = 0; k < p.Edges.Length; k++)
            {
                var e = p.Edges[k];
                dto.edges[k] = new BordyEdgeDto { row = e.Row, col = e.Col, horizontal = e.Horizontal, mustMatch = e.MustMatch };
            }

            string json = JsonUtility.ToJson(dto, true);
            string path = $"Assets/Bordy/{dto.date}.json";
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"[Bordy] Exported daily template → {path}\n{json}");
        }
    }
}
