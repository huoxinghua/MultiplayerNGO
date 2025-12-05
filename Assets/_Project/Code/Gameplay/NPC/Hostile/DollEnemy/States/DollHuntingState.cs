using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollHuntingState : DollBaseState
    {
        public DollHuntingState(DollStateMachine stateMachine, StateEnum stateEnum) : base(stateMachine, stateEnum)
        {
        
        }
        public override void OnEnter()
        {
            //choose new pose for next looked at (with animator likely)
            //set speed to hunting speed
            //unfreeze anything nessesary
            Debug.Log("Entering hunt");
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
        public override void StateLookedAt()
        {
            StateMachine.TransitionTo(StateEnum.LookedAtState);
        }
        
        public override void StateLookedAway(Transform playerToHunt)
        {
            // Already hunting, no need to do anything
        }

        public override void StateNoValidPlayer()
        {
            StateMachine.TransitionTo(StateEnum.WanderState);
        }

        public override void StateAttemptKill()
        {
            StateMachine.RequestKill(StateMachine.CurrentPlayerToHunt.parent.gameObject);
        }
    }
}
