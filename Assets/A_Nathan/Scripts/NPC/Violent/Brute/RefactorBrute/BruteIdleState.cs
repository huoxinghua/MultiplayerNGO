using UnityEngine;

public class BruteIdleState : BruteBaseState
{
    public BruteIdleState(BruteStateMachine stateController) : base(stateController)
    {

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
    public override void OnHearPlayer()
    {
        //stateController.TransitionTo();
    }
    public override void OnHeartDestroyed()
    {

    }

    //Better way to do this? probably
    public override void OnTimerDone()
    {

    }
}
