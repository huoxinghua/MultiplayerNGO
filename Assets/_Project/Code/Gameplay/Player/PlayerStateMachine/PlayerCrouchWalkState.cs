using _Project.Code.Utilities.Singletons;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerCrouchWalkState : PlayerBaseState
    {
        LayerMask standCrouchMask = LayerMasks.Instance.PlayerMask;
        public PlayerCrouchWalkState(PlayerStateMachine stateController) : base(stateController)
        {
        }
        public override void OnEnter()
        {
            TryCrouch();
            Animator.PlayCrouch();
        }
        public override void OnExit()
        {

        }
        public override void StateUpdate()
        {
            base.StateUpdate();
            Vector3 move = new Vector3(stateController.MoveInput.x, 0f, stateController.MoveInput.y);
            Animator.PlayWalk(1,1);
            move = stateController.transform.TransformDirection(move);

            characterController.Move(move * playerSO.MoveSpeed * playerSO.CrouchMoveMultiplier * Time.deltaTime);
        }
        public override void StateFixedUpdate()
        {
            stateController.OnSoundMade(playerSO.CrouchSoundRange);
        }
        bool CanStandUp()
        {
            Vector3 RayOrigin = new Vector3(stateController.transform.position.x, stateController.transform.position.y + playerSO.CrouchHeight / 2, stateController.transform.position.z);
            return !Physics.Raycast(RayOrigin, Vector3.up, out _, playerSO.StandHeight - playerSO.CrouchHeight, standCrouchMask);
        }
        void TryCrouch()
        {
            characterController.height = playerSO.CrouchHeight;
            characterController.center = new Vector3(stateController.OriginalCenter.x, (playerSO.CrouchHeight - 2f) * 0.5f, stateController.OriginalCenter.z);
            stateController.TargetCameraHeight = playerSO.CrouchingCameraHeight;
        }
        public override void OnCrouchInput()
        {
            if (CanStandUp())
            {
                stateController.TransitionTo(stateController.WalkState);
            }
        }
        public override void OnMoveInput(Vector2 movementDirection)
        {
            if (movementDirection == Vector2.zero) stateController.TransitionTo(stateController.CrouchIdleState);
        }
    }
}
