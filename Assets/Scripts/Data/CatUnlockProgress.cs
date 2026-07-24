using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PocketRoguelike
{
    public static class CatUnlockProgress
    {
        public const string PlayerPrefsKey = "PocketRoguelike.UnlockedStarterCats.v1";
        public const int DefaultUnlockedCount = 6;

        public static bool IsUnlocked(int dexNo)
        {
            return LoadUnlockedDexNumbers().Contains(dexNo);
        }

        public static bool Unlock(CatDataSO cat)
        {
            if (cat == null) return false;
            HashSet<int> unlocked = LoadUnlockedDexNumbers();
            if (!unlocked.Add(cat.dexNo)) return false;
            Save(unlocked);
            Debug.Log($"[CatUnlockProgress] Unlocked Cat #{cat.dexNo} ({cat.catName}) for starter selection.");
            return true;
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
            PlayerPrefs.SetString(PlayerPrefsKey, string.Join(",", unlocked.OrderBy(value => value)));
            PlayerPrefs.Save();
        }
    }
}
