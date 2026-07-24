using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class CatchTimingUI : MonoBehaviour
    {
        [Header("Gauge UI Components")]
        [SerializeField] private Slider gaugeSlider;
        [SerializeField] private RectTransform cursorRect;
        [SerializeField] private RectTransform sweetSpotRect;
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button throwButton;

        private void OnEnable()
        {
            if (CatchManager.Instance != null)
            {
                CatchManager.Instance.OnGaugeUpdated += UpdateGaugeUI;
                CatchManager.Instance.OnCatchResult += DisplayCatchResult;
            }

            if (throwButton != null)
            {
                throwButton.onClick.RemoveAllListeners();
                throwButton.onClick.AddListener(() => CatchManager.Instance?.ExecuteCatchThrow());
            }

            if (resultText != null) resultText.text = "";
            if (instructionText != null) instructionText.text = "Press [SPACE] when the cursor is inside GREEN zone!";
        }

        private void OnDisable()
        {
            if (CatchManager.Instance != null)
            {
                CatchManager.Instance.OnGaugeUpdated -= UpdateGaugeUI;
                CatchManager.Instance.OnCatchResult -= DisplayCatchResult;
            }
        }

        private void UpdateGaugeUI(float val)
        {
            if (gaugeSlider != null)
            {
                gaugeSlider.value = val;
            }
        }

        private void DisplayCatchResult(bool isSuccess, CatInstance cat)
        {
            if (resultText != null)
            {
                resultText.text = isSuccess 
                    ? $"★ SUCCESS! Caught {cat.Data.catName}! ★" 
                    : "❌ CATCH FAILED! The cat broke free!";
                resultText.color = isSuccess ? Color.green : Color.red;
            }
        }
    }
}
