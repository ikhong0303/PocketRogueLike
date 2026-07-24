using System;
using System.Collections;
using UnityEngine;

namespace PocketRoguelike
{
    [Serializable]
    public struct VictoryReward
    {
        public bool monsterBall;
        public bool potion;
        public bool revive;
        public bool HasAny => monsterBall || potion || revive;
    }

    public class CatchManager : MonoBehaviour
    {
        public static CatchManager Instance { get; private set; }

        [Header("Run Inventory")]
        [SerializeField, Min(0)] private int startingBallCount = 5;
        [SerializeField, Min(1)] private int maxBallCount = 99;
        [SerializeField, Min(0)] private int ballCount;
        [SerializeField, Min(0)] private int potionCount;
        [SerializeField, Min(1)] private int maxPotionCount = 99;
        [SerializeField, Range(0.01f, 1f)] private float potionHealRatio = 0.50f;
        [SerializeField, Min(0)] private int reviveCount;
        [SerializeField, Min(1)] private int maxReviveCount = 99;
        [SerializeField, Range(0.01f, 1f)] private float reviveHealRatio = 0.50f;

        [Header("Victory Drop Rates")]
        [SerializeField, Range(0f, 1f)] private float monsterBallDropChance = 0.10f;
        [SerializeField, Range(0f, 1f)] private float potionDropChance = 0.10f;
        [SerializeField, Range(0f, 1f)] private float reviveDropChance = 0.10f;

        [Header("Capture Probability")]
        [SerializeField, Range(0.01f, 0.99f)] private float fullHpShakeChance = 0.25f;
        [SerializeField, Range(0.01f, 0.999f)] private float lowHpShakeChance = 0.985f;
        [SerializeField, Range(0f, 0.1f)] private float rarityPenaltyPerTier = 0.025f;
        [SerializeField, Min(0.05f)] private float shakeInterval = 0.4f;

        public int BallCount => ballCount;
        public int PotionCount => potionCount;
        public int ReviveCount => reviveCount;
        public int StartingBallCount => startingBallCount;
        public bool HasBalls => ballCount > 0;
        public bool IsCaptureResolving { get; private set; }
        public float LastShakeChance { get; private set; }

        public event Action<int> OnBallCountChanged;
        public event Action<int> OnPotionCountChanged;
        public event Action<int> OnReviveCountChanged;
        public event Action<CatInstance> OnCaptureStarted;
        public event Action<int, bool> OnCaptureShake;
        public event Action<bool, CatInstance> OnCatchResult;

        private CatInstance targetEnemy;
        private Coroutine captureCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void InitRunInventory()
        {
            ballCount = Mathf.Clamp(startingBallCount, 0, maxBallCount);
            potionCount = 0;
            reviveCount = 0;
            OnBallCountChanged?.Invoke(ballCount);
            OnPotionCountChanged?.Invoke(potionCount);
            OnReviveCountChanged?.Invoke(reviveCount);
            Debug.Log($"[CatchManager] Run inventory initialized: {ballCount} Monster Balls, {potionCount} Potions, {reviveCount} Revives.");
        }

        public void InitRunBalls() => InitRunInventory();

        public void AddBalls(int amount)
        {
            if (amount <= 0) return;
            ballCount = Mathf.Clamp(ballCount + amount, 0, maxBallCount);
            OnBallCountChanged?.Invoke(ballCount);
        }

        public void AddPotions(int amount)
        {
            if (amount <= 0) return;
            potionCount = Mathf.Clamp(potionCount + amount, 0, maxPotionCount);
            OnPotionCountChanged?.Invoke(potionCount);
        }

        public void AddRevives(int amount)
        {
            if (amount <= 0) return;
            reviveCount = Mathf.Clamp(reviveCount + amount, 0, maxReviveCount);
            OnReviveCountChanged?.Invoke(reviveCount);
        }

        public bool UsePotion(CatInstance target)
        {
            if (potionCount <= 0 || target == null || target.IsFainted || target.CurrentHp >= target.MaxHp) return false;
            potionCount--;
            target.Heal(Mathf.Max(1, Mathf.RoundToInt(target.MaxHp * potionHealRatio)));
            OnPotionCountChanged?.Invoke(potionCount);
            return true;
        }

