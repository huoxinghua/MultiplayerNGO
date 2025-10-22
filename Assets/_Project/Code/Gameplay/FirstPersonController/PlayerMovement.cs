using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Gameplay.FirstPersonController
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInputManager inputManager;
        private GroundCheck groundCheck;
        [Header("move")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 moveDirection;
        private bool isSprinting = false;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [Header("jump")]
        [SerializeField] private float jumpStrength = 2f;
        private Rigidbody rb;
        [SerializeField] private float fallMultiplier = 4f;
        //event
        public Action<GameObject> OnWalking;
        public Action<GameObject> OnRunning;
        public Action<GameObject> OnFalling;
        public Action<GameObject> OnLand;
        [Header("Crouch")]
        public Action<GameObject> OnCrouching;
        [SerializeField] private float standHeight = 1f;
        [SerializeField] private float crouchHeight =0.5f;
        private bool isCrouching = false;

        public static List<PlayerMovement> AllPlayers = new List<PlayerMovement>();
        public static event Action<PlayerMovement> OnPlayerAdded;
        public static event Action<PlayerMovement> OnPlayerRemoved;

        void OnDestroy()
        {
            AllPlayers.Remove(this);
            OnPlayerRemoved?.Invoke(this);
        }
        private void Awake()
        {
            if (!AllPlayers.Contains(this))
                AllPlayers.Add(this);
            OnPlayerAdded?.Invoke(this);
            rb = GetComponent<Rigidbody>();
            groundCheck = GetComponentInChildren<GroundCheck>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
 
        private void OnEnable()
        {
            inputManager = GetComponentInChildren<PlayerInputManager>();

            if (inputManager != null)
            {
                // inputManager.OnMoveInput += Move;
                //inputManager.OnJumpInput += Jump;
                // inputManager.OnCrouchInput += Crouch;
            }
            else
            {
                Debug.Log("input manager is null ");
            }
        }
        private void OnDisable()
        {
            inputManager = GetComponent<PlayerInputManager>();
            if (inputManager != null)
            {
                //  inputManager.OnMoveInput -= Move;
                // inputManager.OnJumpInput -= Jump;
                // inputManager.OnCrouchInput -= Crouch;
            }
            else
            {
                Debug.Log("input manager is null ");
            }
        }
        private void FixedUpdate()
        {
            //move
            Vector3 velocity = rb.linearVelocity;
            var currentSpeed = moveSpeed;
            if (groundCheck.IsGrounded && rb.linearVelocity.magnitude > 0.01f && isSprinting && !isCrouching)
            {
                OnRunning?.Invoke(gameObject);
            }
            else if(groundCheck.IsGrounded &&rb.linearVelocity.magnitude > 0.01f && !isSprinting && !isCrouching)
            {
                OnWalking?.Invoke(gameObject);
            }
            if (isSprinting)
            {
                currentSpeed = moveSpeed * sprintMultiplier;
            }
            else
            {
                currentSpeed = moveSpeed;
            }
            velocity.x = moveDirection.x * currentSpeed;
            velocity.z = moveDirection.y * currentSpeed;
            rb.linearVelocity = transform.rotation * (velocity + direction);

            // extra gravity when falling
            if (!groundCheck.IsGrounded && rb.linearVelocity.y < 0)
            {
          
                rb.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
                OnFalling?.Invoke(gameObject);
            }
        }
        public void Landed()
        {
            OnLand?.Invoke(gameObject);
        }
        Vector3 direction;
        public void Move(Vector2 dir,bool spriting)
        {
            moveDirection = dir;
            if (!isCrouching)
            {
                isSprinting = spriting;
            }
            if (isCrouching)
            {
                isSprinting = false;
            }
        }
        public void Jump()
        {
            if (groundCheck && groundCheck.IsGrounded)
            {
                rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
                //Debug.Log("player jump");
            }
        }
   

        public void Crouch()
        {
            isCrouching = !isCrouching; 

            Vector3 scale = transform.localScale;
            if (isCrouching)
            {
                scale.y = crouchHeight;
            }
            else
            {
                scale.y = standHeight;
            }
      
            transform.localScale = scale;

            OnCrouching?.Invoke(gameObject);
        }
    }
}
