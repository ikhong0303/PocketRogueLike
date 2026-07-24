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
            public Button reviveButton;
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
            int revives = CatchManager.Instance != null ? CatchManager.Instance.ReviveCount : 0;
            string reviveInfo = $" ({LanguageManager.Format("revive_count_info", revives)})";

            if (closeButton != null) closeButton.gameObject.SetActive(!forcedSwitch);
            if (headerText != null)
            {
                headerText.text = (hasPendingCat
                    ? LanguageManager.Format("party_full_replace", LanguageManager.CatName(GameManager.Instance.PendingCaughtCat.Data))
                    : forcedSwitch
                        ? LanguageManager.Get("forced_switch")
                        : LanguageManager.Get("party_switch_turn_cost")) + reviveInfo;
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

                // Revive Button setup
                Button reviveBtn = slot.reviveButton;
                if (reviveBtn == null && slot.container != null)
                {
                    Transform reviveTrans = slot.container.transform.Find("ReviveButton");
                    if (reviveTrans != null)
                    {
                        reviveBtn = reviveTrans.GetComponent<Button>();
                    }
                    else
                    {
                        GameObject reviveGO = new GameObject("ReviveButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                        reviveGO.transform.SetParent(slot.container.transform, false);
                        RectTransform rt = reviveGO.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.67f, 0.16f);
                        rt.anchorMax = new Vector2(0.81f, 0.84f);
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;

                        Image bg = reviveGO.GetComponent<Image>();
                        bg.color = new Color(0.18f, 0.65f, 0.28f, 1f);

                        GameObject txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                        txtGO.transform.SetParent(reviveGO.transform, false);
                        RectTransform txtRt = txtGO.GetComponent<RectTransform>();
                        txtRt.anchorMin = Vector2.zero;
                        txtRt.anchorMax = Vector2.one;
                        txtRt.offsetMin = Vector2.zero;
                        txtRt.offsetMax = Vector2.zero;

                        TextMeshProUGUI label = txtGO.GetComponent<TextMeshProUGUI>();
                        label.text = LanguageManager.Get("revive");
                        label.alignment = TextAlignmentOptions.Center;
                        label.fontSize = 18f;
                        label.fontStyle = FontStyles.Bold;
                        label.color = Color.white;
                        if (slot.nameText != null && slot.nameText.font != null) label.font = slot.nameText.font;

                        reviveBtn = reviveGO.GetComponent<Button>();

                        if (slot.replaceButton != null)
                        {
                            RectTransform rRt = slot.replaceButton.GetComponent<RectTransform>();
                            rRt.anchorMin = new Vector2(0.51f, 0.16f);
                            rRt.anchorMax = new Vector2(0.65f, 0.84f);
                        }
                        if (slot.releaseButton != null)
                        {
                            RectTransform relRt = slot.releaseButton.GetComponent<RectTransform>();
                            relRt.anchorMin = new Vector2(0.83f, 0.16f);
                            relRt.anchorMax = new Vector2(0.97f, 0.84f);
                        }
                    }
                    slot.reviveButton = reviveBtn;
                }

                if (reviveBtn != null)
                {
                    reviveBtn.gameObject.SetActive(cat.IsFainted);
                    reviveBtn.interactable = cat.IsFainted && CatchManager.Instance != null && CatchManager.Instance.ReviveCount > 0;
                    TMP_Text reviveLabel = reviveBtn.GetComponentInChildren<TMP_Text>();
                    if (reviveLabel != null) reviveLabel.text = LanguageManager.Get("revive");

                    reviveBtn.onClick.RemoveAllListeners();
                    reviveBtn.onClick.AddListener(() =>
                    {
                        if (CatchManager.Instance != null && CatchManager.Instance.UseRevive(cat))
                        {
                            RefreshModal();
                        }
                    });
                }
            }
        }
    }
}