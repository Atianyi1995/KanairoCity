using UnityEngine;
using System.Collections.Generic;

namespace Kanairo.Core
{
    public class CleanlinessSpawner : MonoBehaviour
    {
        public static CleanlinessSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        public GameObject trashPrefab;
        public Transform[] spawnPoints;
        public int maxTrashCount = 50;

        private List<GameObject> activeTrash = new List<GameObject>();

        private void Awake() => Instance = this;

        public void OnBudgetUpdated(float budgetPercent)
        {
            // Simplified: Lower budget spawns more trash immediately or over time
            int targetCount = Mathf.FloorToInt((1f - budgetPercent) * maxTrashCount);
            
            while (activeTrash.Count < targetCount)
            {
                SpawnTrash();
            }
            
            while (activeTrash.Count > targetCount && activeTrash.Count > 0)
            {
                GameObject t = activeTrash[0];
                activeTrash.RemoveAt(0);
                Destroy(t);
            }
        }

        private void SpawnTrash()
        {
            if (spawnPoints.Length == 0 || trashPrefab == null) return;
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 randomPos = sp.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            GameObject trash = Instantiate(trashPrefab, randomPos, Quaternion.identity);
            activeTrash.Add(trash);
        }
    }
}