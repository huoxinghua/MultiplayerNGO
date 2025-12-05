using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public class DollAnimation : EnemyAnimation
    {
        public override void PlayRandomIdle(float currentIdleTime, float idleStart)
        {
        }

        public void PlaySwitchPose()
        {
            if (idleIndex == 0) return;
            anim.SetFloat(hIdleSlot, Random.Range(0, idleIndex));
            anim.SetTrigger(hRandomIdle);
        }
    }
}
