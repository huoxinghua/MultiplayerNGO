using UnityEngine;

public class BaseballBatModel
{
    public bool IsInHand = false;
    float damage;
    public GameObject Owner;

    //must set through something. Maybe SO
    float attackRange = 1;
    float attackRadius = 1;

    public float GetAttackRange()
    {
        return attackRange;
    }
    public float GetAttackRadius()
    {
        return attackRadius;
    }
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

