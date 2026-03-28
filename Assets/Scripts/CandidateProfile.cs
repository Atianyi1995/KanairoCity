using UnityEngine;
using System.Collections.Generic;

namespace Kanairo.Core
{
    public class CandidateProfile : MonoBehaviour
    {
        public string candidateName;
        public bool isPlayerControlled;
        public CampaignPromise campaignFocus;
        
        [Header("Stats")]
        public float totalApprovalPoints;
        public float supportPercentage;
        
        public List<CampaignPromise> promisesMade = new List<CampaignPromise>();

        public void AddPromise(CampaignPromise promise)
        {
            if (!promisesMade.Contains(promise))
            {
                promisesMade.Add(promise);
            }
        }
    }
}