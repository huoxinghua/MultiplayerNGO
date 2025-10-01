using UnityEngine;

public class BruteChaseState : BruteBaseState
{
      public BruteChaseState(BruteStateMachine stateController) : base(stateController)
    {
        this.stateController = stateController;
    }
    public override void OnEnter()
    {
        agent.speed = bruteSO.RunSpeed;
    }
    public override void OnExit()
    {
        stateController.TimesAlerted = 0;
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
}
