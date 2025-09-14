using UnityEngine;

public class EnemyBaseAnimation : MovementAnimation
{
    [SerializeField] int idleIndex;

    public virtual void OnIdleRandom(float currentIdleTime)
    {
        bool isIdle = anim.GetBool("isIdle");

        if (isIdle)
        {
            anim.SetFloat("idleSlot", Random.Range(0, idleIndex));
            if (currentIdleTime > 4)
            {
                anim.SetTrigger("RandomIdle");
            }
        }
    }
}

