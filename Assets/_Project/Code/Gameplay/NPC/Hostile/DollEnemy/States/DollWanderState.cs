namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollWanderState : DollBaseState
    {
        public DollWanderState(DollStateMachine stateMachine, StateEnum stateEnum) : base(stateMachine,  stateEnum)
        {
        
        }
        public override void OnEnter()
        {
            //find point to move to
            //set agent speed to wander (done)
            //choose pose for nextLookedAt (with animator likely) (done)?
            //unfreeze as a precaution?
            Agent.stoppingDistance = DollSO.StoppingDist;
            Agent.speed = DollSO.WalkSpeed;
            Agent.isStopped = false;
            Animator.PlaySwitchPose();
        }

        public override void OnExit()
        {
        
        }

        public override void StateFixedUpdate()
        {
            //if reached moveto dest, choose new dest. No "idling"
        }

        public override void StateUpdate()
        {
        
        }
    }
}
