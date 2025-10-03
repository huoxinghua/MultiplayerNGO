using UnityEngine;
using UnityEngine.AI;

public class BruteBaseState : BaseState
{
    protected BruteStateMachine stateController;
    protected BruteAnimation animator;
    protected NavMeshAgent agent;
    protected BruteSO bruteSO;
    public BruteBaseState(BruteStateMachine stateController)
    {
        this.stateController = stateController;
        animator = stateController.animator;
        agent = stateController.agent;
        bruteSO = stateController.BruteSO;
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
}
