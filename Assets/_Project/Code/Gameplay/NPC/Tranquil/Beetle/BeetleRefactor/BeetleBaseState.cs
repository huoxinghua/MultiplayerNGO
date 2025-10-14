using UnityEngine;
using UnityEngine.AI;

public class BeetleBaseState : BaseState
{
    protected BeetleStateMachine StateController;
    protected BeetleSO BeetleSO;
    protected BeetleAnimation Animator;
    protected NavMeshAgent Agent;
    public BeetleBaseState(BeetleStateMachine stateController)
    {
        StateController = stateController;
        BeetleSO = stateController.BeetleSO;
        Animator = stateController.Animator;
        Agent = stateController.Agent;
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

    public virtual void OnSpotPlayer(bool isHostilePlayer)
    {

    }
    public virtual void OnHitByPlayer()
    {

    }
}
