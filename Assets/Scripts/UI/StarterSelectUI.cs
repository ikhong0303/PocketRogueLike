using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class StarterSelectUI : MonoBehaviour
    {
        public const int MAX_BUDGET = 10;
        public const int MAX_STARTERS = 6;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text budgetText;
        [SerializeField] private Button startRunButton;
        [SerializeField] private Transform slotsContainer;

        [Header("Selected Starters (Max 6, Cost 10)")]
        [SerializeField] private List<CatDataSO> selectedStarters = new List<CatDataSO>();

        private readonly Dictionary<CatDataSO, Image> cardBgImages = new Dictionary<CatDataSO, Image>();
        private readonly Dictionary<CatDataSO, TMP_Text> cardInfoTexts = new Dictionary<CatDataSO, TMP_Text>();

        public IReadOnlyList<CatDataSO> SelectedStarters => selectedStarters;
        public int CurrentTotalCost
        {
            get
            {
                int cost = 0;
                foreach (CatDataSO starter in selectedStarters)
                    if (starter != null) cost += starter.StarterCost;
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
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            selectedStarters.Clear();

            if (startRunButton != null)
            {
                startRunButton.onClick.RemoveAllListeners();
                startRunButton.onClick.AddListener(OnStartRunClicked);
            }

            PopulateStarterOptions();
            UpdateBudgetUI();
        }

        private void OnDisable()
        {
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(GameLanguage _)
        {
            RefreshStarterCardTexts();
            UpdateBudgetUI();
        }

        private void EnsureUIComponents()
        {
            if (budgetText == null) budgetText = transform.Find("BudgetText")?.GetComponent<TMP_Text>();
            if (startRunButton == null) startRunButton = transform.Find("StartRunButton")?.GetComponent<Button>();
            if (slotsContainer != null) return;

            Transform existingContent = transform.Find("StarterScrollView/Viewport/Content");
            if (existingContent != null)
            {
                slotsContainer = existingContent;
                return;
            }

            GameObject scrollObject = new GameObject("StarterScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(transform, false);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            SetRect(scrollRectTransform, 0.05f, 0.18f, 0.95f, 0.70f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            SetFullStretch(viewportRect);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(230f, 280f);
            grid.spacing = new Vector2(20f, 20f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;
            slotsContainer = content.transform;
        }

        private void SelectDefaultStarters()
        {
            if (GameManager.Instance == null || GameManager.Instance.CatDatabase == null) return;
            List<CatDataSO> unlocked = CatUnlockProgress.GetUnlockedCats(GameManager.Instance.CatDatabase);
            foreach (CatDataSO cat in unlocked)
            {
                if (selectedStarters.Count >= 3) break;
                if (cat != null && CurrentTotalCost + cat.StarterCost <= MAX_BUDGET) selectedStarters.Add(cat);
            }
        }

        private void PopulateStarterOptions()
        {
            if (GameManager.Instance == null || GameManager.Instance.CatDatabase == null) return;
            List<CatDataSO> candidates = CatUnlockProgress.GetUnlockedCats(GameManager.Instance.CatDatabase);

            cardBgImages.Clear();
            cardInfoTexts.Clear();
            foreach (Transform child in slotsContainer) Destroy(child.gameObject);

            foreach (CatDataSO cat in candidates)
            {
                if (cat == null) continue;
                GameObject cardObject = new GameObject($"Card_{cat.dexNo}", typeof(RectTransform), typeof(Image), typeof(Button));
                cardObject.transform.SetParent(slotsContainer, false);

                Image cardBackground = cardObject.GetComponent<Image>();
                cardBackground.color = selectedStarters.Contains(cat) ? new Color(0.2f, 0.6f, 0.2f, 0.9f) : new Color(0.15f, 0.18f, 0.25f, 0.9f);
                cardBgImages[cat] = cardBackground;

                GameObject imageObject = new GameObject("CatSprite", typeof(RectTransform), typeof(Image));
                imageObject.transform.SetParent(cardObject.transform, false);
                SetRect(imageObject.GetComponent<RectTransform>(), 0.1f, 0.44f, 0.9f, 0.94f);
                Image catImage = imageObject.GetComponent<Image>();
                catImage.sprite = cat.sprite;
                catImage.preserveAspect = true;

                GameObject textObject = new GameObject("CatInfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObject.transform.SetParent(cardObject.transform, false);
                SetRect(textObject.GetComponent<RectTransform>(), 0.04f, 0.03f, 0.96f, 0.42f);
                TMP_Text infoText = textObject.GetComponent<TMP_Text>();
                infoText.text = LanguageManager.Format("starter_card", LanguageManager.CatName(cat), cat.StarterCost, LanguageManager.Rarity(cat.rarity), LanguageManager.SkillName(cat), cat.baseHp, cat.baseAtk);
                infoText.font = budgetText != null ? budgetText.font : infoText.font;
                infoText.fontSize = 16f;
                infoText.alignment = TextAlignmentOptions.Center;
                infoText.color = Color.white;
                cardInfoTexts[cat] = infoText;

                CatDataSO targetCat = cat;
                cardObject.GetComponent<Button>().onClick.AddListener(() => ToggleStarterSelection(targetCat));
            }
        }

        private void RefreshStarterCardTexts()
        {
            foreach (KeyValuePair<CatDataSO, TMP_Text> entry in cardInfoTexts)
            {
                if (entry.Key != null && entry.Value != null)
                    entry.Value.text = LanguageManager.Format("starter_card", LanguageManager.CatName(entry.Key), entry.Key.StarterCost, LanguageManager.Rarity(entry.Key.rarity), LanguageManager.SkillName(entry.Key), entry.Key.baseHp, entry.Key.baseAtk);
            }
        }

        public void ToggleStarterSelection(CatDataSO cat)
        {
            if (cat == null || !CatUnlockProgress.IsUnlocked(cat.dexNo)) return;
            if (selectedStarters.Contains(cat))
            {
                selectedStarters.Remove(cat);
            }
            else
            {
                if (selectedStarters.Count >= MAX_STARTERS)
                {
                    Debug.LogWarning("[StarterSelectUI] Max 6 starters allowed.");
                    return;
                }
                if (CurrentTotalCost + cat.StarterCost > MAX_BUDGET)
                {
                    Debug.LogWarning("[StarterSelectUI] Exceeds the 10 point starter budget.");
                    return;
                }
                selectedStarters.Add(cat);
            }
            UpdateBudgetUI();
        }

        private void UpdateBudgetUI()
        {
            if (budgetText != null) budgetText.text = LanguageManager.Format("starter_budget", CurrentTotalCost, selectedStarters.Count);
            if (startRunButton != null)
            {
                TMP_Text startLabel = startRunButton.GetComponentInChildren<TMP_Text>(true);
                if (startLabel != null) startLabel.text = LanguageManager.Get("start_run");
                startRunButton.interactable = selectedStarters.Count > 0 && selectedStarters.Count <= MAX_STARTERS && CurrentTotalCost <= MAX_BUDGET;
            }
            foreach (KeyValuePair<CatDataSO, Image> entry in cardBgImages)
            {
                if (entry.Value == null) continue;
                bool selected = selectedStarters.Contains(entry.Key);
                entry.Value.color = selected ? new Color(0.2f, 0.7f, 0.3f, 0.95f) : new Color(0.15f, 0.18f, 0.25f, 0.9f);
            }
        }

        public void StartRunFromUI()
        {
            OnStartRunClicked();
        }

        private void OnStartRunClicked()
        {
            if (selectedStarters.Count == 0 || selectedStarters.Count > MAX_STARTERS || CurrentTotalCost > MAX_BUDGET) return;
            Debug.Log($"[StarterSelectUI] Starting run with {selectedStarters.Count} cats, cost {CurrentTotalCost}/{MAX_BUDGET}.");
            GameManager.Instance?.StartRun(selectedStarters);
        }

        private static void SetRect(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
