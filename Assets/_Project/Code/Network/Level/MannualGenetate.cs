using DunGen;
using Unity.Netcode;
using UnityEngine;

public class MannualGenetate : NetworkBehaviour
{
    [SerializeField] private RuntimeDungeon generator;
    private void Start()
    {
        generator=GetComponent<RuntimeDungeon>();
        generator.Generate();
    }

}
