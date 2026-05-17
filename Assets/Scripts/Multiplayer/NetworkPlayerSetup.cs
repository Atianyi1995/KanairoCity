using UnityEngine;
using FishNet.Object;
using Invector.vCharacterController;

namespace Kanairo.Multiplayer
{
    public class NetworkPlayerSetup : NetworkBehaviour
    {
        [Header("Invector Components to Disable")]
        public MonoBehaviour[] invectorComponents;
        public GameObject playerCamera;

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetupPlayer();
        }

        private void SetupPlayer()
        {
            if (IsOwner)
            {
                // Ensure camera is enabled and following the local player
                if (playerCamera != null)
                {
                    playerCamera.SetActive(true);
                    var vCam = playerCamera.GetComponent<Invector.vCamera.vThirdPersonCamera>();
                    if (vCam != null) vCam.SetMainTarget(transform);
                }
            }
            else
            {
                // Disable control components for remote players
                foreach (var component in invectorComponents)
                {
                    if (component != null) component.enabled = false;
                }

                // Ensure remote cameras are disabled
                if (playerCamera != null) playerCamera.SetActive(false);
                
                // Disable Rigidbody if necessary (FishNet handles transform sync)
                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
        }
    }
}
