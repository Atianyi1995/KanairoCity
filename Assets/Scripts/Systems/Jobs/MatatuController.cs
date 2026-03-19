using UnityEngine;

namespace KanairoCity.Systems.Jobs
{
    public class MatatuController : BaseJobController
    {
        [Header("Matatu Settings")]
        public string currentRoute = "R1";
        public float currentFare = 50f;
        public int currentPassengers = 0;
        public int maxPassengers = 14;

        public override void PerformAction()
        {
            if (currentPassengers >= maxPassengers)
            {
                Debug.Log("Matatu is FULL! Start the journey.");
                return;
            }
            
            Debug.Log($"Shouting for passengers as a {definition.jobName} driver...");
        }

        public override void HandleInteraction(GameObject target)
        {
            if (currentPassengers < maxPassengers)
            {
                Debug.Log($"Passenger {target.name} boarded Matatu.");
                currentPassengers++;
                // Add money logic
            }
        }
    }
}
