using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button restartButton;

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => GameManager.Instance?.InitGame());
            }

            UpdateResultView();
        }

        public void UpdateResultView()
        {
            if (GameManager.Instance == null) return;

            bool isVictory = GameManager.Instance.CurrentState == GameState.Victory;

            if (titleText != null)
            {
                titleText.text = isVictory ? "🏆 VICTORY! 🏆" : "💀 DEFEAT! 💀";
                titleText.color = isVictory ? Color.gold : Color.red;
            }

            if (descriptionText != null)
            {
                int stage = StageManager.Instance != null ? StageManager.Instance.CurrentStage : 1;
                descriptionText.text = isVictory 
                    ? "Congratulations! You cleared all 100 Stages of PocketRoguelike!" 
                    : $"Your party fainted on Stage {stage}. Better luck next time!";
            }
        }
    }
}
