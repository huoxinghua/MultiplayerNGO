using UnityEngine;
using UnityEngine.AI;

public class PlayerBaseState : BaseState
{
    protected PlayerStateMachine stateController;

    public PlayerBaseState(PlayerStateMachine stateController)
    {
        this.stateController = stateController;

    }
    public override void OnEnter()
    {
        
    }
    public override void OnExit()
    {
        
    }

    public override void StateFixedUpdate()
    {
        
    }

    public override void StateUpdate()
    {

    }
    public virtual void OnCrouchInput()
    {

    }
    public virtual void OnSprintInput()
    {

    }
    public virtual void OnMoveInput()
    {

    }
}
