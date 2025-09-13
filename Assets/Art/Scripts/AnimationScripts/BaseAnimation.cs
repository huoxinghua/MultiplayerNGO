using UnityEngine;

public class BaseAnimation : MonoBehaviour
{
    [SerializeField] int idleIndex;
    private Animator anim;

    protected virtual void Awake()
    {
        anim ??= GetComponent<Animator>();
        if(anim == null) Debug.Log("Animator not found on" + gameObject.name);
    }

    public virtual void OnIdle(float currentIdleTime)
    {
        bool isIdle = anim.GetBool("isIdle");

        if (isIdle)
        {
            anim.SetFloat("idleSlot", Random.Range(0, idleIndex));
            if (currentIdleTime > 4)
            {
                anim.SetTrigger("RandomIdle");
            }
        }
    }

    public virtual void OnWalk(float currentSpeed, float maxSpeed)
    {
        UpdateMovement(currentSpeed, maxSpeed, isRunning: false);
    }

    public virtual void OnRun(float currentSpeed, float maxSpeed)
    {
        UpdateMovement(currentSpeed, maxSpeed, isRunning: true);
    }

    public virtual void OnJump()
    {

    }

    public virtual void OnAttack()
    {

    }

    protected void UpdateMovement(float currentSpeed, float maxSpeed, bool isRunning)
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetFloat("speed", currentSpeed / maxSpeed);

        anim.SetBool("isIdle", currentSpeed < 0.01);
    }
}

