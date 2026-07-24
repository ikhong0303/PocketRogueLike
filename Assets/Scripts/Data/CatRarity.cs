using UnityEngine;

namespace PocketRoguelike
{
    public enum CatRarity
    {
        Basic = 0,   // 1~30
        EX = 1,      // 31~50
        Rare = 2,    // 51~70
        Unique = 3,  // 71~85
        Epic = 4,    // 86~95
        Legend = 5   // 96~100
    }

    public static class CatRarityExtensions
    {
        public static int GetStarterCost(this CatRarity rarity)
        {
            switch (rarity)
            {
                case CatRarity.Basic: return 1;
                case CatRarity.EX: return 2;
                case CatRarity.Rare: return 3;
                case CatRarity.Unique: return 4;
                case CatRarity.Epic: return 5;
                case CatRarity.Legend: return 7;
                default: return 1;
            }
        }

        public static Color GetRarityColor(this CatRarity rarity)
        {
            switch (rarity)
            {
                case CatRarity.Basic: return Color.gray;
                case CatRarity.EX: return new Color(0.2f, 0.8f, 0.2f); // Green
                case CatRarity.Rare: return new Color(0.2f, 0.5f, 1f); // Blue
                case CatRarity.Unique: return new Color(0.7f, 0.2f, 0.9f); // Purple
                case CatRarity.Epic: return new Color(1f, 0.5f, 0f); // Orange
                case CatRarity.Legend: return new Color(1f, 0.84f, 0f); // Gold
                default: return Color.white;
            }
        }
    }
}
