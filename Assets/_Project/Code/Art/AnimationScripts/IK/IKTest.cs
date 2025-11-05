using System;
using _Project.Code.Art.AnimationScripts.IK;
using UnityEngine;
using Unity.Netcode;

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
        Debug.Log("isowner:"+IsOwner);
        if (!IsOwner)
        {
            Debug.Log("isowne fALSE SKIP:");
            return;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            if(IsOwner) ik.PickupAnimation(fpsController, GetOwner);
            else ik.PickupAnimation(tpsController, !GetOwner);
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            if(IsOwner) ik.PlayIKIdle(GetOwner);
            else ik.PlayIKIdle(!GetOwner);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if(IsOwner) ik.PlayIKWalk(walkSpeed, GetOwner);
            else ik.PlayIKWalk(walkSpeed, !GetOwner);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if(IsOwner) ik.PlayIKRun(GetOwner);
            else ik.PlayIKRun(!GetOwner);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (IsOwner) ik.PlayIKInteract(GetOwner);
            else ik.PlayIKInteract(!GetOwner);
        }
            
    }
}
