using UnityEngine;

public class BruteChaseState : BruteBaseState
{
      public BruteChaseState(BruteStateMachine stateController) : base(stateController)
    {
        this.stateController = stateController;
    }
    public override void OnEnter()
    {
        animator.PlayAlert();
        agent.speed = bruteSO.RunSpeed;
    }
    public override void OnExit()
    {
        stateController.TimesAlerted = 0;
    }

    public override void StateUpdate()
    {
        
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

    }
}
