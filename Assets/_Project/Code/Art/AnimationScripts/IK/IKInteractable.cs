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

        public void PlayIKIdle(bool isFPS)
        {
            currentTween?.Kill();

            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyOffset(Vector3.zero, isFPS))
            {
                duration = ikInteractSo.ikIdle.resetDuration;
            }

            transform.DOLocalMove(ApplyOffset(ikInteractSo.ikIdle.waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ApplyOffset(ikInteractSo.ikIdle.waypoints[1], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .Append(transform.DOLocalMove(ApplyOffset(ikInteractSo.ikIdle.waypoints[0], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType))
                        .SetLoops(-1, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKWalk(bool isFPS)
        {
            currentTween.Kill();

            transform.DOLocalMove(ApplyOffset(ikInteractSo.ikWalk.waypoints[0], isFPS), ikInteractSo.ikWalk.transitionDuration).SetEase(ikInteractSo.ikWalk.easeType)
                .OnComplete(() =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalPath(ApplyOffset(ikInteractSo.ikWalk.waypoints, isFPS), ikInteractSo.ikWalk.loopDuration, ikInteractSo.ikWalk.pathType, ikInteractSo.ikWalk.pathMode)
                            .SetEase(ikInteractSo.ikWalk.easeType))
                            .SetLoops(-1, ikInteractSo.ikWalk.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKRun(bool isFPS)
        {
            currentTween.Kill();

            transform.DOLocalMove(ApplyOffset(ikInteractSo.ikRun.waypoints[0], isFPS), ikInteractSo.ikRun.transitionDuration).SetEase(ikInteractSo.ikRun.easeType)
                .OnComplete(() =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalPath(ApplyOffset(ikInteractSo.ikRun.waypoints, isFPS), ikInteractSo.ikRun.loopDuration, ikInteractSo.ikRun.pathType, ikInteractSo.ikRun.pathMode)
                            .SetEase(ikInteractSo.ikRun.easeType))
                            .SetLoops(-1, ikInteractSo.ikRun.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKInteract(bool isFPS)
        {
            currentTween.Kill();
            var seq = DOTween.Sequence();
            seq.Append(transform.DOLocalMove(ApplyOffset(ikInteractSo.ikInteract.posWaypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration))
                    .SetEase(ikInteractSo.ikInteract.easeAnti)
                .Join(transform.DOLocalRotate(ApplyOffset(ikInteractSo.ikInteract.rotWaypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration))
                    .SetEase(ikInteractSo.ikInteract.easeAnti)
                .Append(transform.DOLocalMove(ApplyOffset(ikInteractSo.ikInteract.posWaypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration))
                    .SetEase(ikInteractSo.ikInteract.easeHit)    
                .Join(transform.DOLocalRotate(ApplyOffset(ikInteractSo.ikInteract.rotWaypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration))
                    .SetEase(ikInteractSo.ikInteract.easeHit)
                .Append(transform.DOLocalMove(ApplyOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.hitDuration))
                    .SetEase(ikInteractSo.ikInteract.easeHit)
                .Join(transform.DOLocalRotate(ApplyOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration))
                    .SetEase(ikInteractSo.ikInteract.easeAnti);
        }

        public void StopIKAnimation()
        {
            currentTween?.Kill();
            currentTween = null;
        }

        private Vector3 ApplyOffset(Vector3 point, bool isFPS)
        {
            return point + (isFPS? ikInteractSo.fpsObjectPosition : ikInteractSo.tpsObjectPosition);
        }

        private Vector3[] ApplyOffset(Vector3[] points, bool isFPS)
        {
            Vector3 offset = isFPS? ikInteractSo.fpsObjectPosition : ikInteractSo.tpsObjectPosition;
            Vector3[] result = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = points[i] + offset;
            }

            return result;
        }
    }
}


[Serializable]
public struct IdlePreset
{
    public float resetDuration;
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
    public float resetDuration;
    public float transitionDuration;
    public float hitDuration;
    public Vector3[] posWaypoints;
    public Vector3[] rotWaypoints;
    public Ease easeAnti;
    public Ease easeHit;
}
