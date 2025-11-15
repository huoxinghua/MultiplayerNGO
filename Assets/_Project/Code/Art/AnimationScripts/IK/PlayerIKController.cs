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

        private void Update()
        {
            if (!IsOwner) return;

            Transform rootTransform = transform; // PlayerCharacter_NW Root

            if (handR != null)
            {
                netHandRPosition.Value = rootTransform.InverseTransformPoint(handR.position);
                netHandRRotation.Value = Quaternion.Inverse(rootTransform.rotation) * handR.rotation;
                netElbowRPosition.Value = rootTransform.InverseTransformPoint(elbowR.position);
            }

            if (handL != null)
            {
                netHandLPosition.Value = rootTransform.InverseTransformPoint(handL.position);
                netHandLRotation.Value = Quaternion.Inverse(rootTransform.rotation) * handL.rotation;
                netElbowLPosition.Value = rootTransform.InverseTransformPoint(elbowL.position);
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
            
            Transform rootTransform = transform; // PlayerCharacter_NW Root

            if (ikActive)
            {
                Vector3 finalHandRPosition;
                Quaternion finalHandRRotation;
                Vector3 finalElbowRPosition;
                
                Vector3 finalHandLPosition;
                Quaternion finalHandLRotation;
                Vector3 finalElbowLPosition;

                if (IsOwner)
                {
                    finalHandRPosition = handR != null ? handR.position : Vector3.zero;
                    finalHandRRotation = handR != null ? handR.rotation : Quaternion.identity;
                    finalElbowRPosition = elbowR != null ? elbowR.position : Vector3.zero;
                    
                    finalHandLPosition = handL != null ? handL.position : Vector3.zero;
                    finalHandLRotation = handL != null ? handL.rotation : Quaternion.identity;
                    finalElbowLPosition = elbowL != null ? elbowL.position : Vector3.zero;
                }
                else
                {
                    finalHandRPosition = rootTransform.TransformPoint(netHandRPosition.Value);
                    finalHandRRotation = rootTransform.rotation * netHandRRotation.Value;
                    finalElbowRPosition = rootTransform.TransformPoint(netElbowRPosition.Value);
                    
                    finalHandLPosition = rootTransform.TransformPoint(netHandLPosition.Value);
                    finalHandLRotation = rootTransform.rotation * netHandLRotation.Value;
                    finalElbowLPosition = rootTransform.TransformPoint(netElbowLPosition.Value);
                }

                if (IsOwner && handR != null || !IsOwner) 
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);

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

                if (IsOwner && handL != null || !IsOwner)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);

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