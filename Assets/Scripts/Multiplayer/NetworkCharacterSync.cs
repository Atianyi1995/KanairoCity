using UnityEngine;
using FishNet.Object;
using Bozo.ModularCharacters;
using System.Collections;

namespace KanairoCity.Multiplayer
{
    /// <summary>
    /// Synchronizes the BoZo Modular Character appearance across the network.
    /// Uses FishNet's ServerRpc and ObserversRpc to broadcast character data.
    /// </summary>
    public class NetworkCharacterSync : NetworkBehaviour
    {
        private OutfitSystem _outfitSystem;
        private bool _isInitialized = false;

        private void Awake()
        {
            _outfitSystem = GetComponentInChildren<OutfitSystem>();
            if (_outfitSystem == null)
            {
                _outfitSystem = GetComponentInParent<OutfitSystem>();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            StartCoroutine(WaitForInitialization());
        }

        private IEnumerator WaitForInitialization()
        {
            // Wait for OutfitSystem to be ready
            while (_outfitSystem == null || !_outfitSystem.initalized)
            {
                if (_outfitSystem == null) _outfitSystem = GetComponentInChildren<OutfitSystem>();
                yield return null;
            }

            _isInitialized = true;

            if (IsOwner)
            {
                // Send local character data to server to broadcast to others
                SendCharacterDataToServer();
            }
        }

        /// <summary>
        /// Collects local character data and sends it to the server.
        /// </summary>
        public void SendCharacterDataToServer()
        {
            if (!IsOwner || !_isInitialized) return;

            CharacterData data = BMAC_SaveSystem.GetCharacterData(_outfitSystem);
            string json = JsonUtility.ToJson(data);
            ServerSetCharacterData(json);
        }

        [ServerRpc]
        private void ServerSetCharacterData(string json)
        {
            // Broadcast to all clients including the one that just joined later
            ObserversSetCharacterData(json);
        }

        [ObserversRpc(BufferLast = true)]
        private void ObserversSetCharacterData(string json)
        {
            // Don't apply to self as it's already set locally
            if (IsOwner) return;

            StartCoroutine(ApplyCharacterData(json));
        }

        private IEnumerator ApplyCharacterData(string json)
        {
            // Ensure local outfit system is ready before applying
            while (_outfitSystem == null || !_outfitSystem.initalized) yield return null;

            CharacterData data = JsonUtility.FromJson<CharacterData>(json);
            
            // BMAC_SaveSystem.LoadCharacter is an async Task in BoZo 2.0
            var task = BMAC_SaveSystem.LoadCharacter(_outfitSystem, data);
            
            // Wait for task completion if needed, though LoadCharacter handles most internal logic
            while (!task.IsCompleted) yield return null;
            
            Debug.Log($"[NetworkCharacterSync] Applied character data for {gameObject.name}");
        }
    }
}
