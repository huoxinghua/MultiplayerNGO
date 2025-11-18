namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollWanderState : DollBaseState
    {
        public DollWanderState(DollStateMachine stateMachine) : base(stateMachine)
        {
        
        }
        public override void OnEnter()
        {
            //find point to move to
            //set agent speed to wander
            //choose pose for nextLookedAt (with animator likely)
            //unfreeze as a precaution?
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
