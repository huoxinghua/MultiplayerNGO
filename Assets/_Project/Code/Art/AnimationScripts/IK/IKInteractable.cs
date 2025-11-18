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
        [SerializeField] private IkInteractSO ikInteractSo;

        private Tween interactTween;
        private Tween currentTween;
        private bool currentCrouch;

        private PlayerIKController _currentFPSIKController;
        private PlayerIKController _currentTPSIKController;
        
        
        private NetworkVariable<IKAnimState> currentAnimaState = new NetworkVariable<IKAnimState>(
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
        
        public bool IsInteract { get; private set; } = false;


        private void Update()
        {
            if (IsServer && currentAnimaState.Value != IKAnimState.Idle)
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

            currentAnimaState.OnValueChanged += OnAnimStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            currentAnimaState.OnValueChanged -= OnAnimStateChanged;
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
                // Client เรียกให้ Server ทำงานแทน
                SetAnimStateServerRPC(newState, isCrouch);
                return;
            }

            // Server ทำเอง
            SetAnimStateServerRPC(newState, isCrouch);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetAnimStateServerRPC(IKAnimState newState, bool isCrouch)
        {
            if (IsInteract && newState != IKAnimState.Interact)
                return;

            if (currentAnimaState.Value == newState && currentCrouch == isCrouch) 
                return;

            currentAnimaState.Value = newState;
            currentCrouch = isCrouch;
            

            OnAnimStateChanged(currentAnimaState.Value, newState);
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
            if (IsInteract && newState != IKAnimState.Interact)
                return;

            if (currentAnimaState.Value == newState) 
                return;

            currentAnimaState.Value = newState;
            

            OnAnimStateChanged(currentAnimaState.Value, newState);
        }

        public void PickupAnimation(PlayerIKController ikController)
        {
            ikController.IKPos(this, handL, handR, elbowL, elbowR, ikInteractSo);
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
            transform.localPosition = ApplyPosOffset(Vector3.zero, IsFPS);
            transform.localRotation = Quaternion.Euler(ApplyRotOffset(Vector3.zero, IsFPS));
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
            currentAnimaState.Value = IKAnimState.Idle;
            animTime.Value = 0f;
            
            //Broadcast to Client
            PlayIKIdleClientRpc();
        }

        [ClientRpc]
        private void PlayIKIdleClientRpc()
        {
            localAnimTime = 0f;
            
            //Play Animation in Local
            PlayIKIdleLocal(IsFPS);
        }

        private void PlayIKIdleLocal(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            
            var waypoints = isFPS ? ikInteractSo.ikIdle.fpsWaypoints :  ikInteractSo.ikIdle.tpsWaypoints;
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = ikInteractSo.ikIdle.resetDuration;
            else duration = ikInteractSo.ikIdle.transitionDuration;

            transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[1], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .SetLoops(int.MaxValue, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
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
            currentAnimaState.Value = isRunning ? IKAnimState.Run :  IKAnimState.Walk;
            animTime.Value = 0f;
            
            //Broadcast to Client
            PlayIKMoveClientRpc(slowSpeed, isRunning);
        }

        [ClientRpc]
        private void PlayIKMoveClientRpc(float slowSpeed, bool isRunning)
        {
            localAnimTime = 0f;
            
            //Play Animation in Local
            PlayIKMoveLocal(slowSpeed, IsFPS, isRunning);
        }

        private void PlayIKMoveLocal(float slowSpeed, bool isFPS, bool isRunning)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }

            var preset = isRunning ? ikInteractSo.ikRun : ikInteractSo.ikWalk;
            var waypoints = isFPS ? preset.fpsWaypoints : preset.tpsWaypoints;
            var followThroughs = isFPS ? preset.fpsFollowThrough : preset.tpsFollowThrough;
            
            float duration = preset.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = preset.resetDuration;
            else duration = preset.transitionDuration;

            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .Join(transform.DOLocalRotate(ApplyRotOffset(followThroughs[0], isFPS), duration)
                    .SetEase(ikInteractSo.ikWalk.easeType))
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    
                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyPosOffset(waypoints, isFPS), ikInteractSo.ikWalk.loopDuration * slowSpeed, ikInteractSo.ikWalk.pathType, ikInteractSo.ikWalk.pathMode)
                        .SetEase(ikInteractSo.ikWalk.easeType)
                        .SetLoops(int.MaxValue, ikInteractSo.ikWalk.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                    rotateSeq.Append(transform.DOLocalRotate(ApplyRotOffset(followThroughs[1], isFPS), ikInteractSo.ikWalk.loopDuration * slowSpeed).SetEase(ikInteractSo.ikWalk.easeType))
                        .Append(transform.DOLocalRotate(ApplyRotOffset(followThroughs[0], isFPS), ikInteractSo.ikWalk.loopDuration * slowSpeed).SetEase(ikInteractSo.ikWalk.easeType))
                        .SetLoops(int.MaxValue, ikInteractSo.ikWalk.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                });
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
            currentAnimaState.Value = IKAnimState.Interact;
            animTime.Value = 0f;
            
            //Broadcast to Client
            PlayIKInteractClientRpc();
        }

        [ClientRpc]
        private void PlayIKInteractClientRpc()
        {
            localAnimTime = 0f;
            
            //Play Animation in Local
            PlayIKInteractLocal(IsFPS);
        }

        private void PlayIKInteractLocal(bool isFPS)
        {
            if (IsInteract) return;
                    IsInteract = true;
                    if(interactTween != null)
                    {
                        interactTween.Kill(true);
                        interactTween = null;
                    }
                    var waypoints = isFPS ? ikInteractSo.ikInteract.fpsPosWaypoints :  ikInteractSo.ikInteract.tpsPosWaypoints;
                    var RotPoints = isFPS ? ikInteractSo.ikInteract.fpsRotWaypoints : ikInteractSo.ikInteract.tpsRotWaypoints;
                    float duration = ikInteractSo.ikInteract.transitionDuration;

                    if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = ikInteractSo.ikInteract.resetDuration;
                    else duration = ikInteractSo.ikInteract.transitionDuration;

                    var seq = DOTween.Sequence();

                    seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), duration)
                            .SetEase(ikInteractSo.ikInteract.easeAnti))
                        .Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), duration)
                            .SetEase(ikInteractSo.ikInteract.easeAnti));

                    seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration)
                            .SetEase(ikInteractSo.ikInteract.easeAnti))
                        .Join(transform.DOLocalRotate(ApplyRotOffset(RotPoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration)
                            .SetEase(ikInteractSo.ikInteract.easeAnti));

                    seq.Append(transform.DOLocalMove(ApplyPosOffset(waypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration)
                            .SetEase(ikInteractSo.ikInteract.easeHit))
                        .Join(transform.DOLocalRotate(ApplyRotOffset(RotPoints[1], isFPS), ikInteractSo.ikInteract.hitDuration)
                            .SetEase(ikInteractSo.ikInteract.easeHit));

                    seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration)
                            .SetEase(ikInteractSo.ikInteract.easeHit))
                        .Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration)
                            .SetEase(ikInteractSo.ikInteract.easeHit));

                    seq.OnComplete(() => {IsInteract = false;});
                    interactTween = seq;
        }
        
        private Vector3 ApplyPosOffset(Vector3 point, bool isFPS)
        {
            return point + (isFPS? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset);
        }

        private Vector3[] ApplyPosOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = isFPS? ikInteractSo.fpsOffset.posOffset : ikInteractSo.tpsOffset.posOffset;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
        
        private Vector3 ApplyRotOffset(Vector3 point, bool isFPS)
        {
            return point + (isFPS? ikInteractSo.fpsOffset.rotOffset : ikInteractSo.tpsOffset.rotOffset);
        }

        private Vector3[] ApplyRotOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = isFPS? ikInteractSo.fpsOffset.rotOffset : ikInteractSo.tpsOffset.rotOffset;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
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