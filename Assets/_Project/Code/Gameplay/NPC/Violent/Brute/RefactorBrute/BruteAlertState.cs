using UnityEngine;

public class BruteAlertState : BruteBaseState
{
    Timer alertTimer = new Timer(0f);
    public BruteAlertState(BruteStateMachine stateController) : base(stateController)
    {
        this.stateController = stateController;
    }
    public override void OnEnter()
    {
        Debug.Log("Alert not chase " + bruteSO.AlertWalkSpeed);
        animator.PlayAlert();
        agent.speed = bruteSO.AlertWalkSpeed;
        alertTimer.Reset(bruteSO.LoseInterestTimeInvestigate);
        agent.SetDestination(stateController.lastHeardPlayer.transform.position);
    }
    public override void OnExit()
    {
        alertTimer.Stop();
        Debug.Log("Left Alert");
    }

    public override void StateUpdate()
    {
        alertTimer.Update(Time.deltaTime);
        if (alertTimer.IsDone)
        {
            stateController.TimesAlerted = 0;
            stateController.TransitionTo(stateController.idleState);
        }
        
    }
    public override void StateFixedUpdate()
    {
        animator.PlayWalk(agent.velocity.magnitude, agent.speed);
    }
    public override void OnHearPlayer()
    {
        stateController.TransitionTo(stateController.BruteHeardPlayerState);
    }
}
