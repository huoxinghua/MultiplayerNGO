using UnityEngine;

public class BruteHurtIdleState : BruteBaseState
{
    private Timer _idleTimer;

    public BruteHurtIdleState(BruteStateMachine stateController) : base(stateController)
    {
    }
    public override void OnEnter()
    {
        _idleTimer = new Timer(BruteSO.RandomIdleTime);
        Animator.PlayInjured();
        Agent.SetDestination(StateController.gameObject.transform.position);
        _idleTimer.Start();
    }
    public override void OnExit()
    {
        _idleTimer.Stop();
        _idleTimer = null;
    }

    public override void StateUpdate()
    {
        _idleTimer.TimerUpdate(Time.deltaTime);
        if (_idleTimer.IsComplete)
        {
            StateController.TransitionTo(StateController.WanderState);
        }
    }
    public override void StateFixedUpdate()
    {
        foreach (PlayerList player in PlayerList.AllPlayers)
        {
            if (Vector3.Distance(player.transform.position, StateController.transform.position) < BruteSO.AttackDistance)
            {
                StateController.OnAttack(player.gameObject);
            }
        }
    }
    public override void OnHearPlayer()
    {

    }
}
