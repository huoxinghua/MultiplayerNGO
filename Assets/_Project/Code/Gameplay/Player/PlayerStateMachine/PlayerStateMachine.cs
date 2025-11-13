using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.Singletons;
using _Project.Code.Utilities.StateMachine;
using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    [RequireComponent(typeof(GroundCheck))]
    public class PlayerStateMachine : BaseStateController
    {
        protected PlayerBaseState currentState;
        [field: SerializeField] public PlayerSO PlayerSO { get; private set; }
        [field: SerializeField] public CharacterController CharacterController { get; private set;}
        [field: SerializeField] public PlayerAnimation Animator { get; private set; }
        [field: SerializeField] public LayerMask groundMask { get; private set; }
        [SerializeField] Transform _cameraTransform;
        [field: SerializeField] public float GroundCheckOffset { get; private set; }
        [field: SerializeField] public float GroundCheckDistance { get; private set; }
        [field: SerializeField] public Transform GroundSpherePosition {  get; private set; }
        public GroundCheck GroundChecker { get; private set; }
        private bool _isGrounded;
        public bool IsSprintHeld { get; private set; }
        public Vector2 MoveInput { get; private set; }
        public PlayerIdleState IdleState { get; private set; }
        public PlayerWalkState WalkState { get; private set; }
        public PlayerSprintState SprintState { get; private set; }
        public PlayerCrouchIdleState CrouchIdleState { get; private set; }
        public PlayerCrouchWalkState CrouchWalkState { get; private set; }
        public PlayerInAirState InAirState { get; private set; }
        public PlayerMenuState MenuState { get; private set; }

        public bool IsInMenu => currentState == MenuState;

        public PlayerInputManager InputManager { get; private set; }
        public Vector3 OriginalCenter { get; private set; }

        private float CoyoteTimer = 0f;
        public bool CanJump => GroundChecker.IsGrounded || GroundChecker.CoyoteTime < PlayerSO.CoyoteTime;

        public Vector3 VerticalVelocity;
        public bool JumpRequested { get; set; } = false;
        //needs to be changed in children. Is this an acceptable way to do so?
        private float _targetCameraHeight;
    
        public float TargetCameraHeight { get { return _targetCameraHeight; } set { _targetCameraHeight = value; } }

        Timer _groundTimer;
        private float _groundTimerLength = 0.2f;

        public event Action<float, GameObject> SoundMade;
        public static List<PlayerStateMachine> AllPlayers = new List<PlayerStateMachine>();
        public static event Action<PlayerStateMachine> OnPlayerAdded;
        public static event Action<PlayerStateMachine> OnPlayerRemoved;
        public void OnSoundMade(float soundRange)
        {
            SoundMade?.Invoke(soundRange, gameObject);
        }
        private void Awake()
        {
            InputManager = GetComponent<PlayerInputManager>();
            GroundChecker = GetComponent<GroundCheck>();
            CurrentPlayers.Instance.AddPlayer(gameObject);
            IdleState = new PlayerIdleState(this);
            WalkState = new PlayerWalkState(this);
            SprintState = new PlayerSprintState(this);
            CrouchIdleState = new PlayerCrouchIdleState(this);
            CrouchWalkState = new PlayerCrouchWalkState(this);
            InAirState = new PlayerInAirState(this);
            MenuState = new PlayerMenuState(this);
            OriginalCenter = CharacterController.center;
            TargetCameraHeight = PlayerSO.StandingCameraHeight;

            _groundTimer = new Timer(.1f);
            _groundTimer.Start();

        }
        public void OnEnable()
        {
            GroundChecker.OnGroundedChanged += OnGroundStateChange;
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
            if (!AllPlayers.Contains(this))
                AllPlayers.Add(this);
            OnPlayerAdded?.Invoke(this);
        }
        public void OnDisable()
        {
            GroundChecker.OnGroundedChanged -= OnGroundStateChange;
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
            AllPlayers.Remove(this);
            OnPlayerRemoved?.Invoke(this);
        
        }
        public void Start()
        {
            ////test
            transform.position = new Vector3(57,10,-30) + Vector3.up * 3f;// this is for the secoundshow case Art
            //transform.position =  Vector3.up * 3f;//this is for game gym
            /*var controller = GetComponent<CharacterController>();
            controller.enabled = false;
            StartCoroutine(EnablePlayerController(controller));*/
            //test end
            TransitionTo(IdleState);
        }
        //test
        private IEnumerator EnablePlayerController(CharacterController con)
        {
            yield return new WaitForSeconds(1f);
            con.enabled = true;
        }
        //test end
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
        public void OnJumpInput(PlayerJumpEvent jumpEvent)
        {
            currentState.OnJumpInput(jumpEvent.IsPressed);
        }
        #endregion
        public virtual void TransitionTo(PlayerBaseState newState)
        {
            if (newState == currentState) return;
            currentState?.OnExit();
            currentState = newState;
            currentState.OnEnter();
        }

        void Update()
        {
            //Debug.Log(currentState.ToString());
            currentState?.StateUpdate();
            SmoothCameraTransition();
            //HandleJump();
        }
        // bool IsGroundedCheck()
        // {
        //
        //     float radius = CharacterController.radius;
        //
        //     float distance = GroundCheckDistance;
        //     DebugDrawSphereCast(GroundSpherePosition.position, radius * 0.9f, Vector3.down, distance, Color.red);
        //     return Physics.SphereCast(GroundSpherePosition.position, radius * 0.9f, Vector3.down, out _, distance, groundMask);
        // }
        void DebugDrawSphereCast(Vector3 origin, float radius, Vector3 direction, float distance, Color color)
        {
            Vector3 end = origin + direction.normalized * distance;

            // Draw center line
            Debug.DrawLine(origin, end, color);

            // Draw start and end wire spheres
            DrawWireSphere(origin, radius, color);
            DrawWireSphere(end, radius, color);
        }

        void DrawWireSphere(Vector3 position, float radius, Color color)
        {
            // Basic wireframe spheres using Debug.DrawLine to simulate a circle
            int segments = 16;
            float angleStep = 360f / segments;

            // Draw horizontal ring
            for (int i = 0; i < segments; i++)
            {
                float angleCurrent = Mathf.Deg2Rad * i * angleStep;
                float angleNext = Mathf.Deg2Rad * (i + 1) * angleStep;

                Vector3 pointCurrent = position + new Vector3(Mathf.Cos(angleCurrent), 0f, Mathf.Sin(angleCurrent)) * radius;
                Vector3 pointNext = position + new Vector3(Mathf.Cos(angleNext), 0f, Mathf.Sin(angleNext)) * radius;

                Debug.DrawLine(pointCurrent, pointNext, color);
            }

            // Optionally draw vertical rings (XZ and YZ) for better 3D feel
        }
        void SmoothCameraTransition()
        {
            Vector3 camPos = _cameraTransform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, -TargetCameraHeight, Time.deltaTime * PlayerSO.CameraTransitionSpeed);
            _cameraTransform.localPosition = camPos;
        }

        public void OnGroundStateChange(bool isGrounded)
        {
            if (!isGrounded)
            {
                TransitionTo(IdleState);
                OnSoundMade(PlayerSO.LandingSoundRange);
            }
            else
            {
                TransitionTo(InAirState);
            }
        }
        public void HandleOpenMenu(bool didOpen)
        {
            if (didOpen) TransitionTo(MenuState);
            else TransitionTo(IdleState);
        }
        void FixedUpdate()
        {
            currentState?.StateFixedUpdate();
        }
    }
}
