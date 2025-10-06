using UnityEngine;

public class PlayerStateMachine : BaseStateController
{
    protected PlayerBaseState currentState;

    public PlayerIdleState idleState { get; private set; }
    public PlayerWalkState walkState { get; private set; }
    public PlayerSprintState sprintState { get; private set; }
    public PlayerCrouchIdleState crouchIdleState { get; private set; }
    public PlayerCrouchWalkState crouchWalkState { get; private set; }
    public PlayerInAirState inAirState { get; private set; }
    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        walkState = new PlayerWalkState(this);
        sprintState = new PlayerSprintState(this);
        crouchIdleState = new PlayerCrouchIdleState(this);
        crouchWalkState = new PlayerCrouchWalkState(this);
        inAirState = new PlayerInAirState(this);
    }
    public void Start()
    {
        TransitionTo(idleState);
    }

    public virtual void TransitionTo(PlayerBaseState newState)
    {
        if (newState == currentState) return;
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    void Update()
    {
        currentState?.StateUpdate();
    }
    void FixedUpdate()
    {
        currentState?.StateFixedUpdate();
    }
}
