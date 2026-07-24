using System.Collections.Generic;
using UnityEngine;

namespace PocketRoguelike
{
    [CreateAssetMenu(fileName = "CatDatabase", menuName = "PocketRoguelike/CatDatabase", order = 0)]
    public class CatDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<CatDataSO> allCats = new List<CatDataSO>();

        public IReadOnlyList<CatDataSO> AllCats => allCats;

        public void SetCats(List<CatDataSO> cats)
        {
            allCats = new List<CatDataSO>(cats);
            allCats.Sort((a, b) => a.dexNo.CompareTo(b.dexNo));
        }

        public CatDataSO GetByDexNo(int dexNo)
        {
            if (allCats == null || allCats.Count == 0) return null;
            return allCats.Find(c => c != null && c.dexNo == dexNo);
        }

        public CatDataSO GetRandomCat(CatRarity maxRarity = CatRarity.Legend)
        {
            if (allCats == null || allCats.Count == 0) return null;
            List<CatDataSO> candidates = allCats.FindAll(c => c != null && c.rarity <= maxRarity);
            if (candidates.Count == 0) candidates = allCats;
            return candidates[Random.Range(0, candidates.Count)];
        }
    }
}
