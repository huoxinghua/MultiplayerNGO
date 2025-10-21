using UnityEngine;

public abstract class EnemyAnimation : BaseAnimation
{
    [SerializeField] protected int idleIndex;
    public virtual void PlayRandomIdle(float currentIdleTime, float idleStart)
    {
        if (idleIndex == 0) return;
        if (anim.GetFloat(hSpeed) < 0.01 && currentIdleTime > idleStart)
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
