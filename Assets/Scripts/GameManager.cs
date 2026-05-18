using UnityEngine;
using System.Collections;

namespace Kanairo.Core
{
    public enum GameState
    {
        MainMenu,
        Tutorial,
        Campaigning,
        ElectionResult,
        Victory,
        Defeat,
        OpponentTerm,
        InOffice
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private GameState initialState = GameState.MainMenu;

        [Header("UI References")]
        public GameObject tutorialCanvas;
        public GameObject campaignCanvas;
        public GameObject resultCanvas;
        public GameObject officeCanvas;

        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            ChangeState(initialState);
        }

        private void Update()
        {
            if (CurrentState == GameState.Tutorial)
            {
                if (Cursor.visible == false || Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }

        private IEnumerator DelayedTutorialStart()
        {
            // Initial delay as requested
            yield return new WaitForSeconds(2.0f);
            
            // Handle UI Visibility
            if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
            
            // Handle input lock
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LockPlayerInput(true);
        }

        public void ChangeState(GameState newState)
        {
            // First stop any running unlock or delay coroutines to prevent conflicts
            StopAllCoroutines();
            
            CurrentState = newState;
            Debug.Log($"Game State Changed to: {newState}");
            
            // Handle UI Visibility (Tutorial handled via coroutine if state is Tutorial)
            if (tutorialCanvas != null && newState != GameState.Tutorial) tutorialCanvas.SetActive(false);
            
            // Campaign UI stays active during all gameplay phases
            if (campaignCanvas != null) 
            {
                campaignCanvas.SetActive(newState == GameState.Campaigning || 
                                       newState == GameState.OpponentTerm || 
                                       newState == GameState.InOffice || newState == GameState.Victory ||
                                     newState == GameState.Defeat);
                Debug.Log(newState);
            }

            if (resultCanvas != null) 
            {
                resultCanvas.SetActive(newState == GameState.Victory || 
                                     newState == GameState.Defeat);
            }

            if (officeCanvas != null) officeCanvas.SetActive(newState == GameState.InOffice);
            
            // Handle time scale, input, and cursor
            if (newState == GameState.Tutorial)
            {
                StartCoroutine(DelayedTutorialStart());
            }
            else if (newState == GameState.MainMenu || newState == GameState.ElectionResult || newState == GameState.Victory || newState == GameState.Defeat)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                LockPlayerInput(true);
            }
            else
            {
                // States like Campaigning, OpponentTerm, and InOffice should unlock input
                StartCoroutine(UnlockInputRoutine());
            }

            // Handle state entry logic here if needed
            switch (newState)
            {
                case GameState.OpponentTerm:
                case GameState.InOffice:
                    if (CampaignManager.Instance != null) CampaignManager.Instance.StartPostElectionTerm(newState == GameState.InOffice);
                    break;
            }
        }

        private void LockPlayerInput(bool locked)
        {
            // Find all vThirdPersonInput components in the scene
            var inputs = Object.FindObjectsByType<Invector.vCharacterController.vThirdPersonInput>(FindObjectsSortMode.None);
            
            foreach (var input in inputs)
            {
                // Invector standard lock
                input.SetLockAllInput(locked);
                
                // Explicitly set these as well to be sure
                input.lockInput = locked;
                input.lockMoveInput = locked;
                input.lockCameraInput = locked;

                var camera = Invector.vCamera.vThirdPersonCamera.instance;
                if (camera != null)
                {
                    if (locked)
                    {
                        camera.isFreezed = true;
                        input.SetLockCameraInput(true);
                        // Reset camera rotation to face the same direction as the player
                        camera.RotateCamera(input.transform.eulerAngles.y, 0);
                    }
                    else
                    {
                        camera.isFreezed = false;
                        camera.isInit = true;
                        camera.SetTarget(input.transform);
                        input.SetLockCameraInput(false);
                        input.tpCamera = camera;
                        
                        // Force internal state re-init
                        camera.enabled = false;
                        camera.enabled = true;
                    }
                }
            }

            // Also ensure cursor state matches the input lock
            if (!locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        private IEnumerator UnlockInputRoutine()
        {
            // First frame: Force cursor and stay locked
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LockPlayerInput(true);
            
            // Brief buffer delay to let the player get ready
            //yield return new WaitForSecondsRealtime(2.0f);
            
            // Now unlock player input
            LockPlayerInput(false);
            
            // Re-find all player-related components
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var input = player.GetComponent<Invector.vCharacterController.vThirdPersonInput>();
                var camera = Invector.vCamera.vThirdPersonCamera.instance;

                // Ensure cursor is locked for Invector to start processing mouse delta
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                yield return new WaitForEndOfFrame();

                if (input != null)
                {
                    input.SetLockAllInput(false);
                    input.SetLockCameraInput(false);
                    input.lockInput = false;
                    input.lockCameraInput = false;
                    input.lockMoveInput = false;

                    // Ensure Invector's internal input state is refreshed
                    if (camera != null)
                    {
                        input.tpCamera = camera;
                        camera.SetTarget(player.transform);
                    }
                }

                if (camera != null)
                {
                    camera.isFreezed = false;
                    camera.isInit = true;
                    camera.gameObject.SetActive(false);
                    camera.gameObject.SetActive(true);
                }
            }
        }

        public void SetCampaign()
        {
            // Clear all saved data for a fresh start
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            // Start the actual CampaignManager reset
            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.StartCampaign(true);
            }
            
            // ChangeState is already handled inside StartCampaign(true)
        }

        public void ContinueCampaign()
        {
            if (PlayerPrefs.HasKey("CampaignTimer") || PlayerPrefs.HasKey("Tot"))
            {
                // Start the actual CampaignManager with existing data
                if (CampaignManager.Instance != null)
                {
                    CampaignManager.Instance.StartCampaign(false);
                }
                
                // ChangeState is already handled inside StartCampaign(false) 
                // to correctly restore Campaigning, OpponentTerm, or InOffice
            }
            else
            {
                // If no save exists, treat it as a new start
                SetCampaign();
            }
        }

        public void FinishTutorial()
        {
            SetTut();
            ChangeState(GameState.Campaigning);
        }

        public void SetTut()
        {
            PlayerPrefs.SetInt("Tot", 1);
        }

        public void StartMultiplayerGame()
        {
            // Set tutorial as finished for multiplayer to skip tutorial canvas
            SetTut();
            
            // Start campaign logic (handles NPC loading and initial percentages)
            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.StartCampaign(false);
            }

            // Immediately change state to Campaigning to hide menu and enable player control
            ChangeState(GameState.Campaigning);
        }
    }
}