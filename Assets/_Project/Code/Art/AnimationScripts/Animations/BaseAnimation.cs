using System.Collections;
using UnityEngine;

public abstract class BaseAnimation : MonoBehaviour
{
    [SerializeField] protected float walkRunTransition = 1f;
    protected Animator anim;

    protected float currentWalkRunType = 0;

    protected int hSpeed = Animator.StringToHash("speed");
    protected int hIsRunning = Animator.StringToHash("locomotionType");
    protected int hAttack = Animator.StringToHash("attack");
    protected int hAttackType = Animator.StringToHash("attackType");


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

    public abstract void PlayAttack();


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
