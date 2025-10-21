using UnityEngine;
using UnityEngine.AI;
namespace _Project.Code.Core.GamePlay.AI.NetWork
{
    public class BeetleWanderState : BeetleBaseState
    {
        public BeetleWanderState(BeetleStateMachine stateController) : base(stateController)
        {

        }
        #region PathFinding
        void OnWander()
        {
            Vector3 newPos = GetNextPosition();
            if (newPos == Vector3.zero)
            {
                OnWander();
                return;
            }
            Agent.SetDestination(newPos);
        }
        Vector3 GetNextPosition()
        {

            Vector3 nextPos = Vector3.zero;

            Vector3 temp = new Vector3(BeetleSO.RandomWanderDist, BeetleSO.RandomWanderDist, BeetleSO.RandomWanderDist);
            // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
            if (NavMesh.SamplePosition(StateController.transform.position + temp, out NavMeshHit hit, BeetleSO.MaxWanderDist * 3f, NavMesh.AllAreas))
            {
                if (GetPathLength(Agent, hit.position) == -1)
                {
                    return Vector3.zero;
                }
                return hit.position;
            }
            else
            {
                return Vector3.zero;
            }
        }
        float GetPathLength(NavMeshAgent navAgent, Vector3 targetPosition)
        {
            NavMeshPath path = new NavMeshPath();
            if (navAgent.CalculatePath(targetPosition, path))
            {
                float length = 0.0f;

                if (path.corners.Length < 2)
                    return 0;

                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }

                return length;
            }

            return -1f; // Invalid path
        }
        #endregion
        public override void OnEnter()
        {
            Agent.speed = BeetleSO.WalkSpeed;
            OnWander();

        }
        public override void OnExit()
        {

        }

        public override void StateUpdate()
        {

        }
        public override void StateFixedUpdate()
        {
            Animator.PlayWalk(Agent.velocity.magnitude, Agent.speed);
            if (Vector3.Distance(StateController.transform.position, Agent.destination) <= BeetleSO.StoppingDist)
            {
                StateController.TransitionTo(StateController.IdleState);
            }
        }
        public override void OnSpotPlayer(bool isHostilePlayer)
        {
            if (isHostilePlayer)
            {
                StateController.TransitionTo(StateController.RunState);
            }
            else if (StateController.FollowCooldown.IsComplete || StateController.IsFirstFollow)
            {
                StateController.TransitionTo(StateController.FollowState);
            }
        }
        public override void OnHitByPlayer()
        {
            StateController.TransitionTo(StateController.RunState);
        }
    }
}