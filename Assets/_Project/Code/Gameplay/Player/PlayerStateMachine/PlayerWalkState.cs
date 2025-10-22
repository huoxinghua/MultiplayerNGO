using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerWalkState : PlayerBaseState
    {
        public PlayerWalkState(PlayerStateMachine stateController) : base(stateController)
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
            stateController.OnSoundMade(playerSO.WalkSoundRange);
        }
        public override void StateUpdate()
        {
            Vector3 move = new Vector3(stateController.MoveInput.x, 0f, stateController.MoveInput.y);

            move = stateController.transform.TransformDirection(move);

            characterController.Move(move * playerSO.MoveSpeed * Time.deltaTime);
            base.StateUpdate();
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
            stateController.TransitionTo(stateController.CrouchWalkState);
        }
        public override void OnMoveInput(Vector2 moveInput)
        {
            if (moveInput == Vector2.zero)
            {
                stateController.TransitionTo(stateController.IdleState);
            }
        }
        public override void OnSprintInput(bool isPerformed)
        {
            if (isPerformed)
            {
                stateController.TransitionTo(stateController.SprintState);
            }
        }
    }
}
