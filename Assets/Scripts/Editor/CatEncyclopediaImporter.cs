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

            List<CatDataSO> cats = new List<CatDataSO>(ImplementedCatCount);
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

                Sprite sprite = LoadSpriteForId(id);
                data.dexNo = id;
                data.catName = entry.KoreanName;
                data.catNameKorean = entry.KoreanName;
                data.catNameEnglish = $"Cat #{id} ({entry.KoreanName})";
                data.baseHp = entry.Hp;
                data.baseAtk = entry.Atk;
                data.speed = 30 + (id % 70);
                data.skillNameKorean = entry.PrimarySkillKorean;
                data.skillNameEnglish = entry.PrimarySkillKorean;
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
            Debug.Log("[CatEncyclopediaImporter] Applied PDF names, HP, ATK and skills to CatData 1-300 with exact cat_ID sprite mapping.");
        }

        [MenuItem("Tools/PocketRoguelike/Validate PDF Encyclopedia Cat Mapping")]
        public static void Validate()
        {
            if (CatEncyclopediaTable.Entries.Count != 300)
                throw new InvalidOperationException($"Expected 300 master records, found {CatEncyclopediaTable.Entries.Count}.");

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
                if (data.sprite == null || data.sprite.name != $"cat_{id}")
                    throw new InvalidOperationException($"CatData #{id} references '{data.sprite?.name ?? "null"}' instead of cat_{id}.");
                if (!uniqueSprites.Add(data.sprite))
                    throw new InvalidOperationException($"Sprite '{data.sprite.name}' is assigned to more than one CatData asset.");
            }

            CatDatabaseSO database = AssetDatabase.LoadAssetAtPath<CatDatabaseSO>(DatabasePath);
            if (database == null || database.AllCats.Count != ImplementedCatCount)
                throw new InvalidOperationException($"CatDatabase must contain exactly {ImplementedCatCount} implemented cats.");
            for (int id = 1; id <= ImplementedCatCount; id++)
                if (database.GetByDexNo(id) == null) throw new InvalidOperationException($"CatDatabase is missing ID {id}.");

            Debug.Log("[CatEncyclopediaValidation] PASS: 300 PDF rows, 300 implemented CatData assets, 300 unique cat_ID sprites, all names/stats/skills matched by ID.");
        }

        private static Sprite LoadSpriteForId(int id)
        {
            int first = ((id - 1) / 25) * 25 + 1;
            int last = first + 24;
            string sheetPath = $"Assets/Image/Cats/{first}-{last}.png";
            Sprite[] matches = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().Where(sprite => sprite.name == $"cat_{id}").ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException($"Expected exactly one cat_{id} sprite in {sheetPath}, found {matches.Length}.");
            return matches[0];
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
