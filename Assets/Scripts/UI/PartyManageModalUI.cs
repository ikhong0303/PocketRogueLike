using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PocketRoguelike
{
    public class PartyManageModalUI : MonoBehaviour
    {
        [System.Serializable]
        public class ModalSlot
        {
            public GameObject container;
            public Image iconImage;
            public TMP_Text nameText;
            public TMP_Text levelText;
            public TMP_Text hpText;
            public Button releaseButton;
        }

        [SerializeField] private List<ModalSlot> modalSlots = new List<ModalSlot>(6);
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text headerText;

        private void OnEnable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => GameManager.Instance?.ClosePartyManagement());
            }
            RefreshModal();
        }

        public void RefreshModal()
        {
            if (PartyManager.Instance == null) return;

            var party = PartyManager.Instance.Party;
            if (headerText != null)
            {
                headerText.text = PartyManager.Instance.IsFull 
                    ? "Party Full (6/6)! Select a Cat to Release / Swap:" 
                    : "Party Management (Press [ESC] to Return)";
            }

            for (int i = 0; i < modalSlots.Count; i++)
            {
                int index = i;
                if (i < party.Count)
                {
                    CatInstance cat = party[i];
                    modalSlots[i].container.SetActive(true);
                    if (modalSlots[i].iconImage != null) modalSlots[i].iconImage.sprite = cat.Data.sprite;
                    if (modalSlots[i].nameText != null) modalSlots[i].nameText.text = cat.Data.catName;
                    if (modalSlots[i].levelText != null) modalSlots[i].levelText.text = $"Lv.{cat.Level}";
                    if (modalSlots[i].hpText != null) modalSlots[i].hpText.text = $"HP: {cat.CurrentHp}/{cat.MaxHp}";

                    if (modalSlots[i].releaseButton != null)
                    {
                        modalSlots[i].releaseButton.onClick.RemoveAllListeners();
                        modalSlots[i].releaseButton.onClick.AddListener(() =>
                        {
                            PartyManager.Instance.ReleaseCat(index);
                            RefreshModal();
                        });
                    }
                }
                else
                {
                    modalSlots[i].container.SetActive(false);
                }
            }
        }
    }
}
