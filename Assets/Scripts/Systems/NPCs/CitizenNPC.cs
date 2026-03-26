using UnityEngine;
using UnityEngine.AI;

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
        public NavMeshAgent agent;
        public Animator animator;

        protected virtual void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        protected virtual void Start()
        {
            // Set area mask to prioritize sidewalks and crosswalks.
            if (agent != null)
            {
                // Bitmask for Area 0 (Walkable), 3 (Sidewalk), and 4 (Crosswalk)
                int mask = (1 << 0) | (1 << 3) | (1 << 4);
                agent.areaMask = mask;
            }
        }

        private float wanderTimer = 0f;
        public float wanderInterval = 5f;
        public float wanderRadius = 10f;

        protected virtual void Update()
        {
            UpdateNeeds();
            UpdateState();
            UpdateAnimations();
            
            if (currentState == NPCState.Idle)
            {
                wanderTimer += Time.deltaTime;
                if (wanderTimer >= wanderInterval)
                {
                    Wander();
                    wanderTimer = 0f;
                }
            }
        }

        private void Wander()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, agent.areaMask))
            {
                MoveTo(hit.position);
            }
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

        private void UpdateAnimations()
        {
            if (animator != null && agent != null)
            {
                float speed = agent.velocity.magnitude;
                animator.SetBool("isWalk", speed > 0.1f);
            }
        }

        public virtual void SetState(NPCState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
        }

        protected virtual void HandleIdle() { }
        protected virtual void HandleWalk() 
        {
            if (agent != null && !agent.pathPending && agent.remainingDistance < 0.5f)
                SetState(NPCState.Idle);
        }
        protected virtual void HandleInteract() { }
        protected virtual void HandleFlee() { }
        protected virtual void HandleQueue() { }

        public void MoveTo(Vector3 destination)
        {
            currentDestination = destination;
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(destination);
                SetState(NPCState.Walk);
            }
        }
    }
}
