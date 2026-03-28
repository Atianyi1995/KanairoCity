using UnityEngine;

namespace Kanairo.Core
{
    public class TeleportTrigger : MonoBehaviour
    {
        [Header("Settings")]
        public Transform destination;
        public bool faceDestinationRotation = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TeleportPlayer(other.gameObject);
            }
        }

        private void TeleportPlayer(GameObject player)
        {
            if (destination == null) return;

            // CharacterController usually needs to be disabled during teleport
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = destination.position;
            if (faceDestinationRotation)
                player.transform.rotation = destination.rotation;

            if (cc != null) cc.enabled = true;
            
            Debug.Log($"Teleported {player.name} to {destination.name}");
        }
    }
}