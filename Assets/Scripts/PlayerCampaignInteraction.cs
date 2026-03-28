using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Kanairo.Core
{
    public class PlayerCampaignInteraction : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject interactionPrompt;
        public GameObject dialoguePanel;
        public TextMeshProUGUI npcNameText;
        public TextMeshProUGUI npcRoleText;
        public TextMeshProUGUI npcHintText;
        
        [Header("Detection Settings")]
        public float interactRange = 3f;
        public LayerMask npcLayer;

        private NPCVotingProfile currentNPC;

        private void Update()
        {
            DetectNPC();

            if (currentNPC != null && Input.GetKeyDown(KeyCode.E))
            {
                OpenDialogue();
            }
        }

        private void DetectNPC()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, npcLayer);
            if (colliders.Length > 0)
            {
                currentNPC = colliders[0].GetComponent<NPCVotingProfile>();
                interactionPrompt.SetActive(!dialoguePanel.activeSelf);
            }
            else
            {
                currentNPC = null;
                interactionPrompt.SetActive(false);
            }
        }

        private void OpenDialogue()
        {
            if (currentNPC == null) return;

            dialoguePanel.SetActive(true);
            interactionPrompt.SetActive(false);
            
            npcNameText.text = currentNPC.npcName;
            npcRoleText.text = currentNPC.role;
            npcHintText.text = "Hint: They seem concerned about " + currentNPC.primaryNeed.ToString();
            
            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void MakePromise(int promiseIndex)
        {
            CampaignPromise promise = (CampaignPromise)promiseIndex;
            if (currentNPC != null)
            {
                currentNPC.ProcessPlayerPromise(promise);
                CampaignManager.Instance.RefreshApprovalTotals();
            }
            CloseDialogue();
        }

        public void CloseDialogue()
        {
            dialoguePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}