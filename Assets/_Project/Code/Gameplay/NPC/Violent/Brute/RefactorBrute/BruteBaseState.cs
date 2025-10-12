using UnityEngine;
using UnityEngine.AI;

public class BruteBaseState : BaseState
{
    protected BruteStateMachine StateController;
    protected BruteAnimation Animator;
    protected NavMeshAgent Agent;
    protected BruteSO BruteSO;
    public BruteBaseState(BruteStateMachine stateController)
    {
        this.StateController = stateController;
        Animator = stateController.Animator;
        Agent = stateController.agent;
        BruteSO = stateController.BruteSO;
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
