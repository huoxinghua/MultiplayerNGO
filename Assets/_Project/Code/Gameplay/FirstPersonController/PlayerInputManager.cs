using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public InputSystem_Actions inputActions;
    public event Action<Vector2, bool> OnMoveInput;
    public event Action OnJumpInput;
    public event Action<Vector2> OnLookInput;
    public event Action OnShootInput;
    public event Action OnChangeWeaponInput;
    public event Action OnSprintInput;
    public event Action CancelHold;
    public event Action OnCrouchInput;
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

    }
    Vector2 moveInput;
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        OnMoveInput?.Invoke(moveInput, false);
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
        OnMoveInput?.Invoke(moveInput, isSprinting);
    }
    private void HandleLook(InputAction.CallbackContext context)
    {

        OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }
    private void HandleJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpInput?.Invoke();
        }
    }
    private void HandleCrouch(InputAction.CallbackContext context)
    {
        OnCrouchInput?.Invoke();
    }

    private void HandleShoot(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            OnShootInput?.Invoke();
        }
    }
    private void HandleChangeWeapon(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            OnChangeWeaponInput?.Invoke();
        }
    }


}

