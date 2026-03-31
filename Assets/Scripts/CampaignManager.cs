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

        private readonly string[] kenyanNames = new string[]
        {
            "Maina", "Kamau", "Otieno", "Omondi", "Kiprono", "Mutua", "Wanjala", "Mwangi", 
            "Juma", "Ochieng", "Kariuki", "Kimutai", "Waweru", "Ondieki", "Wanyama", "Njoroge"
        };

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

            // Assign a random Kenyan name to the Rival Candidate (or load existing)
            if (RivalCandidate != null)
            {
                string savedRivalName = PlayerPrefs.GetString("RivalCandidateName", "");
                if (string.IsNullOrEmpty(savedRivalName))
                {
                    savedRivalName = kenyanNames[Random.Range(0, kenyanNames.Length)];
                    PlayerPrefs.SetString("RivalCandidateName", savedRivalName);
                }
                RivalCandidate.candidateName = savedRivalName;
                RivalCandidate.gameObject.name = "Rival_" + savedRivalName;
            }

            // Register Multiplayer Candidates found in the scene
            var candidates = Object.FindObjectsByType<CandidateProfile>(FindObjectsSortMode.None);
            foreach (var c in candidates)
            {
                RegisterMultiplayerCandidate(c);
            }

            isCampaignActive = false; // Campaign starts inactive
            RefreshApprovalTotals();
        }

        public List<CandidateProfile> multiplayerCandidates = new List<CandidateProfile>();

        public void RegisterMultiplayerCandidate(CandidateProfile candidate)
        {
            if (!multiplayerCandidates.Contains(candidate))
            {
                multiplayerCandidates.Add(candidate);
                Debug.Log($"[CAMPAIGN] Registered candidate for election: {candidate.candidateName}");
            }
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
                PlayerPrefs.DeleteKey("CurrentCycleState");

                // Start with the Tutorial for new campaigns
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeState(GameState.Tutorial);
                }
            }
            else
            {
                // Continue from where we left off
                timer = PlayerPrefs.GetFloat("CampaignTimer", campaignDuration);
                
                // If we were in a post-election state, restore it
                string savedState = PlayerPrefs.GetString("CurrentCycleState", "Campaigning");
                
                // Set the state first to ensure UI activates
                if (GameManager.Instance != null)
                {
                    if (savedState == "OpponentTerm") 
                        GameManager.Instance.ChangeState(GameState.OpponentTerm);
                    else if (savedState == "InOffice")
                        GameManager.Instance.ChangeState(GameState.InOffice);
                    else
                        GameManager.Instance.ChangeState(GameState.Campaigning);
                }

                // If the timer is already 0 and we were campaigning, it means we closed during result screen
                if (timer <= 0 && savedState == "Campaigning")
                {
                    RefreshApprovalTotals();
                    if (playerSupportPercent > 50f)
                        GameManager.Instance.ChangeState(GameState.Victory);
                    else
                        GameManager.Instance.ChangeState(GameState.Defeat);
                }
                //Debug.Log("Cntinue Campaign");
                RefreshApprovalTotals();
                //Debug.Log("Updated Totals");
                return; // ChangeState handles the rest
            }

            RefreshApprovalTotals();
            
            Debug.Log(isNewGame ? "New Campaign has started with Tutorial!" : "Campaign continued from saved state!");
        }

        private void Update()
        {
            if (!isCampaignActive) return;

            timer -= Time.deltaTime;
            
            // Save timer periodically (every 5 seconds)
            if (Mathf.FloorToInt(timer) % 5 == 0)
            {
                PlayerPrefs.SetFloat("CampaignTimer", timer);
                PlayerPrefs.SetString("CurrentCycleState", GameManager.Instance.CurrentState.ToString());
            }

            if (timer <= 0)
            {
                if (GameManager.Instance.CurrentState == GameState.Campaigning)
                    EndCampaign();
                else
                    ResetToNewCampaign();
            }
        }

        private void ResetToNewCampaign()
        {
            // After 30 mins of office/rival rule, start a new 2-minute election
            timer = campaignDuration;
            GameManager.Instance.ChangeState(GameState.Campaigning);
            Debug.Log("New Election Cycle Started!");
        }

        public void RefreshApprovalTotals()
        {
            // Ensure we have all NPCs if list is empty or some might have been added
            if (allNPCs == null || allNPCs.Count == 0)
                allNPCs = Object.FindObjectsByType<NPCVotingProfile>(FindObjectsSortMode.None).ToList();

            float totalInfluence = allNPCs.Sum(n => n.influenceValue);
            if (totalInfluence <= 0) return;

            // Use a dictionary to track support for all registered candidates (multiplayer and AI)
            Dictionary<CandidateProfile, float> supportMap = new Dictionary<CandidateProfile, float>();
            foreach (var candidate in multiplayerCandidates)
            {
                supportMap[candidate] = 0f;
            }

            foreach (var npc in allNPCs)
            {
                if (npc.currentSupportedCandidate != null && supportMap.ContainsKey(npc.currentSupportedCandidate))
                {
                    supportMap[npc.currentSupportedCandidate] += npc.influenceValue;
                }
            }

            // Update individual candidate support percentages
            foreach (var candidate in multiplayerCandidates)
            {
                candidate.supportPercentage = (supportMap[candidate] / totalInfluence) * 100f;
            }

            // Keep these fields synced for backward compatibility with existing UI
            if (PlayerCandidate != null) playerSupportPercent = PlayerCandidate.supportPercentage;
            if (RivalCandidate != null) rivalSupportPercent = RivalCandidate.supportPercentage;
            
            float totalSupportedPercent = 0;
            foreach (var candidate in multiplayerCandidates) totalSupportedPercent += candidate.supportPercentage;
            undecidedPercent = Mathf.Max(0, 100f - totalSupportedPercent);
        }

        private void EndCampaign()
        {
            isCampaignActive = false;
            RefreshApprovalTotals(); // Final check
            
            // Determine winner in a field of multiple candidates
            CandidateProfile winner = null;
            float highestSupport = -1f;

            foreach (var candidate in multiplayerCandidates)
            {
                if (candidate.supportPercentage > highestSupport)
                {
                    highestSupport = candidate.supportPercentage;
                    winner = candidate;
                }
            }

            if (winner == PlayerCandidate && playerSupportPercent > 0) // Basic win condition check
            {
                GameManager.Instance.ChangeState(GameState.Victory);
            }
            else
            {
                // If the player didn't win, someone else did (AI or other player)
                GameManager.Instance.ChangeState(GameState.Defeat);
            }
        }

        public void StartPostElectionTerm(bool isPlayerInOffice)
        {
            // If the timer was 0 or invalid when starting a new term, reset to full 30 mins
            if (timer <= 0 || timer < 1000f) timer = 1800f; // Reset to 30 minutes if term is essentially over
            
            isCampaignActive = true; 
            
            // If we lost, ensure NPCs aren't completely biased against us for the next cycle
            if (!isPlayerInOffice)
            {
                foreach (var npc in allNPCs)
                {
                    // Give the player a small "underdog" trust boost to make a comeback possible
                    npc.AddPlayerTrust(10f);
                }
                RefreshApprovalTotals();
            }
            
            // Save state immediately
            PlayerPrefs.SetFloat("CampaignTimer", timer);
            PlayerPrefs.SetString("CurrentCycleState", isPlayerInOffice ? "InOffice" : "OpponentTerm");
            PlayerPrefs.Save();
            
            if (isPlayerInOffice)
                Debug.Log("You are now in office! You have 30 minutes to govern and keep public appeal.");
            else
                Debug.Log($"The Rival ({RivalCandidate.candidateName}) has taken office! You have 30 minutes to rebuild appeal as a civilian.");
        }

        public float GetRemainingTime() => Mathf.Max(0, timer);
        public List<NPCVotingProfile> GetAllNPCs() => allNPCs;
    }
}