using UnityEngine;
using TMPro;
using player2_sdk;
using Invector.vCharacterController.AI;
using Invector.vCharacterController;
using Invector.vCharacterController.AI.FSMBehaviour;
using Invector;
using UnityEngine.UI;

namespace player2_sdk
{
    /// <summary>
    /// Handles player interaction with NPCs. 
    /// Determines the active NPC based on proximity and gaze (forward direction).
    /// </summary>
    public class PlayerInteractionManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float detectionRadius = 5.0f;
        [SerializeField] private float focusAngle = 60f; // Max angle forGazetargeting
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("UI References")]
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private GameObject promptUI;
        [SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;

        private Player2Npc activeNpc;
        private vThirdPersonController playerController;
        private vThirdPersonInput playerInput;
        private Rigidbody playerRigidbody;
        private Animator playerAnimator;
        private vControlAI activeAi;
        private vFSMBehaviourController activeFsm;
        private vAIHeadtrack activeHeadtrack;
        private bool isChatting = false;

        private void Awake()
        {
            playerController = GetComponent<vThirdPersonController>();
            playerInput = GetComponent<vThirdPersonInput>();
            playerRigidbody = GetComponent<Rigidbody>();
            playerAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (chatPanel != null) chatPanel.SetActive(false);
            if (promptUI != null) promptUI.SetActive(false);

            if (sendButton != null) sendButton.onClick.AddListener(() => SubmitMessage(chatInputField.text));
            if (chatInputField != null) chatInputField.onEndEdit.AddListener((msg) => { if (Input.GetKeyDown(KeyCode.Return)) SubmitMessage(msg); });
            if (closeButton != null) closeButton.onClick.AddListener(CloseChat);
        }

        private void Update()
        {
            if (isChatting)
            {
                // Force cursor state
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                // NPC face player
                if (activeNpc != null)
                {
                    LookAtPlayer(activeNpc.transform);
                }

                // Walk away to close
                if (activeNpc != null && Vector3.Distance(transform.position, activeNpc.transform.position) > detectionRadius + 1f)
                {
                    CloseChat();
                }

                return;
            }

            FindBestNpc();

            if (activeNpc != null && Input.GetKeyDown(interactionKey))
            {
                OpenChat();
            }
        }

        private void FindBestNpc()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
            Player2Npc bestNpc = null;
            float bestWeight = float.MinValue;

            foreach (var col in colliders)
            {
                Player2Npc npc = col.GetComponent<Player2Npc>();
                if (npc == null) continue;

                Vector3 dirToNpc = (npc.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dirToNpc);

                if (angle < focusAngle)
                {
                    // Weight = 1/distance + normalized angle weight
                    float dist = Vector3.Distance(transform.position, npc.transform.position);
                    float weight = (1f / (dist + 1f)) + (1f - (angle / focusAngle));

                    if (weight > bestWeight)
                    {
                        bestWeight = weight;
                        bestNpc = npc;
                    }
                }
            }

            if (bestNpc != activeNpc)
            {
                activeNpc = bestNpc;
                if (promptUI != null) promptUI.SetActive(activeNpc != null);
            }
        }

        private void OpenChat()
        {
            if (activeNpc == null) return;

            isChatting = true;
            chatPanel.SetActive(true);
            if (promptUI != null) promptUI.SetActive(false);

            // Update UI Name
            if (nameLabel != null) nameLabel.text = activeNpc.gameObject.name;

            // Stop NPC
            activeAi = activeNpc.GetComponent<vControlAI>();
            activeFsm = activeNpc.GetComponent<vFSMBehaviourController>();
            activeHeadtrack = activeNpc.GetComponent<vAIHeadtrack>();

            if (activeAi != null) activeAi.stopMove = true;
            if (activeFsm != null) activeFsm.isStopped = true;
            if (activeHeadtrack != null) activeHeadtrack.mainLookTarget = transform;

            // Lock Player Movement, Jumping, and Camera
            if (playerController != null)
            {
                playerController.lockMovement = true;
                playerController.lockRotation = true;
                playerController.input = Vector3.zero;
                playerController.moveDirection = Vector3.zero;
                playerController.verticalSpeed = 0;
                playerController.horizontalSpeed = 0;
                playerController.inputMagnitude = 0;
                playerController.ResetInputAnimatorParameters();
            }
            if (playerInput != null)
            {
                playerInput.lockCameraInput = true;
                playerInput.lockMoveInput = true;
                playerInput.SetLockBasicInput(true);
            }

            // Kill momentum
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.isKinematic = true;
            }

            // Clear old text
            if (chatInputField != null) chatInputField.text = "";
            if (activeNpc != null && activeNpc.outputMessage != null) activeNpc.outputMessage.text = "";
            
            // Link STT if possible (needs a central STT manager or similar redirection)
        }

        public void CloseChat()
        {
            isChatting = false;
            chatPanel.SetActive(false);

            // Hide and lock cursor when chat closes
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (activeAi != null) activeAi.stopMove = false;
            if (activeFsm != null) activeFsm.isStopped = false;
            if (activeHeadtrack != null) activeHeadtrack.mainLookTarget = null;

            // Unlock Player Movement, Jumping, and Camera
            if (playerController != null)
            {
                playerController.lockMovement = false;
                playerController.lockRotation = false;
            }
            if (playerInput != null)
            {
                playerInput.lockCameraInput = false;
                playerInput.lockMoveInput = false;
                playerInput.SetLockBasicInput(false);
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;
            }

            activeNpc = null;
        }

        private void SubmitMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || activeNpc == null) return;

            activeNpc.OnChatMessageSubmitted(text);
            if (chatInputField != null) chatInputField.text = "";
        }

        private void LookAtPlayer(Transform npcTransform)
        {
            Vector3 direction = transform.position - npcTransform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                npcTransform.rotation = Quaternion.Slerp(npcTransform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}