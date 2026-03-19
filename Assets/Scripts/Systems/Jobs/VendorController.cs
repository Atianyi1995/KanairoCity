using UnityEngine;

namespace KanairoCity.Systems.Jobs
{
    public class VendorController : BaseJobController
    {
        [Header("Vendor Config")]
        public float unitPrice = 50f;
        public int currentStock = 100;

        public override void PerformAction()
        {
            if (currentStock <= 0)
            {
                Debug.Log("Out of Stock! Go buy more supplies.");
                return;
            }
            
            Debug.Log($"Preparing product as a {definition.jobName}...");
            // Job logic to be expanded for specific items like Smokies
        }

        public override void HandleInteraction(GameObject target)
        {
            if (currentStock <= 0)
            {
                Debug.Log("Sorry, out of stock!");
                return;
            }

            Debug.Log($"Sold item to {target.name} for {unitPrice}");
            currentStock--;
            // Reward player logic
        }
    }
}
