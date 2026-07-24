using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class BattleUI : MonoBehaviour
    {
        [Header("Stage Info")]
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text bossTagText;

        [Header("Player Cat Display")]
        [SerializeField] private Image playerCatImage;
        [SerializeField] private TMP_Text playerCatNameText;
        [SerializeField] private TMP_Text playerCatLevelText;
        [SerializeField] private Slider playerHpSlider;
        [SerializeField] private TMP_Text playerHpText;

        [Header("Enemy Cat Display")]
        [SerializeField] private Image enemyCatImage;
        [SerializeField] private TMP_Text enemyCatNameText;
        [SerializeField] private TMP_Text enemyCatLevelText;
        [SerializeField] private TMP_Text enemyRarityText;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private TMP_Text enemyHpText;

        [Header("Battle Log & Prompts")]
        [SerializeField] private TMP_Text battleLogText;
        [SerializeField] private TMP_Text actionPromptText;

        private void Awake()
        {
            EnsureUIComponents();
        }

        private void OnEnable()
        {
            EnsureUIComponents();
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStarted += UpdateBattleView;
                BattleManager.Instance.OnAttackExecuted += OnAttackExecuted;
                if (BattleManager.Instance.IsBattleActive)
                {
                    UpdateBattleView(BattleManager.Instance.PlayerCat, BattleManager.Instance.EnemyCat);
                }
            }
        }

        private void EnsureUIComponents()
        {
            if (stageText == null) stageText = transform.Find("TopBar/StageText")?.GetComponent<TMP_Text>();
            if (bossTagText == null) bossTagText = transform.Find("TopBar/BossTagText")?.GetComponent<TMP_Text>();
            if (playerCatImage == null) playerCatImage = transform.Find("PlayerView/PlayerCatSprite")?.GetComponent<Image>();
            if (playerCatNameText == null) playerCatNameText = transform.Find("PlayerView/PlayerNameText")?.GetComponent<TMP_Text>();
            if (playerCatLevelText == null) playerCatLevelText = transform.Find("PlayerView/PlayerLevelText")?.GetComponent<TMP_Text>();
            if (playerHpSlider == null) playerHpSlider = transform.Find("PlayerView/PlayerHPSlider")?.GetComponent<Slider>();
            if (enemyCatImage == null) enemyCatImage = transform.Find("EnemyView/EnemyCatSprite")?.GetComponent<Image>();
            if (enemyCatNameText == null) enemyCatNameText = transform.Find("EnemyView/EnemyNameText")?.GetComponent<TMP_Text>();
            if (enemyCatLevelText == null) enemyCatLevelText = transform.Find("EnemyView/EnemyLevelText")?.GetComponent<TMP_Text>();
            if (enemyRarityText == null) enemyRarityText = transform.Find("EnemyView/EnemyRarityText")?.GetComponent<TMP_Text>();
            if (enemyHpSlider == null) enemyHpSlider = transform.Find("EnemyView/EnemyHPSlider")?.GetComponent<Slider>();
            if (battleLogText == null) battleLogText = transform.Find("BottomPanel/BattleLogText")?.GetComponent<TMP_Text>();
            if (actionPromptText == null) actionPromptText = transform.Find("BottomPanel/ActionPromptText")?.GetComponent<TMP_Text>();
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleStarted -= UpdateBattleView;
                BattleManager.Instance.OnAttackExecuted -= OnAttackExecuted;
            }
        }

        public void UpdateBattleView(CatInstance player, CatInstance enemy)
        {
            if (StageManager.Instance != null)
            {
                if (stageText != null) stageText.text = $"STAGE {StageManager.Instance.CurrentStage} / 100";
                if (bossTagText != null)
                {
                    bossTagText.gameObject.SetActive(StageManager.Instance.IsBossStage);
                    bossTagText.text = StageManager.Instance.IsFinalBoss ? "★ FINAL BOSS ★" : "★ BOSS STAGE ★";
                }
            }

            // Player Cat View
            if (player != null)
            {
                if (playerCatImage != null) playerCatImage.sprite = player.Data.sprite;
                if (playerCatNameText != null) playerCatNameText.text = player.Data.catName;
                if (playerCatLevelText != null) playerCatLevelText.text = $"Lv. {player.Level}";
                if (playerHpSlider != null)
                {
                    playerHpSlider.maxValue = player.MaxHp;
                    playerHpSlider.value = player.CurrentHp;
                }
                if (playerHpText != null) playerHpText.text = $"{player.CurrentHp} / {player.MaxHp}";
            }

            // Enemy Cat View
            if (enemy != null)
            {
                if (enemyCatImage != null) enemyCatImage.sprite = enemy.Data.sprite;
                if (enemyCatNameText != null) enemyCatNameText.text = enemy.Data.catName;
                if (enemyCatLevelText != null) enemyCatLevelText.text = $"Lv. {enemy.Level}";
                if (enemyRarityText != null)
                {
                    enemyRarityText.text = enemy.Data.rarity.ToString();
                    enemyRarityText.color = enemy.Data.rarity.GetRarityColor();
                }
                if (enemyHpSlider != null)
                {
                    enemyHpSlider.maxValue = enemy.MaxHp;
                    enemyHpSlider.value = enemy.CurrentHp;
                }
                if (enemyHpText != null) enemyHpText.text = $"{enemy.CurrentHp} / {enemy.MaxHp}";
            }

            if (actionPromptText != null)
            {
                actionPromptText.text = "[SPACE] : Catch Attempt  |  [P] : Party Management";
            }
        }

        private void OnAttackExecuted(CatInstance attacker, CatInstance defender, int damage, string logMsg)
        {
            if (battleLogText != null)
            {
                battleLogText.text = logMsg;
            }

            // Update HP Bars
            CatInstance player = BattleManager.Instance.PlayerCat;
            CatInstance enemy = BattleManager.Instance.EnemyCat;

            if (player != null && playerHpSlider != null)
            {
                playerHpSlider.value = player.CurrentHp;
                if (playerHpText != null) playerHpText.text = $"{player.CurrentHp} / {player.MaxHp}";
            }

            if (enemy != null && enemyHpSlider != null)
            {
                enemyHpSlider.value = enemy.CurrentHp;
                if (enemyHpText != null) enemyHpText.text = $"{enemy.CurrentHp} / {enemy.MaxHp}";
            }
        }
    }
}
