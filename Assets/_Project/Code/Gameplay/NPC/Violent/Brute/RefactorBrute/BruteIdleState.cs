using System.Threading;
using UnityEngine;

public class BruteIdleState : BruteBaseState
{
    Timer idleTimer = new Timer(0f);
    public BruteIdleState(BruteStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        animator.PlayNormal();
        agent.SetDestination(stateController.gameObject.transform.position);
        idleTimer.Reset(Random.Range(bruteSO.MinIdleTime, bruteSO.MaxIdleTime));

        Debug.Log("Idle");
    }
    public override void OnExit()
    {
        idleTimer.Stop();
    }

    public override void StateUpdate()
    {
        idleTimer.Update(Time.deltaTime);
        if (idleTimer.IsRunning)
        {
            stateController.TransitionTo(stateController.wanderState);
        }
    }
    public override void StateFixedUpdate()
    {

    }
    public override void OnHearPlayer()
    {
        stateController.TransitionTo(stateController.BruteHeardPlayerState);
    }
}
