using System;
using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.NPC.Tranquil.Beetle;
using UnityEngine;

public class BeetlePickupRelay : MonoBehaviour, IInteractable
{
    [SerializeField] private BeetleDead BeetleDeadScript;

    private void Awake()
    {
        if (BeetleDeadScript == null)
        {
            BeetleDeadScript = GetComponentInParent<BeetleDead>();
        }
    }

    private BeetleDead GetBeetleDead()
    {
        // Re-fetch if null (parent hierarchy may have changed)
        if (BeetleDeadScript == null)
        {
            BeetleDeadScript = GetComponentInParent<BeetleDead>();
        }
        return BeetleDeadScript;
    }

    public void OnInteract(GameObject interactingPlayer)
    {
        var beetle = GetBeetleDead();
        if (beetle != null)
        {
            beetle.OnInteract(interactingPlayer);
        }
        else
        {
            Debug.LogError("[BeetlePickupRelay] BeetleDeadScript is null!");
        }
    }

    public void HandleHover(bool isHovering)
    {
        var beetle = GetBeetleDead();
        if (beetle != null)
        {
            beetle.HandleHover(isHovering);
        }
    }
}
