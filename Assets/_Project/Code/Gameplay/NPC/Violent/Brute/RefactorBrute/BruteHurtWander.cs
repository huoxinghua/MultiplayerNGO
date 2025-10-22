using _Project.Code.Gameplay.Player;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteHurtWander : BruteBaseState
    {
        public BruteHurtWander(BruteStateMachine stateController) : base(stateController)
        {
        }
        public override void OnEnter()
        {
            Animator.PlayInjured();
            Agent.speed = BruteSO.HurtWalkSpeed;
            WanderTo();
        }
        public override void OnExit()
        {

        }
        public Vector3 GetNextPosition()
        {
            Vector3 nextPos = Vector3.zero;

            Vector3 temp = new Vector3(Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance)
                                       * (Random.Range(0, 2) * 2 - 1), Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance) *
                                                                       (Random.Range(0, 2) * 2 - 1), Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance) * (Random.Range(0, 2) * 2 - 1));
            // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
            if (NavMesh.SamplePosition(StateController.gameObject.transform.position + temp, out NavMeshHit hit, BruteSO.MaxWanderDistance * 3f, NavMesh.AllAreas))
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
        void WanderTo()
        {
            Vector3 newPos = GetNextPosition();
            if (newPos == Vector3.zero)
            {
                WanderTo();
                return;
            }
            Agent.SetDestination(newPos);
        }
        public override void StateUpdate()
        {
        
        }
        public override void StateFixedUpdate()
        {
            if (Vector3.Distance(StateController.gameObject.transform.position, Agent.destination) <= Agent.stoppingDistance)
            {
                StateController.TransitionTo(StateController.IdleState);
            }
            foreach (PlayerList player in PlayerList.AllPlayers)
            {
                if (Vector3.Distance(player.transform.position,StateController.transform.position) < BruteSO.AttackDistance)
                {
                    StateController.OnAttack(player.gameObject);
                }
            }
            Animator.PlayWalk(Agent.velocity.magnitude, Agent.speed);
        }
        public override void OnHearPlayer()
        {

        }
    }
}
