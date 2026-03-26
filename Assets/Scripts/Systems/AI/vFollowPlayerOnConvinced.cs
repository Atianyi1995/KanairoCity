using UnityEngine;
using Invector.vCharacterController.AI;
using Invector.vCharacterController.AI.FSMBehaviour;

namespace KanairoCity.Systems.AI.FSM
{
    public class vFollowPlayerOnConvinced : vStateAction
    {
        public override string categoryName => "KanairoCity/AI/";
        public override string defaultName => "Follow Player On Convinced";

        [Tooltip("Speed to follow the player")]
        public vAIMovementSpeed speed = vAIMovementSpeed.Running;

        [Tooltip("Stop following when reaching this distance")]
        public float stopDistance = 2f;

        public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour == null || fsmBehaviour.aiController == null) return;

            // Check if our custom Player2AIWrapper marked this NPC as convinced
            var aiWrapper = fsmBehaviour.aiController.gameObject.GetComponent<KanairoCity.Systems.AI.Player2AIWrapper>();
            
            if (aiWrapper != null && aiWrapper.isConvinced && fsmBehaviour.aiController.currentTarget.transform != null)
            {
                // Set the destination to the player (target)
                fsmBehaviour.aiController.MoveTo(fsmBehaviour.aiController.currentTarget.transform.position, speed);

                // If close enough, we could trigger another state or animation
                if (Vector3.Distance(fsmBehaviour.aiController.transform.position, fsmBehaviour.aiController.currentTarget.transform.position) < stopDistance)
                {
                    fsmBehaviour.aiController.Stop();
                }
            }
        }
    }
}
