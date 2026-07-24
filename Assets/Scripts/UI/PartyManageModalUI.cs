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
            public Button replaceButton;
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
            LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
            LanguageManager.OnLanguageChanged += HandleLanguageChanged;
            RefreshModal();
        }

        private void OnDisable() => LanguageManager.OnLanguageChanged -= HandleLanguageChanged;
        private void HandleLanguageChanged(GameLanguage _) => RefreshModal();

        public void RefreshModal()
        {
            if (PartyManager.Instance == null) return;

            var party = PartyManager.Instance.Party;
            bool hasPendingCat = GameManager.Instance != null && GameManager.Instance.HasPendingCaughtCat;
            bool forcedSwitch = GameManager.Instance != null && GameManager.Instance.IsForcedSwitch;
            if (closeButton != null) closeButton.gameObject.SetActive(!forcedSwitch);
            if (headerText != null)
            {
                headerText.text = hasPendingCat
                    ? LanguageManager.Format("party_full_replace", LanguageManager.CatName(GameManager.Instance.PendingCaughtCat.Data))
                    : forcedSwitch
                        ? LanguageManager.Get("forced_switch")
                        : LanguageManager.Get("party_switch_turn_cost");
            }

            for (int i = 0; i < modalSlots.Count; i++)
            {
                int index = i;
                ModalSlot slot = modalSlots[i];
                if (i >= party.Count)
                {
                    if (slot.container != null) slot.container.SetActive(false);
                    continue;
                }

                CatInstance cat = party[i];
                if (slot.container != null) slot.container.SetActive(true);
                if (slot.iconImage != null) slot.iconImage.sprite = cat.Data.sprite;
                if (slot.nameText != null) slot.nameText.text = LanguageManager.CatName(cat.Data);
                if (slot.levelText != null) slot.levelText.text = LanguageManager.Format("level", cat.Level);
                if (slot.hpText != null) slot.hpText.text = LanguageManager.Format("hp", cat.CurrentHp, cat.MaxHp);

                if (slot.replaceButton != null)
                {
                    slot.replaceButton.GetComponentInChildren<TMP_Text>()?.SetText(LanguageManager.Get("replace"));
                    slot.replaceButton.interactable = hasPendingCat || !cat.IsFainted;
                    slot.replaceButton.onClick.RemoveAllListeners();
                    slot.replaceButton.onClick.AddListener(() =>
                    {
                        if (GameManager.Instance == null) return;
                        if (GameManager.Instance.HasPendingCaughtCat)
                        {
                            GameManager.Instance.ReplacePartyMemberWithCaughtCat(index);
                        }
                        else
                        {
                            GameManager.Instance.SwitchActivePartyMember(index);
                        }
                    });
                }

                if (slot.releaseButton != null)
                {
                    slot.releaseButton.GetComponentInChildren<TMP_Text>()?.SetText(LanguageManager.Get("release"));
                    slot.releaseButton.interactable = party.Count > 1 && !forcedSwitch;
                    slot.releaseButton.onClick.RemoveAllListeners();
                    slot.releaseButton.onClick.AddListener(() =>
                    {
                        GameManager.Instance?.ReleasePartyMember(index);
                        RefreshModal();
                    });
                }
            }
        }
    }
}