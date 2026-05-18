using UnityEngine;
using player2_sdk;

namespace Kanairo.Core
{
    [RequireComponent(typeof(Player2Npc))]
    [RequireComponent(typeof(NPCVotingProfile))]
    public class Player2CampaignLink : MonoBehaviour
    {
        private Player2Npc player2Npc;
        private NPCVotingProfile votingProfile;

        private void Awake()
        {
            player2Npc = GetComponent<Player2Npc>();
            votingProfile = GetComponent<NPCVotingProfile>();
        }

        private void Update()
        {
            // We can periodically sync the voting state to the NPC's "game_state_info" 
            // if the SDK supports passing it in ChatRequest. 
            // Looking at ChatRequest in Player2Npc.cs, there is a 'game_state_info' field.
        }

        public string GetCampaignContext()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Your name is {votingProfile.npcName} and your role in Kanairo is {votingProfile.role}.");
            sb.AppendLine($"The election is coming up. You care most about {votingProfile.primaryNeed} and your second priority is {votingProfile.secondaryNeed}.");

            // Add Political Party Awareness
            if (PlayerPrefs.GetInt("HasPoliticalParty", 0) == 1)
            {
                string partyName = PlayerPrefs.GetString("PoliticalPartyName", "Fimbo");
                sb.AppendLine($"The player has joined the {partyName} party. This makes them look more professional and serious as a candidate.");
            }
            else
            {
                sb.AppendLine("The player is currently an independent candidate with no party backing.");
            }
            
            if (votingProfile.currentSupportedCandidate != null)
            {
                if (votingProfile.currentSupportedCandidate == CampaignManager.Instance.PlayerCandidate)
                    sb.AppendLine("You are satisfied with the player's campaign so far and you are leaning towards supporting them.");
                else
                    sb.AppendLine($"You currently feel aligned with {votingProfile.currentSupportedCandidate.candidateName}, which makes the player your opponent for now.");
            }
            else
            {
                sb.AppendLine("You are currently undecided on who to vote for and open to being persuaded by the player.");
            }

            sb.AppendLine($"Trust in Player: {votingProfile.trustInPlayer}. Trust in Rival: {votingProfile.trustInRival}.");
            
            if (!string.IsNullOrEmpty(votingProfile.memoryNotes))
            {
                sb.AppendLine($"Recent Interaction Memory: {votingProfile.memoryNotes}");
            }

            return sb.ToString();
        }
    }
}