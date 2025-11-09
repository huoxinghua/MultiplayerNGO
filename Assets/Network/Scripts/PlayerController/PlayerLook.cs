using _Project.Code.Gameplay.FirstPersonController;
using UnityEngine;

namespace Network.Scripts.PlayerController
{
    public class PlayerLook : MonoBehaviour
    {
        private PlayerInputManager inputManager;
        [SerializeField] private Transform character;
        [SerializeField] private float sensitivity = 2;
        [SerializeField] private float smoothing = 1.5f;
        [SerializeField] private float rawLookMultiply = 0.009f;
        Vector2 velocity;
        Vector2 frameVelocity;

       
        private void Awake()
        {
            inputManager = GetComponentInParent<PlayerInputManager>();
        }
        private void OnEnable()
        {
            if (inputManager != null)
            {

                inputManager.OnLookInput += Look;
            }
            else
            {
                Debug.Log("input manager is null ");
            }
        }
        private void OnDisable()
        {
            if (inputManager != null)
            {
                inputManager.OnLookInput -= Look;
            }
            else
            {
                Debug.Log("input manager is null ");
            }
        }
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        Vector2 rawLook;
        void Update()
        {
            rawLook = inputManager.inputActions.Player.Look.ReadValue<Vector2>();
            Vector2 rawLookScale = Vector2.Scale(rawLook, Vector2.one * rawLookMultiply);

            Vector2 rawFrameVelocity = Vector2.Scale(rawLookScale, Vector2.one * sensitivity);
            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
            velocity += frameVelocity;
            velocity.y = Mathf.Clamp(velocity.y, -90, 90);

            // Rotate camera up-down 
            transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
            character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
        }

        private void Look(Vector2 dir)
        {
            rawLook = dir;
        }
    }
}
