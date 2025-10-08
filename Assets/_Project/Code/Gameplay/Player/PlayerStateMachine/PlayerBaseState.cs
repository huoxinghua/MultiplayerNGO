using UnityEngine;
using UnityEngine.AI;

public class PlayerBaseState : BaseState
{
    protected PlayerStateMachine stateController;
    protected PlayerSO playerSO;
    protected CharacterController characterController;
    protected bool _jumpRequested = false;

    public PlayerBaseState(PlayerStateMachine stateController)
    {
        this.stateController = stateController;
        playerSO = stateController.PlayerSO;
        characterController = stateController.CharacterController;
    }
    public override void OnEnter()
    {
        //subscribe to input events
    }
    public override void OnExit()
    {
        //unsubscribe
    }

    public override void StateFixedUpdate()
    {
        
    }

    public override void StateUpdate()
    {
        HandleJump();
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

    public virtual void OnJumpInput(bool isPerformed)
    {
        stateController.JumpRequested = isPerformed;
        
    }

    public virtual void HandleJump()
    {

        if (stateController.CanJump && stateController.JumpRequested)
        {
            if (stateController.VerticalVelocity.y < 0f)
                stateController.VerticalVelocity.y = -2f;
            stateController.VerticalVelocity.y = Mathf.Sqrt(playerSO.JumpStrength * -2f * playerSO.PlayerGravity);
        }
        stateController.VerticalVelocity.y += playerSO.PlayerGravity * Time.deltaTime;
        characterController.Move(stateController.VerticalVelocity * Time.deltaTime);


    }
}
