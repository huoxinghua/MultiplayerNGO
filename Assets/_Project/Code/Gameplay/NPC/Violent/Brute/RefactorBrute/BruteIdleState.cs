using System.Data.Common;
using System.Threading;
using UnityEngine;

public class BruteIdleState : BruteBaseState
{

    private Timer _idleTimer;

    public BruteIdleState(BruteStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        float randomIdleTime = BruteSO.RandomIdleTime;
        _idleTimer = new Timer(randomIdleTime);
        
        Animator.PlayNormal();
        Agent.ResetPath();
        _idleTimer.Start();

        Debug.Log("Idle");
    }
    public override void OnExit()
    {
        _idleTimer.Stop();
        _idleTimer = null;
    }
   
    public override void StateUpdate()
    {
        _idleTimer.TimerUpdate(Time.deltaTime);
        Animator.PlayWalk(0, 10);
        if (!_idleTimer.IsComplete) return;
      
        StateController.TransitionTo(StateController.WanderState);        
    }
    public override void StateFixedUpdate()
    {

    }

    public override void OnHearPlayer()
    {
        StateController.TransitionTo(StateController.BruteHeardPlayerState);
    }
}
