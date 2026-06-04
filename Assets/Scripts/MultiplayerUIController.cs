using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using Kanairo.Core;

namespace KanairoCity.Multiplayer
{
    /// <summary>
    /// Handles the UI interaction for the Multiplayer Lobby.
    /// This script acts as a bridge between the UI and the FishNet networking system.
    /// </summary>
    public class MultiplayerUIController : MonoBehaviour
    {
        [Header("UI Inputs")]
        public TMP_InputField playerNameInput;
        public TMP_InputField roomNameInput; // Used for Client Address in this implementation

        [Header("Status & Lists")]
        public TextMeshProUGUI statusText;
        public RectTransform lobbyListContainer;
        public GameObject roomEntryPrefab;

        [Header("Buttons")]
        public Button createRoomButton; // Acts as "Host"
        public Button joinRandomButton; // Acts as "Join"
        public Button exitButton;

        private NetworkManager _networkManager;
        private Tugboat _tugboat;

        private void Start()
        {
            _networkManager = FindFirstObjectByType<NetworkManager>();
            if (_networkManager != null)
            {
                _tugboat = _networkManager.GetComponent<Tugboat>();
            }

            // Load saved player name if available
            if (playerNameInput != null)
            {
                playerNameInput.text = PlayerPrefs.GetString("Multiplayer_PlayerName", "Player_" + Random.Range(100, 999));
            }

            // Setup button listeners
            if (createRoomButton != null) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            if (joinRandomButton != null) joinRandomButton.onClick.AddListener(OnJoinRandomClicked);
            
            SetStatus("Disconnected");

            if (_networkManager == null)
            {
                SetStatus("Error: NetworkManager not found in scene");
            }
        }

        public void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private void OnCreateRoomClicked()
        {
            if (_networkManager == null) return;

            string playerName = playerNameInput != null ? playerNameInput.text : "Unknown";
            string roomName = roomNameInput != null ? roomNameInput.text : "DefaultRoom";
            
            PlayerPrefs.SetString("Multiplayer_PlayerName", playerName);

            SetStatus($"Starting Host for Room: {roomName}...");
            
            // Start the server and client
            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();

            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartMultiplayerGame();
            }
        }

        private void OnJoinRandomClicked()
        {
            if (_networkManager == null || _tugboat == null) return;

            string playerName = playerNameInput != null ? playerNameInput.text : "Unknown";
            string roomName = roomNameInput != null ? roomNameInput.text : "";

            if (string.IsNullOrEmpty(roomName))
            {
                SetStatus("Error: Enter Room Name to join");
                return;
            }

            PlayerPrefs.SetString("Multiplayer_PlayerName", playerName);
            
            SetStatus($"Joining Room: {roomName}...");

            // In a Room-based system, we connect to the Master Server first
            // For now, we still use Tugboat's address as the "Master Server" location
            _networkManager.ClientManager.StartConnection();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartMultiplayerGame();
            }
        }

        public void UpdateLobbyList(List<string> roomNames)
        {
            if (lobbyListContainer == null) return;

            // Clear existing list
            foreach (Transform child in lobbyListContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate new list (Placeholder for future Lobby/Discovery features)
            foreach (string name in roomNames)
            {
                if (roomEntryPrefab != null)
                {
                    Instantiate(roomEntryPrefab, lobbyListContainer);
                }
            }
        }
    }
}
