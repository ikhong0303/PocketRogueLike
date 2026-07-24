using UnityEngine;

namespace PocketRoguelike
{
    [CreateAssetMenu(fileName = "CatData_", menuName = "PocketRoguelike/CatData", order = 1)]
    public class CatDataSO : ScriptableObject
    {
        [Header("Identity")]
        public int dexNo;             // 0 = database dummy, 1 ~ 300 = playable cats
        public string catName;        // Legacy/default name
        public string catNameKorean;
        public string catNameEnglish;
        public CatRarity rarity;

        [Header("Visuals")]
        public Sprite sprite;

        [Header("Base Stats")]
        public int baseHp = 100;
        public int baseAtk = 20;
        public int speed = 50;        // Higher speed acts first in battle turn order

        [Header("Encyclopedia Skills")]
        public string skillNameKorean;
        public string skillNameEnglish;
        [TextArea(2, 4)] public string attackSkillsKorean;
        [TextArea(2, 4)] public string defenseSkillKorean;
        [TextArea(2, 4)] public string debuffSkillKorean;

        public int StarterCost => rarity.GetStarterCost();

        private void OnValidate()
        {
            if (dexNo < 0) dexNo = 0;
            if (dexNo > 300) dexNo = 300;
            if (string.IsNullOrEmpty(catName)) catName = $"Cat #{dexNo}";
            if (string.IsNullOrEmpty(catNameEnglish)) catNameEnglish = catName;
            if (string.IsNullOrEmpty(catNameKorean)) catNameKorean = $"고양이 #{dexNo}";
            if (string.IsNullOrEmpty(skillNameKorean)) skillNameKorean = "기본 공격";
            if (string.IsNullOrEmpty(skillNameEnglish)) skillNameEnglish = skillNameKorean;
        }
    }
}
