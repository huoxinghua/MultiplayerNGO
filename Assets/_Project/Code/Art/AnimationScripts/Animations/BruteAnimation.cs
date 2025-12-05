using System.Collections;
using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public enum BruteAnimationState
    {
        Alert,
        Normal,
        Injured
    }
    public class BruteAnimation : EnemyAnimation
    {
        private int hStatus = Animator.StringToHash("bruteStatus");
        private int hInjured = Animator.StringToHash("isInjured");
        private float currentStatus = (float)BruteAnimationState.Normal;

        private void Start()
        {
            anim.SetFloat(hStatus, currentStatus);
        }

        public override void PlayRandomIdle(float currentIdleTime, float idleStart)
        {
        }

        public void PlayNormal()
        {
            ChangeStatus(BruteAnimationState.Normal);
            anim.SetBool(hAlert, false);
            anim.SetBool(hInjured, false);
        }

        public override void PlayAlert()
        {
            ChangeStatus(BruteAnimationState.Alert);
            anim.SetBool(hAlert, true);
            anim.SetBool(hInjured, false);
        }

        public void PlayInjured()
        {
            ChangeStatus(BruteAnimationState.Injured);
            anim.SetBool(hAlert, false);
            anim.SetBool(hInjured, true);
        }

        private void ChangeStatus(BruteAnimationState state)
        {
            StartCoroutine(SmoothStatusChange(state, 1));
        }

        public override void PlayAttack()
        {
            if (anim.GetBool(hAlert) && !anim.GetBool(hInjured))
            {
                anim.SetFloat(hAttackType, Random.Range(0, 2));
                anim.SetTrigger(hAttack);
            }
            else if (!anim.GetBool(hAlert) && anim.GetBool(hInjured))
            {
                anim.SetFloat(hAttackType, 2);
                anim.SetTrigger(hAttack);
            }
        }

        private IEnumerator SmoothStatusChange(BruteAnimationState targetStatus, float duration)
        {
            float targetValue = (float)targetStatus;
            float time = 0f;
        
            while (time < duration && targetValue != currentStatus)
            {
                time += Time.deltaTime;
                float value = Mathf.Lerp(currentStatus, targetValue, time / duration);
                anim.SetFloat(hStatus, value);
                yield return null;
            }

            anim.SetFloat(hStatus, targetValue);
            currentStatus = targetValue;
        }
    }
}