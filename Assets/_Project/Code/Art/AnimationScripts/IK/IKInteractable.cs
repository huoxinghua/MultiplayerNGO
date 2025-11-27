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
        [SerializeField] private IKItemAnimation ikAnim;
        private bool currentCrouch;
        private PlayerIKController _currentFPSIKController;
        private PlayerIKController _currentTPSIKController;
        
        public bool IsInteractComplete => ikAnim.IsInteractComplete;
        

        public void SetAnimState(IKAnimState newState, bool isFPS, bool isCrouch)
        {
            // Kill any existing animation before starting new one
            ikAnim.StopIKAnimation();

            currentCrouch = isCrouch;

            switch (newState)
            {
                case IKAnimState.None:
                    ikAnim.StopIKAnimation();
                    break;
                case IKAnimState.Idle:
                    ikAnim.PlayIKIdle(isFPS);
                    break;
                case IKAnimState.Walk:
                    ikAnim.PlayIKMove(currentCrouch ? 2f : 1f, isFPS, false);
                    break;
                case IKAnimState.Run:
                    ikAnim.PlayIKMove(1f, isFPS, true);
                    break;
                case IKAnimState.Interact:
                    ikAnim.PlayIKInteract(isFPS);
                    break;
            }
        }

        public void SetAnimState(IKAnimState newState, bool isFPS)
        {
            // Kill any existing animation before starting new one
            ikAnim.StopIKAnimation();

            switch (newState)
            {
                case IKAnimState.None:
                    ikAnim.StopIKAnimation();
                    break;
                case IKAnimState.Idle:
                    ikAnim.PlayIKIdle(isFPS);
                    break;
                case IKAnimState.Walk:
                    ikAnim.PlayIKMove(currentCrouch ? 2f : 1f, isFPS, false);
                    break;
                case IKAnimState.Run:
                    ikAnim.PlayIKMove(1f, isFPS, true);
                    break;
                case IKAnimState.Interact:
                    ikAnim.PlayIKInteract(isFPS);
                    break;
            }
        }

        public void PickupAnimation(PlayerIKController ikController, bool isFPS)
        {
            // Clear any existing controller reference for this view FIRST
            // This prevents phantom hands when ownership changes between players
            if (isFPS && _currentFPSIKController != null && _currentFPSIKController != ikController)
            {
                Debug.Log($"[{gameObject.name}] PickupAnimation clearing OLD FPS controller - Player: {_currentFPSIKController.transform.root.name}");
                _currentFPSIKController.IkActive = false;
                _currentFPSIKController.IKPos(null, null, null, null, null, null);
                _currentFPSIKController = null;
            }
            else if (!isFPS && _currentTPSIKController != null && _currentTPSIKController != ikController)
            {
                Debug.Log($"[{gameObject.name}] PickupAnimation clearing OLD TPS controller - Player: {_currentTPSIKController.transform.root.name}");
                _currentTPSIKController.IkActive = false;
                _currentTPSIKController.IKPos(null, null, null, null, null, null);
                _currentTPSIKController = null;
            }

            // Now set the new controller
            ikController.IKPos(this, handL, handR, elbowL, elbowR, ikAnim.ikInteractSo);
            ikController.IkActive = true;
            if (isFPS)
            {
                _currentFPSIKController = ikController;
            }
            else
            {
                _currentTPSIKController = ikController;
            }
            transform.localPosition = ikAnim.ApplyPosOffset(Vector3.zero, isFPS);
            transform.localRotation = Quaternion.Euler(ikAnim.ApplyRotOffset(Vector3.zero, isFPS));

            SetAnimState(IKAnimState.Idle, isFPS);
        }

        public void DropAnimation()
        {
            ikAnim.StopIKAnimation();

            if (_currentFPSIKController != null)
            {
                Debug.Log($"[{gameObject.name}] DropAnimation clearing FPS - Player: {_currentFPSIKController.transform.root.name}");
                _currentFPSIKController.IkActive = false;
                _currentFPSIKController.IKPos(null, null, null, null, null, null);
                _currentFPSIKController =  null;
            }

            if (_currentTPSIKController != null)
            {
                Debug.Log($"[{gameObject.name}] DropAnimation clearing TPS - Player: {_currentTPSIKController.transform.root.name}");
                _currentTPSIKController.IkActive = false;
                _currentTPSIKController.IKPos(null, null, null, null, null, null);
                _currentTPSIKController =  null;
            }
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