using UnityEngine;
using UnityEngine.SceneManagement;
using KanairoCity.Models;
using KanairoCity.UI;
using Invector.vCharacterController;
using Invector.vCamera;
using System.Collections;
using Kanairo.Core;

namespace KanairoCity.UI
{
    /// <summary>
    /// Controls the entry and flow of the main menu without deactivating the character model.
    /// </summary>
    public class MenuStartFlowController : MonoBehaviour
    {
        [Header("Data Profile")]
        [SerializeField] private PlayerProfileData profileData;

        [Header("Menu Components")]
        [SerializeField] private MainMenuManager menuManager;
        [SerializeField] private GameModeSelectorUI modeSelector;
        [SerializeField] private ProfessionSelectorUI professionSelector;
        [SerializeField] private CharacterMenuController characterCreator;
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject continueButton;

        [Header("Invector Setup")]
        [SerializeField] private GameObject invectorPlayer; // References vBasicController
        [SerializeField] private GameObject menuCharacter; // References the standalone BSMC_CharacterBase
        [SerializeField] private GameObject invectorHUD;
        [SerializeField] private GameObject invectorInputType;

        [Header("Scene Loading")]
        [SerializeField] private string gameplaySceneName = "GameScene";

        private void Start()
        {
            if (menuManager == null) menuManager = GetComponent<MainMenuManager>();
            
            // Set Initial State: Menu Character Active, Player Inactive
            SetInvectorState(false);

            // Toggle Continue Button Visibility
            if (continueButton != null)
            {
                bool hasSave = PlayerPrefs.HasKey("CampaignTimer");
                continueButton.SetActive(hasSave);
            }

            // Link UI listeners to update play button status
            if (modeSelector != null) modeSelector.OnModeSelected += OnSelectionChanged;
            if (professionSelector != null) professionSelector.OnProfessionUpdated += OnSelectionChanged;
            if (characterCreator != null) characterCreator.OnCharacterSaved += OnSelectionChanged;

            OnSelectionChanged();
        }

        private void Update()
        {
            // Force cursor visibility and unlock every frame during menu
            if (menuCanvas != null && menuCanvas.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnSelectionChanged()
        {
            if (menuManager == null || profileData == null) return;
            bool canPlay = profileData.SelectedProfession != Profession.Unemployed && profileData.IsCharacterCreated;
            menuManager.SetPlayButtonInteractable(canPlay);
        }


        public void StartGame()
        {
            // Close Menu and Deactivate Character Creator
            if (menuCanvas != null) menuCanvas.SetActive(false);
            if (menuManager != null) menuManager.DeactivateCharacterCreator();
            
            // Swap Characters and Enable Gameplay
            SetInvectorState(true);
            
            // Start the Campaign Logic via GameManager to handle fresh start and tutorial delay
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCampaign();
            }

            if (profileData.LastSelectedMode == GameMode.Multiplayer)
            {
                StartMultiplayer();
            }
            else
            {
                StartSinglePlayer();
            }
        }

        public void ContinueGame()
        {
            // Close Menu and Deactivate Character Creator
            if (menuCanvas != null) menuCanvas.SetActive(false);
            if (menuManager != null) menuManager.DeactivateCharacterCreator();
            
            // Swap Characters and Enable Gameplay
            SetInvectorState(true);
            StartCoroutine(ContinueDelay());
        }
        IEnumerator ContinueDelay()
        {
            yield return new WaitForSeconds(0.5f);

            // Continue the Campaign Logic via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueCampaign();
            }

            if (profileData.LastSelectedMode == GameMode.Multiplayer)
            {
                StartMultiplayer();
            }
            else
            {
                StartSinglePlayer();
            }
        }
        private void SetInvectorState(bool enabled)
        {
            // Toggle between Menu Character and Playable Character
            if (menuCharacter != null) menuCharacter.SetActive(!enabled);
            if (invectorPlayer != null) 
            {
                invectorPlayer.SetActive(enabled);
                
                if (enabled)
                {
                    // Reset Invector Controller State when starting
                    var controller = invectorPlayer.GetComponent<vThirdPersonController>();
                    if (controller != null)
                    {
                        controller.isDead = false;
                        controller.ChangeHealth(controller.maxHealth); // Use method instead of property
                        controller.lockMovement = false;
                        controller.lockRotation = false;
                    }

                    var input = invectorPlayer.GetComponent<vThirdPersonInput>();
                    if (input != null)
                    {
                        input.lockInput = false;
                        input.lockMoveInput = false;
                        input.lockCameraInput = false;
                        
                        // Set internal references that might be null
                        if (input.cc == null) input.cc = controller;
                        if (input.tpCamera == null) input.tpCamera = vThirdPersonCamera.instance;
                        
                        // Force camera to re-target the new player
                        if (input.tpCamera != null) input.tpCamera.SetTarget(invectorPlayer.transform);
                        
                        // Ensure input is initialized
                        input.enabled = true;
                    }
                }
            }
            
            // Toggle Logic and HUD
            if (invectorHUD != null) invectorHUD.SetActive(enabled);
            if (invectorInputType != null) 
            {
                invectorInputType.SetActive(enabled);
                // If it's the Invector vInput script, ensure it's re-initializing
                var vInputComp = invectorInputType.GetComponent<vInput>();
                if (enabled && vInputComp != null) vInputComp.enabled = true;
            }
            
            // Force cursor state immediately for menu
            if (!enabled)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            // For enabled = true (Play), the cursor lock is handled by GameManager's 
            // UnlockInputRoutine which includes a 1-second "buffer" delay.
        }

        private void StartSinglePlayer()
        {
            Debug.Log("Starting Single Player Mode...");
        }

        private void StartMultiplayer()
        {
            Debug.Log("Starting Multiplayer Mode...");
        }
    }
}
