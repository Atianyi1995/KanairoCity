using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Kanairo.Core
{
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        [Header("Candidates")]
        public CandidateProfile PlayerCandidate;
        public CandidateProfile RivalCandidate;

        [Header("Timer")]
        public float campaignDuration = 120f; // 2 minutes
        private float timer;
        private bool isCampaignActive;

        [Header("Results")]
        public float playerSupportPercent;
        public float rivalSupportPercent;
        public float undecidedPercent;

        private List<NPCVotingProfile> allNPCs = new List<NPCVotingProfile>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            allNPCs = Object.FindObjectsByType<NPCVotingProfile>(FindObjectsSortMode.None).ToList();
            
            // Try to load existing timer
            if (PlayerPrefs.HasKey("CampaignTimer"))
            {
                timer = PlayerPrefs.GetFloat("CampaignTimer");
            }
            else
            {
                timer = campaignDuration;
            }

            isCampaignActive = false; // Campaign starts inactive
            RefreshApprovalTotals();
        }

        public void StartCampaign(bool isNewGame = true)
        {
            isCampaignActive = true;
            
            if (isNewGame)
            {
                timer = campaignDuration;
                // Clear all NPC trust data when starting a completely new life
                foreach(var npc in allNPCs)
                {
                    PlayerPrefs.DeleteKey(npc.gameObject.name + "_TrustPlayer");
                    PlayerPrefs.DeleteKey(npc.gameObject.name + "_TrustRival");
                    PlayerPrefs.DeleteKey(npc.gameObject.name + "_HasTalked");
                }
                PlayerPrefs.DeleteKey("CampaignTimer");
            }
            else
            {
                // Continue from where we left off
                timer = PlayerPrefs.GetFloat("CampaignTimer", campaignDuration);
            }

            RefreshApprovalTotals();
            
            // Sync with GameManager state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Campaigning);
            }

            Debug.Log(isNewGame ? "New Campaign has started!" : "Campaign continued from saved state!");
        }

        private void Update()
        {
            if (!isCampaignActive) return;

            timer -= Time.deltaTime;
            
            // Save timer periodically (every 5 seconds)
            if (Mathf.FloorToInt(timer) % 5 == 0)
            {
                PlayerPrefs.SetFloat("CampaignTimer", timer);
            }

            if (timer <= 0)
            {
                EndCampaign();
            }
        }

        public void RefreshApprovalTotals()
        {
            // Ensure we have all NPCs if list is empty or some might have been added
            if (allNPCs == null || allNPCs.Count == 0)
                allNPCs = Object.FindObjectsByType<NPCVotingProfile>(FindObjectsSortMode.None).ToList();

            float totalInfluence = allNPCs.Sum(n => n.influenceValue);
            if (totalInfluence <= 0) return;

            float playerPoints = 0;
            float rivalPoints = 0;

            foreach (var npc in allNPCs)
            {
                if (npc.currentSupportedCandidate == PlayerCandidate)
                    playerPoints += npc.influenceValue;
                else if (npc.currentSupportedCandidate == RivalCandidate)
                    rivalPoints += npc.influenceValue;
            }

            playerSupportPercent = (playerPoints / totalInfluence) * 100f;
            rivalSupportPercent = (rivalPoints / totalInfluence) * 100f;
            undecidedPercent = 100f - playerSupportPercent - rivalSupportPercent;
            
            if (PlayerCandidate != null) PlayerCandidate.supportPercentage = playerSupportPercent;
            if (RivalCandidate != null) RivalCandidate.supportPercentage = rivalSupportPercent;
        }

        private void EndCampaign()
        {
            isCampaignActive = false;
            RefreshApprovalTotals(); // Final check
            
            if (playerSupportPercent > 50f)
            {
                GameManager.Instance.ChangeState(GameState.Victory);
            }
            else
            {
                // If you don't win, you lost to the rival
                GameManager.Instance.ChangeState(GameState.Defeat);
            }
        }

        public void StartOpponentTerm()
        {
            timer = 1800f; // 30 minutes
            isCampaignActive = true; 
            // During OpponentTerm, player can still interact and gain trust
            // but Rival won't be actively campaigning as much or at all
        }

        public float GetRemainingTime() => Mathf.Max(0, timer);
        public List<NPCVotingProfile> GetAllNPCs() => allNPCs;
    }
}