        public bool UseRevive(CatInstance target)
        {
            if (reviveCount <= 0 || target == null || !target.IsFainted) return false;
            reviveCount--;
            int reviveHp = Mathf.Max(1, Mathf.RoundToInt(target.MaxHp * reviveHealRatio));
            target.Heal(reviveHp);
            OnReviveCountChanged?.Invoke(reviveCount);
            Debug.Log($"[CatchManager] Used Revive on {target.Data.catName}! Revived to {target.CurrentHp}/{target.MaxHp} HP (50%).");
            return true;
        }

        public VictoryReward RollVictoryDrops()
        {
            VictoryReward reward = new VictoryReward
            {
                monsterBall = UnityEngine.Random.value < monsterBallDropChance,
                potion = UnityEngine.Random.value < potionDropChance,
                revive = UnityEngine.Random.value < reviveDropChance
            };
            if (reward.monsterBall) AddBalls(1);
            if (reward.potion) AddPotions(1);
            if (reward.revive) AddRevives(1);
            Debug.Log($"[Rewards] Victory drops - Monster Ball: {reward.monsterBall}, Potion: {reward.potion}, Revive: {reward.revive}");
            return reward;
        }

        public bool TryThrowBall(CatInstance enemy)
        {
            if (IsCaptureResolving || enemy == null || enemy.IsFainted || !HasBalls) return false;
            ballCount--;
            OnBallCountChanged?.Invoke(ballCount);
            targetEnemy = enemy;
            IsCaptureResolving = true;
            LastShakeChance = CalculateShakeChance(enemy);
            OnCaptureStarted?.Invoke(enemy);
            if (captureCoroutine != null) StopCoroutine(captureCoroutine);
            captureCoroutine = StartCoroutine(ResolveCapture(enemy, LastShakeChance));
            Debug.Log($"[CatchManager] Threw 1 ball at {enemy.Data.catName}. Remaining: {ballCount}, per-shake chance: {LastShakeChance:P1}");
            return true;
        }

        public float CalculateShakeChance(CatInstance enemy)
        {
            if (enemy == null) return 0f;
            float missingHp = 1f - Mathf.Clamp01(enemy.HpRatio);
            float hpCurve = Mathf.Pow(missingHp, 0.55f);
            float hpChance = Mathf.Lerp(fullHpShakeChance, lowHpShakeChance, hpCurve);
            float rarityPenalty = (int)enemy.Data.rarity * rarityPenaltyPerTier;
            return Mathf.Clamp(hpChance - rarityPenalty, 0.05f, 0.995f);
        }

        private IEnumerator ResolveCapture(CatInstance expectedTarget, float shakeChance)
        {
            for (int shake = 1; shake <= 3; shake++)
            {
                yield return new WaitForSeconds(shakeInterval);
                if (!IsCaptureResolving || targetEnemy != expectedTarget) yield break;
                bool passed = UnityEngine.Random.value < shakeChance;
                OnCaptureShake?.Invoke(shake, passed);
                Debug.Log($"[CatchManager] Shake {shake}/3: {(passed ? "TRUE" : "FALSE")}");
                if (!passed)
                {
                    FinishCapture(false, expectedTarget);
                    yield break;
                }
            }
            FinishCapture(true, expectedTarget);
        }

        private void FinishCapture(bool success, CatInstance expectedTarget)
        {
            if (!IsCaptureResolving || targetEnemy != expectedTarget) return;
            IsCaptureResolving = false;
            targetEnemy = null;
            captureCoroutine = null;
            Debug.Log($"[CatchManager] Capture result: {(success ? "SUCCESS" : "FAILED")}");
            OnCatchResult?.Invoke(success, expectedTarget);
        }

        public void CancelCapture()
        {
            if (captureCoroutine != null) StopCoroutine(captureCoroutine);
            captureCoroutine = null;
            targetEnemy = null;
            IsCaptureResolving = false;
        }
    }
}
