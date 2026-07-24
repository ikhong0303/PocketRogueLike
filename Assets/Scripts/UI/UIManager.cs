using UnityEngine;

namespace PocketRoguelike
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject starterSelectPanel;
        [SerializeField] private GameObject battlePanel;
        [SerializeField] private GameObject partyPanel;
        [SerializeField] private GameObject partyManageModalPanel;
        [SerializeField] private GameObject stageClearPanel;
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
            EnsurePanels();
        }

        private void EnsurePanels()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            if (starterSelectPanel == null) starterSelectPanel = canvas.transform.Find("StarterSelectPanel")?.gameObject;
            if (battlePanel == null) battlePanel = canvas.transform.Find("BattlePanel")?.gameObject;
            if (partyPanel == null) partyPanel = canvas.transform.Find("PartyPanel")?.gameObject;
            if (partyManageModalPanel == null) partyManageModalPanel = canvas.transform.Find("PartyManageModalPanel")?.gameObject;
            if (stageClearPanel == null) stageClearPanel = canvas.transform.Find("StageClearPanel")?.gameObject;
            if (resultPanel == null) resultPanel = canvas.transform.Find("ResultPanel")?.gameObject;
        }

        private void Start()
        {
            EnsurePanels();
            SubscribeToGameManager();
            if (GameManager.Instance != null)
            {
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnEnable()
        {
            SubscribeToGameManager();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void SubscribeToGameManager()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        public void HandleStateChanged(GameState state)
        {
            // Toggle panels based on current GameState
            if (starterSelectPanel != null) starterSelectPanel.SetActive(state == GameState.StarterSelect);
            if (battlePanel != null) battlePanel.SetActive(state == GameState.StageBattle || state == GameState.Catching || state == GameState.PartyManage || state == GameState.StageClear);
            if (partyPanel != null) partyPanel.SetActive(state == GameState.StageBattle || state == GameState.Catching || state == GameState.PartyManage);
            if (partyManageModalPanel != null) partyManageModalPanel.SetActive(state == GameState.PartyManage);
            if (stageClearPanel != null) stageClearPanel.SetActive(state == GameState.StageClear);
            if (resultPanel != null) resultPanel.SetActive(state == GameState.GameOver || state == GameState.Victory);
        }
    }
}
