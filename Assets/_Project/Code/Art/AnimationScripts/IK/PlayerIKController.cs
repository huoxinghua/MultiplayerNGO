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
        private IkInteractSO interactSO;
        
        public IKInteractable Interactable => interactable;

        public bool IkActive
        {
            get { return ikActive; }
            set { ikActive = value; }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if(layerIndex != 0) return;
            if(animator == null) return;


            if (ikActive)
            {
                Vector3 finalHandRPosition = handR != null ? handR.position : Vector3.zero;
                Vector3 finalElbowRPosition = elbowR != null ? elbowR.position : Vector3.zero;
                
                Vector3 finalHandLPosition = handL != null ? handL.position : Vector3.zero;
                Vector3 finalElbowLPosition = elbowL != null ? elbowL.position : Vector3.zero;

                Quaternion finalHandRRotation = handR != null ? handR.rotation : Quaternion.identity;
                Quaternion finalHandLRotation = handL != null ? handL.rotation : Quaternion.identity;
                
                if (handR != null) 
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);

                    animator.SetIKPosition(AvatarIKGoal.RightHand, finalHandRPosition);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, finalHandRRotation);
                    animator.SetIKHintPosition(AvatarIKHint.RightElbow, finalElbowRPosition);
                    
                    if (IsOwner)
                    {
                        ApplyFinger(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate,
                                    HumanBodyBones.RightThumbDistal, interactSO.thumbR);
                        ApplyFinger(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate,
                                    HumanBodyBones.RightIndexDistal, interactSO.indexR);
                        ApplyFinger(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate,
                                    HumanBodyBones.RightMiddleDistal, interactSO.middleR);
                        ApplyFinger(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate,
                                    HumanBodyBones.RightRingDistal, interactSO.ringR);
                        ApplyFinger(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate,
                                    HumanBodyBones.RightLittleDistal, interactSO.littleR);
                    }
                }

                if (handL != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);

                    animator.SetIKPosition(AvatarIKGoal.LeftHand, finalHandLPosition);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, finalHandLRotation);
                    animator.SetIKHintPosition(AvatarIKHint.LeftElbow, finalElbowLPosition);

                    if (IsOwner)
                    {
                        ApplyFinger(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate,
                                    HumanBodyBones.LeftThumbDistal, interactSO.thumbL);
                        ApplyFinger(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate,
                                    HumanBodyBones.LeftIndexDistal, interactSO.indexL);
                        ApplyFinger(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate,
                                    HumanBodyBones.LeftMiddleDistal, interactSO.middleL);
                        ApplyFinger(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate,
                                    HumanBodyBones.LeftRingDistal, interactSO.ringL);
                        ApplyFinger(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate,
                                    HumanBodyBones.LeftLittleDistal, interactSO.littleL);
                    }
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
            interactSO = ikInteract;
        }

        private void ApplyFinger(HumanBodyBones proximalBone, HumanBodyBones intermediateBone, HumanBodyBones distalBone, FingerData finger)
        {
            Transform proximal = animator.GetBoneTransform(proximalBone);
            Transform intermediate = animator.GetBoneTransform(intermediateBone);
            Transform distal = animator.GetBoneTransform(distalBone);

            if (proximal != null) animator.SetBoneLocalRotation(proximalBone, proximal.localRotation *= finger.proximal);
            if (intermediate != null) animator.SetBoneLocalRotation(intermediateBone, intermediate.localRotation *= finger.intermediate);
            if (distal != null) animator.SetBoneLocalRotation(distalBone, distal.localRotation *= finger.distal);
        }
    }
}