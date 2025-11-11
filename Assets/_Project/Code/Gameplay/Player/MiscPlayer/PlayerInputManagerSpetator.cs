using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Gameplay.Player.MiscPlayer
{
    public class PlayerInputManagerSpectator : MonoBehaviour
    {
        public InputSystem_Actions inputActions;

        public event Action<Vector2> OnSpectatorLookInput;
        public event Action OnNext;
        public event Action OnPrev;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }

        public void EnableSpectatorInput()
        {
            inputActions.Spectator.Enable();
            inputActions.Player.Disable();

            inputActions.Spectator.Look.performed += HandleLook;
            inputActions.Spectator.Look.canceled += HandleLook;

            inputActions.Spectator.SwitchPrevPlayer.performed += HandleSwitchNextPlayer;
            inputActions.Spectator.SwitchPrevPlayer.performed += HandleSwitchPrevPlayer;
        }

        private void OnDisable()
        {
            inputActions.Spectator.Disable();
            inputActions.Spectator.Look.performed -= HandleLook;
            inputActions.Spectator.Look.canceled -= HandleLook;

            inputActions.Spectator.SwitchPrevPlayer.performed -= HandleSwitchNextPlayer;
            inputActions.Spectator.SwitchPrevPlayer.performed -= HandleSwitchPrevPlayer;
        }
        
        private void HandleLook(InputAction.CallbackContext context)
        {
            OnSpectatorLookInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void HandleSwitchNextPlayer(InputAction.CallbackContext context)
        {
            OnNext?.Invoke();
        }

        private void HandleSwitchPrevPlayer(InputAction.CallbackContext context)
        {
            OnPrev?.Invoke();
        }
    }
}