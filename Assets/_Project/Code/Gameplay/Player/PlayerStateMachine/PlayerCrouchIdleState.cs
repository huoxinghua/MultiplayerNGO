using _Project.Code.Utilities.Singletons;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerCrouchIdleState : PlayerBaseState
    {
        LayerMask standCrouchMask = LayerMasks.Instance.PlayerMask;
        public PlayerCrouchIdleState(PlayerStateMachine stateController) : base(stateController)
        {
        }
        public override void OnEnter()
        {
            stateController.CurrentMovement = PlayerStateMachine.MovementContext.Idle;
            TryCrouch();
            Animator.PlayCrouch();
        }
        public override void OnExit()
        {

        }
        public override void StateUpdate()
        {
            base.StateUpdate();
            Animator.PlayWalk(0,1);
        }
        public override void StateFixedUpdate()
        {

        }
        bool CanStandUp()
        {
            Vector3 RayOrigin = new Vector3(stateController.transform.position.x,stateController.transform.position.y + playerSO.CrouchHeight/2,stateController.transform.position.z);
            return !Physics.Raycast(RayOrigin, Vector3.up, out _,playerSO.StandHeight - playerSO.CrouchHeight, standCrouchMask);
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
}
