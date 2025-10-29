using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Utilities.StateMachine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteBaseState : BaseState
    {
        protected BruteStateMachine StateController;
        protected BruteAnimation Animator;
        protected NavMeshAgent Agent;
        protected BruteSO BruteSO;
        public BruteBaseState(BruteStateMachine stateController)
        {
            this.StateController = stateController;
            Animator = stateController.Animator;
            Agent = stateController.agent;
            BruteSO = stateController.BruteSO;
        }
        public override void OnEnter()
        {

        }
        public override void OnExit()
        {

        }
        
        public override void StateUpdate()
        {

        }
        public override void StateFixedUpdate()
        {

        }
        public virtual void OnHearPlayer()
        {

        }
        public virtual void OnStateAnimatorMove()
        {
        }
    }
}
