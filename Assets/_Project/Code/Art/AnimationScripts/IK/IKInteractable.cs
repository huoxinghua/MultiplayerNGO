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
            public Vector3 startPos;
            public Vector3 endPos;
            public LoopType loopType;
            public Ease easeType;
        }
        
        [System.Serializable]
        public struct MovementPreset
        {
            public float duration;
            public Vector3 startPos;
            public Vector3 midPos;
            public Vector3 endPos;
            public LoopType loopType;
            public Ease easeType;
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
            transform.localPosition = ikIdle.startPos;
            currentTween = transform.DOLocalMove(ikIdle.endPos, ikIdle.duration).SetLoops(-1, ikIdle.loopType).SetEase(ikIdle.easeType);
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
