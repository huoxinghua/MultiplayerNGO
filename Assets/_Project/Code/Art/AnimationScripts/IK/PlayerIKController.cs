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
            
            // ไม่ต้องใช้ rootTransform ในการคำนวณ Rotation แล้ว

            if (ikActive)
            {
                // World Position
                Vector3 finalHandRPosition = handR != null ? handR.position : Vector3.zero;
                Vector3 finalElbowRPosition = elbowR != null ? elbowR.position : Vector3.zero;
                
                Vector3 finalHandLPosition = handL != null ? handL.position : Vector3.zero;
                Vector3 finalElbowLPosition = elbowL != null ? elbowL.position : Vector3.zero;

                // World Rotation (ใช้ World Rotation จาก Target Transform โดยตรง)
                Quaternion finalHandRRotation = handR != null ? handR.rotation : Quaternion.identity;
                Quaternion finalHandLRotation = handL != null ? handL.rotation : Quaternion.identity;
                
                if (handR != null) 
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0); // <-- ปิด Hint เพื่อทดสอบ Rotation

                    animator.SetIKPosition(AvatarIKGoal.RightHand, finalHandRPosition);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, finalHandRRotation);
                    animator.SetIKHintPosition(AvatarIKHint.RightElbow, finalElbowRPosition);
                    
                    if (IsOwner)
                    {
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
                }

                if (handL != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0); // <-- ปิด Hint เพื่อทดสอบ Rotation

                    animator.SetIKPosition(AvatarIKGoal.LeftHand, finalHandLPosition);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, finalHandLRotation);
                    animator.SetIKHintPosition(AvatarIKHint.LeftElbow, finalElbowLPosition);

                    if (IsOwner)
                    {
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