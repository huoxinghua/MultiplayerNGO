using UnityEngine;

public class BaseballBatModel : MonoBehaviour
{
    public bool IsInHand = false;
    float damage;
    public GameObject Owner;
    public void InHand(bool inHand)
    {
        IsInHand = inHand;
    }
    public float GetDamage()
    {
        return damage;
    }
    public void SetOwner(GameObject player)
    {
        Owner = player;
    }

    public void ClearOwner()
    {
        Owner = null;
    }

    public bool HasOwner => Owner != null;
}

