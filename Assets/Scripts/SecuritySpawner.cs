using UnityEngine;
using System.Collections.Generic;

namespace Kanairo.Core
{
    public class SecuritySpawner : MonoBehaviour
    {
        public static SecuritySpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        public GameObject thiefPrefab;
        public Transform[] spawnPoints;
        public float baseSpawnRate = 30f;
        public float maxThieves = 10;

        private List<GameObject> activeThieves = new List<GameObject>();
        private float currentSpawnTimer;
        private float spawnMultiplier = 1f;

        private void Awake() => Instance = this;

        private void Update()
        {
            if (spawnMultiplier >= 2f) // Only spawn if security budget is very low
            {
                currentSpawnTimer -= Time.deltaTime;
                if (currentSpawnTimer <= 0 && activeThieves.Count < maxThieves)
                {
                    SpawnThief();
                    currentSpawnTimer = baseSpawnRate / spawnMultiplier;
                }
            }
            
            // Cleanup dead/destroyed thieves
            activeThieves.RemoveAll(t => t == null);
        }

        public void OnBudgetUpdated(float budgetPercent)
        {
            // Lower budget = Higher multiplier (more thieves)
            if (budgetPercent < 0.2f) spawnMultiplier = 5f;
            else if (budgetPercent < 0.4f) spawnMultiplier = 2f;
            else spawnMultiplier = 0.5f;
        }

        private void SpawnThief()
        {
            if (spawnPoints.Length == 0 || thiefPrefab == null) return;
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject thief = Instantiate(thiefPrefab, sp.position, sp.rotation);
            activeThieves.Add(thief);
        }
    }
}