using System;
using _Project.Code.Art.AnimationScripts.FingerPoseSOs;
using DG.Tweening;
using UnityEngine;


namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKInteractable : MonoBehaviour
    {
        #region Hands and Fingers Position
        [Header("Hands and Fingers Position")]
        [SerializeField] private Transform handR;
        [SerializeField] private Transform handL;
        [SerializeField] FingerPoseSO fingerSo;
        #endregion
        
        [Header("---------------------------------")]

        #region IK Animation

        [Header("IK Animation Preset")]
        [SerializeField] public IdlePreset ikIdle;
        [SerializeField] private MovementPreset ikWalk;
        [SerializeField] private MovementPreset ikRun;
        [SerializeField] private InteractPreset ikInteract;
        
        [System.Serializable]
        public struct IdlePreset
        {
            public float transitionDuration;
            public float loopDuration;
            public Vector3[] waypoints;
            public LoopType loopType;
            public Ease easeType;
        }
        
        [System.Serializable]
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
        
        [System.Serializable]
        public struct InteractPreset
        {
            public float transitionDuration;
            public float loopDuration;
            public Vector3[] waypoints;
            public Ease easeAnti;
            public Ease easeHit;
        }
        #endregion

        private Tween currentTween;

        public void PickupAnimation(PlayerIKController ikController)
        {
            ikController.IKPos(handL, handR, fingerSo);
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

            transform.DOLocalMove(ikIdle.waypoints[0], ikIdle.transitionDuration * 0.5f).SetEase(ikIdle.easeType).OnComplete(() =>
                {
                    Sequence seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ikIdle.waypoints[1], ikIdle.loopDuration).SetEase(ikIdle.easeType))
                        .Append(transform.DOLocalMove(ikIdle.waypoints[0], ikIdle.loopDuration).SetEase(ikIdle.easeType))
                        .SetLoops(-1, ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKWalk()
        {
            Vector3[] revesreWayPoints = new Vector3[] {ikWalk.waypoints[2],  ikWalk.waypoints[1], ikWalk.waypoints[0]};
            
            currentTween.Kill();
            
            transform.DOLocalMove(ikWalk.waypoints[0], ikWalk.transitionDuration * 0.5f).SetEase(ikWalk.easeType).OnComplete(() =>
                {
                    Sequence seq = DOTween.Sequence();
                    currentTween = seq;
                    seq.Append(transform.DOLocalPath(ikWalk.waypoints, ikWalk.loopDuration, ikWalk.pathType, ikWalk.pathMode).SetEase(ikWalk.easeType))
                        .Append(transform.DOLocalPath(revesreWayPoints, ikWalk.loopDuration, ikWalk.pathType, ikWalk.pathMode).SetEase(ikWalk.easeType))
                        .SetLoops(-1, ikWalk.loopType);
                });
        }

        public void PlayIKRun()
        {
            Vector3[] revesreWayPoints = new Vector3[] {ikRun.waypoints[2],  ikRun.waypoints[1], ikRun.waypoints[0]};
            
            currentTween.Kill();
            
            transform.DOLocalMove(ikRun.waypoints[0], ikRun.transitionDuration * 0.05f).SetEase(ikRun.easeType).OnComplete(() =>
            {
                Sequence seq = DOTween.Sequence();
                currentTween = seq;
                seq.Append(transform.DOLocalPath(ikRun.waypoints, ikRun.loopDuration, ikRun.pathType, ikRun.pathMode).SetEase(ikRun.easeType))
                    .Append(transform.DOLocalPath(revesreWayPoints, ikRun.loopDuration, ikRun.pathType, ikRun.pathMode).SetEase(ikRun.easeType))
                    .SetLoops(-1, ikRun.loopType);
            });
        }

        public void PlayIKInteract()
        {
            currentTween.Kill();
                Sequence seq = DOTween.Sequence();
                seq.Append(transform.DORotate(ikInteract.waypoints[0], ikInteract.transitionDuration * 2)).SetEase(ikInteract.easeAnti)
                    .Append(transform.DOPunchRotation(ikInteract.waypoints[0], ikInteract.loopDuration)).SetEase(ikInteract.easeHit);
        }

        public void StopIKAnimation()
        {
            currentTween?.Kill();
            currentTween = null;
        }
    }
}
