using UnityEngine;

public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerStateMachine stateController) : base(stateController)
    {
    }
    public override void OnEnter()
    {
        TryStand();
    }
    public override void OnExit()
    {

    }

    public override void StateFixedUpdate()
    {

    }
    //Probably not needed as crouch does not come here. Safety precaution
    void TryStand()
    {
        if (characterController.center == stateController.OriginalCenter) return;
        characterController.height = playerSO.StandHeight;
        characterController.center = stateController.OriginalCenter;
        stateController.TargetCameraHeight = playerSO.StandingCameraHeight;
    }
    public override void StateUpdate()
    {
        Vector3 move = new Vector3(stateController.MoveInput.x, 0f, stateController.MoveInput.y);

        move = stateController.transform.TransformDirection(move);

        characterController.Move(move * playerSO.MoveSpeed * playerSO.SprintMultiplier * Time.deltaTime);
    }
    public override void OnCrouchInput()
    {
        stateController.TransitionTo(stateController.CrouchWalkState);
    }
    public override void OnSprintInput(bool isPerformed)
    {
        if (!isPerformed)
        {
            stateController.TransitionTo(stateController.WalkState);
        }
    }
    public override void OnMoveInput(Vector2 movementDirection)
    {
        if (movementDirection == Vector2.zero)
        {
            stateController.TransitionTo(stateController.IdleState);
        }
    }
}
