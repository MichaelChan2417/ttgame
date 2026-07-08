using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bordy.EditorTools
{
    /// <summary>
    /// Copies a macOS system CJK font into Resources so WebGL can render 简体中文.
    /// Avoids huge PingFang.ttc (~77MB) which spams Unity import errors.
    /// </summary>
    public static class BordyFontImport
    {
        private const long MaxFontBytes = 15 * 1024 * 1024; // 15 MB

        private static readonly (string path, string label)[] MacSources =
        {
            ("/Library/Fonts/Arial Unicode.ttf", "Arial Unicode"),
            ("/System/Library/Fonts/Supplemental/Arial Unicode.ttf", "Arial Unicode"),
            ("/System/Library/Fonts/STHeiti Light.ttc", "STHeiti Light"),
        };

        [MenuItem("Bordy/Import UI Font (macOS)")]
        public static void ImportFromMac()
        {
            string src = null;
            string label = null;
            foreach (var (path, name) in MacSources)
            {
                if (!File.Exists(path))
                    continue;
                var size = new FileInfo(path).Length;
                if (size > MaxFontBytes)
                {
                    Debug.LogWarning($"[BordyFontImport] Skip {name} ({size / (1024 * 1024)}MB) — too large.");
                    continue;
                }
                src = path;
                label = name;
                break;
            }

            if (src == null)
            {
                EditorUtility.DisplayDialog(
                    "Bordy UI Font",
                    "No suitable CJK font found under 15MB.\n\n" +
                    "Manually copy a small .ttf/.otf (Noto Sans SC subset recommended) to:\n" +
                    "Assets/Bordy/Resources/Bordy/BordyUI.ttf",
                    "OK");
                return;
            }

            string destDir = "Assets/Bordy/Resources/Bordy";
            Directory.CreateDirectory(destDir);
            string dest = $"{destDir}/BordyUI.ttf";
            File.Copy(src, dest, overwrite: true);
            AssetDatabase.ImportAsset(dest);
            var importer = AssetImporter.GetAtPath(dest) as TrueTypeFontImporter;
            if (importer != null)
            {
                importer.fontTextureCase = FontTextureCase.Dynamic;
                importer.fontRenderingMode = FontRenderingMode.OSDefault;
                importer.SaveAndReimport();
            }

            Debug.Log($"[BordyFontImport] Imported {label} from {src}");
            EditorUtility.DisplayDialog(
                "Bordy UI Font",
                $"Imported {label}.\n\nNext: Bordy → Run Full Setup",
                "OK");
        }

        [MenuItem("Bordy/Remove Imported UI Font")]
        public static void RemoveImportedFont()
        {
            string dest = "Assets/Bordy/Resources/Bordy/BordyUI.ttf";
            if (!File.Exists(dest))
            {
                EditorUtility.DisplayDialog("Bordy UI Font", "No imported font at Assets/Bordy/Resources/Bordy/BordyUI.ttf", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Bordy UI Font",
                    "Delete BordyUI.ttf? Settings will show \"Chinese (Simplified)\" until you import a smaller font.",
                    "Delete", "Cancel"))
                return;

            AssetDatabase.DeleteAsset(dest);
            Debug.Log("[BordyFontImport] Removed BordyUI.ttf");
        }
    }
}
