using UnityEngine;

public abstract class BaseAnimation : MonoBehaviour
{
    [SerializeField] protected int idleIndex;
    protected Animator anim;

    protected int hSpeed = Animator.StringToHash("speed");
    protected int hIdleSlot = Animator.StringToHash("idleSlot");
    protected int hIsIdle = Animator.StringToHash("isIdle");
    protected int hIsRunning = Animator.StringToHash("isRunning");
    protected int hRandomIdle = Animator.StringToHash("randomIdle");
    protected int hAttack = Animator.StringToHash("attack");
    protected int hJump = Animator.StringToHash("jump");
    protected int hInAir = Animator.StringToHash("isInAir");
    protected int hIsGround = Animator.StringToHash("isGround");
    protected int hAlert = Animator.StringToHash("alert");

    protected virtual void Awake()
    {
        anim ??= GetComponent<Animator>();
        if (anim == null) Debug.Log("Animator not found on" + gameObject.name);
    }

    public virtual void PlayRandomIdle(float currentIdleTime, float idleStart)
    {
        if (idleIndex == 0) return;
        if (anim.GetBool(hIsIdle) && currentIdleTime > idleStart)
        {
            anim.SetFloat(hIdleSlot, Random.Range(0, idleIndex));
            anim.SetTrigger(hRandomIdle);
        }
    }

    public virtual void PlayWalk(float currentSpeed, float maxSpeed) => UpdateMovement(currentSpeed, maxSpeed, isRunning: false);

    public virtual void PlayRun(float currentSpeed, float maxSpeed) => UpdateMovement(currentSpeed, maxSpeed, isRunning: false);

    protected void UpdateMovement(float currentSpeed, float maxSpeed, bool isRunning)
    {
        anim.SetBool(hIsRunning, isRunning);
        anim.SetFloat(hSpeed, currentSpeed / maxSpeed);

        anim.SetBool(hIsIdle, currentSpeed < 0.01);
    }

    public abstract void PlayAttack();

    public abstract void PlayJump();

    public abstract void PlayAlert();

    public abstract void PlayCrouch();
    public abstract void PlayInteract();
}
