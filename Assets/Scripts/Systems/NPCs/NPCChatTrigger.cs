using UnityEngine;
using TMPro;
using player2_sdk;
using Invector.vCharacterController.AI;
using Invector.vCharacterController;
using Invector.vCharacterController.AI.FSMBehaviour;
using Invector;

namespace player2_sdk
{
    public class NPCChatTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float triggerRadius = 4.5f;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private KeyCode chatKey = KeyCode.E;

        [Header("References")]
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private GameObject promptUI;

        private Player2Npc npcComponent;
        private SphereCollider triggerCollider;
        private vControlAI vControl;
        private vFSMBehaviourController fsmController;
        private vAIHeadtrack headtrack;
        private vThirdPersonInput playerInput;
        private Transform playerTransform;
        private bool playerInRange = false;
        private bool isChatting = false;

        // Static flag to prevent multiple NPCs from opening chat simultaneously
        public static bool IsAnyChatActive { get; private set; }

        private void Awake()
        {
            npcComponent = GetComponent<Player2Npc>();
            vControl = GetComponent<vControlAI>();
            fsmController = GetComponent<vFSMBehaviourController>();
            headtrack = GetComponent<vAIHeadtrack>();
            
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerRadius;

            if (chatPanel == null) chatPanel = GameObject.Find("ChatBoxPanel");
            
            if (nameLabel == null)
            {
                var nameObj = GameObject.Find("Player2Canvas/ChatBoxPanel/Name");
                if (nameObj != null) nameLabel = nameObj.GetComponent<TextMeshProUGUI>();
            }

            var playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerInput = playerObj.GetComponent<vThirdPersonInput>();
            }
        }

        private void Update()
        {
            // Safeguard: Block if any chat is already active
            if (playerInRange && !IsAnyChatActive && Input.GetKeyDown(chatKey))
            {
                if (chatPanel != null && !chatPanel.activeSelf)
                {
                    OpenChat();
                }
            }

            if (isChatting)
            {
                if (playerTransform != null)
                {
                    LookAtPlayer();
                    if (headtrack != null)
                    {
                        headtrack.mainLookTarget = playerTransform;
                    }
                }
                
                // Force cursor state every frame to prevent Invector from hiding it
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void LookAtPlayer()
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0; // Keep rotation on the Y axis
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        private void OnTriggerEnter(Collider foreign)
        {
            if (foreign.CompareTag(playerTag))
            {
                playerInRange = true;
                // Only show prompt if no other chat is active
                if (promptUI != null && !IsAnyChatActive) promptUI.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider foreign)
        {
            if (foreign.CompareTag(playerTag))
            {
                playerInRange = false;
                if (promptUI != null) promptUI.SetActive(false);
                
                // Auto-close if the player walks away
                if (isChatting) CloseChat();
            }
        }

        public void OpenChat()
        {
            if (IsAnyChatActive) return;

            if (chatPanel != null)
            {
                chatPanel.SetActive(true);
                isChatting = true;
                IsAnyChatActive = true;
                
                if (promptUI != null) promptUI.SetActive(false);
                
                // Stop NPC movement and AI
                if (vControl != null) vControl.stopMove = true;
                if (fsmController != null) fsmController.isStopped = true;
                
                // Lock player input and put camera behind
                if (playerInput != null)
                {
                    playerInput.SetLockAllInput(true);
                    if (playerInput.tpCamera != null)
                    {
                        playerInput.tpCamera.RotateCamera(playerInput.transform.eulerAngles.y, 0);
                    }
                }

                if (nameLabel != null && npcComponent != null)
                {
                    nameLabel.text = gameObject.name; 
                }
            }
        }

        public void CloseChat()
        {
            if (chatPanel != null)
            {
                chatPanel.SetActive(false);
            }
            
            isChatting = false;
            IsAnyChatActive = false;
            
            // Resume NPC movement and AI
            if (vControl != null) vControl.stopMove = false;
            if (fsmController != null) fsmController.isStopped = false;
            
            // Release Headtrack target
            if (headtrack != null) headtrack.mainLookTarget = null;

            // Unlock player input
            if (playerInput != null)
            {
                playerInput.SetLockAllInput(false);
            }

            // Restore cursor state for Invector 3rd Person Controller
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}