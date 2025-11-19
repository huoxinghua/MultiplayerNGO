namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollHuntingState : DollBaseState
    {
        public DollHuntingState(DollStateMachine stateMachine) : base(stateMachine)
        {
        
        }
        public override void OnEnter()
        {
            //choose new pose for next looked at (with animator likely)
            //set speed to hunting speed
            //unfreeze anything nessesary
            Agent.stoppingDistance = DollSO.StoppingDist;
            Agent.speed = DollSO.RunSpeed;
            Agent.isStopped = false;
            Animator.PlaySwitchPose();
            Agent.SetDestination(StateMachine.CurrentPlayerToHunt.position);
        }

        public override void OnExit()
        {
        
        }

        public override void StateFixedUpdate()
        {
            //update destination to be accurate to the currentHuntedPlayer
            Agent.SetDestination(StateMachine.CurrentPlayerToHunt.position);
        }

        public override void StateUpdate()
        {
        
        }
    }
}
