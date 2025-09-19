using UnityEngine;

public abstract class EnemyAnimation : BaseAnimation
{
    public virtual void PlayRandomIdle(float currentIdleTime, float idleStart)
    {
        if (idleIndex == 0) return;
        if (anim.GetBool(hIsIdle) && currentIdleTime > idleStart)
        {
            anim.SetFloat(hIdleSlot, Random.Range(0, idleIndex));
            anim.SetTrigger(hRandomIdle);
        }
    }

    public override void PlayAttack()
    {
        
    }

    public virtual void PlayAlert()
    {
        
    }

    public override void PlayJump()
    {
        
    }
}
