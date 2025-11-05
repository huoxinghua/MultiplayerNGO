using _Project.Code.Art.AnimationScripts.IKInteractSOs;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class PlayerIKController : MonoBehaviour
    {
        [SerializeField] private IKInteractable interactable;
        [SerializeField] private Animator animator;
        [SerializeField] private bool ikActive;

        private Transform handL;
        private Transform handR;
        private IkInteractSO fingerSO;

        public bool IkActive
        {
            get { return ikActive; }
            set { ikActive = value; }
        }

        private void OnAnimatorIK()
        {
            if (IkActive)
            {
                if (handR != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);


                    animator.SetIKPosition(AvatarIKGoal.RightHand, handR.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, handR.rotation);

                    ApplyFinger(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal, fingerSO.thumbR);
                    ApplyFinger(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal, fingerSO.indexR);
                    ApplyFinger(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal, fingerSO.middleR);
                    ApplyFinger(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal, fingerSO.ringR);
                    ApplyFinger(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal, fingerSO.littleR);
                }
                if (handL != null)
                {

                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

                    animator.SetIKPosition(AvatarIKGoal.LeftHand, handL.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, handL.rotation);

                    ApplyFinger(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal, fingerSO.thumbL);
                    ApplyFinger(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal, fingerSO.indexL);
                    ApplyFinger(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal, fingerSO.middleL);
                    ApplyFinger(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal, fingerSO.ringL);
                    ApplyFinger(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal, fingerSO.littleL);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

            }
        }

        public void IKPos(IKInteractable obj, Transform handLPos, Transform handRPos, IkInteractSO ikInteract)
        {
            interactable = obj;
            handL = handLPos;
            handR = handRPos;
            fingerSO = ikInteract;
            Debug.Log(interactable.gameObject.name);
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
