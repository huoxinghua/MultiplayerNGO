using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerInAirState : PlayerBaseState
    {
        public PlayerInAirState(PlayerStateMachine stateController) : base(stateController)
        {
        }
        private bool _isSprinting; 

        private int _currentSpeedMultiplier => _isSprinting ? 1 : 0;
        private int _dumbMathForSpeedMult => _isSprinting ? 0 : 1;
        public override void OnEnter()
        {
            TryStand();
            _isSprinting = stateController.IsSprintHeld;
        }
        public override void OnExit()
        {
            stateController.VerticalVelocity.y = 0;
        }
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

            characterController.Move(move * playerSO.MoveSpeed * (playerSO.SprintMultiplier * _currentSpeedMultiplier + _dumbMathForSpeedMult) * playerSO.AirSpeedMult* Time.deltaTime);
        }
        public override void StateFixedUpdate()
        {
            
        }
        public override void OnMoveInput(Vector2 movementDirection)
        {
        
        }
        public override void OnSprintInput(bool isPerformed)
        {
            if(!isPerformed)
            {
                _isSprinting = false;
            }
        }
    }
}
