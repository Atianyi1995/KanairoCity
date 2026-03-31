using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace KanairoCity.UI
{
    /// <summary>
    /// Core manager for navigating between menu panels and managing UI state.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private GameObject modeSelectPanel;
        [SerializeField] private GameObject multiplayerPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Character Creator Integration")]
        [SerializeField] private GameObject characterCreatorRoot; // Added reference for CharacterCreator root
        [SerializeField] private GameObject characterCreatorCanvas;
        [SerializeField] private GameObject characterCreatorCamera;

        [Header("UI Feedback")]
        [SerializeField] private Button playButton;
        
        private GameObject currentActivePanel;
        private List<GameObject> panels = new List<GameObject>();

        private void Awake()
        {
            panels.AddRange(new[] { homePanel, characterPanel, statsPanel, modeSelectPanel, multiplayerPanel, settingsPanel });
            InitializePanels();
        }

        private void InitializePanels()
        {
            foreach (var panel in panels)
            {
                if (panel != null) panel.SetActive(false);
            }
            
            // Set Character Creator Camera as default for menu
            if (characterCreatorCamera != null) characterCreatorCamera.SetActive(true);
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);

            // Hide Multiplayer panel specifically if not in list
            if (multiplayerPanel != null) multiplayerPanel.SetActive(false);

            // Start with Home Panel
            ShowPanel(homePanel);
        }

        public void ShowMultiplayer()
        {
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);
            ShowPanel(multiplayerPanel);
        }

        public void ShowHome()
        {
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);
            // characterCreatorRoot remains active to keep the character visible in the background
            ShowPanel(homePanel);
        }

        public void ShowCharacterCreator()
        {
            if (characterCreatorRoot != null) characterCreatorRoot.SetActive(true);
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(true);
            ShowPanel(characterPanel);
        }

        public void ShowStats()
        {
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);
            // characterCreatorRoot remains active to keep the character visible in the background
            ShowPanel(statsPanel);
        }

        public void ShowModeSelect()
        {
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);
            // characterCreatorRoot remains active to keep the character visible in the background
            ShowPanel(modeSelectPanel);
        }

        public void ShowSettings()
        {
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);
            if (characterCreatorRoot != null) characterCreatorRoot.SetActive(false);
            ShowPanel(settingsPanel);
        }

        public void DeactivateCharacterCreator()
        {
            if (characterCreatorCanvas != null) characterCreatorCanvas.SetActive(false);
            if (characterCreatorRoot != null) characterCreatorRoot.SetActive(false);
            if (characterCreatorCamera != null) characterCreatorCamera.SetActive(false);
        }

        private void ShowPanel(GameObject targetPanel)
        {
            if (targetPanel == null) return;

            if (currentActivePanel != null)
            {
                currentActivePanel.SetActive(false);
            }

            targetPanel.SetActive(true);
            currentActivePanel = targetPanel;
        }

        public void SetPlayButtonInteractable(bool interactable)
        {
            if (playButton != null)
            {
                playButton.interactable = interactable;
            }
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
