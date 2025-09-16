using UnityEngine;

public interface IHitable
{
    public void OnHit(GameObject attacker, float damage, float knockoutPower);
}
