using System;
using System.Collections.Generic;
using UnityEngine;

namespace PocketRoguelike
{
    public enum GameState
    {
        StarterSelect,
        StageBattle,
        Catching,
        PartyManage,
        StageClear,
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

        private CatInstance pendingCaughtCat;
        public bool HasPendingCaughtCat => pendingCaughtCat != null;
        public CatInstance PendingCaughtCat => pendingCaughtCat;
        public bool IsForcedSwitch { get; private set; }
        public VictoryReward LastVictoryReward { get; private set; }
        public CatInstance LastDefeatedEnemy { get; private set; }

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
            SubscribeToGameEvents();
            InitGame();
        }

        private void OnEnable()
        {
            SubscribeToGameEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }

        private void SubscribeToGameEvents()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
                BattleManager.Instance.OnBattleEnded += HandleBattleEnded;
            }

            if (CatchManager.Instance != null)
            {
                CatchManager.Instance.OnCatchResult -= HandleCatchResult;
                CatchManager.Instance.OnCatchResult += HandleCatchResult;
            }
        }

        private void UnsubscribeFromGameEvents()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnBattleEnded -= HandleBattleEnded;
            }

            if (CatchManager.Instance != null)
            {
                CatchManager.Instance.OnCatchResult -= HandleCatchResult;
            }
        }

        private void Update()
        {
            bool isSpacePressed = false;
            bool isPPressed = false;
            bool isEscPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) isSpacePressed = true;
                if (UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame) isPPressed = true;
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) isEscPressed = true;
            }
