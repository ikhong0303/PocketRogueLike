using System;
using System.Collections;
using UnityEngine;

namespace PocketRoguelike
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Battle Settings")]
        [SerializeField] private float turnDelay = 1.2f;
        [SerializeField] private float presentationTimeout = 6f;

        [Header("Active Battle Participants")]
        [SerializeField] private CatInstance playerCat;
        [SerializeField] private CatInstance enemyCat;
        [SerializeField] private bool isBattleActive;

        public CatInstance PlayerCat => playerCat;
        public CatInstance EnemyCat => enemyCat;
        public bool IsBattleActive => isBattleActive;
        public bool IsPresentingAttack => presentationPending;

        public event Action<CatInstance, CatInstance> OnBattleStarted;
        public event Action<CatInstance, CatInstance, int, string> OnAttackExecuted;
        public event Action<CatInstance> OnCatDefeated;
        public event Action<CatInstance> OnEnemyCaptured;
        public event Action<bool> OnBattleEnded;

        private Coroutine battleCoroutine;
        private bool presentationPending;

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

        public void StartBattle(CatInstance player, CatInstance enemy)
        {
            if (player?.Data == null || enemy?.Data == null)
            {
                Debug.LogError("[BattleManager] Cannot start battle with invalid participants.");
                return;
            }

            StopBattleCoroutine();
            playerCat = player;
            enemyCat = enemy;
            isBattleActive = true;
            presentationPending = false;
            OnBattleStarted?.Invoke(playerCat, enemyCat);
            battleCoroutine = StartCoroutine(AutoBattleLoop());
        }

        public void PauseBattle()
        {
            isBattleActive = false;
            StopBattleCoroutine();
        }

        public void ResumeBattle()
        {
            if (isBattleActive || !CanParticipantsFight()) return;
            isBattleActive = true;
            StopBattleCoroutine();
            battleCoroutine = StartCoroutine(AutoBattleLoop());
        }

        public void SetPlayerCat(CatInstance newPlayerCat)
        {
            if (newPlayerCat?.Data == null || newPlayerCat.IsFainted) return;
            playerCat = newPlayerCat;
            OnBattleStarted?.Invoke(playerCat, enemyCat);
        }

        public void ResumeAfterPlayerSwitch(bool consumePlayerTurn)
        {
            if (!CanParticipantsFight()) return;
            StopBattleCoroutine();
            isBattleActive = true;
            battleCoroutine = consumePlayerTurn
                ? StartCoroutine(EnemyFreeAttackAfterSwitch())
                : StartCoroutine(AutoBattleLoop());
        }

        public void CompleteAttackPresentation()
        {
            presentationPending = false;
        }

        public bool CompleteBattleByCapture(CatInstance capturedEnemy)
        {
            if (capturedEnemy == null || capturedEnemy != enemyCat)
            {
                Debug.LogWarning("[BattleManager] Ignored capture result for a stale enemy.");
                return false;
            }

            PauseBattle();
            CatInstance completedEnemy = enemyCat;
            enemyCat = null;
            OnEnemyCaptured?.Invoke(completedEnemy);
            Debug.Log($"[BattleManager] Battle completed by capturing {completedEnemy.Data.catName}.");
            return true;
        }

        private IEnumerator AutoBattleLoop()
        {
            while (CanContinueBattle())
            {
                yield return new WaitForSeconds(turnDelay);
                if (!isBattleActive) yield break;

                bool playerFirst = playerCat.Speed >= enemyCat.Speed;
                CatInstance firstAttacker = playerFirst ? playerCat : enemyCat;
                CatInstance firstDefender = playerFirst ? enemyCat : playerCat;

                yield return ExecuteAttackAndWait(firstAttacker, firstDefender);
                if (firstDefender.IsFainted)
                {
                    HandleFaint(firstDefender);
                    yield break;
                }
                if (!isBattleActive) yield break;

                yield return new WaitForSeconds(0.25f);
                if (!isBattleActive) yield break;

                yield return ExecuteAttackAndWait(firstDefender, firstAttacker);
                if (firstAttacker.IsFainted)
                {
                    HandleFaint(firstAttacker);
                    yield break;
                }
            }
        }

        private IEnumerator EnemyFreeAttackAfterSwitch()
        {
            yield return new WaitForSeconds(turnDelay * 0.65f);
            if (!CanContinueBattle()) yield break;

            yield return ExecuteAttackAndWait(enemyCat, playerCat);
            if (playerCat.IsFainted)
            {
                HandleFaint(playerCat);
                yield break;
            }
            if (!isBattleActive) yield break;

            battleCoroutine = StartCoroutine(AutoBattleLoop());
        }

        private IEnumerator ExecuteAttackAndWait(CatInstance attacker, CatInstance defender)
        {
            if (attacker == null || defender == null || attacker.IsFainted || defender.IsFainted) yield break;

            int damage = Mathf.RoundToInt(attacker.Atk * UnityEngine.Random.Range(0.85f, 1.15f));
            defender.TakeDamage(damage);
            string message = LanguageManager.Format("attack_log", LanguageManager.CatName(attacker.Data), LanguageManager.SkillName(attacker.Data), LanguageManager.CatName(defender.Data), damage);
            Debug.Log($"[BattleManager] {message}");

            presentationPending = OnAttackExecuted != null;
            OnAttackExecuted?.Invoke(attacker, defender, damage, message);

            float elapsed = 0f;
            while (presentationPending && elapsed < presentationTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (presentationPending)
            {
                presentationPending = false;
                Debug.LogWarning("[BattleManager] Attack presentation timed out; battle flow resumed safely.");
            }
        }

        private bool CanParticipantsFight()
        {
            return playerCat?.Data != null && enemyCat?.Data != null && !playerCat.IsFainted && !enemyCat.IsFainted;
        }

        private bool CanContinueBattle() => isBattleActive && CanParticipantsFight();

        private void HandleFaint(CatInstance faintedCat)
        {
            isBattleActive = false;
            battleCoroutine = null;
            OnCatDefeated?.Invoke(faintedCat);
            bool isPlayerWin = faintedCat == enemyCat;
            Debug.Log($"[BattleManager] Battle Over! Result: {(isPlayerWin ? "PLAYER VICTORY" : "PLAYER DEFEAT")}");
            OnBattleEnded?.Invoke(isPlayerWin);
        }

        private void StopBattleCoroutine()
        {
            if (battleCoroutine == null) return;
            StopCoroutine(battleCoroutine);
            battleCoroutine = null;
        }
    }
}
