using System;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using UnityEngine;

public class TempAnimMove : MonoBehaviour
{
    private BruteStateMachine _stateMachine;
    public void Awake()
    {
        _stateMachine = GetComponentInParent<BruteStateMachine>();
    }

    public void OnAnimatorMove()
    {
        _stateMachine?.TempAnimMove();
    }
}
