using System.Timers;
using UnityEngine;
using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Utilities.StateMachine;
using UnityEngine.AI;
using Timer = _Project.Code.Utilities.Utility.Timer;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    
    public class DollWanderState : DollBaseState
    {
        private Timer PathControl = new Timer(1f);
        bool hasPath = false;
        public DollWanderState(DollStateMachine stateMachine, StateEnum stateEnum) : base(stateMachine, stateEnum)
        {
        }

        public override void OnEnter()
        {
            //find point to move to
            //set agent speed to wander (done)
            //choose pose for nextLookedAt (with animator likely) (done)?
            //unfreeze as a precaution?
            Debug.Log("Entering DollWanderState");
            Agent.stoppingDistance = DollSO.StoppingDist;
            Agent.speed = DollSO.WalkSpeed;
            Agent.isStopped = false;
            Animator.PlaySwitchPose();
            PathControl.Start();
            OnWander();
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
            if (Vector3.Distance(Agent.destination, StateMachine.transform.position) < DollSO.StoppingDist * 1.5f)
            {
                OnWander();
            }
            PathControl.TimerUpdate(Time.deltaTime);
            if (!hasPath && PathControl.IsComplete)
            {
                OnWander();
            }
        }

        public override void StateLookedAt()
        {
            StateMachine.TransitionTo(StateEnum.LookedAtState);
        }

        public override void StateHuntTimerComplete()
        {
            StateMachine.TransitionTo(StateEnum.HuntingState);
        }

        #region PathFinding
        private void OnWander()
        {
            Vector3 newPos = GetNextPosition();
            if (newPos == Vector3.zero)
            {
                PathControl.Reset(2f);
                hasPath = false;
                return;
            }
            hasPath = true;
            PathControl.Stop();
            Agent.SetDestination(newPos);
        }
        private Vector3 GetNextPosition()
        {

            Vector3 nextPos = Vector3.zero;

            Vector3 temp = new Vector3(DollSO.RandomWanderDist, DollSO.RandomWanderDist, DollSO.RandomWanderDist);
            // Debug.Log(temp.x +" "+ temp.y +" " + temp.z);
            if (NavMesh.SamplePosition(StateMachine.transform.position + temp, out NavMeshHit hit, DollSO.MaxWanderDistance * 3f, NavMesh.AllAreas))
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
        private float GetPathLength(NavMeshAgent navAgent, Vector3 targetPosition)
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
    }
}