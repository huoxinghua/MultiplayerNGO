using System.Collections;
using UnityEngine;

public abstract class BaseAnimation : MonoBehaviour
{
    [SerializeField] protected int idleIndex;
    [SerializeField] protected float walkRunTransition = 1f;
    protected Animator anim;

    private float currentWalkRunType = 0;

    protected int hSpeed = Animator.StringToHash("speed");
    protected int hIdleSlot = Animator.StringToHash("idleSlot");
    protected int hIsIdle = Animator.StringToHash("isIdle");
    protected int hIsRunning = Animator.StringToHash("locomotionType");
    protected int hRandomIdle = Animator.StringToHash("randomIdle");
    protected int hAttack = Animator.StringToHash("attack");
    protected int hAttackType = Animator.StringToHash("attackType");
    protected int hJump = Animator.StringToHash("jump");
    protected int hInAir = Animator.StringToHash("isInAir");
    protected int hIsGround = Animator.StringToHash("isGround");
    protected int hAlert = Animator.StringToHash("isAlert");

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
        //Debug.Log(isRunning);
    }

    public abstract void PlayAttack();

    public abstract void PlayJump();

    public virtual void PlayCrouch() { }
    public virtual void PlayInteract() { }

    private IEnumerator SmoothWalkRun(float target)
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
