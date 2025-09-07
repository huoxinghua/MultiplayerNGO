using UnityEngine;

public class BaseFlashLight : MonoBehaviour , IHeldItem
{
    public void Use()
    {
        Debug.Log("ToggleFlashLight");
    }
}
