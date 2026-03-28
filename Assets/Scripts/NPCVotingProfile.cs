using UnityEngine;

namespace Kanairo.Core
{
    public class NPCVotingProfile : MonoBehaviour
    {
        public string npcName;
        public string role;
        public float influenceValue = 1f;

        [Header("Needs")]
        public CampaignPromise primaryNeed;
        public CampaignPromise secondaryNeed;

        [Header("Trust Scores")]
        public float trustInPlayer;
        public float trustInRival;

        [Header("Current Status")]
        public CandidateProfile currentSupportedCandidate;
        public bool hasTalkedToPlayer;
        [TextArea] public string memoryNotes;

        public void ProcessPlayerPromise(CampaignPromise promise)
        {
            float boost = VotingLogic.CalculateTrustChange(promise, primaryNeed, secondaryNeed, currentSupportedCandidate, true);
            AddPlayerTrust(boost);
            memoryNotes = $"Player promised {promise}. Status: {(trustInPlayer > trustInRival ? "Supports Player" : "Skeptical")}";
        }

        public void AddPlayerTrust(float amount)
        {
            trustInPlayer += amount;
            hasTalkedToPlayer = true;
            UpdateSupport();
        }

        public void ProcessRivalInfluence(CampaignPromise rivalFocus)
        {
            float boost = VotingLogic.CalculateTrustChange(rivalFocus, primaryNeed, secondaryNeed, currentSupportedCandidate, false);
            trustInRival += boost;
            
            UpdateSupport();
            
            // Log for debugging
            Debug.Log($"[NPC] {npcName} received rival influence. Rival Trust: {trustInRival}");
        }

        public void BribeInfluence()
        {
            // Bribing gives a massive boost but is ethically questionable
            trustInPlayer += 50f; 
            memoryNotes = "Player bribed me. I am now a 'loyal' supporter.";
            UpdateSupport();
        }

        private void UpdateSupport()
        {
            CampaignManager manager = CampaignManager.Instance;
            if (manager == null) return;

            CandidateProfile player = manager.PlayerCandidate;
            CandidateProfile rival = manager.RivalCandidate;

            // NPCs now have a "Trust Threshold" to commit their vote
            // If trust is high enough, they commit to the candidate
            const float VOTE_THRESHOLD = 50f;

            if (trustInPlayer >= VOTE_THRESHOLD && trustInPlayer > trustInRival)
                currentSupportedCandidate = player;
            else if (trustInRival >= VOTE_THRESHOLD && trustInRival > trustInPlayer)
                currentSupportedCandidate = rival;
            else
                currentSupportedCandidate = null; // Undecided or below threshold

            // Save the trust levels to PlayerPrefs whenever they change
            SaveTrust();
        }

        private void SaveTrust()
        {
            string npcID = gameObject.name; // Using name as a simple unique ID
            PlayerPrefs.SetFloat(npcID + "_TrustPlayer", trustInPlayer);
            PlayerPrefs.SetFloat(npcID + "_TrustRival", trustInRival);
            PlayerPrefs.SetInt(npcID + "_HasTalked", hasTalkedToPlayer ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void Start()
        {
            LoadTrust();
        }

        private void LoadTrust()
        {
            string npcID = gameObject.name;
            if (PlayerPrefs.HasKey(npcID + "_TrustPlayer"))
            {
                trustInPlayer = PlayerPrefs.GetFloat(npcID + "_TrustPlayer");
                trustInRival = PlayerPrefs.GetFloat(npcID + "_TrustRival");
                hasTalkedToPlayer = PlayerPrefs.GetInt(npcID + "_HasTalked") == 1;
                
                // Immediately update support based on loaded data
                UpdateSupport();
            }
        }
    }
}