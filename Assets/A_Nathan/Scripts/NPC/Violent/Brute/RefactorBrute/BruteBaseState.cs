using UnityEngine;
using UnityEngine.AI;

public class BruteBaseState : BaseState
{
    protected BruteStateMachine stateController;
    protected Animator animator;
    protected NavMeshAgent agent;
    public BruteBaseState(BruteStateMachine stateController)
    {
        this.stateController = stateController;
        animator = stateController.animator;
        agent = stateController.agent;
    }
    public override void OnEnter()
    {

    }
    public override void OnExit()
    {

    }

    public override void StateUpdate()
    {

    }
    public override void StateFixedUpdate()
    {

    }
    public virtual void OnHearPlayer()
    {

    }
    public virtual void OnHeartDestroyed()
    {

    }
    //figure out how to handle this? Might need multiple timers eventually.
    public virtual void OnTimerDone()
    {

    }
}
