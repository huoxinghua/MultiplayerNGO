using UnityEngine;
using UnityEngine.AI;
using Timer = _Project.Code.Utilities.Utility.Timer;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle.BeetleRefactor.Network
{
    public class BeetleRunState : BeetleBaseState
    {
        public BeetleRunState(BeetleStateMachine stateController) : base(stateController)
        {

        }
        private Timer _runAwayTimer;
        public override void OnEnter()
        {
            _runAwayTimer = new Timer(5);
            _runAwayTimer.Start();
            Agent.speed = BeetleSO.RunSpeed;
            RunAwayLogic(StateController.PlayerToRunFrom);
        }
        public override void OnExit()
        {
            _runAwayTimer?.Stop();
            _runAwayTimer = null;
        }
        #region Pathfinding
        public void RunAwayLogic(GameObject threat)
        {
            Vector3 directionAway = (StateController.transform.position - threat.transform.position).normalized;
            Vector3 randomOffset = new Vector3(BeetleSO.RandomRunOffset, 0, BeetleSO.RandomRunOffset);
            Vector3 rawFleePosition = StateController.transform.position + (directionAway * BeetleSO.FleeDistance) + randomOffset;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(rawFleePosition, out hit, BeetleSO.FleeDistance * 2f, NavMesh.AllAreas))
            {
                Agent.SetDestination(hit.position);
            }
            else
            {
                Debug.Log("No valid NavMesh point found to flee to.");
            }
        }
        #endregion
        public override void StateUpdate()
        {
            _runAwayTimer.TimerUpdate(Time.deltaTime);
            if (_runAwayTimer.IsComplete)
            {
                RunAwayLogic(StateController.PlayerToRunFrom);
                _runAwayTimer.Reset(5);
            }
        }
        public override void StateFixedUpdate()
        {
            Animator.PlayRun(Agent.velocity.magnitude, Agent.speed);
            if (Vector3.Distance(StateController.transform.position, StateController.PlayerToRunFrom.transform.position) >= BeetleSO.StopRunDistance)
            {
                StateController.TransitionTo(StateController.IdleState);
            }
        }
        public override void OnSpotPlayer(bool isHostilePlayer)
        {

        }
        public override void OnHitByPlayer()
        {

        }
    }
}