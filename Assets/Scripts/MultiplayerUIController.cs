using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace KanairoCity.Multiplayer
{
    /// <summary>
    /// Handles the UI interaction for the Multiplayer Lobby.
    /// This script acts as a bridge between the UI and the networking system.
    /// </summary>
    public class MultiplayerUIController : MonoBehaviour
    {
        [Header("UI Inputs")]
        public TMP_InputField playerNameInput;
        public TMP_InputField roomNameInput;

        [Header("Status & Lists")]
        public TextMeshProUGUI statusText;
        public RectTransform lobbyListContainer;
        public GameObject roomEntryPrefab;

        [Header("Buttons")]
        public Button createRoomButton;
        public Button joinRandomButton;
        public Button exitButton;

        private void Start()
        {
            // Load saved player name if available
            if (playerNameInput != null)
            {
                playerNameInput.text = PlayerPrefs.GetString("Multiplayer_PlayerName", "Player_" + Random.Range(100, 999));
            }

            // Setup button listeners
            if (createRoomButton != null) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            if (joinRandomButton != null) joinRandomButton.onClick.AddListener(OnJoinRandomClicked);
            
            SetStatus("Disconnected");
        }

        public void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private void OnCreateRoomClicked()
        {
            string playerName = playerNameInput != null ? playerNameInput.text : "Unknown";
            string roomName = roomNameInput != null ? roomNameInput.text : "";

            if (string.IsNullOrEmpty(roomName))
            {
                SetStatus("Error: Room name cannot be empty");
                return;
            }

            PlayerPrefs.SetString("Multiplayer_PlayerName", playerName);
            SetStatus($"Creating room: {roomName}...");
            
            // Logic to trigger networking 'CreateRoom' goes here
            // Example: NetworkManager.Instance.CreateRoom(roomName);
        }

        private void OnJoinRandomClicked()
        {
            string playerName = playerNameInput != null ? playerNameInput.text : "Unknown";
            PlayerPrefs.SetString("Multiplayer_PlayerName", playerName);
            
            SetStatus("Joining random match...");
            // Logic to trigger networking 'JoinRandom' goes here
        }

        public void UpdateLobbyList(List<string> roomNames)
        {
            // Clear existing list
            foreach (Transform child in lobbyListContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate new list
            foreach (string name in roomNames)
            {
                // Instantiate roomEntryPrefab and set its name/button callback
            }
        }
    }
}
