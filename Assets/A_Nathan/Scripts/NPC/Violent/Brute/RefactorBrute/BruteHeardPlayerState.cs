using UnityEngine;

public class BruteHeardPlayerState : BruteBaseState
{
    public BruteHeardPlayerState(BruteStateMachine stateController) : base(stateController)
    {

    }
    public override void OnEnter()
    {
        stateController.TimesAlerted++; 
        
        // !!!! Need to replace two with a SO variable? yeah, that !!!!
        if(stateController.TimesAlerted >= 2)
        {

        }
        else
        {

        }
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
}
