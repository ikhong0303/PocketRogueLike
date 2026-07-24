using UnityEngine;

namespace PocketRoguelike
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject starterSelectPanel;
        [SerializeField] private GameObject battlePanel;
        [SerializeField] private GameObject catchTimingPanel;
        [SerializeField] private GameObject partyPanel;
        [SerializeField] private GameObject partyManageModalPanel;
        [SerializeField] private GameObject resultPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        public void HandleStateChanged(GameState state)
        {
            // Toggle panels based on current GameState
            if (starterSelectPanel != null) starterSelectPanel.SetActive(state == GameState.StarterSelect);
            if (battlePanel != null) battlePanel.SetActive(state == GameState.StageBattle || state == GameState.Catching || state == GameState.PartyManage);
            if (catchTimingPanel != null) catchTimingPanel.SetActive(state == GameState.Catching);
            if (partyPanel != null) partyPanel.SetActive(state == GameState.StageBattle || state == GameState.Catching || state == GameState.PartyManage);
            if (partyManageModalPanel != null) partyManageModalPanel.SetActive(state == GameState.PartyManage);
            if (resultPanel != null) resultPanel.SetActive(state == GameState.GameOver || state == GameState.Victory);
        }
    }
}
