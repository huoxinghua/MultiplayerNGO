using UnityEngine;

public class FlashModel
{
    public bool IsOn = false;
    public bool IsInHand = false;
    public GameObject Owner;

    public void Toggle()
    {
        IsOn = !IsOn;
    }
    public void InHand(bool inHand)
    {
        IsInHand = inHand;
    }
    public void SetOwner(GameObject player)
    {
        Owner = player;
    }

    public void ClearOwner()
    {
        Owner = null;
        IsOn = false;
    }

    public bool HasOwner => Owner != null;
}
