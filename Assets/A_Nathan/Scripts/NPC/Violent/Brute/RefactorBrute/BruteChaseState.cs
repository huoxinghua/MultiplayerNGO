using UnityEngine;

public class BruteChaseState : BruteBaseState
{
      public BruteChaseState(BruteStateMachine stateController) : base(stateController)
    {
        this.stateController = stateController;
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

    }
    public override void OnHeartDestroyed()
    {

    }

    //Better way to do this? probably
    public override void OnTimerDone()
    {

    }
}
