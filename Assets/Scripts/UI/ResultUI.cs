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
            DisableDynamicLocalizedText(titleText);
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => GameManager.Instance?.InitGame());
            }
            UpdateResultView();
        }

        private void OnDisable() => LanguageManager.OnLanguageChanged -= HandleLanguageChanged;

        private static void DisableDynamicLocalizedText(TMP_Text text)
        {
            LocalizedText localized = text != null ? text.GetComponent<LocalizedText>() : null;
            if (localized != null) localized.enabled = false;
        }
        private void HandleLanguageChanged(GameLanguage _) => UpdateResultView();

        public void UpdateResultView()
        {
            if (GameManager.Instance == null) return;
            bool isVictory = GameManager.Instance.CurrentState == GameState.Victory;
            if (titleText != null)
            {
                titleText.text = LanguageManager.Get(isVictory ? "victory" : "defeat");
                titleText.color = isVictory ? Color.gold : Color.red;
            }
            if (descriptionText != null)
            {
                int stage = StageManager.Instance != null ? StageManager.Instance.CurrentStage : 1;
                descriptionText.text = isVictory ? LanguageManager.Get("victory_description") : LanguageManager.Format("defeat_description", stage);
            }
            if (restartButton != null)
            {
                TMP_Text label = restartButton.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = LanguageManager.Get("play_again");
            }
        }
    }
}