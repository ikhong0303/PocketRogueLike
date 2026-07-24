using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class StageClearUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button confirmButton;

        private void OnEnable()
        {
            EnsureReferences();
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() => GameManager.Instance?.ConfirmStageClear());
            }
            Refresh();
        }

        private void OnDisable() => LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        private void HandleLanguageChanged(GameLanguage _) => Refresh();

        private void EnsureReferences()
        {
            if (titleText == null) titleText = transform.Find("Window/Title")?.GetComponent<TMP_Text>();
            if (descriptionText == null) descriptionText = transform.Find("Window/Description")?.GetComponent<TMP_Text>();
            if (rewardText == null) rewardText = transform.Find("Window/Reward")?.GetComponent<TMP_Text>();
            if (confirmButton == null) confirmButton = transform.Find("Window/ConfirmButton")?.GetComponent<Button>();
        }

        public void Refresh()
        {
            if (titleText != null) titleText.text = LanguageManager.Get("stage_clear");
            CatInstance defeated = GameManager.Instance != null ? GameManager.Instance.LastDefeatedEnemy : null;
            if (descriptionText != null)
                descriptionText.text = LanguageManager.Format("stage_clear_description", LanguageManager.CatName(defeated?.Data));

            VictoryReward reward = GameManager.Instance != null ? GameManager.Instance.LastVictoryReward : default;
            string rewardKey = reward.monsterBall && reward.potion ? "reward_both"
                : reward.monsterBall ? "reward_ball"
                : reward.potion ? "reward_potion"
                : "reward_none";
            if (rewardText != null) rewardText.text = LanguageManager.Get(rewardKey);
            if (confirmButton != null)
            {
                TMP_Text label = confirmButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = LanguageManager.Get("confirm_next");
            }
        }
    }
}
