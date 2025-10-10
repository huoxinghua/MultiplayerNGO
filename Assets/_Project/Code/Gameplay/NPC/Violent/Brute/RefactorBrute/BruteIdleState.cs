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
        float randomIdleTime = bruteSO.RandomIdleTime;
        Debug.Log($"randomIdletime {randomIdleTime}");
        _idleTimer = new Timer(randomIdleTime);
        
        animator.PlayNormal();
        agent.ResetPath();
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
        animator.PlayWalk(0, 10);
        if (!_idleTimer.IsComplete) return;
      
        stateController.TransitionTo(stateController.wanderState);        
    }
    public override void StateFixedUpdate()
    {

    }

    public override void OnHearPlayer()
    {
        stateController.TransitionTo(stateController.BruteHeardPlayerState);
    }
}
