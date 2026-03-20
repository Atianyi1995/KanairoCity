using UnityEngine;
using player2_sdk;

namespace KanairoCity.Systems.AI
{
    public interface IPlayer2AI
    {
        void HandleBehavior(GameObject npc, string response);
    }

    /// <summary>
    /// This is a wrapper for AI integration using the Player2 SDK.
    /// It works alongside the Player2Npc component to manage AI-driven behaviors.
    /// </summary>
    [RequireComponent(typeof(player2_sdk.Player2Npc))]
    public class Player2AIWrapper : MonoBehaviour, IPlayer2AI
    {
        [Header("Player2 References")]
        public player2_sdk.Player2Npc p2Npc;
        
        private void Awake()
        {
            if (p2Npc == null) p2Npc = GetComponent<player2_sdk.Player2Npc>();
        }

        public void HandleBehavior(GameObject npc, string response)
        {
            // Simple keyword-based behavior mapping from Player2 AI response
            string lowerResponse = response.ToLower();
            
            // Logic to check if AI convinced NPC to do something
            if (lowerResponse.Contains("fine, i'll buy") || lowerResponse.Contains("okay, i'm getting in"))
            {
                Debug.Log($"{npc.name} was convinced by the Player!");
                // Trigger success logic (e.g., Passenger boards, Customer buys)
            }
            
            if (lowerResponse.Contains("walk away") || lowerResponse.Contains("not interested"))
            {
                Debug.Log($"{npc.name} is walking away.");
            }
        }
    }
}
