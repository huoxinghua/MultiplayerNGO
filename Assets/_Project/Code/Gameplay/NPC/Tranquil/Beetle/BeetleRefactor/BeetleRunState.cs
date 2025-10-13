using UnityEngine;

public class BeetleRunState : BeetleBaseState
{
    public BeetleRunState(BeetleStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        Agent.speed = BeetleSO.RunSpeed;
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
    public override void OnSpotPlayer()
    {
        
    }
    public override void OnHitByPlayer()
    {
       
    }
}
