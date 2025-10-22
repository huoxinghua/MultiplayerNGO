using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimation : BaseAnimation
{
    [SerializeField] NetworkAnimator netAnim;

    protected override void UpdateMovement(float currentSpeed, float maxSpeed, bool isRunning)
    {
        base.UpdateMovement(currentSpeed, maxSpeed, isRunning);

        netAnim.Animator.SetFloat(hSpeed, currentSpeed / maxSpeed);
    }

    public override void PlayJump()
    {
        anim.SetTrigger(hJump);
        netAnim.Animator.SetBool(hCrouch, false);

        anim.SetTrigger(hJump);
        netAnim.Animator.SetBool(hCrouch, false);

    }

    public override void PlayCrouch()
    {
        anim.SetBool(hCrouch, true);
        netAnim.Animator.SetBool(hCrouch, true);
    }

    public override void PlayStanding()
    {
        anim.SetBool(hCrouch, false);
        netAnim.Animator.SetBool(hCrouch, false);
    }

    public override void PlayAttack()
    {

    }

    protected override IEnumerator SmoothWalkRun(float target)
    {
        float time = 0f;

        while (time < walkRunTransition && target != currentWalkRunType)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(currentWalkRunType, target, time / walkRunTransition);
            anim.SetFloat(hIsRunning, value);
            netAnim.Animator.SetFloat(hIsRunning, value);
            yield return null;
        }

        anim.SetFloat(hIsRunning, target);
        netAnim.Animator.SetFloat(hIsRunning, target);
        currentWalkRunType = target;
    }
}
