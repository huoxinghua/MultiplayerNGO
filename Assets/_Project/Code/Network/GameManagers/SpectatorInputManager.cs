using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace _Project.Code.Network.GameManagers
{
    public class SpectatorInputManager : MonoBehaviour
    {
        public InputSystem_Actions inputActions;
        public event Action<Vector2> OnLookInput;

        public Vector2 LookInput { get; private set; }
        private void Awake()
        {
            inputActions = new InputSystem_Actions();
        }
        private void OnEnable()
        {
            inputActions.Enable();

            inputActions.Player.Look.performed += HandleLook;
            inputActions.Player.Look.canceled += HandleLook;
        }

        private void HandleLook(InputAction.CallbackContext context)
        {
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
        }
     
    }
}