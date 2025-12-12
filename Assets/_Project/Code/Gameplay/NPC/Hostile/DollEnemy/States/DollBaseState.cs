using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Utilities.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollBaseState : BaseState
    {
        protected DollStateMachine StateMachine;
        protected DollAnimation Animator;
        protected NavMeshAgent Agent;
        protected DollSO DollSO;
        protected StateEnum ThisStateEnum;
        public DollBaseState(DollStateMachine stateMachine, StateEnum stateEnum)
        {
            this.StateMachine = stateMachine;
            Animator = stateMachine.Animator;
            Agent = stateMachine.Agent;
            DollSO = stateMachine.DollSO;
            ThisStateEnum = stateEnum;
        }
        public override void OnEnter()
        {
        
        }

        public override void OnExit(){ }

        public override void StateFixedUpdate()
        {
        
        }

        public override void StateUpdate()
        {
        
        }

        public virtual void StateLookedAt()
        {
            
        }

        public virtual void StateLookedAway(Transform playerToHunt)
        {
            
        }

        public virtual void StateAttemptKill()
        {
            
        }

        public virtual void StateNoValidPlayer()
        {
            
        }

        public virtual void StateHuntTimerComplete()
        {
            
        }
    }
}
