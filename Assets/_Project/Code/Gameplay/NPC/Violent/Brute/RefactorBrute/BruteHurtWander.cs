using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.Utility;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteHurtWander : BruteBaseState
    {
        private const float ATTACK_CHECK_INTERVAL = 0.1f;
        private Timer _attackCheckTimer;

        public BruteHurtWander(BruteStateMachine stateController) : base(stateController)
        {
        }

        public override void OnEnter()
        {
            _attackCheckTimer = new Timer(ATTACK_CHECK_INTERVAL);
            _attackCheckTimer.Start();
            Animator.PlayInjured();
            Agent.speed = BruteSO.HurtWalkSpeed;
            Agent.updatePosition = false;
            WanderTo();
        }
        public override void OnExit()
        {

        }
        private void WanderTo()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float randomDist = Random.Range(BruteSO.MinWanderDistance, BruteSO.MaxWanderDistance);

                Vector3 wanderOffset = new Vector3(randomDir.x, 0f, randomDir.y) * randomDist;
                Vector3 targetPoint = StateController.transform.position + wanderOffset;

                if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, BruteSO.MaxWanderDistance, NavMesh.AllAreas))
                {
                    Agent.SetDestination(hit.position);
                    return;
                }
            }
        }
        public override void StateUpdate()
        {
            var worldVel = Agent.desiredVelocity;
            var localVel = StateController.transform.InverseTransformDirection(worldVel);
            Animator.PlayWalk(localVel.magnitude, Agent.speed);
        }
        public override void StateFixedUpdate()
        {
            if (Vector3.Distance(StateController.gameObject.transform.position, Agent.destination) <= Agent.stoppingDistance)
            {
                StateController.TransitionTo(StateController.IdleState);
            }

            _attackCheckTimer.TimerUpdate(Time.fixedDeltaTime);
            if (_attackCheckTimer.IsComplete)
            {
                _attackCheckTimer.Reset();
                foreach (PlayerList player in PlayerList.AllPlayers)
                {
                    if (Vector3.Distance(player.transform.position, StateController.transform.position) < BruteSO.AttackDistance)
                    {
                        StateController.OnAttack(player.gameObject);
                    }
                }
            }

            Animator.PlayWalk(Agent.velocity.magnitude, Agent.speed);
        }
        public override void OnStateAnimatorMove()
        {
            var delta = Animator.GetAnimator().deltaPosition;
            StateController.transform.position += delta;               // capsule follows the clip
            Agent.nextPosition = StateController.transform.position;  // keep agent and capsule in sync
            StateController.transform.rotation = Animator.GetAnimator().rootRotation;
            Agent.nextPosition = StateController.transform.position;
        }
        public override void OnHearPlayer()
        {

        }
    }
}
