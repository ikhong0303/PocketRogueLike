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

        private void OnEnable()
        {
            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.OnPartyUpdated += RefreshPartyUI;
            }
            RefreshPartyUI();
        }

        private void OnDisable()
        {
            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.OnPartyUpdated -= RefreshPartyUI;
            }
        }

        public void RefreshPartyUI()
        {
            if (PartyManager.Instance == null) return;

            var party = PartyManager.Instance.Party;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < party.Count)
                {
                    CatInstance cat = party[i];
                    slots[i].container.SetActive(true);
                    if (slots[i].iconImage != null) slots[i].iconImage.sprite = cat.Data.sprite;
                    if (slots[i].nameText != null) slots[i].nameText.text = cat.Data.catName;
                    if (slots[i].levelText != null) slots[i].levelText.text = $"Lv.{cat.Level}";
                    if (slots[i].hpSlider != null)
                    {
                        slots[i].hpSlider.maxValue = cat.MaxHp;
                        slots[i].hpSlider.value = cat.CurrentHp;
                    }
                }
                else
                {
                    slots[i].container.SetActive(false);
                }
            }
        }
    }
}
