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
    //needs to be changed in children. Is this an acceptable way to do so?
    public float TargetCameraHeight;
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
        currentState.OnCrouchInput();
    }
    public void OnSprintInput(bool isPerformed)
    {
        currentState.OnSprintInput(isPerformed);
        IsSprintHeld = isPerformed;
    }
    public void OnJumpInput()
    {
        //transition to air state
    }
    #endregion
    public virtual void TransitionTo(PlayerBaseState newState)
    {
        if (newState == currentState) return;
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
        Debug.Log(newState.ToString());
    }

    void Update()
    {
        currentState?.StateUpdate();
        SmoothCameraTransition();
    }
    void SmoothCameraTransition()
    {
        Vector3 camPos = _cameraTransform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, TargetCameraHeight, Time.deltaTime * PlayerSO.CameraTransitionSpeed);
        _cameraTransform.localPosition = camPos;
    }
    void FixedUpdate()
    {
        currentState?.StateFixedUpdate();
    }
}
