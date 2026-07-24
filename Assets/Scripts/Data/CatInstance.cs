using System;
using UnityEngine;

namespace PocketRoguelike
{
    [Serializable]
    public class CatInstance
    {
        [SerializeField] private CatDataSO data;
        [SerializeField] private int level = 1;
        [SerializeField] private int currentHp;
        [SerializeField] private int maxHp;
        [SerializeField] private int atk;
        [SerializeField] private int speed;
        [SerializeField] private bool isBoss = false;

        public CatDataSO Data => data;
        public int Level => level;
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int Atk => atk;
        public int Speed => speed;
        public bool IsBoss => isBoss;
        public bool IsFainted => currentHp <= 0;
        public float HpRatio => maxHp > 0 ? (float)currentHp / maxHp : 0f;

        public event Action<CatInstance> OnHpChanged;
        public event Action<CatInstance> OnFainted;

        public CatInstance(CatDataSO catData, int startLevel = 1, bool boss = false)
        {
            data = catData;
            level = Mathf.Max(1, startLevel);
            isBoss = boss;
            CalculateStats();
            currentHp = maxHp;
        }

        public void CalculateStats()
        {
            if (data == null) return;

            // Stat Scaling curve based on level & rarity
            float rarityMultiplier = 1f + ((int)data.rarity * 0.2f);
            float levelMultiplier = 1f + ((level - 1) * 0.12f);

            maxHp = Mathf.RoundToInt(data.baseHp * levelMultiplier * rarityMultiplier);
            if (isBoss)
            {
                maxHp *= 2; // Boss HP multiplier (from system spec 5.4)
            }

            atk = Mathf.RoundToInt(data.baseAtk * levelMultiplier * rarityMultiplier);
            speed = data.speed;
        }

        public void TakeDamage(int damage)
        {
            if (IsFainted) return;

            int actualDamage = Mathf.Max(1, damage);
            currentHp = Mathf.Max(0, currentHp - actualDamage);
            OnHpChanged?.Invoke(this);

            if (IsFainted)
            {
                OnFainted?.Invoke(this);
            }
        }

        public void Heal(int amount)
        {
            if (IsFainted && amount <= 0) return;

            currentHp = Mathf.Min(maxHp, currentHp + amount);
            OnHpChanged?.Invoke(this);
        }

        public void FullHeal()
        {
            currentHp = maxHp;
            OnHpChanged?.Invoke(this);
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Max(1, newLevel);
            CalculateStats();
            currentHp = maxHp;
            OnHpChanged?.Invoke(this);
        }
    }
}
