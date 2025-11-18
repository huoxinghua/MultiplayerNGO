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
        }

        public override void OnExit()
        {
        
        }

        public override void StateFixedUpdate()
        {
            //update destination to be accurate to the currentHuntedPlayer
            //check if distance to player is close enough to kill
        }

        public override void StateUpdate()
        {
        
        }
    }
}
