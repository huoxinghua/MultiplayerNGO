using System.Collections;
using UnityEngine;

public class BruteAnimation : EnemyAnimation
{
    private int hStatus = Animator.StringToHash("bruteStatus");
    private int hInjured = Animator.StringToHash("isInjured");
    private float currentStatus = 1;

    private void Start()
    {
        anim.SetFloat(hStatus, currentStatus);
    }

    public override void PlayRandomIdle(float currentIdleTime, float idleStart)
    {
        Debug.Log("No random idle animation for brute!");
    }

    public void PlayNormal()
    {
        ChangeStatus(1);
        anim.SetBool(hAlert, false);
        anim.SetBool(hInjured, false);
    }

    public override void PlayAlert()
    {
        ChangeStatus(0);
        anim.SetBool(hAlert, true);
        anim.SetBool(hInjured, false);
    }

    public void PlayInjured()
    {
        ChangeStatus(2);
        anim.SetBool(hAlert, false);
        anim.SetBool(hInjured, true);
    }

    private void ChangeStatus(float statusNumber)
    {
        StartCoroutine(SmoothStatusChange(statusNumber, 1));
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

    private IEnumerator SmoothStatusChange(float targetStatus, float duration)
    {
        float time = 0f;
        
        while (time < duration && targetStatus != currentStatus)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(currentStatus, targetStatus, time / duration);
            anim.SetFloat(hStatus, value);
            yield return null;
        }

        anim.SetFloat(hStatus, targetStatus);
        currentStatus = targetStatus;
    }
}
