using UnityEngine;

public class BruteHurtIdleState : BruteBaseState
{
    Timer idleTimer;

    public BruteHurtIdleState(BruteStateMachine stateController) : base(stateController)
    {
    }
    public override void OnEnter()
    {
        idleTimer = new Timer(bruteSO.RandomIdleTime);
        animator.PlayInjured();
        agent.SetDestination(stateController.gameObject.transform.position);
        idleTimer.Start();
    }
    public override void OnExit()
    {
        idleTimer.Stop();
        idleTimer = null;
    }

    public override void StateUpdate()
    {
        idleTimer.TimerUpdate(Time.deltaTime);
        if (idleTimer.IsComplete)
        {
            stateController.TransitionTo(stateController.wanderState);
        }
    }
    public override void StateFixedUpdate()
    {
        foreach (PlayerList player in PlayerList.AllPlayers)
        {
            if (Vector3.Distance(player.transform.position, stateController.transform.position) < bruteSO.AttackDistance)
            {
                stateController.OnAttack(player.gameObject);
            }
        }
    }
    public override void OnHearPlayer()
    {

    }
}
