using UnityEngine;

namespace Kanairo.Core
{
    /// <summary>
    /// Handles teleporting the player between two points (e.g., in and out of the office).
    /// </summary>
    public class OfficeTeleportTrigger : MonoBehaviour
    {
        [Header("Teleport Settings")]
        [SerializeField] private Transform destination;
        [SerializeField] private bool requireButtonPress = true;
        [SerializeField] private KeyCode teleportKey = KeyCode.E;

        private bool isPlayerInRange = false;

        private void Update()
        {
            if (requireButtonPress && isPlayerInRange && Input.GetKeyDown(teleportKey))
            {
                TeleportPlayer();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                if (!requireButtonPress)
                {
                    TeleportPlayer();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
            }
        }

        private void TeleportPlayer()
        {
            if (destination == null)
            {
                Debug.LogWarning($"[Teleport] Destination not set on {gameObject.name}");
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // If the player has a CharacterController, we must disable it briefly to move the transform
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.transform.position = destination.position;
                player.transform.rotation = destination.rotation;

                if (cc != null) cc.enabled = true;

                Debug.Log($"[Teleport] Player moved to {destination.name}");
            }
        }
    }
}
