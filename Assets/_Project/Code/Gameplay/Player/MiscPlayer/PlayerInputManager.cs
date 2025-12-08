using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Gameplay.Player.MiscPlayer
{
    public interface IEvent
    {
    }

    public struct PlayerJumpEvent:IEvent
    {
        public bool IsPressed { get; set; }
        public float TimeJumped { get; private set; }
    }
    public class PlayerInputManager : MonoBehaviour
    {
        public InputSystem_Actions inputActions;
        public event Action<Vector2> OnMoveInput;
        public event Action<PlayerJumpEvent> OnJumpInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnUse;
        public event Action <bool> OnSecondaryUse;
        public event Action OnChangeWeaponInput;
        public event Action<bool> OnSprintInput;
        public event Action CancelHold;
        public event Action OnCrouchInput;
        public event Action<int> OnNumPressed;
        public event Action OnNumOne;
        public event Action OnNumTwo;
        public event Action OnNumThree;
        public event Action OnNumFour;
        public event Action OnNumFive;
        public event Action OnDropItem;
        public event Action OnInteract;
        public Vector2 LookInput { get; private set; }
        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }
        
        private void OnEnable()
        {
            SwitchToPlayerMode();
            inputActions.Player.Move.performed += HandleMove;
            inputActions.Player.Move.canceled += HandleMove;
            inputActions.Player.Jump.performed += HandleJump;
            inputActions.Player.Jump.canceled += HandleJump;
            inputActions.Player.Look.performed += HandleLook;
            inputActions.Player.Look.canceled += HandleLook;

            inputActions.Player.Sprint.performed += HandleSprint;
            inputActions.Player.Sprint.canceled += HandleSprint;

            inputActions.Player.Crouch.performed += HandleCrouch;


            inputActions.Player.DropItem.performed += HandleDropItem;
            inputActions.Player.Interact.performed += HandleInteract;
            inputActions.Player.Use.performed += HandleUse;
            inputActions.Player.SecondaryUse.performed += HandleSecondaryUse;
            inputActions.Player.SecondaryUse.canceled += HandleSecondaryUse;
            inputActions.Player.KeyPressed.performed += HandleKeyPressed;
        }
        private void OnDisable()
        {
            inputActions.Player.Disable();
            inputActions.Spectator.Disable();

            inputActions.Player.Move.performed -= HandleMove;
            inputActions.Player.Move.canceled -= HandleMove;
            inputActions.Player.Jump.performed -= HandleJump;
            inputActions.Player.Jump.canceled -= HandleJump;
            inputActions.Player.Look.performed -= HandleLook;
            inputActions.Player.Look.canceled -= HandleLook;
            inputActions.Player.Sprint.performed -= HandleSprint;
            inputActions.Player.Sprint.canceled -= HandleSprint;


            inputActions.Player.Crouch.performed -= HandleCrouch;


            inputActions.Player.DropItem.performed -= HandleDropItem;
            inputActions.Player.Interact.performed -= HandleInteract;
            inputActions.Player.Use.performed -= HandleUse;
            inputActions.Player.SecondaryUse.performed -= HandleSecondaryUse;
            inputActions.Player.SecondaryUse.canceled -= HandleSecondaryUse;
            inputActions.Player.KeyPressed.performed -= HandleKeyPressed;
        }
        public void SwitchToSpectatorMode()
        {
            inputActions.Player.Disable();
            inputActions.Spectator.Enable();
        }

        public void SwitchToPlayerMode()
        {
            inputActions.Spectator.Disable();
            inputActions.Player.Enable();
        }
        Vector2 moveInput;
        private void HandleMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            OnMoveInput?.Invoke(moveInput);
        }

        private void HandleSprint(InputAction.CallbackContext context)
        {
            bool isSprinting = false;
            if (context.performed)
            {
                isSprinting = true;
            }
            else if (context.canceled)
            {
                isSprinting = false;
            }
            OnSprintInput?.Invoke(isSprinting);
        }
        private void HandleLook(InputAction.CallbackContext context)
        {
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
        }
        private void HandleJump(InputAction.CallbackContext context)
        {
            OnJumpInput?.Invoke(new PlayerJumpEvent{IsPressed = context.performed});
        
        }
        private void HandleCrouch(InputAction.CallbackContext context)
        {
            OnCrouchInput?.Invoke();
        }

        #region Item slots
        private void HandleKeyPressed(InputAction.CallbackContext context)
        {
            var keyValue = int.Parse(context.control.displayName);
            OnNumPressed?.Invoke(keyValue);
        }
        #endregion


        #region Item Control
        private void HandleDropItem(InputAction.CallbackContext context)
        {
            OnDropItem?.Invoke();
        }
        private void HandleUse(InputAction.CallbackContext context)
        {

            if (context.performed)
            {
                OnUse?.Invoke();
            }
        }
        private void HandleSecondaryUse(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnSecondaryUse?.Invoke(true);
            }
            else if (context.canceled)
            {
                OnSecondaryUse?.Invoke(false);
            }
        }
        #endregion


        private void HandleInteract(InputAction.CallbackContext context)
        {
            OnInteract?.Invoke();
        }
    }
}