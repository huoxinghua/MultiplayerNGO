using System;
using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using DG.Tweening;
using UnityEngine;
using Unity.Netcode;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public enum IKAnimState
    {
        Idle,
        Walk,
        Run,
        Interact
    }
    public class IKInteractable : NetworkBehaviour
    {
        [Header("Hands and Fingers Position")] 
        [SerializeField] private Transform handR;
        [SerializeField] private Transform handL;
        [SerializeField] private Transform elbowR;
        [SerializeField] private Transform elbowL;
        [SerializeField] private IKItemAnimation ikAnim;
        
        private bool currentCrouch;

        private PlayerIKController _currentFPSIKController;
        private PlayerIKController _currentTPSIKController;
        
        public IKItemAnimation IKAnim => ikAnim;
        
        
        private NetworkVariable<IKAnimState> currentAnimState = new NetworkVariable<IKAnimState>(
            IKAnimState.Idle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private NetworkVariable<float> animTime = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private float localAnimTime = 0f;
        private const float DRIFT_CORRECTION_THRESHOLD = 0.1f;
        private bool IsFPS => _currentFPSIKController != null;
        
        private void Update()
        {
            if (IsServer && currentAnimState.Value != IKAnimState.Idle)
            {
                animTime.Value += Time.deltaTime;
            }
            
            //Client : Drift Correction
            if (!IsServer)
            {
                float drift = Math.Abs(localAnimTime - animTime.Value);

                if (drift > DRIFT_CORRECTION_THRESHOLD)
                {
                    localAnimTime = animTime.Value;
                    //Debug.LogWarning($"IK animation drift corrected: {drift:F3}s");
                }
                else
                {
                    localAnimTime +=  Time.deltaTime;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            currentAnimState.OnValueChanged += OnAnimStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            currentAnimState.OnValueChanged -= OnAnimStateChanged;
        }

        private void OnAnimStateChanged(IKAnimState oldState, IKAnimState newState)
        {
            //Debug.Log($"[IK] Animation state changed: {oldState} → {newState}");

            switch (newState)
            {
                case IKAnimState.Idle:
                    PlayIKIdle();
                    break;
                case IKAnimState.Walk:
                    PlayIKMove(currentCrouch ? 2f : 1f, false);
                    break;
                case IKAnimState.Run:
                    PlayIKMove(1f, true);
                    break;
                case IKAnimState.Interact:
                    PlayIKInteract();
                    break;
            }
        }

        public void SetAnimState(IKAnimState newState, bool isCrouch)
        {
            if (!IsServer)
            {
                //If !server, send to server
                SetAnimStateServerRPC(newState, isCrouch);
                return;
            }

            //If server, just run
            SetAnimStateServerRPC(newState, isCrouch);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetAnimStateServerRPC(IKAnimState newState, bool isCrouch)
        {
            if (ikAnim.isInteract && newState != IKAnimState.Interact)
                return;

            if (currentAnimState.Value == newState && currentCrouch == isCrouch) 
                return;

            currentAnimState.Value = newState;
            currentCrouch = isCrouch;
            

            OnAnimStateChanged(currentAnimState.Value, newState);
        }
        
        public void SetAnimState(IKAnimState newState)
        {
            if (!IsServer)
            {
                //If !server, send to server
                SetAnimStateServerRPC(newState);
                return;
            }
            //If server, just run
            SetAnimStateServerRPC(newState);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetAnimStateServerRPC(IKAnimState newState)
        {
            if (ikAnim.isInteract && newState != IKAnimState.Interact)
                return;

            if (currentAnimState.Value == newState) 
                return;

            currentAnimState.Value = newState;
            

            OnAnimStateChanged(currentAnimState.Value, newState);
        }

        public void PickupAnimation(PlayerIKController ikController)
        {
            ikController.IKPos(this, handL, handR, elbowL, elbowR, ikAnim.ikInteractSo);
            ikController.IkActive = true;
            if (ikController.IsOwner)
            {
                _currentFPSIKController = ikController;
                _currentTPSIKController = null;
            }
            else
            {
                _currentTPSIKController = ikController;
                _currentFPSIKController = null;
            }
            transform.localPosition = ikAnim.ApplyPosOffset(Vector3.zero, IsFPS);
            transform.localRotation = Quaternion.Euler(ikAnim.ApplyRotOffset(Vector3.zero, IsFPS));
            SetAnimState(IKAnimState.Idle, IsFPS);
            Debug.Log($"[PickupAnimation] {gameObject.name} | IKname {ikController} | current isFPS(after): {IsFPS} | Owner: {NetworkObject.OwnerClientId}");
        }

        public void DropAnimation()
        {
            if (_currentFPSIKController != null)
            {
                _currentFPSIKController.IkActive = false;
                _currentFPSIKController.IKPos(null, null, null, null, null, null);
                _currentFPSIKController =  null;
            }
            if (_currentTPSIKController != null)
            {
                _currentTPSIKController.IkActive = false;
                _currentTPSIKController.IKPos(null, null, null, null, null, null);
                _currentTPSIKController =  null;
            }
            Debug.Log($"DropAnimation Called | Owner={IsOwner} | FPS={_currentFPSIKController} | TPS={_currentTPSIKController}");
        }
        
        private void PlayIKIdle()
        {
            if (!IsServer)
            {
                //If !server, send to server
                PlayIKIdleServerRpc();
                return;
            }
            //If server, just run
            PlayIKIdleServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayIKIdleServerRpc()
        {
            //State Update
            currentAnimState.Value = IKAnimState.Idle;
            animTime.Value = 0f;
            
            //Broadcast to Client
            PlayIKIdleClientRpc();
        }

        [ClientRpc]
        private void PlayIKIdleClientRpc()
        {
            localAnimTime = 0f;
            
            //Play Animation in Local
            ikAnim.PlayIKIdleLocal(IsFPS);
        }
        
        private void PlayIKMove(float slowSpeed, bool isRunning)
        {
            if (!IsServer)
            {
                //If !server, send to server
                PlayIKMoveServerRpc(slowSpeed, isRunning);
                return;
            }
            //If server, just run
            PlayIKMoveServerRpc(slowSpeed, isRunning);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayIKMoveServerRpc(float slowSpeed, bool isRunning)
        {
            //State Update
            currentAnimState.Value = isRunning ? IKAnimState.Run :  IKAnimState.Walk;
            animTime.Value = 0f;
            
            //Broadcast to Client
            PlayIKMoveClientRpc(slowSpeed, isRunning);
        }

        [ClientRpc]
        private void PlayIKMoveClientRpc(float slowSpeed, bool isRunning)
        {
            localAnimTime = 0f;
            
            //Play Animation in Local
            ikAnim.PlayIKMoveLocal(slowSpeed, IsFPS, isRunning);
        }

        private void PlayIKInteract()
        {
            if (!IsServer)
            {
                //If !server, send to server
                PlayIKInteractServerRpc();
                return;
            }
            //If server, just run
            PlayIKInteractServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlayIKInteractServerRpc()
        {
            //State Update
            currentAnimState.Value = IKAnimState.Interact;
            animTime.Value = 0f;
            
            //Broadcast to Client
            PlayIKInteractClientRpc();
        }

        [ClientRpc]
        private void PlayIKInteractClientRpc()
        {
            localAnimTime = 0f;
            
            //Play Animation in Local
            ikAnim.PlayIKInteractLocal(IsFPS);
        }
    }

    [Serializable]
    public struct OffsetPos
    {
        public Vector3 posOffset;
        public Vector3 rotOffset;
    }

    [Serializable]
    public struct IdlePreset
    {
        public float resetDuration;
        public float transitionDuration;
        public float loopDuration;
        public Vector3[] fpsWaypoints;
        public Vector3[] tpsWaypoints;
        public LoopType loopType;
        public Ease easeType;
    }

    [Serializable]
    public struct MovementPreset
    {
        public float resetDuration;
        public float transitionDuration;
        public float loopDuration;
        public Vector3[] fpsWaypoints;
        public Vector3[] tpsWaypoints;
        public Vector3[] fpsFollowThrough;
        public Vector3[] tpsFollowThrough;
        public LoopType loopType;
        public Ease easeType;
        public PathType pathType;
        public PathMode pathMode;
    }

    [Serializable]
    public struct InteractPreset
    {
        public float resetDuration;
        public float transitionDuration;
        public float hitDuration;
        public Vector3[] fpsPosWaypoints;
        public Vector3[] tpsPosWaypoints;
        public Vector3[] fpsRotWaypoints;
        public Vector3[] tpsRotWaypoints;
        public Ease easeAnti;
        public Ease easeHit;
    }
}