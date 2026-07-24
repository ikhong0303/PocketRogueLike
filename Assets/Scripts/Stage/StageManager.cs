using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PocketRoguelike
{
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        public const int MAX_STAGES = 100;

        [SerializeField] private int currentStage = 1;
        [SerializeField] private List<int> backboneDexList = new List<int>();

        public int CurrentStage => currentStage;
        public bool IsBossStage => currentStage % 10 == 0;
        public bool IsFinalBoss => currentStage == MAX_STAGES;

        public event Action<int, bool> OnStageStarted; // stageNumber, isBoss

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

        public void InitRun(int seed = 0)
        {
            currentStage = 1;
            GenerateBackbone(seed);
            Debug.Log($"[StageManager] Run Initialized with {backboneDexList.Count} stages.");
        }

        private void GenerateBackbone(int seed)
        {
            System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);
            
            // Random sequence of 100 dex numbers from 1..100 sorted in ascending order for stage curve
            List<int> available = Enumerable.Range(1, 100).ToList();
            backboneDexList = available.OrderBy(_ => rng.Next()).Take(100).OrderBy(dex => dex).ToList();
        }

        public int GetTargetDexNo(int stage)
        {
            int idx = Mathf.Clamp(stage - 1, 0, backboneDexList.Count - 1);
            return backboneDexList[idx];
        }

        public CatInstance GenerateEnemyForStage(CatDatabaseSO database)
        {
            if (database == null) return null;

            int targetDex = GetTargetDexNo(currentStage);
            CatDataSO data = database.GetByDexNo(targetDex) ?? database.GetRandomCat();

            int enemyLevel = Mathf.Max(1, Mathf.RoundToInt(currentStage * 1.2f));
            bool isBoss = IsBossStage;

            CatInstance enemyCat = new CatInstance(data, enemyLevel, isBoss);
            Debug.Log($"[StageManager] Stage {currentStage} Enemy: {enemyCat.Data.catName} (Lv.{enemyCat.Level}, Boss: {isBoss})");
            return enemyCat;
        }

        public void AdvanceStage()
        {
            currentStage++;
            if (currentStage > MAX_STAGES)
            {
                currentStage = MAX_STAGES;
            }
            OnStageStarted?.Invoke(currentStage, IsBossStage);
        }
    }
}
