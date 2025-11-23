using System;
using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using DG.Tweening;
using UnityEngine;
using Unity.Netcode;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public enum IKAnimState
    {
        None,
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

        [Header("Position/Rotation Anchor")]
        [SerializeField] private Transform gripAnchor;
        
        private Tween currentTween;
        private bool currentCrouch;
        private bool currentFPS;
        private PlayerIKController _currentFPSIKController;
        private PlayerIKController _currentTPSIKController;

        private float localAnimTime = 0f;
        private IKAnimState _currentState = IKAnimState.None;
        public bool IsInteract { get; private set; } = false;

        public void SetAnimState(IKAnimState newState, bool isFPS, bool isCrouch)
        {
            if (IsInteract && newState != IKAnimState.Interact)
                return;

            // Skip if already in this state with same parameters
            if (_currentState == newState && currentCrouch == isCrouch && currentFPS == isFPS)
                return;

            _currentState = newState;
            currentCrouch = isCrouch;
            currentFPS = isFPS;

            switch (newState)
            {
                case IKAnimState.None:
                    StopIKAnimation();
                    break;
                case IKAnimState.Idle:
                    PlayIKIdleLocal(isFPS);
                    break;
                case IKAnimState.Walk:
                    PlayIKMoveLocal(currentCrouch ? 2f : 1f, isFPS, false);
                    break;
                case IKAnimState.Run:
                    PlayIKMoveLocal(1f, isFPS, true);
                    break;
                case IKAnimState.Interact:
                    IsInteract = true; // Lock state immediately to prevent interruption
                    PlayIKInteractLocal(isFPS);
                    break;
            }
        }

        public void SetAnimState(IKAnimState newState, bool isFPS)
        {
            if (IsInteract && newState != IKAnimState.Interact)
                return;

            // Skip if already in this state with same parameters
            if (_currentState == newState && currentFPS == isFPS)
                return;

            _currentState = newState;
            currentFPS = isFPS;

            switch (newState)
            {
                case IKAnimState.None:
                    StopIKAnimation();
                    break;
                case IKAnimState.Idle:
                    PlayIKIdleLocal(isFPS);
                    break;
                case IKAnimState.Walk:
                    PlayIKMoveLocal(currentCrouch ? 2f : 1f, isFPS, false);
                    break;
                case IKAnimState.Run:
                    PlayIKMoveLocal(1f, isFPS, true);
                    break;
                case IKAnimState.Interact:
                    IsInteract = true; // Lock state immediately to prevent interruption
                    PlayIKInteractLocal(isFPS);
                    break;
            }
        }

        public void PickupAnimation(PlayerIKController ikController, bool isFPS)
        {
            ikController.IKPos(this, handL, handR, elbowL, elbowR, ikInteractSo);
            ikController.IkActive = true;
            if (isFPS)
            {
                _currentFPSIKController = ikController;
            }
            else
            {
                _currentTPSIKController = ikController;
            }
            transform.localPosition = ApplyPosOffset(Vector3.zero, isFPS);
            transform.localRotation = Quaternion.Euler(ApplyRotOffset(Vector3.zero, isFPS));

            // Trigger entry animation
            PlayIKIdleLocal(isFPS);
        }

        public void DropAnimation()
        {
            // Stop animation
            StopIKAnimation();

            // Reset state
            _currentState = IKAnimState.None;

            // Cleanup Controllers
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

        private void PlayIKIdleLocal(bool isFPS)
        {
            Debug.Log($"[{gameObject.name}] PlayIKIdleLocal called - isFPS:{isFPS}");

            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }

            localAnimTime = 0f;

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

        private void PlayIKMoveLocal(float slowSpeed, bool isFPS, bool isRunning)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }

            localAnimTime = 0f;

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

        private void PlayIKInteractLocal(bool isFPS)
        {
            Debug.Log($"[IKInteractable] PlayIKInteractLocal called - IsInteract:{IsInteract}, isFPS:{isFPS}");

            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }

            localAnimTime = 0f;
            IsInteract = true;

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

            seq.OnComplete(() =>
            {
                IsInteract = false;
                // Update state to Idle so next Interact isn't blocked by state caching
                _currentState = IKAnimState.Idle;
                // Return to idle after interact animation completes
                PlayIKIdleLocal(isFPS);
            });
            currentTween = seq;
        }

        public void StopIKAnimation()
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
        }

        private Vector3 ApplyPosOffset(Vector3 point, bool isFPS)
        {
            Vector3 offset = gripAnchor != null ? gripAnchor.localPosition : Vector3.zero;
            return point + offset;
        }

        private Vector3[] ApplyPosOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = gripAnchor != null ? gripAnchor.localPosition : Vector3.zero;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
        
        private Vector3 ApplyRotOffset(Vector3 point, bool isFPS)
        {
            Vector3 offset = gripAnchor != null ? gripAnchor.localEulerAngles : Vector3.zero;
            return point + offset;
        }

        private Vector3[] ApplyRotOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = gripAnchor != null ? gripAnchor.localEulerAngles : Vector3.zero;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
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