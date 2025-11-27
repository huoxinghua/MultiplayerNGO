using System;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.NPC.Tranquil.Beetle;
using UnityEngine;

public class BeetlePickupRelay : MonoBehaviour, IInteractable
{
    private BeetleDead BeetleDeadScript;

    private void Awake()
    {
        BeetleDeadScript = GetComponentInParent<BeetleDead>();
    }

    public void OnInteract(GameObject interactingPlayer)
    {
        BeetleDeadScript.OnInteract(interactingPlayer);
    }
    public void HandleHover(bool isHovering)
    {
        BeetleDeadScript.HandleHover(isHovering);
    }
}
