using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Art.RagdollScripts;
using _Project.Code.Utilities.StateMachine;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using UnityEngine.AI;

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
        [SerializeField]private BeetleDead BeetleDeadScript;
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
        public void Start()
        {
            TransitionTo(WanderState);
        }
        void Update()
        {
            FollowCooldown.TimerUpdate(Time.deltaTime);
            CurrentState?.StateUpdate();
        }
        void FixedUpdate()
        {
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
            BeetleDeadScript.enabled = true;
            transform.GetChild(1).parent = null;
            _ragdollScript.EnableRagdoll();
            Destroy(transform.GetChild(0).gameObject);
            Destroy(gameObject);
        }
        public void HandleDeath()
        {
            BeetleDeadScript.enabled = true;
            transform.GetChild(1).parent = null;
            _ragdollScript.EnableRagdoll();
            Destroy(transform.GetChild(0).gameObject);
            Destroy(gameObject);
        }
        public void HandleHitByPlayer(GameObject player)
        {
            PlayerToRunFrom = player;
            CurrentState.OnHitByPlayer();
        }
    }
}
