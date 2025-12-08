using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Art.RagdollScripts;
using _Project.Code.Utilities.StateMachine;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle.BeetleRefactor
{
    public class BeetleStateMachine : BaseStateController
    {
        protected BeetleBaseState CurrentState;
        [field: SerializeField] public BeetleSO BeetleSO { get; private set; }
        public BeetleAnimation Animator { get; private set; }
        public NavMeshAgent Agent { get; private set; }

        //states
        public BeetleIdleState IdleState { get; private set; }
        public BeetleWanderState WanderState { get; private set; }
        public BeetleFollowState FollowState { get; private set; }
        public BeetleRunState RunState { get; private set; }

        public GameObject PlayerToRunFrom { get; private set; }
        public GameObject PlayerToFollow { get; private set; }

        public Timer FollowCooldown { get; private set; }
        public bool IsFirstFollow { get; set; }
        [SerializeField] private Ragdoll _ragdollScript;
        [SerializeField] private BeetleDead BeetleDeadScript;
        //Nathan CHanged
        [SerializeField] private BeetleHealth _beetleHealthScript;
        [SerializeField] private BeetleLineOfSight _beetleLineOfSightScript;
        //end
        public void Awake()
        {
            IsFirstFollow = true;
            FollowCooldown = new Timer(BeetleSO.FollowCooldown);
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<BeetleAnimation>();
            IdleState = new BeetleIdleState(this);
            WanderState = new BeetleWanderState(this);
            FollowState = new BeetleFollowState(this);
            RunState = new BeetleRunState(this);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;
            transform.parent = null;
            TransitionTo(WanderState);
        }

        void Update()
        {
            if (!IsServer) return;
            // Debug.Log(CurrentState);
            FollowCooldown.TimerUpdate(Time.deltaTime);
            CurrentState?.StateUpdate();
        }

        void FixedUpdate()
        {
            if (!IsServer) return;
            if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh)
                return;
            CurrentState?.StateFixedUpdate();
        }

        public void TransitionTo(BeetleBaseState newState)
        {
            if (newState == CurrentState) return;
            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }

        public void HandleFollowPlayer(GameObject playerToFollow)
        {
            PlayerToFollow = playerToFollow;
            CurrentState.OnSpotPlayer(false);
        }

        public void HandleRunFromPlayer(GameObject playerToRunFrom)
        {
            PlayerToRunFrom = playerToRunFrom;
            CurrentState.OnSpotPlayer(true);
        }

        public void HandleKnockedOut()
        {
            if (!IsServer)
            {
                RequestKnockedOutServerRPC();
                return;
            }
            else
            {
                ApplyKnockedOut();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestKnockedOutServerRPC()
        {
            ApplyKnockedOut();
        }

        private void ApplyKnockedOut()
        {
            BeetleDeadScript.enabled = true;
           // transform.GetChild(1).parent = null;
            _ragdollScript.EnableRagdoll();
            Destroy(transform.GetChild(0).gameObject);
            DetachRagdollClientRpc();
            PlayRagdollClientRpc();
            //  Destroy(gameObject);
            DisableVisualClientRPC();
        }

        public void HandleDeath()
        {
            // Death and knockout are functionally identical - both permanent.
            // Use knockout path since it doesn't break pickup system.
            HandleKnockedOut();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestHandleDeathServerRPC()
        {
        }

        private void ApplyDeath()
        {
            BeetleDeadScript.enabled = true;
            transform.GetChild(1).parent = null;
            _ragdollScript.EnableRagdoll();
            DetachRagdollClientRpc();
            PlayRagdollClientRpc();
            if(IsServer)
            {
                ApplyDisableVisual();
            }
            else
            {
                DisableVisualClientRPC();
            }
                
            /*Destroy(transform.GetChild(0).gameObject);
            Destroy(gameObject,3f);*/
        }
        [ClientRpc]
        private void DisableVisualClientRPC()
        {
            ApplyDisableVisual(); 
        }
        
        private void ApplyDisableVisual()
        {   
            var mesh = GetComponent<MeshRenderer>();
            if (mesh != null)
                mesh.enabled = false;

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            var health = GetComponent<BeetleHealth>();
            if (health != null)
            {
                health.enabled = false;
            }
            else
            {
            }
      
                
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                try
                {
                    agent.ResetPath();
                    agent.isStopped = true;
                    agent.enabled = false;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BeetleStateMachine] DisableVisualClientRPC NavMeshAgent issue: {e.Message}");
                }
            }
            else if (agent != null && agent.enabled)
            {
                // if not on NavMesh
                agent.enabled = false;
            }


            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;
            
            
            //Nathan Changed
            _beetleHealthScript.enabled = false;    
            _beetleLineOfSightScript.enabled = false;
            this.enabled = false;
            //end
        }

        [ClientRpc]
        private void DetachRagdollClientRpc()
        {

            if (this == null || transform == null)
            {
                Debug.LogWarning("[CLIENT] Beetle already destroyed, skip detach");
                return;
            }

            try
            {
                BeetleDeadScript.enabled = true;
                /*var child = transform.GetChild(1);
                child.parent = null;*/
                DisableVisualClientRPC();
                _ragdollScript.EnableRagdoll();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[CLIENT] Failed to detach ragdoll: {e.Message}");
            }
        }

        [ClientRpc]
        private void PlayRagdollClientRpc()
        {
            DisableVisualClientRPC();
            BeetleDeadScript.enabled = true;
            /*transform.GetChild(1).parent = null;
            _ragdollScript.EnableRagdoll();*/
           
        }

        public void HandleHitByPlayer(GameObject player)
        {
            PlayerToRunFrom = player;
            CurrentState.OnHitByPlayer();
        }
    }
}