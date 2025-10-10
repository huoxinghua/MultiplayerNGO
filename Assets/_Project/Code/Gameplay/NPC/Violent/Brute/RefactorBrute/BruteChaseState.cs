using UnityEngine;

public class BruteChaseState : BruteBaseState
{
      public BruteChaseState(BruteStateMachine stateController) : base(stateController)
    {
        this.stateController = stateController;
    }
    private Timer _chaseTimer;
    public override void OnEnter()
    {
        _chaseTimer = new Timer(bruteSO.LoseInterestTimeChase);
        _chaseTimer.Start();
        animator.PlayAlert();
        agent.speed = bruteSO.RunSpeed;
    }
    public override void OnExit()
    {
        stateController.TimesAlerted = 0;
        _chaseTimer = null;
    }

    public override void StateUpdate()
    {
        if (_chaseTimer.IsComplete || Vector3.Distance(stateController.lastHeardPlayer.transform.position,stateController.transform.position) >= bruteSO.LoseInterestDistanceChase)
        {
            stateController.TransitionTo(stateController.idleState);
        }
    }
    public override void StateFixedUpdate()
    {
        agent.SetDestination(stateController.lastHeardPlayer.transform.position);
        foreach (PlayerList player in PlayerList.AllPlayers)
        {
            if (Vector3.Distance(player.transform.position, stateController.transform.position) < bruteSO.AttackDistance)
            {
                stateController.OnAttack(player.gameObject);
            }
        }
        animator.PlayRun(agent.velocity.magnitude, agent.speed);
    }
    public override void OnHearPlayer()
    {
        _chaseTimer.Reset(bruteSO.LoseInterestTimeChase);
    }
}
