using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine stateController) : base(stateController)
    {
    }
    public override void OnEnter()
    {
        TryStand();
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
    void TryStand()
    {
        if (characterController.center == stateController.OriginalCenter) return;
        characterController.height = playerSO.StandHeight;
        characterController.center = stateController.OriginalCenter;
        stateController.TargetCameraHeight = playerSO.StandingCameraHeight;
    }

    public override void OnCrouchInput()
    {
        stateController.TransitionTo(stateController.CrouchIdleState);
    }

    public override void OnMoveInput(Vector2 movementDirection)
    {
        if (movementDirection == Vector2.zero) return;
        if (stateController.IsSprintHeld)
        {
            stateController.TransitionTo(stateController.SprintState);
        }
        else
        {
            stateController.TransitionTo(stateController.WalkState);
        }
    }
}
