using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PocketRoguelike.EditorTools
{
    public static class CatDataAutoGenerator
    {
        [MenuItem("Tools/Generate 100 Cat Data ScriptableObjects")]
        public static void Generate100CatData()
        {
            Debug.Log("[CatDataAutoGenerator] Starting generation of 100 CatDataSO ScriptableObjects...");

            string resourcesDir = "Assets/Resources";
            string catDataDir = "Assets/Resources/CatData";

            if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);
            if (!Directory.Exists(catDataDir)) Directory.CreateDirectory(catDataDir);

            AssetDatabase.Refresh();

            // Load 100 sprites from Assets/Image/Cats
            string catsFolder = "Assets/Image/Cats";
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { catsFolder });

            Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null && !spriteMap.ContainsKey(sprite.name))
                {
                    spriteMap[sprite.name] = sprite;
                }
            }

            Debug.Log($"[CatDataAutoGenerator] Found {spriteMap.Count} sub-sprites in {catsFolder}.");

            List<CatDataSO> createdCats = new List<CatDataSO>();

            for (int i = 1; i <= 100; i++)
            {
                string catNameKey = $"cat_{i}";
                string assetPath = $"{catDataDir}/CatData_{i}.asset";

                CatDataSO catData = AssetDatabase.LoadAssetAtPath<CatDataSO>(assetPath);
                if (catData == null)
                {
                    catData = ScriptableObject.CreateInstance<CatDataSO>();
                    AssetDatabase.CreateAsset(catData, assetPath);
                }

                catData.dexNo = i;
                catData.catName = $"Cat #{i}";

                // Rarity assignment based on dexNo range
                if (i <= 30) catData.rarity = CatRarity.Basic;
                else if (i <= 50) catData.rarity = CatRarity.EX;
                else if (i <= 70) catData.rarity = CatRarity.Rare;
                else if (i <= 85) catData.rarity = CatRarity.Unique;
                else if (i <= 95) catData.rarity = CatRarity.Epic;
                else catData.rarity = CatRarity.Legend;

                // Base Stats Scaling
                catData.baseHp = 80 + (i * 2);
                catData.baseAtk = 15 + Mathf.RoundToInt(i * 0.4f);
                catData.speed = 30 + (i % 70);

                if (spriteMap.TryGetValue(catNameKey, out Sprite foundSprite))
                {
                    catData.sprite = foundSprite;
                }

                EditorUtility.SetDirty(catData);
                createdCats.Add(catData);
            }

            // Create or update CatDatabaseSO
            string dbPath = $"{resourcesDir}/CatDatabase.asset";
            CatDatabaseSO database = AssetDatabase.LoadAssetAtPath<CatDatabaseSO>(dbPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CatDatabaseSO>();
                AssetDatabase.CreateAsset(database, dbPath);
            }

            database.SetCats(createdCats);
            EditorUtility.SetDirty(database);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CatDataAutoGenerator] Successfully generated 100 CatDataSO assets & CatDatabase at {dbPath}!");
        }
    }
}
