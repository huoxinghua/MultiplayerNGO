using _Project.Code.Art.AnimationScripts.FingerPoseSOs;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKInteractable : MonoBehaviour
    {
        [SerializeField] Transform handR;
        [SerializeField] Transform handL;
        [SerializeField] FingerPoseSO fingerSO;

        public void PickupAnimation(PlayerIKController ikController)
        {
            ikController.IKPos(handL, handR, fingerSO);
            ikController.IkActive = true;
        }

        public void DropAnimation(PlayerIKController ikController)
        {
            ikController.IkActive = false;
            ikController.IKPos(null, null, null);
        }
    }
}
