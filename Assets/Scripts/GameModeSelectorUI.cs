using UnityEngine;
using UnityEngine.UI;
using KanairoCity.Models;
using System;

namespace KanairoCity.UI
{
    /// <summary>
    /// UI component for selecting between Single Player and Multiplayer modes.
    /// </summary>
    public class GameModeSelectorUI : MonoBehaviour
    {
        [Header("Data Profile")]
        [SerializeField] private PlayerProfileData profileData;

        [Header("UI Feedback")]
        [SerializeField] private Button singlePlayerButton;
        [SerializeField] private Button multiplayerButton;

        public event Action OnModeSelected;

        private void Start()
        {
            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.AddListener(() => OnButtonClicked(GameMode.SinglePlayer));
            }

            if (multiplayerButton != null)
            {
                multiplayerButton.onClick.AddListener(() => OnButtonClicked(GameMode.Multiplayer));
            }

            // Set default visuals based on last selection
            UpdateButtonVisuals(profileData.LastSelectedMode);
        }

        private void OnButtonClicked(GameMode mode)
        {
            profileData.LastSelectedMode = mode;
            UpdateButtonVisuals(mode);
            OnModeSelected?.Invoke();
            
            Debug.Log($"Mode selected: {mode}");
        }

        private void UpdateButtonVisuals(GameMode mode)
        {
            if (singlePlayerButton != null)
            {
                singlePlayerButton.interactable = mode != GameMode.SinglePlayer;
            }

            if (multiplayerButton != null)
            {
                multiplayerButton.interactable = mode != GameMode.Multiplayer;
            }
        }
    }
}
