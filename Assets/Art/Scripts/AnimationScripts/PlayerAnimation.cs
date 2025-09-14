using UnityEngine;

public class PlayerAnimation : CombatAnimation
{
    public virtual void OnJump()
    {
        anim.SetTrigger("jump");
    }
}
