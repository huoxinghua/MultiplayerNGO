using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollLookedAtState : DollBaseState
    {
        public DollLookedAtState(DollStateMachine stateMachine, StateEnum stateEnum) : base(stateMachine, stateEnum)
        {
        
        }
        public override void OnEnter()
        {
            //set speed to 0
            //stop agents pathfinding
            //Ensure ZERO movement
            Debug.Log("Entering lookedat");
            Agent.stoppingDistance = DollSO.StoppingDist;
            Agent.velocity = Vector3.zero;
            Agent.speed = 0;
            Agent.isStopped = true;
        }

        public override void OnExit()
        {
        
        }

        public override void StateFixedUpdate()
        {
        
        }

        public override void StateUpdate()
        {
        
        }

        public override void StateLookedAway()
        {
            StateMachine.TransitionTo(StateEnum.HuntingState);
        }
        public override void StateNoValidPlayer()
        {
            StateMachine.TransitionTo(StateEnum.WanderState);
        }
    }
}
