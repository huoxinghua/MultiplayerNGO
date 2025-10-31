using System;
using _Project.Code.Art.AnimationScripts.FingerPoseSOs;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKInteractable : MonoBehaviour
    {
        [Header("Hands and Fingers Position")] 
        [SerializeField] private Transform handR;

        [SerializeField] private Transform handL;
        [SerializeField] private ikInteractSO ikInteractSo;
        

        
        private Tween currentTween;

        public void PickupAnimation(PlayerIKController ikController)
        {
            ikController.IKPos(handL, handR, ikInteractSo);
            ikController.IkActive = true;
        }

        public void DropAnimation(PlayerIKController ikController)
        {
            ikController.IkActive = false;
            ikController.IKPos(null, null, null);
        }

        public void PlayIKIdle()
        {
            currentTween?.Kill();

            transform.DOLocalMove(ikInteractSo.ikIdle.waypoints[0], ikInteractSo.ikIdle.transitionDuration * 0.5f).SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ikInteractSo.ikIdle.waypoints[1], ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .Append(
                            transform.DOLocalMove(ikInteractSo.ikIdle.waypoints[0], ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .SetLoops(-1, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKWalk()
        {
            var revesreWayPoints = new[]
                { ikInteractSo.ikWalk.waypoints[2], ikInteractSo.ikWalk.waypoints[1], ikInteractSo.ikWalk.waypoints[0] };

            currentTween.Kill();

            transform.DOLocalMove(ikInteractSo.ikWalk.waypoints[0], ikInteractSo.ikWalk.transitionDuration * 0.5f).SetEase(ikInteractSo.ikWalk.easeType)
                .OnComplete(() =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(transform
                            .DOLocalPath(ikInteractSo.ikWalk.waypoints, ikInteractSo.ikWalk.loopDuration, ikInteractSo.ikWalk.pathType, ikInteractSo.ikWalk.pathMode)
                            .SetEase(ikInteractSo.ikWalk.easeType))
                        .SetLoops(-1, ikInteractSo.ikWalk.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKRun()
        {
            var revesreWayPoints = new[] { ikInteractSo.ikRun.waypoints[2], ikInteractSo.ikRun.waypoints[1], ikInteractSo.ikRun.waypoints[0] };

            currentTween.Kill();

            transform.DOLocalMove(ikInteractSo.ikRun.waypoints[0], ikInteractSo.ikRun.transitionDuration * 0.05f).SetEase(ikInteractSo.ikRun.easeType)
                .OnComplete(() =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(transform
                            .DOLocalPath(ikInteractSo.ikRun.waypoints, ikInteractSo.ikRun.loopDuration, ikInteractSo.ikRun.pathType, ikInteractSo.ikRun.pathMode)
                            .SetEase(ikInteractSo.ikRun.easeType))
                        .SetLoops(-1, ikInteractSo.ikRun.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKInteract()
        {
            currentTween.Kill();
            var seq = DOTween.Sequence();
            seq.Append(transform.DORotate(ikInteractSo.ikInteract.waypoints[0], ikInteractSo.ikInteract.transitionDuration * 2))
                .SetEase(ikInteractSo.ikInteract.easeAnti)
                .Append(transform.DOPunchRotation(ikInteractSo.ikInteract.waypoints[0], ikInteractSo.ikInteract.loopDuration))
                .SetEase(ikInteractSo.ikInteract.easeHit);
        }

        public void StopIKAnimation()
        {
            currentTween?.Kill();
            currentTween = null;
        }

        
    }
}


[Serializable]
public struct IdlePreset
{
    public float transitionDuration;
    public float loopDuration;
    public Vector3[] waypoints;
    public LoopType loopType;
    public Ease easeType;
}

[Serializable]
public struct MovementPreset
{
    public float transitionDuration;
    public float loopDuration;
    public Vector3[] waypoints;
    public LoopType loopType;
    public Ease easeType;
    public PathType pathType;
    public PathMode pathMode;
}

[Serializable]
public struct InteractPreset
{
    public float transitionDuration;
    public float loopDuration;
    public Vector3[] waypoints;
    public Ease easeAnti;
    public Ease easeHit;
}
