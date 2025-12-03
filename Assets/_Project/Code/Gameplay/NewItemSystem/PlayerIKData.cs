using Unity.Netcode;
using UnityEngine;
using System.Collections;
using _Project.Code.Art.AnimationScripts.IK;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class PlayerIKData : MonoBehaviour
    {
        [field: SerializeField] public PlayerIKController FPSIKController { get; private set;}
        [field: SerializeField] public PlayerIKController TPSIKController { get; private set;}
    }
}