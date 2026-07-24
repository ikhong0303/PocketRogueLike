using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PocketRoguelike
{
    public static class CatUnlockProgress
    {
        public const string PlayerPrefsKey = "PocketRoguelike.UnlockedStarterCats.v1";
        public const string TotalCatchCountKey = "PocketRoguelike.TotalCatchCount.v1";
        public const string CatCatchCountKeyPrefix = "PocketRoguelike.CatCatchCount.v1.";
        public const int DefaultUnlockedCount = 6;

        public static bool IsUnlocked(int dexNo)
        {
            return LoadUnlockedDexNumbers().Contains(dexNo);
        }

        public static bool Unlock(CatDataSO cat)
        {
            return RecordCapture(cat);
        }

        public static bool RecordCapture(CatDataSO cat)
        {
            if (cat == null) return false;

            HashSet<int> unlocked = LoadUnlockedDexNumbers();
            bool newlyUnlocked = unlocked.Add(cat.dexNo);

            PlayerPrefs.SetString(PlayerPrefsKey, Serialize(unlocked));
            PlayerPrefs.SetInt(GetCatchCountKey(cat.dexNo), GetCatchCount(cat.dexNo) + 1);
            PlayerPrefs.SetInt(TotalCatchCountKey, GetTotalCatchCount() + 1);
            PlayerPrefs.Save();

            string result = newlyUnlocked ? "unlocked for starter selection" : "capture record updated";
            Debug.Log($"[CatUnlockProgress] Cat #{cat.dexNo} ({cat.catName}) {result}. Catch count: {GetCatchCount(cat.dexNo)}.");
            return newlyUnlocked;
        }

        public static int GetCatchCount(int dexNo)
        {
            if (dexNo <= 0) return 0;
            return PlayerPrefs.GetInt(GetCatchCountKey(dexNo), 0);
        }

        public static int GetTotalCatchCount()
        {
            return PlayerPrefs.GetInt(TotalCatchCountKey, 0);
        }

        public static List<CatDataSO> GetUnlockedCats(CatDatabaseSO database)
        {
            List<CatDataSO> result = new List<CatDataSO>();
            if (database == null || database.AllCats == null) return result;

            HashSet<int> unlocked = LoadUnlockedDexNumbers();
            foreach (CatDataSO cat in database.AllCats)
            {
                if (cat != null && unlocked.Contains(cat.dexNo)) result.Add(cat);
            }
            result.Sort((a, b) => a.dexNo.CompareTo(b.dexNo));
            return result;
        }

        private static HashSet<int> LoadUnlockedDexNumbers()
        {
            HashSet<int> result = new HashSet<int>();
            string saved = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(saved))
            {
                string[] values = saved.Split(',');
                foreach (string value in values)
                {
                    if (int.TryParse(value, out int dexNo) && dexNo > 0) result.Add(dexNo);
                }
            }

            bool changed = false;
            for (int dexNo = 1; dexNo <= DefaultUnlockedCount; dexNo++)
            {
                if (result.Add(dexNo)) changed = true;
            }
            if (changed) Save(result);
            return result;
        }

        private static void Save(HashSet<int> unlocked)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, Serialize(unlocked));
            PlayerPrefs.Save();
        }

        private static string Serialize(HashSet<int> unlocked)
        {
            return string.Join(",", unlocked.OrderBy(value => value));
        }

        private static string GetCatchCountKey(int dexNo)
        {
            return CatCatchCountKeyPrefix + dexNo;
        }
    }
}
