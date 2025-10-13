using UnityEngine;
using UnityEngine.AI;

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

    public void Awake()
    {
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
        TransitionTo(FollowState);
    }
    public void HandleRunFromPlayer(GameObject playerToRunFrom)
    {
        PlayerToRunFrom = playerToRunFrom;
        TransitionTo(RunState);
    }
    public void HandleKnockedOut()
    {

    }
    public void HandleDeath()
    {

    }
    public void HandleHitByPlayer(GameObject player)
    {

    }
}
