using UnityEngine;
using Kanairo.Core;

namespace Kanairo.Multiplayer
{
    /// <summary>
    /// Represents a Party Leader NPC who can accept players into their party.
    /// Being in a party provides a multiplier to persuasion trust gains.
    /// </summary>
    public class PartyLeader : MonoBehaviour
    {
        [Header("Leader Settings")]
        public string partyName = "Fimbo";
        public float joinFinancialBonus = 100000f; // Cash given when joining
        public float approvalMultiplier = 1.25f; // +25% trust gain boost
        
        [Header("Dialogue")]
        public string initialGreeting = "I am the head of the Fimbo party. We seek leaders who can move the people.";
        public string joinSuccessMessage = "Welcome to Fimbo. Here is some capital to start your campaign.";
        public string alreadyMemberMessage = "Go out there and address the people! The rally is waiting.";

        public void TryJoinParty()
        {
            if (PlayerPrefs.GetInt("HasPoliticalParty", 0) == 1)
            {
                Debug.Log(alreadyMemberMessage);
                return;
            }

            // Logic for joining
            JoinParty();
        }

        private void JoinParty()
        {
            PlayerPrefs.SetInt("HasPoliticalParty", 1);
            PlayerPrefs.SetString("PoliticalPartyName", partyName);
            PlayerPrefs.SetFloat("PartyApprovalMultiplier", approvalMultiplier);
            PlayerPrefs.Save();

            // Give money
            if (OfficeBudgetManager.Instance != null)
            {
                OfficeBudgetManager.Instance.personalFunds += joinFinancialBonus;
            }

            Debug.Log(joinSuccessMessage);
        }
    }
}
