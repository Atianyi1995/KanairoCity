using UnityEngine;

namespace KanairoCity.Systems.Traffic
{
    public class SimpleCarController : MonoBehaviour
    {
        public TrafficWaypoints route;
        public float speed = 10f;
        public float rotationSpeed = 5f;
        public float stopDistance = 3f;
        public float detectionRange = 5f;
        public LayerMask obstacleLayer;

        private int currentWaypointIndex = 0;
        private bool isStopped = false;

        private void Update()
        {
            if (route == null || route.waypoints.Count == 0) return;

            HandleDetection();
            if (isStopped) return;

            Move();
        }

        private void HandleDetection()
        {
            // Cast a ray or box forward to detect NPCs at crosswalks
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, detectionRange, obstacleLayer))
            {
                isStopped = true;
                return;
            }
            isStopped = false;
        }

        private void Move()
        {
            Transform target = route.waypoints[currentWaypointIndex];
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < stopDistance)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % route.waypoints.Count;
            }
        }
    }
}
