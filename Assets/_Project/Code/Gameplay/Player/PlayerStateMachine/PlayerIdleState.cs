using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerStateMachine stateController) : base(stateController)
        {
        }
        public override void OnEnter()
        {
            Animator.PlayStanding();

            TryStand();
            if(stateController.MoveInput != Vector2.zero)
            {
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
        public override void OnExit()
        {

        }
        public override void StateUpdate()
        {
            base.StateUpdate();
        }
        public override void StateFixedUpdate()
        {
            Animator.PlayWalk(0,1);
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
}
