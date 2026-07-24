using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class StarterSelectUI : MonoBehaviour
    {
        public const int MAX_BUDGET = 10;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text budgetText;
        [SerializeField] private Button startRunButton;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject starterSlotPrefab;

        [Header("Selected Starters (Max 3)")]
        [SerializeField] private List<CatDataSO> selectedStarters = new List<CatDataSO>();

        private int CurrentTotalCost
        {
            get
            {
                int cost = 0;
                foreach (var s in selectedStarters) if (s != null) cost += s.StarterCost;
                return cost;
            }
        }

        private void OnEnable()
        {
            selectedStarters.Clear();
            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveAllListeners();
                startRunButton.onClick.AddListener(OnStartRunClicked);
            }

            PopulateStarterOptions();

            // Default select Cat #1, Cat #2, Cat #3 as initial starters
            if (GameManager.Instance != null && GameManager.Instance.CatDatabase != null)
            {
                var db = GameManager.Instance.CatDatabase;
                CatDataSO c1 = db.GetByDexNo(1);
                CatDataSO c2 = db.GetByDexNo(2);
                CatDataSO c3 = db.GetByDexNo(3);

                if (c1 != null && !selectedStarters.Contains(c1)) selectedStarters.Add(c1);
                if (c2 != null && !selectedStarters.Contains(c2)) selectedStarters.Add(c2);
                if (c3 != null && !selectedStarters.Contains(c3)) selectedStarters.Add(c3);
            }

            UpdateBudgetUI();
        }

        private void PopulateStarterOptions()
        {
            if (GameManager.Instance == null || GameManager.Instance.CatDatabase == null) return;
            var allCats = GameManager.Instance.CatDatabase.AllCats;

            // Pick 6 random starter candidates from Database
            List<CatDataSO> candidates = new List<CatDataSO>();
            for (int i = 0; i < 6 && i < allCats.Count; i++)
            {
                candidates.Add(allCats[i]);
            }

            // Create buttons in grid if container exists
            if (slotsContainer != null && starterSlotPrefab != null)
            {
                foreach (Transform child in slotsContainer) Destroy(child.gameObject);

                foreach (var cat in candidates)
                {
                    GameObject go = Instantiate(starterSlotPrefab, slotsContainer);
                    Image img = go.GetComponentInChildren<Image>();
                    TMP_Text txt = go.GetComponentInChildren<TMP_Text>();
                    Button btn = go.GetComponent<Button>();

                    if (img != null) img.sprite = cat.sprite;
                    if (txt != null) txt.text = $"{cat.catName}\nCost: {cat.StarterCost}";

                    btn.onClick.AddListener(() => ToggleStarterSelection(cat, go));
                }
            }
        }

        public void ToggleStarterSelection(CatDataSO cat, GameObject slotGO)
        {
            if (selectedStarters.Contains(cat))
            {
                selectedStarters.Remove(cat);
            }
            else
            {
                if (selectedStarters.Count >= 3)
                {
                    Debug.LogWarning("[StarterSelectUI] Max 3 starters allowed!");
                    return;
                }

                if (CurrentTotalCost + cat.StarterCost > MAX_BUDGET)
                {
                    Debug.LogWarning("[StarterSelectUI] Exceeds 10 points budget!");
                    return;
                }

                selectedStarters.Add(cat);
            }

            UpdateBudgetUI();
        }

        private void UpdateBudgetUI()
        {
            if (budgetText != null)
            {
                budgetText.text = $"Starter Cost Budget: {CurrentTotalCost} / {MAX_BUDGET} Points (Selected: {selectedStarters.Count} / 3)";
            }

            if (startRunButton != null)
            {
                startRunButton.interactable = selectedStarters.Count > 0 && CurrentTotalCost <= MAX_BUDGET;
            }
        }

        private void OnStartRunClicked()
        {
            if (selectedStarters.Count == 0) return;

            CatDataSO s1 = selectedStarters.Count > 0 ? selectedStarters[0] : null;
            CatDataSO s2 = selectedStarters.Count > 1 ? selectedStarters[1] : null;
            CatDataSO s3 = selectedStarters.Count > 2 ? selectedStarters[2] : null;

            GameManager.Instance?.StartRun(s1, s2, s3);
        }
    }
}