#endif
            if (!isSpacePressed) { try { isSpacePressed = Input.GetKeyDown(KeyCode.Space); } catch { } }
            if (!isPPressed) { try { isPPressed = Input.GetKeyDown(KeyCode.P); } catch { } }
            if (!isEscPressed) { try { isEscPressed = Input.GetKeyDown(KeyCode.Escape); } catch { } }

            // Global Hotkeys based on System Spec 4.3 / 4.4
            if (currentState == GameState.StageBattle)
            {
                // [SPACE] to start Catch Process
                if (isSpacePressed)
                {
                    TryStartCatch();
                }
                // [P] to open Party Management modal
                else if (isPPressed)
                {
                    OpenPartyManagement();
                }
            }
            else if (currentState == GameState.PartyManage)
            {
                if (isEscPressed)
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
                    pendingCaughtCat = null;
                    PartyManager.Instance.ClearParty();
                    StageManager.Instance.InitRun();
                    break;

                case GameState.StageBattle:
                    if (BattleManager.Instance.EnemyCat == null || BattleManager.Instance.EnemyCat.IsFainted ||
                        BattleManager.Instance.PlayerCat == null || BattleManager.Instance.PlayerCat.IsFainted)
                    {
                        StartStageBattle();
                    }
                    break;

                case GameState.NextStage:
                    ProcessNextStage();
                    break;
            }
        }

        public void StartRun(IReadOnlyList<CatDataSO> starters)
        {
            PartyManager.Instance.ClearParty();
            IsForcedSwitch = false;
            LastVictoryReward = default;
            LastDefeatedEnemy = null;

            int totalCost = 0;
            HashSet<int> addedDexNumbers = new HashSet<int>();
            if (starters != null)
            {
                foreach (CatDataSO starter in starters)
                {
                    if (starter == null || addedDexNumbers.Contains(starter.dexNo)) continue;
                    if (!CatUnlockProgress.IsUnlocked(starter.dexNo)) continue;
                    if (PartyManager.Instance.Count >= StarterSelectUI.MAX_STARTERS) break;
                    if (totalCost + starter.StarterCost > StarterSelectUI.MAX_BUDGET) continue;

                    if (PartyManager.Instance.AddCat(new CatInstance(starter, 5)))
                    {
                        totalCost += starter.StarterCost;
                        addedDexNumbers.Add(starter.dexNo);
                    }
                }
            }

            if (PartyManager.Instance.Count == 0 && catDatabase != null)
            {
                CatDataSO fallback = catDatabase.GetByDexNo(1);
                if (fallback != null) PartyManager.Instance.AddCat(new CatInstance(fallback, 5));
            }

            Debug.Log($"[GameManager] Run started with {PartyManager.Instance.Count} cats, starter cost {totalCost}/{StarterSelectUI.MAX_BUDGET}.");
            CatchManager.Instance.InitRunInventory();
            ChangeState(GameState.StageBattle);
        }

        public void StartRun(CatDataSO s1, CatDataSO s2, CatDataSO s3)
        {
            StartRun(new[] { s1, s2, s3 });
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
            if (currentState != GameState.StageBattle || BattleManager.Instance.IsPresentingAttack) return;

            CatInstance enemy = BattleManager.Instance.EnemyCat;
            if (enemy == null || enemy.IsFainted) return;

            if (!CatchManager.Instance.HasBalls)
            {
                Debug.LogWarning("[GameManager] No Monster Balls remaining.");
                return;
            }

            BattleManager.Instance.PauseBattle();
            ChangeState(GameState.Catching);
            if (!CatchManager.Instance.TryThrowBall(enemy))
            {
                ChangeState(GameState.StageBattle);
                BattleManager.Instance.ResumeBattle();
            }
        }

        public void HandleCatchResult(bool isSuccess, CatInstance cat)
        {
            if (currentState != GameState.Catching)
            {
                Debug.LogWarning("[GameManager] Ignored duplicate or stale catch result.");
                return;
            }

            if (isSuccess)
            {
                if (!BattleManager.Instance.CompleteBattleByCapture(cat))
                {
                    ChangeState(GameState.StageBattle);
                    BattleManager.Instance.ResumeBattle();
                    return;
                }

                if (cat != null && cat.Data != null) CatUnlockProgress.RecordCapture(cat.Data);
                Debug.Log($"[GameManager] Successfully caught {cat.Data.catName}!");
                CatInstance caughtCat = new CatInstance(cat.Data, cat.Level);

                if (PartyManager.Instance.IsFull)
                {
                    pendingCaughtCat = caughtCat;
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

        private void HandleBattleEnded(bool isPlayerWin)
        {
            if (currentState != GameState.StageBattle) return;

            if (isPlayerWin)
            {
                LastDefeatedEnemy = BattleManager.Instance.EnemyCat;
                LastVictoryReward = CatchManager.Instance.RollVictoryDrops();
                ChangeState(GameState.StageClear);
                return;
            }

            if (PartyManager.Instance.IsAllFainted())
            {
                ChangeState(GameState.GameOver);
                return;
            }

            IsForcedSwitch = true;
            ChangeState(GameState.PartyManage);
        }

        public void ReplacePartyMemberWithCaughtCat(int partyIndex)
        {
            if (pendingCaughtCat == null || !PartyManager.Instance.IsFull)
            {
                return;
            }

            if (!PartyManager.Instance.SwapCat(partyIndex, pendingCaughtCat))
            {
                return;
            }

            pendingCaughtCat = null;
            AdvanceToNextStage();
        }

        public void SwitchActivePartyMember(int partyIndex)
        {
            if (pendingCaughtCat != null || currentState != GameState.PartyManage) return;
            bool consumeTurn = !IsForcedSwitch;
            if (!PartyManager.Instance.SetActiveCat(partyIndex)) return;

            CatInstance selected = PartyManager.Instance.GetActiveCat();
            IsForcedSwitch = false;
            BattleManager.Instance.SetPlayerCat(selected);
            ChangeState(GameState.StageBattle);
            BattleManager.Instance.ResumeAfterPlayerSwitch(consumeTurn);
        }

        public void ReleasePartyMember(int partyIndex)
        {
            if (currentState != GameState.PartyManage || IsForcedSwitch) return;

            CatInstance released = partyIndex >= 0 && partyIndex < PartyManager.Instance.Party.Count
                ? PartyManager.Instance.Party[partyIndex]
                : null;
            if (!PartyManager.Instance.ReleaseCat(partyIndex)) return;

            if (pendingCaughtCat != null)
            {
                PartyManager.Instance.AddCat(pendingCaughtCat);
                pendingCaughtCat = null;
                AdvanceToNextStage();
                return;
            }

            if (released == BattleManager.Instance.PlayerCat)
            {
                BattleManager.Instance.SetPlayerCat(PartyManager.Instance.GetActiveCat());
            }
        }

        public void OpenPartyManagement()
        {
            if (currentState == GameState.StageBattle && !BattleManager.Instance.IsPresentingAttack)
            {
                IsForcedSwitch = false;
                BattleManager.Instance.PauseBattle();
                ChangeState(GameState.PartyManage);
            }
        }

        public void ClosePartyManagement()
        {
            if (currentState == GameState.PartyManage)
            {
                if (IsForcedSwitch) return;
                if (pendingCaughtCat != null)
                {
                    Debug.Log("[GameManager] Pending caught cat released; keeping the current party.");
                    pendingCaughtCat = null;
                    AdvanceToNextStage();
                    return;
                }

                ChangeState(GameState.StageBattle);
                BattleManager.Instance.ResumeBattle();
            }
        }

        public void ConfirmStageClear()
        {
            if (currentState != GameState.StageClear) return;
            AdvanceToNextStage();
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
