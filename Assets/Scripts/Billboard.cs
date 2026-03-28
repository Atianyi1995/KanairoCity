using UnityEngine;

namespace Kanairo.UI
{
    public class Billboard : MonoBehaviour
    {
        private Transform camTransform;

        private void Start()
        {
            if (Camera.main != null)
                camTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (camTransform == null)
            {
                if (Camera.main != null) camTransform = Camera.main.transform;
                return;
            }

            // Standard billboard: face the camera but keep upright (Y-axis only)
            Vector3 targetPosition = new Vector3(camTransform.position.x, transform.position.y, camTransform.position.z);
            transform.LookAt(targetPosition);
            
            // Flip the rotation 180 degrees because UI text/images often face "backwards" by default
            transform.Rotate(0, 180, 0);
        }
    }
}