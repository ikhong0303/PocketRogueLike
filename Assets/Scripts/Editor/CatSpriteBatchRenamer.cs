using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketRoguelike.EditorTools
{
    public static class CatSpriteBatchRenamer
    {
        [MenuItem("Tools/Execute Cat Sprites Batch Rename")]
        public static void ExecuteBatchRename()
        {
            string folder = "Assets/Image/Cats";
            if (!Directory.Exists(folder))
            {
                folder = "Assets/cats";
            }

            if (!Directory.Exists(folder))
            {
                Debug.LogWarning("[CatSpriteBatchRenamer] Neither Assets/Image/Cats nor Assets/cats folder found.");
                return;
            }

            string[] texturePaths = Directory.GetFiles(folder, "*.png")
                .OrderBy(f => f)
                .ToArray();

            if (texturePaths.Length == 0)
            {
                Debug.LogWarning($"[CatSpriteBatchRenamer] No .png files found in {folder}");
                return;
            }

            int globalCatIndex = 1;
            int totalRenamed = 0;

            foreach (string path in texturePaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;

#pragma warning disable 0618
                SpriteMetaData[] spritesheet = importer.spritesheet;
                if (spritesheet == null || spritesheet.Length == 0) continue;

                // Sort sub-sprites by grid position: Top-to-Bottom (Y desc), then Left-to-Right (X asc)
                SpriteMetaData[] sorted = spritesheet
                    .OrderByDescending(s => Mathf.RoundToInt(s.rect.y))
                    .ThenBy(s => Mathf.RoundToInt(s.rect.x))
                    .ToArray();

                for (int i = 0; i < sorted.Length; i++)
                {
                    sorted[i].name = $"cat_{globalCatIndex}";
                    globalCatIndex++;
                    totalRenamed++;
                }

                importer.spritesheet = sorted;
#pragma warning restore 0618
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                Debug.Log($"[CatSpriteBatchRenamer] Successfully renamed {sorted.Length} sprites in '{Path.GetFileName(path)}'.");
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CatSpriteBatchRenamer] Batch rename complete! Total {totalRenamed} cat sprites renamed (cat_1 ~ cat_{totalRenamed}).");
        }
    }
}
