using UnityEngine;
using AiToolbox;
using System.Collections.Generic;
using System;

namespace KanairoCity.Systems.AI
{
    public interface IPlayer2AI
    {
        void ReceivePrompt(string prompt, Action<string> onResponse);
        void HandleBehavior(GameObject npc, string response);
    }

    /// <summary>
    /// This is a wrapper for AI integration using AI Toolbox (ChatGPT).
    /// It provides a clean way to bridge with AI services
    /// without coupling NPC logic directly to the AI SDK.
    /// </summary>
    public class Player2AIWrapper : MonoBehaviour, IPlayer2AI
    {
        [Header("AI Settings")]
        public ChatGptParameters parameters;
        [TextArea] public string baseContext = "You are an NPC in Nairobi. Your tone is local and friendly.";

        private List<Message> _conversationHistory = new List<Message>();

        private void Start()
        {
            if (parameters == null || string.IsNullOrEmpty(parameters.apiKey))
            {
                Debug.LogWarning($"AI Parameters or API Key missing on {gameObject.name}");
            }
        }

        public void ReceivePrompt(string prompt, Action<string> onResponse)
        {
            if (parameters == null) return;

            // Create a temporary parameters object with the specific role/context for this NPC
            ChatGptParameters npcParams = new ChatGptParameters(parameters) { role = baseContext };
            
            _conversationHistory.Add(new Message(prompt, Role.User));

            ChatGpt.Request(_conversationHistory, npcParams, 
                completeCallback: (response) => {
                    _conversationHistory.Add(new Message(response, Role.AI));
                    onResponse?.Invoke(response);
                },
                failureCallback: (code, error) => {
                    Debug.LogError($"AI Request failed: {error} (Code: {code})");
                }
            );
        }

        public void HandleBehavior(GameObject npc, string response)
        {
            // Simple keyword-based behavior mapping from AI response
            string lowerResponse = response.ToLower();
            
            // Example behaviors
            if (lowerResponse.Contains("walk away") || lowerResponse.Contains("goodbye"))
            {
                // Trigger NPC walk state
            }
        }
    }
}
