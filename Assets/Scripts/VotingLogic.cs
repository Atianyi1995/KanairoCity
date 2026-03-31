using UnityEngine;

using System;
using System.Collections.Generic;

namespace Kanairo.Core
{
    public static class VotingLogic
    {
        public const float PRIMARY_MATCH_BOOST = 25f; 
        public const float SECONDARY_MATCH_BOOST = 15f;
        public const float CONTRADICTION_PENALTY = -5f;
        public const float NEUTRAL_MATCH = 5f;

        /// <summary>
        /// Detects promises within a player's chat message and calculates trust changes.
        /// </summary>
        public static float CalculateTrustFromText(string text, CampaignPromise primaryNeed, CampaignPromise secondaryNeed, CandidateProfile supportedCandidate, bool isPlayer)
        {
            if (string.IsNullOrEmpty(text)) return 0f;

            string lowerText = text.ToLower();
            float totalChange = 0f;
            bool foundPromise = false;

            // Iterate through all promises to see if any keywords match
            foreach (CampaignPromise promise in Enum.GetValues(typeof(CampaignPromise)))
            {
                if (promise == CampaignPromise.None) continue;

                string keyword = promise.ToString().ToLower();
                if (lowerText.Contains(keyword))
                {
                    totalChange += CalculateTrustChange(promise, primaryNeed, secondaryNeed, supportedCandidate, isPlayer);
                    foundPromise = true;
                }
            }

            // Small boost for just talking, even if no keyword matched
            if (!foundPromise) totalChange = NEUTRAL_MATCH;

            return totalChange;
        }

        /// <summary>
        /// Analyzes the AI's response to see if they reacted positively to the player.
        /// Only rewards positive sentiment to ensure progress.
        /// </summary>
        public static float CalculateTrustFromAIResponse(string aiMessage)
        {
            if (string.IsNullOrEmpty(aiMessage)) return 0f;

            string lowerText = aiMessage.ToLower();
            float change = 0f;

            // Positive indicators - expanded list for more reliable detection
            string[] positiveKeywords = { 
                "agree", "great", "thank", "support", "trust", "promise", "deal", 
                "excellent", "wonderful", "happy", "good", "perfect", "definitely",
                "count on", "vote", "support you", "believe you"
            };

            bool isPositive = false;
            foreach (var kw in positiveKeywords)
            {
                if (lowerText.Contains(kw))
                {
                    isPositive = true;
                    break;
                }
            }

            // Removed negative keywords and penalty logic
            if (isPositive) 
            {
                change = 8f; // Increased bonus for positive reaction to ensure clear progress
            }

            return change;
        }

        public static float CalculateTrustChange(CampaignPromise promise, CampaignPromise primaryNeed, CampaignPromise secondaryNeed, CandidateProfile supportedCandidate, bool isPlayer)
        {
            float change = NEUTRAL_MATCH; // Default small boost (5f)
            if (promise == primaryNeed) change = PRIMARY_MATCH_BOOST; // 25f
            else if (promise == secondaryNeed) change = SECONDARY_MATCH_BOOST; // 15f

            // If the NPC already supports the OPPOSITE side, make it harder to change their mind
            if (supportedCandidate != null)
            {
                // Logic check: CandidateProfile should have a way to identify if it belongs to player or rival
                bool switchingSides = (isPlayer && !supportedCandidate.isPlayerControlled) || (!isPlayer && supportedCandidate.isPlayerControlled);
                if (switchingSides)
                {
                    change *= 0.5f; // 50% penalty to persuasion when trying to flip a voter
                }
            }

            // Apply Political Party negotiation boost (20%) for the player
            if (isPlayer && PlayerPrefs.GetInt("HasPoliticalParty", 0) == 1)
            {
                change *= 1.2f;
            }

            Debug.Log($"[LOGIC] Calculating trust for {(isPlayer ? "Player" : "Rival")}. Promise: {promise}. Need: {primaryNeed}. Final Change: {change}");
            return change;
        }
    }
}