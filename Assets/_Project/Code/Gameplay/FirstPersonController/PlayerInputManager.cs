using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Gameplay.FirstPersonController
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
            inputActions.Enable();
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
            inputActions.Player.KeyPressed.performed += HandleKeyPressed;
        }
        private void OnDisable()
        {
            inputActions.Disable();

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
            inputActions.Player.KeyPressed.performed -= HandleKeyPressed;
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
            Debug.Log($"Jump context is {context.performed}");
            OnJumpInput?.Invoke(new PlayerJumpEvent{IsPressed = context.performed});
        
        }
        private void HandleCrouch(InputAction.CallbackContext context)
        {
            OnCrouchInput?.Invoke();
        }


        /*    private void HandleChangeWeapon(InputAction.CallbackContext context)
        {

            if (context.performed)
            {
                OnChangeWeaponInput?.Invoke();
            }
        }*/

        #region Item slots
        private void HandleKeyPressed(InputAction.CallbackContext context)
        {
            var keyValue = int.Parse(context.control.displayName);
            OnNumPressed?.Invoke(keyValue);
        }
        /* private void HandleNumOne(InputAction.CallbackContext context)
    {

        OnNumOne?.Invoke();
    }
    private void HandleNumTwo(InputAction.CallbackContext context)
    {
        OnNumTwo?.Invoke();
    }
    private void HandleNumThree(InputAction.CallbackContext context)
    {
        OnNumThree?.Invoke();
    }
    private void HandleNumFour(InputAction.CallbackContext context) 
    {
        OnNumFour?.Invoke();
    }
    private void HandleNumFive(InputAction.CallbackContext context)
    {
        OnNumFive?.Invoke();
    }*/
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
        #endregion


        private void HandleInteract(InputAction.CallbackContext context)
        {
            OnInteract?.Invoke();
        }
    }
}