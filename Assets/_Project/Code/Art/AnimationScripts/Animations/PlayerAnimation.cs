using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public class PlayerAnimation : BaseAnimation
    {
        [SerializeField] NetworkAnimator netAnim;

        protected int hJump = Animator.StringToHash("jump");
        protected int hInAir = Animator.StringToHash("isInAir");
        protected int hIsGround = Animator.StringToHash("isGrounded");
        protected int hCrouch = Animator.StringToHash("isCrouch");

        protected override void Awake()
        {
            base.Awake();
            anim.SetBool(hIsGround, true);
            anim.SetBool(hInAir, false);
            netAnim.Animator.SetBool(hInAir, false);
            netAnim.Animator.SetBool(hIsGround, true);
        }

        protected override void UpdateMovement(float currentSpeed, float maxSpeed, bool isRunning)
        {
            base.UpdateMovement(currentSpeed, maxSpeed, isRunning);

            netAnim.Animator.SetFloat(hSpeed, currentSpeed / maxSpeed);
        }

        public void PlayJump()
        {
            anim.SetTrigger(hJump);
            netAnim.Animator.SetTrigger(hJump);
        }

        public void PlayCrouch()
        {
            anim.SetBool(hCrouch, true);
            netAnim.Animator.SetBool(hCrouch, true);
        }

        public void PlayStanding()
        {
            anim.SetBool(hCrouch, false);
            netAnim.Animator.SetBool(hCrouch, false);
        }

        public override void PlayAttack()
        {

        }

        public void PlayInAir()
        {
            anim.SetBool(hInAir, true);
            anim.SetBool(hIsGround, false);
            netAnim.Animator.SetBool(hInAir, true);
            netAnim.Animator.SetBool(hIsGround, false);
        }

        public void PlayLand()
        {
            anim.ResetTrigger(hJump);
            netAnim.Animator.ResetTrigger(hJump);
            anim.SetBool(hIsGround, true);
            anim.SetBool(hInAir, false);
            netAnim.Animator.SetBool(hIsGround, true);
            netAnim.Animator.SetBool(hInAir, false);
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
}
