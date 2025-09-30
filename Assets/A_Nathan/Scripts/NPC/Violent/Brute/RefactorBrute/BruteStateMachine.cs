using UnityEngine;
using UnityEngine.AI;

public class BruteStateMachine : BaseStateController
{
    protected BruteBaseState currentState;
    public BruteIdleState idleState { get; private set; }
    public BruteWanderState wanderState { get; private set; }
    public Animator animator { get; private set; }
    public NavMeshAgent agent { get; private set; }
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
    public virtual void TransitionTo(BruteBaseState newState)
    {
        if (currentState == null || newState == currentState) return;
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}
