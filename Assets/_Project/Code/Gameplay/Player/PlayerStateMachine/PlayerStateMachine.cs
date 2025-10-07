using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.XR;

public class PlayerStateMachine : BaseStateController
{
    protected PlayerBaseState currentState;
    [field: SerializeField] public PlayerSO PlayerSO { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public LayerMask groundMask { get; private set; }
    [SerializeField] Transform _cameraTransform;
    [field: SerializeField] public float GroundCheckOffset { get; private set; }
    [field: SerializeField] public float GroundCheckDistance { get; private set; }
    private bool _isGrounded;
    public bool IsSprintHeld { get; private set; }
    public Vector2 MoveInput { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerSprintState SprintState { get; private set; }
    public PlayerCrouchIdleState CrouchIdleState { get; private set; }
    public PlayerCrouchWalkState CrouchWalkState { get; private set; }
    public PlayerInAirState InAirState { get; private set; }
    public PlayerInputManager InputManager { get; private set; }
    public Vector3 OriginalCenter { get; private set; }

    private float _coyoteTimer = 0f;
    private Vector3 _verticalVelocity;
    private bool _jumpRequested = false;
    //needs to be changed in children. Is this an acceptable way to do so?
    private float _targetCameraHeight;
    public float TargetCameraHeight { get { return _targetCameraHeight; } set { _targetCameraHeight = value; } }

    Timer _groundTimer;
    private float _groundTimerLength = 0.2f;
    private void Awake()
    {
        InputManager = GetComponent<PlayerInputManager>();
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        SprintState = new PlayerSprintState(this);
        CrouchIdleState = new PlayerCrouchIdleState(this);
        CrouchWalkState = new PlayerCrouchWalkState(this);
        InAirState = new PlayerInAirState(this);
        OriginalCenter = CharacterController.center;
        TargetCameraHeight = PlayerSO.StandingCameraHeight;
        _groundTimer = new Timer(.1f);
        _groundTimer.Start();
    }
    public void OnEnable()
    {
        if (InputManager != null)
        {
            InputManager.OnMoveInput += OnMoveInput;
            InputManager.OnJumpInput += OnJumpInput;
            InputManager.OnCrouchInput += OnCrouchInput;
            InputManager.OnSprintInput += OnSprintInput;
        }
        else
        {
            Debug.Log("input manager is null ");
        }
    }
    public void OnDisable()
    {
        if (InputManager != null)
        {
            InputManager.OnMoveInput -= OnMoveInput;
            InputManager.OnJumpInput -= OnJumpInput;
            InputManager.OnCrouchInput -= OnCrouchInput;
            InputManager.OnSprintInput -= OnSprintInput;
        }
        else
        {
            Debug.Log("input manager is null ");
        }
    }
    public void Start()
    {
        _isGrounded = IsGroundedCheck();
        TransitionTo(IdleState);
    }
    #region Inputs
    public void OnMoveInput(Vector2 movement)
    {
        MoveInput = movement;
        currentState.OnMoveInput(movement);
    }
    public void OnCrouchInput()
    {
        _groundTimer.Reset(_groundTimerLength);
        currentState.OnCrouchInput();
    }
    public void OnSprintInput(bool isPerformed)
    {
        currentState.OnSprintInput(isPerformed);
        IsSprintHeld = isPerformed;
    }
    public void OnJumpInput()
    {
        _groundTimer.Reset(_groundTimerLength);
        RequestJump();
    }
    public void RequestJump()
    {
        _jumpRequested = true;
    }
    #endregion
    public void HandleJump()
    {
        if (CharacterController.isGrounded)
        {
            _coyoteTimer = PlayerSO.CoyoteTime;

            if (_verticalVelocity.y < 0f)
                _verticalVelocity.y = -2f;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }
        if (_jumpRequested && _coyoteTimer > 0f)
        {
            _verticalVelocity.y = Mathf.Sqrt(PlayerSO.JumpStrength * -2f * PlayerSO.PlayerGravity);
            _coyoteTimer = 0f;
        }
        _jumpRequested = false;
        _verticalVelocity.y += PlayerSO.PlayerGravity * Time.deltaTime;
        CharacterController.Move(_verticalVelocity * Time.deltaTime);
    }
    public virtual void TransitionTo(PlayerBaseState newState)
    {
        if (newState == currentState) return;
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    void Update()
    {
        currentState?.StateUpdate();
        SmoothCameraTransition();
        HandleJump();
    }
    bool IsGroundedCheck()
    {

        float radius = CharacterController.radius;
        Vector3 origin = transform.position + Vector3.up * GroundCheckOffset;
        float distance = GroundCheckDistance;

        return Physics.SphereCast(origin, radius * 0.9f, Vector3.down, out _, distance, groundMask);
    }
    void SmoothCameraTransition()
    {
        Vector3 camPos = _cameraTransform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, -TargetCameraHeight, Time.deltaTime * PlayerSO.CameraTransitionSpeed);
        _cameraTransform.localPosition = camPos;
    }
    void FixedUpdate()
    {
        currentState?.StateFixedUpdate();
        if (_isGrounded != IsGroundedCheck())
        {
            if (IsGroundedCheck())
            {
                TransitionTo(IdleState);
            }
            else
            {
                TransitionTo(InAirState);
            }
        }
        _groundTimer.TimerUpdate(Time.deltaTime);
        if (_groundTimer.IsComplete)
        {
            _isGrounded = IsGroundedCheck();
        }



    }
}
