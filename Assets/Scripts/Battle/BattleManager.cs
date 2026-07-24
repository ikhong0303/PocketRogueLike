using System;
using System.Collections;
using UnityEngine;

namespace PocketRoguelike
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Battle Settings")]
        [SerializeField] private float turnDelay = 1.2f; // Time between auto attacks

        [Header("Active Battle Participants")]
        [SerializeField] private CatInstance playerCat;
        [SerializeField] private CatInstance enemyCat;
        [SerializeField] private bool isBattleActive = false;

        public CatInstance PlayerCat => playerCat;
        public CatInstance EnemyCat => enemyCat;
        public bool IsBattleActive => isBattleActive;

        public event Action<CatInstance, CatInstance> OnBattleStarted;
        public event Action<CatInstance, CatInstance, int, string> OnAttackExecuted; // attacker, defender, damage, logMsg
        public event Action<CatInstance> OnCatDefeated;
        public event Action<bool> OnBattleEnded; // isPlayerWin

        private Coroutine battleCoroutine;

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
            if (player == null || enemy == null)
            {
                Debug.LogError("[BattleManager] Cannot start battle with null participants!");
                return;
            }

            playerCat = player;
            enemyCat = enemy;
            isBattleActive = true;

            OnBattleStarted?.Invoke(playerCat, enemyCat);

            if (battleCoroutine != null) StopCoroutine(battleCoroutine);
            battleCoroutine = StartCoroutine(AutoBattleLoop());
        }

        public void PauseBattle()
        {
            isBattleActive = false;
            if (battleCoroutine != null)
            {
                StopCoroutine(battleCoroutine);
                battleCoroutine = null;
            }
        }

        public void ResumeBattle()
        {
            if (!isBattleActive && playerCat != null && enemyCat != null && !playerCat.IsFainted && !enemyCat.IsFainted)
            {
                isBattleActive = true;
                if (battleCoroutine != null) StopCoroutine(battleCoroutine);
                battleCoroutine = StartCoroutine(AutoBattleLoop());
            }
        }

        private IEnumerator AutoBattleLoop()
        {
            while (isBattleActive && !playerCat.IsFainted && !enemyCat.IsFainted)
            {
                yield return new WaitForSeconds(turnDelay);

                if (!isBattleActive) yield break;

                // Determine turn order based on Speed
                bool playerFirst = playerCat.Speed >= enemyCat.Speed;

                CatInstance firstAttacker = playerFirst ? playerCat : enemyCat;
                CatInstance firstDefender = playerFirst ? enemyCat : playerCat;

                ExecuteAttack(firstAttacker, firstDefender);

                if (firstDefender.IsFainted)
                {
                    HandleFaint(firstDefender);
                    yield break;
                }

                yield return new WaitForSeconds(0.6f);

                if (!isBattleActive) yield break;

                ExecuteAttack(firstDefender, firstAttacker);

                if (firstAttacker.IsFainted)
                {
                    HandleFaint(firstAttacker);
                    yield break;
                }
            }
        }

        private void ExecuteAttack(CatInstance attacker, CatInstance defender)
        {
            if (attacker.IsFainted || defender.IsFainted) return;

            // Damage calculation formula
            int damage = Mathf.RoundToInt(attacker.Atk * UnityEngine.Random.Range(0.85f, 1.15f));
            defender.TakeDamage(damage);

            string msg = $"{attacker.Data.catName} attacked {defender.Data.catName} for {damage} damage!";
            Debug.Log($"[BattleManager] {msg}");

            OnAttackExecuted?.Invoke(attacker, defender, damage, msg);
        }

        private void HandleFaint(CatInstance faintedCat)
        {
            isBattleActive = false;
            OnCatDefeated?.Invoke(faintedCat);

            bool isPlayerWin = faintedCat == enemyCat;
            Debug.Log($"[BattleManager] Battle Over! Result: {(isPlayerWin ? "PLAYER VICTORY" : "PLAYER DEFEAT")}");

            OnBattleEnded?.Invoke(isPlayerWin);
        }
    }
}
