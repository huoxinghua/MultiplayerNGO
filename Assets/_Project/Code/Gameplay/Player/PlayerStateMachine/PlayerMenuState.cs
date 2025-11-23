using _Project.Code.Gameplay.Player.PlayerStateMachine;
using UnityEngine;
namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerMenuState : PlayerBaseState
    {
        public PlayerMenuState(PlayerStateMachine stateController) : base(stateController) { }

        public override void OnEnter()
        {
            //Animator.PlayWalk(0,1);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        public override void OnExit()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        #region Input Blocking
        public override void OnUseInput() { }
        public override void OnSecondaryUseInput(bool isPressed) { }
        public override void OnDropItemInput() { }
        public override void OnInteractInput() { }
        public override void OnNumPressedInput(int slot) { }
        public override void OnChangeWeaponInput() { }
        #endregion
    }
}