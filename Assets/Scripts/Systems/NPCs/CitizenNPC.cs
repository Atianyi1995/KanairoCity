using UnityEngine;

namespace KanairoCity.Systems.NPCs
{
    public enum NPCState { Idle, Walk, Interact, Flee, Queue }

    public class CitizenNPC : MonoBehaviour
    {
        [Header("NPC Data")]
        public string npcName = "Civilian";
        public float walkSpeed = 2f;
        public float runSpeed = 5f;
        
        [Header("Needs")]
        [Range(0, 100)] public float hunger = 100f;
        [Range(0, 100)] public float patience = 100f;
        [Range(0, 100)] public float votePropensity = 0f;

        [Header("State")]
        public NPCState currentState = NPCState.Idle;
        private Vector3 currentDestination;
        private GameObject currentTarget;
        
        [Header("References")]
        public UnityEngine.AI.NavMeshAgent agent;
        public Animator animator;

        protected virtual void Awake()
        {
            if (agent == null) agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        protected virtual void Update()
        {
            UpdateNeeds();
            UpdateState();
        }

        private void UpdateNeeds()
        {
            hunger -= Time.deltaTime * 0.1f;
            if (currentState == NPCState.Queue) patience -= Time.deltaTime * 1f;
        }

        private void UpdateState()
        {
            switch (currentState)
            {
                case NPCState.Idle:
                    HandleIdle();
                    break;
                case NPCState.Walk:
                    HandleWalk();
                    break;
                case NPCState.Interact:
                    HandleInteract();
                    break;
                case NPCState.Flee:
                    HandleFlee();
                    break;
                case NPCState.Queue:
                    HandleQueue();
                    break;
            }
        }

        public virtual void SetState(NPCState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            
            // Log state change for debugging
            // Debug.Log($"{npcName} state: {newState}");
        }

        protected virtual void HandleIdle() { }
        protected virtual void HandleWalk() 
        {
            if (agent != null && agent.remainingDistance < 0.5f)
                SetState(NPCState.Idle);
        }
        protected virtual void HandleInteract() { }
        protected virtual void HandleFlee() { }
        protected virtual void HandleQueue() { }

        public void MoveTo(Vector3 destination)
        {
            currentDestination = destination;
            if (agent != null)
            {
                agent.SetDestination(destination);
                SetState(NPCState.Walk);
            }
        }
    }
}
