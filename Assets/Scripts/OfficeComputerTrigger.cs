using UnityEngine;

namespace Kanairo.Core
{
    public class OfficeComputerTrigger : MonoBehaviour
    {
        public GameObject computerUIPanel;
        public string interactKey = "e";

        private bool playerInRange;

        public GameObject UIHolder;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                UIHolder.SetActive(true);
                playerInRange = true;
            }
                
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                UIHolder.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                computerUIPanel.SetActive(!computerUIPanel.activeSelf);
            }
        }
    }
}