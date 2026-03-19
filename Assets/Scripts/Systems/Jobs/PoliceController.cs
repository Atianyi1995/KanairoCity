using UnityEngine;

namespace KanairoCity.Systems.Jobs
{
    public class PoliceController : BaseJobController
    {
        [Header("Police Settings")]
        public float patrolRadius = 20f;
        public int currentArrests = 0;

        public override void PerformAction()
        {
            Debug.Log($"Performing Police Patrol as {definition.jobName}...");
        }

        public override void HandleInteraction(GameObject target)
        {
            // Logic to arrest thugs or talk to citizens
            if (target.CompareTag("Thug"))
            {
                Debug.Log($"Arrested {target.name} for causing disturbance!");
                currentArrests++;
            }
        }
    }
}
