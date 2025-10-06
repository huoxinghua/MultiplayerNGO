using System.Threading;
using UnityEngine;

public class BruteIdleState : BruteBaseState
{

    private Timer idleTimer = new Timer(0f);

    public BruteIdleState(BruteStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        animator.PlayNormal();
        agent.ResetPath();
        idleTimer.Reset(bruteSO.RandomIdleTime);

        Debug.Log("Idle");
    }
    public override void OnExit()
    {
        idleTimer.Stop();
    }
   
    public override void StateUpdate()
    {
        idleTimer.TimerUpdate(Time.deltaTime);

        if (!idleTimer.IsDone) return;
      
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
