using System;
using _Project.Code.Art.AnimationScripts.IK;
using UnityEngine;

public class IKTest : MonoBehaviour
{
    [SerializeField] private PlayerIKController fpsController;
    [SerializeField] private PlayerIKController tpsController;
    [SerializeField] private IKInteractable ik;
    [SerializeField] private bool isFPS;

    private void Awake()
    {
        ik.PickupAnimation(isFPS ? fpsController : tpsController, isFPS);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ik.PlayIKIdle(isFPS);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            ik.PlayIKWalk(isFPS);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ik.PlayIKRun(isFPS);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ik.PlayIKInteract(isFPS);
        }
            
    }
}
