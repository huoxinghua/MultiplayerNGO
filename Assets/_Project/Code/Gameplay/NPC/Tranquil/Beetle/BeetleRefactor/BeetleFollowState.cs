using UnityEngine;
using UnityEngine.InputSystem.Android;

public class BeetleFollowState : BeetleBaseState
{
    public BeetleFollowState(BeetleStateMachine stateController) : base(stateController)
    {

    }
    private Timer _followTimer;
    public override void OnEnter()
    {
        _followTimer = new Timer(BeetleSO.RandomFollowTime);
        _followTimer.Start();
        Agent.speed = BeetleSO.WalkSpeed;
    }
    public override void OnExit()
    {
        _followTimer?.Stop();
        _followTimer = null;
    }

    public override void StateUpdate()
    {
        _followTimer.TimerUpdate(Time.deltaTime);
    }
    public override void StateFixedUpdate()
    {
        Animator.PlayWalk(Agent.velocity.magnitude, Agent.speed);
        Agent.SetDestination(StateController.PlayerToFollow.transform.position);
        if (_followTimer.IsComplete)
        {
            StateController.TransitionTo(StateController.IdleState);
        }
    }
    public override void OnHitByPlayer()
    {
        
    }
    public override void OnSpotPlayer()
    {

    }
}
