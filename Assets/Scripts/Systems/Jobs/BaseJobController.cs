using UnityEngine;

namespace KanairoCity.Systems.Jobs
{
    public abstract class BaseJobController : MonoBehaviour
    {
        [Header("Job Config")]
        public JobDefinition definition;
        public bool isActive;

        protected virtual void Start()
        {
            if (definition == null)
            {
                Debug.LogWarning($"JobDefinition missing on {gameObject.name}");
            }
        }

        public virtual void AcceptJob()
        {
            isActive = true;
            Debug.Log($"Job Started: {definition.jobName}");
        }

        public virtual void QuitJob()
        {
            isActive = false;
            Debug.Log($"Job Quit: {definition.jobName}");
        }

        /// <summary>
        /// Called when the player performs a job-specific action.
        /// </summary>
        public abstract void PerformAction();
        
        /// <summary>
        /// Handles interaction with an NPC in the context of this job.
        /// </summary>
        public abstract void HandleInteraction(GameObject target);
    }
}
