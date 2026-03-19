using UnityEngine;
using System;
using System.Collections.Generic;

namespace KanairoCity.Systems.Events
{
    public enum GameEventType
    {
        CustomerRequest,
        PoliceIncident,
        MatatuStop,
        VoteInteraction,
        Disturbance,
        ThugAttack
    }

    public static class GameEventManager
    {
        private static Dictionary<GameEventType, Action<GameObject, string>> eventCallbacks = new Dictionary<GameEventType, Action<GameObject, string>>();

        public static void Subscribe(GameEventType eventType, Action<GameObject, string> callback)
        {
            if (!eventCallbacks.ContainsKey(eventType))
                eventCallbacks[eventType] = null;
            eventCallbacks[eventType] += callback;
        }

        public static void Unsubscribe(GameEventType eventType, Action<GameObject, string> callback)
        {
            if (eventCallbacks.ContainsKey(eventType))
                eventCallbacks[eventType] -= callback;
        }

        public static void TriggerEvent(GameEventType eventType, GameObject initiator, string data = "")
        {
            if (eventCallbacks.ContainsKey(eventType))
                eventCallbacks[eventType]?.Invoke(initiator, data);
        }
    }
}
