using UnityEngine;

namespace PocketRoguelike
{
    [CreateAssetMenu(fileName = "CatData_", menuName = "PocketRoguelike/CatData", order = 1)]
    public class CatDataSO : ScriptableObject
    {
        [Header("Identity")]
        public int dexNo;             // 1 ~ 100
        public string catName;        // e.g. "Cat #1"
        public CatRarity rarity;

        [Header("Visuals")]
        public Sprite sprite;

        [Header("Base Stats")]
        public int baseHp = 100;
        public int baseAtk = 20;
        public int speed = 50;        // Higher speed acts first in battle turn order

        public int StarterCost => rarity.GetStarterCost();

        private void OnValidate()
        {
            if (dexNo < 1) dexNo = 1;
            if (dexNo > 100) dexNo = 100;
            if (string.IsNullOrEmpty(catName)) catName = $"Cat #{dexNo}";
        }
    }
}
