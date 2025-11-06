using System;
using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKInteractable : MonoBehaviour
    {
        [Header("Hands and Fingers Position")] 
        [SerializeField] private Transform handR;
        [SerializeField] private Transform handL;
        [SerializeField] private IkInteractSO ikInteractSo; 
        
        private Tween currentTween;

        public void PickupAnimation(PlayerIKController ikController, bool isFPS)
        {
            ikController.IKPos(this, handL, handR, ikInteractSo);
            ikController.IkActive = true;
            transform.localPosition = ApplyPosOffset(Vector3.zero, isFPS);
            transform.localRotation = Quaternion.Euler(ApplyRotOffset(Vector3.zero, isFPS));
            PlayIKIdle(isFPS);
        }

        public void DropAnimation(PlayerIKController ikController)
        {
            ikController.IkActive = false;
            ikController.IKPos(null, null, null, null);
        }

        public void PlayIKIdle(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = ikInteractSo.ikIdle.resetDuration;
            else duration = ikInteractSo.ikIdle.transitionDuration;

            transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikIdle.waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikIdle.easeType)
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikIdle.waypoints[1], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType)).Append(transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikIdle.waypoints[0], isFPS), ikInteractSo.ikIdle.loopDuration).SetEase(ikInteractSo.ikIdle.easeType)).SetLoops(-1, ikInteractSo.ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKWalk(float slowSpeed,bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration * slowSpeed;
            
            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = ikInteractSo.ikWalk.resetDuration;
            else duration = ikInteractSo.ikWalk.transitionDuration;

            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikWalk.waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikWalk.easeType)).Join(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikWalk.followThrough[0], isFPS), duration).SetEase(ikInteractSo.ikWalk.easeType))
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyPosOffset(ikInteractSo.ikWalk.waypoints, isFPS), ikInteractSo.ikWalk.loopDuration, ikInteractSo.ikWalk.pathType, ikInteractSo.ikWalk.pathMode).SetEase(ikInteractSo.ikWalk.easeType).SetLoops(-1, ikInteractSo.ikWalk.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                        rotateSeq.Append(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikWalk.followThrough[1], isFPS), ikInteractSo.ikWalk.loopDuration).SetEase(ikInteractSo.ikWalk.easeType)).Append(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikWalk.followThrough[0], isFPS), ikInteractSo.ikWalk.loopDuration).SetEase(ikInteractSo.ikWalk.easeType)).SetLoops(-1, ikInteractSo.ikWalk.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                    Debug.Log($"[IK] currentTween set, Tween Active: { (currentTween != null && DOTween.IsTweening(currentTween)) }");
                });
        }

        public void PlayIKRun(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = ikInteractSo.ikRun.resetDuration;
            else duration = ikInteractSo.ikRun.transitionDuration;

            var startSeq = DOTween.Sequence();
            startSeq.Append(transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikRun.waypoints[0], isFPS), duration).SetEase(ikInteractSo.ikRun.easeType)).Join(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikRun.followThrough[0], isFPS), duration).SetEase(ikInteractSo.ikRun.easeType))
                .OnComplete(() =>
                {
                    if(currentTween != null)
                    {
                        currentTween.Kill(true);
                        currentTween = null;
                    }
                    var seq = DOTween.Sequence();

                    var moveTween = transform.DOLocalPath(ApplyPosOffset(ikInteractSo.ikRun.waypoints, isFPS), ikInteractSo.ikRun.loopDuration, ikInteractSo.ikRun.pathType, ikInteractSo.ikRun.pathMode).SetEase(ikInteractSo.ikRun.easeType).SetLoops(-1, ikInteractSo.ikRun.loopType);

                    seq.Append(moveTween);

                    var rotateSeq = DOTween.Sequence();
                    rotateSeq.Append(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikRun.followThrough[1], isFPS), ikInteractSo.ikRun.loopDuration).SetEase(ikInteractSo.ikRun.easeType)).Append(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikRun.followThrough[0], isFPS), ikInteractSo.ikRun.loopDuration).SetEase(ikInteractSo.ikRun.easeType)).SetLoops(-1, ikInteractSo.ikRun.loopType);

                    seq.Join(rotateSeq);

                    currentTween = seq;
                });
            
        }

        public void PlayIKInteract(bool isFPS)
        {
            if(currentTween != null)
            {
                currentTween.Kill(true);
                currentTween = null;
            }
            float duration = ikInteractSo.ikIdle.transitionDuration;

            if (transform.localPosition != ApplyPosOffset(Vector3.zero, isFPS)) duration = ikInteractSo.ikInteract.resetDuration;
            else duration = ikInteractSo.ikInteract.transitionDuration;

            var seq = DOTween.Sequence();

            seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), duration).SetEase(ikInteractSo.ikInteract.easeAnti)).Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), duration).SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikInteract.posWaypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration).SetEase(ikInteractSo.ikInteract.easeAnti)).Join(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikInteract.rotWaypoints[0], isFPS), ikInteractSo.ikInteract.transitionDuration).SetEase(ikInteractSo.ikInteract.easeAnti));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(ikInteractSo.ikInteract.posWaypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration).SetEase(ikInteractSo.ikInteract.easeHit)).Join(transform.DOLocalRotate(ApplyRotOffset(ikInteractSo.ikInteract.rotWaypoints[1], isFPS), ikInteractSo.ikInteract.hitDuration).SetEase(ikInteractSo.ikInteract.easeHit));

            seq.Append(transform.DOLocalMove(ApplyPosOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration).SetEase(ikInteractSo.ikInteract.easeHit)).Join(transform.DOLocalRotate(ApplyRotOffset(Vector3.zero, isFPS), ikInteractSo.ikInteract.transitionDuration).SetEase(ikInteractSo.ikInteract.easeHit));

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
    public Vector3[] waypoints;
    public LoopType loopType;
    public Ease easeType;
}

[Serializable]
public struct MovementPreset
{
    public float resetDuration;
    public float transitionDuration;
    public float loopDuration;
    public Vector3[] waypoints;
    public Vector3[] followThrough;
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
