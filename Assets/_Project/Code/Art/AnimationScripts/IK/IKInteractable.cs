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
            public float duration;
            public Vector3[] waypoints;
            public LoopType loopType;
            public Ease easeType;
        }
        
        [System.Serializable]
        public struct MovementPreset
        {
            public float duration;
            public Vector3[] waypoints;
            public LoopType loopType;
            public Ease easeType;
            public PathType pathType;
            public PathMode pathMode;
        }
        
        [System.Serializable]
        public struct InteractPreset
        {
            public float duration;
            public Vector3 startPos;
            public Vector3 endPos;
            public Ease easeType;
        }
        #endregion

        private Tween currentTween;
        
        private void Awake()
        {
            
        }

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

            transform.DOLocalMove(ikIdle.waypoints[0], ikIdle.duration).SetEase(ikIdle.easeType).OnComplete(() =>
                {
                    Sequence seq = DOTween.Sequence();
                    seq.Append(transform.DOLocalMove(ikIdle.waypoints[1], ikIdle.duration).SetEase(ikIdle.easeType)).Append(transform.DOLocalMove(ikIdle.waypoints[0], ikIdle.duration).SetEase(ikIdle.easeType)).SetLoops(-1, ikIdle.loopType);
                    currentTween = seq;
                });
        }

        public void PlayIKWalk()
        {
            
        }

        public void PlayIKRun()
        {
            
        }

        public void PlayIKInteract()
        {
            
        }

        public void StopIKAnimation()
        {
            currentTween?.Kill();
            currentTween = null;
        }
    }
}
