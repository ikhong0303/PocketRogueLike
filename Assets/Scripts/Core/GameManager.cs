using System;
using UnityEngine;

namespace PocketRoguelike
{
    public enum GameState
    {
        StarterSelect,
        StageBattle,
        Catching,
        PartyManage,
        NextStage,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Databases & References")]
        [SerializeField] private CatDatabaseSO catDatabase;
        [SerializeField] private GameState currentState = GameState.StarterSelect;

        public CatDatabaseSO CatDatabase => catDatabase;
        public GameState CurrentState => currentState;

        public event Action<GameState> OnStateChanged;

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

        private void Start()
        {
            InitGame();
        }

        private void Update()
        {
            // Global Hotkeys based on System Spec 4.3 / 4.4
            if (currentState == GameState.StageBattle)
            {
                // [SPACE] to start Catch Process
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    TryStartCatch();
                }
                // [P] to open Party Management modal
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    OpenPartyManagement();
                }
            }
            else if (currentState == GameState.PartyManage)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ClosePartyManagement();
                }
            }
        }

        public void InitGame()
        {
            if (catDatabase == null)
            {
                catDatabase = Resources.Load<CatDatabaseSO>("CatDatabase");
            }

            ChangeState(GameState.StarterSelect);
        }

        public void SetCatDatabase(CatDatabaseSO db)
        {
            catDatabase = db;
        }

        public void ChangeState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"[GameManager] Game State Changed -> {currentState}");
            OnStateChanged?.Invoke(currentState);

            switch (currentState)
            {
                case GameState.StarterSelect:
                    PartyManager.Instance.ClearParty();
                    StageManager.Instance.InitRun();
                    break;

                case GameState.StageBattle:
                    StartStageBattle();
                    break;

                case GameState.NextStage:
                    ProcessNextStage();
                    break;
            }
        }

        public void StartRun(CatDataSO s1, CatDataSO s2, CatDataSO s3)
        {
            PartyManager.Instance.ClearParty();

            if (s1 != null) PartyManager.Instance.AddCat(new CatInstance(s1, 5));
            if (s2 != null) PartyManager.Instance.AddCat(new CatInstance(s2, 5));
            if (s3 != null) PartyManager.Instance.AddCat(new CatInstance(s3, 5));

            ChangeState(GameState.StageBattle);
        }

        private void StartStageBattle()
        {
            CatInstance playerCat = PartyManager.Instance.GetActiveCat();
            if (playerCat == null)
            {
                ChangeState(GameState.GameOver);
                return;
            }

            CatInstance enemyCat = StageManager.Instance.GenerateEnemyForStage(catDatabase);
            BattleManager.Instance.StartBattle(playerCat, enemyCat);
        }

        public void TryStartCatch()
        {
            if (currentState != GameState.StageBattle) return;

            CatInstance enemy = BattleManager.Instance.EnemyCat;
            if (enemy == null || enemy.IsFainted) return;

            BattleManager.Instance.PauseBattle();
            ChangeState(GameState.Catching);
            CatchManager.Instance.StartCatchProcess(enemy);
        }

        public void HandleCatchResult(bool isSuccess, CatInstance cat)
        {
            if (isSuccess)
            {
                Debug.Log($"[GameManager] Successfully caught {cat.Data.catName}!");
                CatInstance caughtCat = new CatInstance(cat.Data, cat.Level);

                if (PartyManager.Instance.IsFull)
                {
                    // Party full (6 cats) -> Open PartyManage modal
                    ChangeState(GameState.PartyManage);
                }
                else
                {
                    PartyManager.Instance.AddCat(caughtCat);
                    AdvanceToNextStage();
                }
            }
            else
            {
                Debug.Log("[GameManager] Catch failed! Resuming battle...");
                ChangeState(GameState.StageBattle);
                BattleManager.Instance.ResumeBattle();
            }
        }

        public void OpenPartyManagement()
        {
            if (currentState == GameState.StageBattle)
            {
                BattleManager.Instance.PauseBattle();
                ChangeState(GameState.PartyManage);
            }
        }

        public void ClosePartyManagement()
        {
            if (currentState == GameState.PartyManage)
            {
                ChangeState(GameState.StageBattle);
                BattleManager.Instance.ResumeBattle();
            }
        }

        public void ProcessNextStage()
        {
            if (StageManager.Instance.IsFinalBoss && BattleManager.Instance.EnemyCat != null && BattleManager.Instance.EnemyCat.IsFainted)
            {
                ChangeState(GameState.Victory);
                return;
            }

            // Heal party if previous stage was Boss
            if (StageManager.Instance.IsBossStage)
            {
                PartyManager.Instance.FullHealAll();
            }

            StageManager.Instance.AdvanceStage();
            ChangeState(GameState.StageBattle);
        }

        public void AdvanceToNextStage()
        {
            ChangeState(GameState.NextStage);
        }
    }
}
