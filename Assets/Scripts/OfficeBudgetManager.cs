using UnityEngine;
using System.Collections.Generic;

namespace Kanairo.Core
{
    public class OfficeBudgetManager : MonoBehaviour
    {
        public static OfficeBudgetManager Instance { get; private set; }

        [Header("Budget Settings")]
        public float totalBudget = 1000000f;
        public float remainingBudget;

        [Header("Allocations")]
        public float securityFunding;
        public float cleanlinessFunding;
        public float roadsFunding;

        [Header("Public Rating")]
        public float publicApproval = 50f;
        public float promiseFulfillmentScore = 0f;

        private void Awake()
        {
            Instance = this;
            remainingBudget = totalBudget;
        }

        [Header("Personal Funds (for bribes)")]
        public float personalFunds = 0f;
        public float bribePenaltyMultiplier = 15f; // Drastic drop per bribe

        public void ApplyBudget(float security, float cleanliness, float roads)
        {
            securityFunding = security;
            cleanlinessFunding = cleanliness;
            roadsFunding = roads;
            
            // Remaining budget after essential spending
            remainingBudget = totalBudget - (security + cleanliness + roads);

            CalculateApproval();
        }

        public bool TryBribe(NPCVotingProfile npc, float cost)
        {
            if (personalFunds >= cost)
            {
                personalFunds -= cost;
                // Drastic drop in public approval
                publicApproval = Mathf.Max(0, publicApproval - bribePenaltyMultiplier);
                npc.BribeInfluence();
                CalculateApproval();
                return true;
            }
            return false;
        }

        public void TransferToPersonal(float amount)
        {
            if (remainingBudget >= amount)
            {
                remainingBudget -= amount;
                personalFunds += amount;
                // Skimming also hurts public approval
                publicApproval = Mathf.Max(0, publicApproval - (amount / 10000f)); 
            }
        }

        private void CalculateApproval()
        {
            // Base approval from funding
            float avgFunding = (securityFunding + cleanlinessFunding + roadsFunding) / totalBudget;
            float targetApproval = Mathf.Clamp(avgFunding * 100f, 0f, 100f);

            // Gradual transition for more dynamic feel
            publicApproval = Mathf.Lerp(publicApproval, targetApproval, Time.deltaTime * 0.1f);
            
            // Invoke events for consequences
            SecuritySpawner.Instance?.OnBudgetUpdated(securityFunding / totalBudget);
            CleanlinessSpawner.Instance?.OnBudgetUpdated(cleanlinessFunding / totalBudget);
        }
    }
}