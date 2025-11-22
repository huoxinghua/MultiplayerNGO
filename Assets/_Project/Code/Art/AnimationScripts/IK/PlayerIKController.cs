using System;
using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using Unity.Netcode;
using UnityEngine;


namespace _Project.Code.Art.AnimationScripts.IK
{
    public class PlayerIKController : NetworkBehaviour
    {
        [SerializeField] private IKInteractable interactable;
        [SerializeField] private Animator animator;
        [SerializeField] private bool ikActive;

        private Transform handL, handR, elbowL, elbowR;
        private IkInteractSO fingerSO;
        
        private NetworkVariable<Vector3> netHandRPosition = new  NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private NetworkVariable<Quaternion> netHandRRotation = new  NetworkVariable<Quaternion>(
            Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private NetworkVariable<Vector3> netElbowRPosition = new  NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private NetworkVariable<Vector3> netHandLPosition = new  NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private NetworkVariable<Quaternion> netHandLRotation = new  NetworkVariable<Quaternion>(
            Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private NetworkVariable<Vector3> netElbowLPosition = new  NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        public IKInteractable Interactable => interactable;

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            if (handR != null)
            {
                netHandRPosition.Value = handR.position;
                netHandRRotation.Value = handR.rotation;
                netElbowRPosition.Value = elbowR.position;
            }

            if (handL != null)
            {
                netHandLPosition.Value = handL.position;
                netHandLRotation.Value = handL.rotation;
                netElbowLPosition.Value = elbowL.position;
            }
        }

        public bool IkActive
        {
            get { return ikActive; }
            set { ikActive = value; }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if(layerIndex != 0) return;
            if(animator == null) return;
            if (handR == null && handL == null) return;
            
            if (ikActive)
            {
                if (handR != null)
                {
                    if (IsOwner)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, handR.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, handR.rotation);
                        animator.SetIKHintPosition(AvatarIKHint.RightElbow, elbowR.position);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, netHandRPosition.Value);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, netHandRRotation.Value);
                        animator.SetIKHintPosition(AvatarIKHint.RightElbow, netElbowRPosition.Value);
                    }

                    ApplyFinger(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate,
                        HumanBodyBones.RightThumbDistal, fingerSO.thumbR);
                    ApplyFinger(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate,
                        HumanBodyBones.RightIndexDistal, fingerSO.indexR);
                    ApplyFinger(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate,
                        HumanBodyBones.RightMiddleDistal, fingerSO.middleR);
                    ApplyFinger(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate,
                        HumanBodyBones.RightRingDistal, fingerSO.ringR);
                    ApplyFinger(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate,
                        HumanBodyBones.RightLittleDistal, fingerSO.littleR);
                }

                if (handL != null)
                {
                    if (IsOwner)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, handL.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, handL.rotation);
                        animator.SetIKHintPosition(AvatarIKHint.LeftElbow, elbowL.position);
                    }
                    else
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, netHandLPosition.Value);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, netHandLRotation.Value);
                        animator.SetIKHintPosition(AvatarIKHint.LeftElbow, netElbowLPosition.Value);
                    }

                    ApplyFinger(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate,
                        HumanBodyBones.LeftThumbDistal, fingerSO.thumbL);
                    ApplyFinger(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate,
                        HumanBodyBones.LeftIndexDistal, fingerSO.indexL);
                    ApplyFinger(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate,
                        HumanBodyBones.LeftMiddleDistal, fingerSO.middleL);
                    ApplyFinger(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate,
                        HumanBodyBones.LeftRingDistal, fingerSO.ringL);
                    ApplyFinger(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate,
                        HumanBodyBones.LeftLittleDistal, fingerSO.littleL);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            }
        }

        public void IKPos(IKInteractable obj, Transform handLPos, Transform handRPos, Transform elbowLPos, Transform elbowRPos, IkInteractSO ikInteract)
        {
            interactable = obj;
            handL = handLPos;
            handR = handRPos;
            elbowL = elbowLPos;
            elbowR = elbowRPos;
            fingerSO = ikInteract;
           // Debug.Log(interactable.gameObject.name);
        }

        private void ApplyFinger(HumanBodyBones proximalBone, HumanBodyBones intermediateBone, HumanBodyBones distalBone, FingerData finger)
        {
            Transform proximal = animator.GetBoneTransform(proximalBone);
            Transform intermediate = animator.GetBoneTransform(intermediateBone );
            Transform distal = animator.GetBoneTransform(distalBone);

            if (proximal != null) animator.SetBoneLocalRotation(proximalBone, proximal.localRotation *= finger.proximal);
            if (intermediate != null) animator.SetBoneLocalRotation(intermediateBone, intermediate.localRotation *= finger.intermediate);
            if (distal != null) animator.SetBoneLocalRotation(distalBone, distal.localRotation *= finger.distal);
        }
    }
}
