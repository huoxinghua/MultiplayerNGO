using UnityEngine;
using UnityEngine.AI;

public class BruteStateMachine : BaseStateController
{
    protected BruteBaseState currentState;
    public BruteIdleState idleState { get; private set; }
    public BruteWanderState wanderState { get; private set; }
    public BruteHurtIdleState bruteHurtIdleState { get; private set; }
    public BruteHurtWander bruteHurtWander { get; private set; }
    public BruteAlertState bruteAlertState { get; private set; }
    public BruteChaseState bruteChaseState { get; private set; }
    public Animator animator { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public BruteSO BruteSO { get; private set; }
    public Transform HeartPosition { get; private set; }
    public GameObject lastHeardPlayer { get; private set; }
    public int TimesAlerted = 0;
    public void Awake()
    {
        idleState = new BruteIdleState(this);
        wanderState = new BruteWanderState(this);
    }
    void Update()
    {
        currentState?.StateUpdate();
    }
    void FixedUpdate()
    {
        currentState?.StateFixedUpdate();
    }
    public void OnHearPlayer(GameObject playerObj)
    {
        lastHeardPlayer = playerObj;
        if (Vector3.Distance(playerObj.transform.position,transform.position) <= BruteSO.InstantAggroDistance)
        {
            TransitionTo(bruteChaseState);
        }
        else
        {
            currentState?.OnHearPlayer();
        }
    }
    public void OnHeartDestroyed()
    {
        TransitionTo(bruteHurtIdleState);
    }
    public virtual void TransitionTo(BruteBaseState newState)
    {
        if (currentState == null || newState == currentState) return;
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}
