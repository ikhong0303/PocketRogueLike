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

        [Header("Selected Starters (Max 3)")]
        [SerializeField] private List<CatDataSO> selectedStarters = new List<CatDataSO>();

        private Dictionary<CatDataSO, Image> cardBgImages = new Dictionary<CatDataSO, Image>();

        private int CurrentTotalCost
        {
            get
            {
                int cost = 0;
                foreach (var s in selectedStarters) if (s != null) cost += s.StarterCost;
                return cost;
            }
        }

        private void Awake()
        {
            EnsureUIComponents();
        }

        private void OnEnable()
        {
            EnsureUIComponents();
            selectedStarters.Clear();

            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveAllListeners();
                startRunButton.onClick.AddListener(OnStartRunClicked);
            }

            // Default select Cat #1, Cat #2, Cat #3 as initial starters
            if (GameManager.Instance != null && GameManager.Instance.CatDatabase != null)
            {
                var db = GameManager.Instance.CatDatabase;
                CatDataSO c1 = db.GetByDexNo(1);
                CatDataSO c2 = db.GetByDexNo(2);
                CatDataSO c3 = db.GetByDexNo(3);

                if (c1 != null) selectedStarters.Add(c1);
                if (c2 != null) selectedStarters.Add(c2);
                if (c3 != null) selectedStarters.Add(c3);
            }

            PopulateStarterOptions();
            UpdateBudgetUI();
        }

        private void EnsureUIComponents()
        {
            if (budgetText == null)
            {
                budgetText = transform.Find("BudgetText")?.GetComponent<TMP_Text>();
            }

            if (startRunButton == null)
            {
                startRunButton = transform.Find("StartRunButton")?.GetComponent<Button>();
            }

            if (slotsContainer == null)
            {
                Transform containerTr = transform.Find("SlotsContainer");
                if (containerTr == null)
                {
                    GameObject containerGO = new GameObject("SlotsContainer", typeof(RectTransform));
                    containerGO.transform.SetParent(transform, false);
                    containerTr = containerGO.transform;

                    RectTransform rt = containerGO.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.1f, 0.2f);
                    rt.anchorMax = new Vector2(0.9f, 0.7f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    GridLayoutGroup grid = containerGO.AddComponent<GridLayoutGroup>();
                    grid.cellSize = new Vector2(230, 260);
                    grid.spacing = new Vector2(20, 20);
                    grid.childAlignment = TextAnchor.MiddleCenter;
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 6;
                }
                slotsContainer = containerTr;
            }
        }

        private void PopulateStarterOptions()
        {
            if (GameManager.Instance == null || GameManager.Instance.CatDatabase == null) return;
            var allCats = GameManager.Instance.CatDatabase.AllCats;
            if (allCats == null || allCats.Count == 0) return;

            cardBgImages.Clear();

            // Clear old children in slotsContainer
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }

            // Candidates: Cat #1 ~ Cat #6
            List<CatDataSO> candidates = new List<CatDataSO>();
            for (int i = 0; i < 6 && i < allCats.Count; i++)
            {
                candidates.Add(allCats[i]);
            }

            foreach (var cat in candidates)
            {
                if (cat == null) continue;

                // Create Card Button
                GameObject cardGO = new GameObject($"Card_{cat.dexNo}", typeof(RectTransform));
                cardGO.transform.SetParent(slotsContainer, false);

                Image cardBg = cardGO.AddComponent<Image>();
                cardBg.color = selectedStarters.Contains(cat) ? new Color(0.2f, 0.6f, 0.2f, 0.9f) : new Color(0.15f, 0.18f, 0.25f, 0.9f);
                cardBgImages[cat] = cardBg;

                Button btn = cardGO.AddComponent<Button>();

                // Cat Sprite Image
                GameObject imgGO = new GameObject("CatSprite", typeof(RectTransform));
                imgGO.transform.SetParent(cardGO.transform, false);
                RectTransform imgRt = imgGO.GetComponent<RectTransform>();
                imgRt.anchorMin = new Vector2(0.1f, 0.35f);
                imgRt.anchorMax = new Vector2(0.9f, 0.92f);
                imgRt.offsetMin = Vector2.zero;
                imgRt.offsetMax = Vector2.zero;

                Image catImg = imgGO.AddComponent<Image>();
                catImg.sprite = cat.sprite;
                catImg.preserveAspect = true;

                // Cat Name & Info Text
                GameObject txtGO = new GameObject("CatInfoText", typeof(RectTransform));
                txtGO.transform.SetParent(cardGO.transform, false);
                RectTransform txtRt = txtGO.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.05f, 0.05f);
                txtRt.anchorMax = new Vector2(0.95f, 0.32f);
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;

                TMP_Text infoTxt = txtGO.AddComponent<TextMeshProUGUI>();
                infoTxt.text = $"<b>{cat.catName}</b>\nCost: {cat.StarterCost} Pt | {cat.rarity}";
                infoTxt.fontSize = 18;
                infoTxt.alignment = TextAlignmentOptions.Center;
                infoTxt.color = Color.white;

                CatDataSO targetCat = cat;
                btn.onClick.AddListener(() => ToggleStarterSelection(targetCat));
            }
        }

        public void ToggleStarterSelection(CatDataSO cat)
        {
            if (cat == null) return;

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

            // Update card background colors for selection state
            foreach (var kvp in cardBgImages)
            {
                if (kvp.Value != null)
                {
                    bool isSelected = selectedStarters.Contains(kvp.Key);
                    kvp.Value.color = isSelected ? new Color(0.2f, 0.7f, 0.3f, 0.95f) : new Color(0.15f, 0.18f, 0.25f, 0.9f);
                }
            }
        }

        private void OnStartRunClicked()
        {
            CatDataSO s1 = selectedStarters.Count > 0 ? selectedStarters[0] : null;
            CatDataSO s2 = selectedStarters.Count > 1 ? selectedStarters[1] : null;
            CatDataSO s3 = selectedStarters.Count > 2 ? selectedStarters[2] : null;

            Debug.Log($"[StarterSelectUI] Starting Run with Starters: {s1?.catName}, {s2?.catName}, {s3?.catName}");
            GameManager.Instance?.StartRun(s1, s2, s3);
        }
    }
}
