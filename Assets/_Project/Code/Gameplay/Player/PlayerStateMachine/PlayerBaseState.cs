using UnityEngine;
using UnityEngine.AI;

public class PlayerBaseState : BaseState
{
    protected PlayerStateMachine stateController;
    protected PlayerSO playerSO;
    protected CharacterController characterController;
    public PlayerBaseState(PlayerStateMachine stateController)
    {
        this.stateController = stateController;
        playerSO = stateController.PlayerSO;
        characterController = stateController.CharacterController;
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
    public virtual void OnSprintInput(bool isPerformed)
    {

    }
    public virtual void OnMoveInput(Vector2 movementDirection)
    {

    }
}
