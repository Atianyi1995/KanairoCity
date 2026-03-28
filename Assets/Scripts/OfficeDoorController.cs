using UnityEngine;

namespace Kanairo.Core
{
    /// <summary>
    /// Handles opening and closing office doors via triggers.
    /// Can be placed on multiple triggers (inside/outside) to control a single door.
    /// </summary>
    public class OfficeDoorController : MonoBehaviour
    {
        [Header("Door Settings")]
        [SerializeField] private Transform doorTransform;
        [SerializeField] private float openRotation = -90f;
        [SerializeField] private float closeRotation = 0f;
        [SerializeField] private float smoothSpeed = 5f;
        
        [Header("Interaction")]
        [SerializeField] private bool autoOpen = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        private bool isPlayerInRange = false;
        private bool isOpen = false;
        private Quaternion targetRotation;

        private void Start()
        {
            if (doorTransform == null)
            {
                doorTransform = transform; // Fallback to current object
            }
            targetRotation = Quaternion.Euler(0, closeRotation, 0);
        }

        private void Update()
        {
            // Handle Manual Interaction
            if (!autoOpen && isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                ToggleDoor();
            }

            // Smoothly Rotate Door
            doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                if (autoOpen)
                {
                    OpenDoor();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                if (autoOpen)
                {
                    CloseDoor();
                }
            }
        }

        public void ToggleDoor()
        {
            if (isOpen) CloseDoor();
            else OpenDoor();
        }

        public void OpenDoor()
        {
            isOpen = true;
            targetRotation = Quaternion.Euler(0, openRotation, 0);
        }

        public void CloseDoor()
        {
            isOpen = false;
            targetRotation = Quaternion.Euler(0, closeRotation, 0);
        }
    }
}
