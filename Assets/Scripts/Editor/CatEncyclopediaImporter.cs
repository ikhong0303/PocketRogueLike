using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PocketRoguelike.EditorTools
{
    public static class CatEncyclopediaImporter
    {
        private const int ImplementedCatCount = 300;
        private const string DataFolder = "Assets/Resources/CatData";
        private const string DatabasePath = "Assets/Resources/CatDatabase.asset";
        private const string DummyDataPath = DataFolder + "/CatData_000.asset";

        [MenuItem("Tools/PocketRoguelike/Apply PDF Encyclopedia And Rebuild UI")]
        public static void ApplyAndRebuildUi()
        {
            Apply();
            LocalizationSceneMigrator.Apply();
            LocalizationValidator.Validate();
            Debug.Log("[CatEncyclopediaImporter] Full PDF data and UI rebuild completed.");
        }

        [MenuItem("Tools/PocketRoguelike/Apply PDF Encyclopedia To Cats 1-300")]
        public static void Apply()
        {
            if (CatEncyclopediaTable.Entries.Count != 300)
                throw new InvalidOperationException($"Expected 300 PDF records, found {CatEncyclopediaTable.Entries.Count}.");

            List<CatDataSO> cats = new List<CatDataSO>(ImplementedCatCount + 1)
            {
                CreateOrUpdateDummyData()
            };
            for (int id = 1; id <= ImplementedCatCount; id++)
            {
                CatEncyclopediaEntry entry = CatEncyclopediaTable.Get(id);
                string assetPath = $"{DataFolder}/CatData_{id}.asset";
                CatDataSO data = AssetDatabase.LoadAssetAtPath<CatDataSO>(assetPath);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<CatDataSO>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                Sprite sprite = data.sprite != null ? data.sprite : LoadSpriteForId(id);
                data.dexNo = id;
                data.catName = entry.KoreanName;
                data.catNameKorean = entry.KoreanName;
                data.catNameEnglish = $"Cat #{id}";
                data.baseHp = entry.Hp;
                data.baseAtk = entry.Atk;
                data.speed = 30 + (id % 70);
                data.skillNameKorean = entry.PrimarySkillKorean;
                data.skillNameEnglish = LanguageManager.TranslateSkillNameToEnglish(entry.PrimarySkillKorean, id);
                data.attackSkillsKorean = entry.AttackSkillsKorean;
                data.defenseSkillKorean = entry.DefenseSkillKorean;
                data.debuffSkillKorean = entry.DebuffSkillKorean;
                data.rarity = RarityForId(id);
                data.sprite = sprite;
                EditorUtility.SetDirty(data);
                cats.Add(data);
            }

            CatDatabaseSO database = AssetDatabase.LoadAssetAtPath<CatDatabaseSO>(DatabasePath);
            if (database == null) throw new InvalidOperationException("CatDatabase.asset is missing.");
            database.SetCats(cats);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Validate();
            Debug.Log("[CatEncyclopediaImporter] Applied PDF names, HP, ATK and skills to CatData 1-300 with flexible sprite mapping.");
        }

        [MenuItem("Tools/PocketRoguelike/Validate PDF Encyclopedia Cat Mapping")]
        public static void Validate()
        {
            if (CatEncyclopediaTable.Entries.Count != 300)
                throw new InvalidOperationException($"Expected 300 master records, found {CatEncyclopediaTable.Entries.Count}.");

            CatDataSO dummy = AssetDatabase.LoadAssetAtPath<CatDataSO>(DummyDataPath);
            if (dummy == null || dummy.dexNo != 0 || dummy.sprite == null)
                throw new InvalidOperationException("CatData_000 dummy slot is missing or invalid.");

            HashSet<Sprite> uniqueSprites = new HashSet<Sprite>();
            for (int id = 1; id <= ImplementedCatCount; id++)
            {
                CatEncyclopediaEntry entry = CatEncyclopediaTable.Get(id);
                CatDataSO data = AssetDatabase.LoadAssetAtPath<CatDataSO>($"{DataFolder}/CatData_{id}.asset");
                if (data == null) throw new InvalidOperationException($"CatData #{id} is missing.");
                if (data.dexNo != id) throw new InvalidOperationException($"CatData_{id} has dexNo {data.dexNo}.");
                if (data.catNameKorean != entry.KoreanName || data.baseHp != entry.Hp || data.baseAtk != entry.Atk)
                    throw new InvalidOperationException($"CatData #{id} does not match its PDF record.");
                if (data.skillNameKorean != entry.PrimarySkillKorean || data.attackSkillsKorean != entry.AttackSkillsKorean || data.defenseSkillKorean != entry.DefenseSkillKorean || data.debuffSkillKorean != entry.DebuffSkillKorean)
                    throw new InvalidOperationException($"CatData #{id} skill fields do not match the PDF record.");
                if (data.sprite == null)
                    throw new InvalidOperationException($"CatData #{id} has no sprite assigned.");
                if (!uniqueSprites.Add(data.sprite))
                    Debug.LogWarning($"[CatEncyclopediaValidation] Sprite '{data.sprite.name}' is assigned to more than one CatData asset.");
            }

            CatDatabaseSO database = AssetDatabase.LoadAssetAtPath<CatDatabaseSO>(DatabasePath);
            if (database == null || database.AllCats.Count != ImplementedCatCount + 1)
                throw new InvalidOperationException($"CatDatabase must contain dummy index 000 plus {ImplementedCatCount} playable cats.");
            if (database.AllCats[0] == null || database.AllCats[0].dexNo != 0)
                throw new InvalidOperationException("CatDatabase index 0 must be CatData_000 dummy.");
            for (int id = 1; id <= ImplementedCatCount; id++)
            {
                CatDataSO indexed = database.AllCats[id];
                if (indexed == null || indexed.dexNo != id || indexed.sprite == null)
                    throw new InvalidOperationException($"CatDatabase index {id} is not mapped to CatData #{id} or missing sprite.");
            }

            Debug.Log("[CatEncyclopediaValidation] PASS: index 000 dummy, indices 001-300 mapped to matching data and sprites, 300 PDF rows verified.");
        }

        private static CatDataSO CreateOrUpdateDummyData()
        {
            CatDataSO dummy = AssetDatabase.LoadAssetAtPath<CatDataSO>(DummyDataPath);
            if (dummy == null)
            {
                dummy = ScriptableObject.CreateInstance<CatDataSO>();
                AssetDatabase.CreateAsset(dummy, DummyDataPath);
            }

            dummy.dexNo = 0;
            dummy.catName = "Dummy Cat 000";
            dummy.catNameKorean = "더미 고양이 000";
            dummy.catNameEnglish = "Dummy Cat 000";
            dummy.baseHp = 1;
            dummy.baseAtk = 0;
            dummy.speed = 0;
            dummy.skillNameKorean = "사용 안 함";
            dummy.skillNameEnglish = "Unused";
            dummy.attackSkillsKorean = string.Empty;
            dummy.defenseSkillKorean = string.Empty;
            dummy.debuffSkillKorean = string.Empty;
            dummy.rarity = CatRarity.Basic;
            dummy.sprite = dummy.sprite != null ? dummy.sprite : LoadSpriteForId(1);
            EditorUtility.SetDirty(dummy);
            return dummy;
        }

        private static Sprite LoadSpriteForId(int id)
        {
            int first = ((id - 1) / 25) * 25 + 1;
            int last = first + 24;
            string sheetPath = $"Assets/Image/Cats/{first}-{last}.png";
            Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
            if (allSprites.Length == 0)
                throw new InvalidOperationException($"No sprites found in {sheetPath}.");

            // 1. Try exact name match "cat_{id}"
            Sprite match = allSprites.FirstOrDefault(s => s.name == $"cat_{id}");
            if (match != null) return match;

            // 2. Fallback: match by grid position index (Top-to-Bottom, Left-to-Right)
            Sprite[] sorted = allSprites
                .OrderByDescending(s => Mathf.RoundToInt(s.rect.y))
                .ThenBy(s => Mathf.RoundToInt(s.rect.x))
                .ToArray();

            int localIndex = (id - 1) % 25;
            if (localIndex < sorted.Length)
            {
                return sorted[localIndex];
            }

            return sorted[0];
        }

        private static CatRarity RarityForId(int id)
        {
            if (id <= 9) return CatRarity.Basic;
            if (id <= 50) return CatRarity.EX;
            if (id <= 100) return CatRarity.Rare;
            if (id <= 150) return CatRarity.Unique;
            if (id <= 270) return CatRarity.Epic;
            return CatRarity.Legend;
        }
    }
}
