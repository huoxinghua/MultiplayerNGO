using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle.BeetleRefactor.Network
{

public class BeetleFollowState : BeetleBaseState
{
    public BeetleFollowState(BeetleStateMachine stateController) : base(stateController)
    {

    }
    private Timer _followTimer;
    public override void OnEnter()
    {
        StateController.IsFirstFollow = false;
        _followTimer = new Timer(BeetleSO.RandomFollowTime);
        _followTimer.Start();
        Agent.speed = BeetleSO.WalkSpeed;
    }
    public override void OnExit()
    {
        _followTimer?.Stop();
        _followTimer = null;
        StateController.FollowCooldown.Reset(BeetleSO.FollowCooldown);
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
        StateController.TransitionTo(StateController.RunState);
    }
    public override void OnSpotPlayer(bool isHostilePlayer)
    {
        if (isHostilePlayer)
        {
            StateController.TransitionTo(StateController.RunState);
        }
    }
}
}