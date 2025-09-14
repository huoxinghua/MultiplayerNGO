using UnityEngine;

public class CombatAnimation : MovementAnimation
{    public virtual void OnAttack()
    {
        anim.SetTrigger("attack");
    }
}
