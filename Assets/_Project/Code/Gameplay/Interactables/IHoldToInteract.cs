using UnityEngine;

public interface IHoldToInteract
{
    public void OnHold(GameObject player);
    public void OnRelease(GameObject player);
}
