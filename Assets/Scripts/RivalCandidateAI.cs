using UnityEngine;

namespace Kanairo.Core
{
    public class RivalCandidateAI : MonoBehaviour
    {
        public float influenceInterval = 5f;
        private float timer;

        private void Start()
        {
            timer = influenceInterval;
        }

        private void Update()
        {
            // Rival only acts if the campaign is active and tutorial is NOT showing
            if (GameManager.Instance.CurrentState != GameState.Campaigning) return;

            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                PerformInfluence();
                timer = influenceInterval;
            }
        }

        private void PerformInfluence()
        {
            var manager = CampaignManager.Instance;
            if (manager == null) return;

            var npcs = manager.GetAllNPCs();
            if (npcs.Count == 0) return;

            // Pick a random NPC to influence
            var target = npcs[Random.Range(0, npcs.Count)];
            
            // Use rival's focus
            target.ProcessRivalInfluence(manager.RivalCandidate.campaignFocus);
            manager.RefreshApprovalTotals();
            
            Debug.Log($"[RIVAL] Influenced {target.npcName} with focus {manager.RivalCandidate.campaignFocus}. Target Trust (R): {target.trustInRival}");
        }
    }
}