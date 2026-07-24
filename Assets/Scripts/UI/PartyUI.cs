using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class PartyUI : MonoBehaviour
    {
        [System.Serializable]
        public class PartySlotUI
        {
            public GameObject container;
            public Image iconImage;
            public TMP_Text nameText;
            public TMP_Text levelText;
            public Slider hpSlider;
        }

        [SerializeField] private List<PartySlotUI> slots = new List<PartySlotUI>(6);

        private void Awake()
        {
            EnsureVerticalLayout();
        }

        private void OnEnable()
        {
            EnsureVerticalLayout();
            if (PartyManager.Instance != null) PartyManager.Instance.OnPartyUpdated += RefreshPartyUI;
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            RefreshPartyUI();
        }

        private void OnDisable()
        {
            if (PartyManager.Instance != null) PartyManager.Instance.OnPartyUpdated -= RefreshPartyUI;
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(GameLanguage _) => RefreshPartyUI();

        public void EnsureVerticalLayout()
        {
            RectTransform panelRt = GetComponent<RectTransform>();
            if (panelRt != null)
            {
                panelRt.anchorMin = new Vector2(0.01f, 0.32f);
                panelRt.anchorMax = new Vector2(0.22f, 0.98f);
                panelRt.offsetMin = Vector2.zero;
                panelRt.offsetMax = Vector2.zero;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null || slots[i].container == null) continue;
                RectTransform slotRt = slots[i].container.GetComponent<RectTransform>();
                if (slotRt == null) continue;

                // Stack slots vertically from top (i=0) to bottom (i=5)
                float maxY = 1f - (i * (1f / 6f));
                float minY = 1f - ((i + 1) * (1f / 6f));

                slotRt.anchorMin = new Vector2(0f, minY);
                slotRt.anchorMax = new Vector2(1f, maxY);
                slotRt.offsetMin = new Vector2(0f, 1f);
                slotRt.offsetMax = new Vector2(0f, -1f);

                if (slots[i].iconImage != null)
                {
                    RectTransform iconRt = slots[i].iconImage.GetComponent<RectTransform>();
                    if (iconRt != null)
                    {
                        iconRt.anchorMin = new Vector2(0.03f, 0.08f);
                        iconRt.anchorMax = new Vector2(0.35f, 0.92f);
                        iconRt.offsetMin = Vector2.zero;
                        iconRt.offsetMax = Vector2.zero;
                    }
                }
                if (slots[i].nameText != null)
                {
                    RectTransform nameRt = slots[i].nameText.GetComponent<RectTransform>();
                    if (nameRt != null)
                    {
                        nameRt.anchorMin = new Vector2(0.38f, 0.52f);
                        nameRt.anchorMax = new Vector2(0.97f, 0.95f);
                        nameRt.offsetMin = Vector2.zero;
                        nameRt.offsetMax = Vector2.zero;
                        slots[i].nameText.fontSize = 14f;
                    }
                }
                if (slots[i].levelText != null)
                {
                    RectTransform lvRt = slots[i].levelText.GetComponent<RectTransform>();
                    if (lvRt != null)
                    {
                        lvRt.anchorMin = new Vector2(0.38f, 0.1f);
                        lvRt.anchorMax = new Vector2(0.62f, 0.5f);
                        lvRt.offsetMin = Vector2.zero;
                        lvRt.offsetMax = Vector2.zero;
                        slots[i].levelText.fontSize = 12f;
                    }
                }
                if (slots[i].hpSlider != null)
                {
                    RectTransform hpRt = slots[i].hpSlider.GetComponent<RectTransform>();
                    if (hpRt != null)
                    {
                        hpRt.anchorMin = new Vector2(0.64f, 0.15f);
                        hpRt.anchorMax = new Vector2(0.97f, 0.45f);
                        hpRt.offsetMin = Vector2.zero;
                        hpRt.offsetMax = Vector2.zero;
                    }
                }
            }
        }

        public void RefreshPartyUI()
        {
            EnsureVerticalLayout();
            if (PartyManager.Instance == null) return;
            var party = PartyManager.Instance.Party;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < party.Count)
                {
                    CatInstance cat = party[i];
                    slots[i].container.SetActive(true);
                    if (slots[i].iconImage != null) slots[i].iconImage.sprite = cat.Data.sprite;
                    if (slots[i].nameText != null) slots[i].nameText.text = LanguageManager.CatName(cat.Data);
                    if (slots[i].levelText != null) slots[i].levelText.text = LanguageManager.Format("level", cat.Level);
                    if (slots[i].hpSlider != null)
                    {
                        slots[i].hpSlider.maxValue = cat.MaxHp;
                        slots[i].hpSlider.value = cat.CurrentHp;
                    }
                }
                else slots[i].container.SetActive(false);
            }
        }
    }
}