using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerSprintState : PlayerBaseState
    {
        public PlayerSprintState(PlayerStateMachine stateController) : base(stateController)
        {
        }
        public override void OnEnter()
        {
            stateController.CurrentMovement = PlayerStateMachine.MovementContext.Running;
            TryStand();
            Animator.PlayStanding();
        }
        public override void OnExit()
        {

        }

        public override void StateFixedUpdate()
        {
            stateController.OnSoundMade(playerSO.SprintSoundRange);
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
            base.StateUpdate();
            Vector3 move = new Vector3(stateController.MoveInput.x, 0f, stateController.MoveInput.y);

            move = stateController.transform.TransformDirection(move);
            Animator.PlayRun(1,1);
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
}
