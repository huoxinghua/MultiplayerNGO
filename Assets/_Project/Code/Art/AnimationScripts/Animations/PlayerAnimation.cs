using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimation : BaseAnimation
{
    [SerializeField] Animator fpsAnim;
    [SerializeField] NetworkAnimator netAnim;

    protected override void Awake()
    {
        base.Awake();

        if (netAnim != null) netAnim.Animator = fpsAnim;
    }

    public override void PlayWalk(float currentSpeed, float maxSpeed)
    {
        base.PlayWalk(currentSpeed, maxSpeed);
    }
    public override void PlayJump()
    {
        fpsAnim.SetTrigger(hJump);
    }

    public override void PlayCrouch()
    {
        fpsAnim.SetBool(hCrouch, true);
    }

    public override void PlayStanding()
    {
        fpsAnim.SetBool(hCrouch, false);
    }

    public override void PlayAttack()
    {

    }
}
