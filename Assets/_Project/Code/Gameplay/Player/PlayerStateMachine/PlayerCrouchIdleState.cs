using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerCrouchIdleState : PlayerBaseState
{
    public PlayerCrouchIdleState(PlayerStateMachine stateController) : base(stateController)
    {
    }
    public override void OnEnter()
    {
        TryCrouch();
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
    bool CanStandUp()
    {
        float standHeight = playerSO.StandHeight;
        float standCenterY = (standHeight - characterController.skinWidth * 2f) / 2f;
        Vector3 capsuleCenter = stateController.transform.position + new Vector3(0, standCenterY, 0);

        Vector3 bottom = capsuleCenter - Vector3.up * (standHeight / 2f);
        Vector3 top = capsuleCenter + Vector3.up * (standHeight / 2f);

        return !Physics.CheckCapsule(bottom, top, characterController.radius, ~stateController.groundMask, QueryTriggerInteraction.Ignore);
    }   
    void TryCrouch()
    {
        characterController.height = playerSO.CrouchHeight;
        characterController.center = new Vector3(stateController.OriginalCenter.x, (playerSO.CrouchHeight - 2f) * 0.5f, stateController.OriginalCenter.z);
        stateController.TargetCameraHeight = playerSO.CrouchingCameraHeight;
    }
    public override void OnMoveInput(Vector2 movementDirection)
    {
        if (movementDirection == Vector2.zero) return;
        stateController.TransitionTo(stateController.CrouchWalkState);

    }
    public override void OnCrouchInput()
    {
        if (CanStandUp())
        {
            stateController.TransitionTo(stateController.IdleState);
        }
    }
}
