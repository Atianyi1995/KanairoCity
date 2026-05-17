using UnityEngine;
using Kanairo.Core;
using System.Collections.Generic;

namespace Kanairo.Multiplayer
{
    /// <summary>
    /// Handles the Rally mechanic where the player addresses a crowd to boost approval.
    /// </summary>
    public class RallyManager : MonoBehaviour
    {
        public static RallyManager Instance { get; private set; }

        [Header("Rally Settings")]
        public float rallyRadius = 15f;
        public float baseApprovalBoost = 5f;
        public float rallyCooldown = 60f;
        
        private float lastRallyTime;

        private void Awake()
        {
            Instance = this;
            lastRallyTime = -rallyCooldown;
        }

        public bool CanHoldRally()
        {
            return Time.time >= lastRallyTime + rallyCooldown;
        }

        /// <summary>
        /// Triggers a rally that boosts trust for all NPCs within range.
        /// </summary>
        public void HoldRally()
        {
            if (!CanHoldRally()) return;

            lastRallyTime = Time.time;
            
            // Find all NPCs in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, rallyRadius);
            int affectedCount = 0;

            foreach (var col in colliders)
            {
                var npc = col.GetComponent<NPCVotingProfile>();
                if (npc != null)
                {
                    float boost = baseApprovalBoost;
                    
                    // Boost rally effectiveness if in a party
                    if (PlayerPrefs.GetInt("HasPoliticalParty", 0) == 1)
                    {
                        boost *= PlayerPrefs.GetFloat("PartyApprovalMultiplier", 1.2f);
                    }

                    npc.AddPlayerTrust(boost);
                    affectedCount++;
                }
            }

            CampaignManager.Instance?.RefreshApprovalTotals();
            Debug.Log($"[RALLY] Addressed {affectedCount} people! Approval boosted.");
        }
    }
}
