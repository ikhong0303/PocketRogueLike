using System;
using UnityEngine;

namespace PocketRoguelike
{
    public class CatchManager : MonoBehaviour
    {
        public static CatchManager Instance { get; private set; }

        [Header("Catch Gauge Settings")]
        [SerializeField] private float gaugeSpeed = 1.5f;       // Oscillations per second
        [SerializeField] private float sweetSpotCenter = 0.5f;   // Center of green zone (0.0 ~ 1.0)
        [SerializeField] private float sweetSpotWidth = 0.15f;   // Green zone width

        [Header("Runtime State")]
        [SerializeField] private bool isGaugeActive = false;
        [SerializeField] private float currentGaugeValue = 0f;  // 0.0 ~ 1.0
        private float gaugeDirection = 1f;

        public bool IsGaugeActive => isGaugeActive;
        public float CurrentGaugeValue => currentGaugeValue;
        public float SweetSpotCenter => sweetSpotCenter;
        public float SweetSpotWidth => sweetSpotWidth;

        public event Action<float> OnGaugeUpdated;
        public event Action<bool, CatInstance> OnCatchResult;

        private CatInstance targetEnemy;

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

        private void Update()
        {
            if (!isGaugeActive) return;

            // Oscillate gauge value between 0 and 1
            currentGaugeValue += gaugeDirection * gaugeSpeed * Time.deltaTime;
            if (currentGaugeValue >= 1f)
            {
                currentGaugeValue = 1f;
                gaugeDirection = -1f;
            }
            else if (currentGaugeValue <= 0f)
            {
                currentGaugeValue = 0f;
                gaugeDirection = 1f;
            }

            OnGaugeUpdated?.Invoke(currentGaugeValue);

            // Detect Spacebar press to execute Catch Throw!
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExecuteCatchThrow();
            }
        }

        public void StartCatchProcess(CatInstance enemy)
        {
            if (enemy == null || enemy.IsFainted)
            {
                Debug.LogWarning("[CatchManager] Cannot catch a null or fainted cat!");
                return;
            }

            targetEnemy = enemy;
            currentGaugeValue = 0f;
            gaugeDirection = 1f;
            isGaugeActive = true;
            Debug.Log($"[CatchManager] Catch Timing Gauge Started for {enemy.Data.catName}! Press SPACE to throw ball!");
        }

        public void StopGauge()
        {
            isGaugeActive = false;
        }

        public void ExecuteCatchThrow()
        {
            if (!isGaugeActive) return;

            isGaugeActive = false;

            // Calculate timing accuracy (0.0 to 1.0)
            float dist = Mathf.Abs(currentGaugeValue - sweetSpotCenter);
            float accuracy = Mathf.Clamp01(1f - (dist / (sweetSpotWidth * 2f)));

            // Calculate catch probability based on enemy HP% + timing accuracy + rarity factor
            float hpBonus = (1f - targetEnemy.HpRatio) * 0.5f; // Lower HP = Higher catch chance
            float timingBonus = accuracy * 0.4f;
            float rarityPenalty = ((int)targetEnemy.Data.rarity) * 0.05f;

            float finalChance = Mathf.Clamp(hpBonus + timingBonus + 0.15f - rarityPenalty, 0.1f, 0.95f);

            bool isSuccess = UnityEngine.Random.value <= finalChance;
            Debug.Log($"[CatchManager] Throw Accuracy: {accuracy:P0}, HP Bonus: {hpBonus:P0}, Final Chance: {finalChance:P0} -> Result: {(isSuccess ? "SUCCESS" : "FAILED")}");

            OnCatchResult?.Invoke(isSuccess, targetEnemy);
        }
    }
}
