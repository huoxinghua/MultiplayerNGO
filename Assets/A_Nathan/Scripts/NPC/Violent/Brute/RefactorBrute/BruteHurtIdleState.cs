using UnityEngine;

public class BruteHurtIdleState : BruteBaseState
{
    Timer idleTimer = new Timer(0f);

    public BruteHurtIdleState(BruteStateMachine stateController) : base(stateController)
    {
    }
    public override void OnEnter()
    {
        agent.SetDestination(stateController.gameObject.transform.position);
        idleTimer.Reset(Random.Range(bruteSO.MinIdleTime, bruteSO.MaxIdleTime));
    }
    public override void OnExit()
    {
        idleTimer.Stop();
    }

    public override void StateUpdate()
    {
        idleTimer.Update(Time.deltaTime);
        if (idleTimer.IsRunning)
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
