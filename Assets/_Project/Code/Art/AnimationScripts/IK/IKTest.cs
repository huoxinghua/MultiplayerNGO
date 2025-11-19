using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKTest : NetworkBehaviour
    {
        [SerializeField] private PlayerIKController fpsController;
        [SerializeField] private PlayerIKController tpsController;
        [SerializeField] private IKInteractable ik;
        [SerializeField] private float walkSpeed;
        [SerializeField] private bool isFPS;

        public bool GetOwner => IsOwner;

        void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.K))
            {
                if(IsOwner) ik.PickupAnimation(fpsController, isFPS);
                else ik.PickupAnimation(tpsController, isFPS);
            }
        
            if (Input.GetKeyDown(KeyCode.L))
            {
                if(IsOwner) ik.PlayIKIdle(isFPS);
                else ik.PlayIKIdle(isFPS);
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                if(IsOwner) ik.PlayIKWalk(walkSpeed, isFPS);
                else ik.PlayIKWalk(walkSpeed, isFPS);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                if(IsOwner) ik.PlayIKRun(isFPS);
                else ik.PlayIKRun(isFPS);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                if (IsOwner) ik.PlayIKInteract(isFPS);
                else ik.PlayIKInteract(isFPS);
            }*/
            
        }
    }
}
