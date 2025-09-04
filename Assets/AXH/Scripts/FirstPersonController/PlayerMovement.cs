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
        }
    }
    Vector3 direction;
    public void Move(Vector2 dir,bool spriting)
    {
        moveDirection = dir;
        isSprinting = spriting;
    }
    public void Jump()
    {
        if (groundCheck && groundCheck.isGrounded)
        {
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            //Debug.Log("player jump");
        }
    }

}
