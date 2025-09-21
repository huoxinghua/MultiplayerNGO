using System;
using System.Text.RegularExpressions;
using UnityEngine;

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
    public static Action<GameObject> OnWalking;
    public static Action<GameObject> OnRunning;
    public static Action<GameObject> OnFalling;
    [Header("Crouch")]
    public static Action<GameObject> OnCrouching;
    [SerializeField] private float standHeight = 1f;
    [SerializeField] private float crouchHeight =0.5f;
    private bool isCrouching = false;
    private void Awake()
    {
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
            inputManager.OnMoveInput += Move;
            inputManager.OnJumpInput += Jump;
            inputManager.OnCrouchInput += Crouch;
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
            inputManager.OnMoveInput -= Move;
            inputManager.OnJumpInput -= Jump;
            inputManager.OnCrouchInput -= Crouch;
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
        if(isSprinting)
        {
            currentSpeed =moveSpeed * sprintMultiplier;
            OnRunning?.Invoke(gameObject);
        }
        else
        {
            currentSpeed = moveSpeed;
        }
        velocity.x = moveDirection.x * currentSpeed;
        velocity.z = moveDirection.y * currentSpeed;
        rb.linearVelocity = transform.rotation * (velocity + direction);

        // extra gravity when falling
        if (!groundCheck.isGrounded && rb.linearVelocity.y < 0)
        {
          
            rb.AddForce(Vector3.down * fallMultiplier, ForceMode.Acceleration);
            OnFalling?.Invoke(gameObject);
        }
    }
    Vector3 direction;
    public void Move(Vector2 dir,bool spriting)
    {
        moveDirection = dir;
        isSprinting = spriting;
        OnWalking?.Invoke(gameObject);
    }
    public void Jump()
    {
        if (groundCheck && groundCheck.isGrounded)
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
