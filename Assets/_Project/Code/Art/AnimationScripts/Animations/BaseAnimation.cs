using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public abstract class BaseAnimation : NetworkBehaviour
    {
        [SerializeField] protected float walkRunTransition = 1f;
        protected Animator anim;

        protected float currentWalkRunType = 0;

        protected int hSpeed = Animator.StringToHash("speed");
        protected int hIsRunning = Animator.StringToHash("locomotionType");
        protected int hAttack = Animator.StringToHash("attack");
        protected int hAttackType = Animator.StringToHash("attackType");

        public Animator GetAnimator()
        {
            return anim;
        }

        protected virtual void Awake()
        {
            anim ??= GetComponent<Animator>();
            if (anim == null) Debug.Log("Animator not found on" + gameObject.name);
        }

        public virtual void PlayWalk(float currentSpeed, float maxSpeed) => UpdateMovement(currentSpeed, maxSpeed, isRunning: false);

        public virtual void PlayRun(float currentSpeed, float maxSpeed) => UpdateMovement(currentSpeed, maxSpeed, isRunning: true);

        protected virtual void UpdateMovement(float currentSpeed, float maxSpeed, bool isRunning)
        {
            if (isRunning) StartCoroutine(SmoothWalkRun(1));
            else StartCoroutine(SmoothWalkRun(0));
            anim.SetFloat(hSpeed, currentSpeed / maxSpeed);
        }


        protected virtual IEnumerator SmoothWalkRun(float target)
        {
            float time = 0f;

            while (time < walkRunTransition && target != currentWalkRunType)
            {
                time += Time.deltaTime;
                float value = Mathf.Lerp(currentWalkRunType, target, time / walkRunTransition);
                anim.SetFloat(hIsRunning, value);
                yield return null;
            }

            anim.SetFloat(hIsRunning, target);
            currentWalkRunType = target;
        }
    }
}
