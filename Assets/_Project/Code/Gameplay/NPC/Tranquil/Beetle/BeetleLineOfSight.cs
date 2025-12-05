using System.Collections.Generic;
using _Project.Code.Gameplay.NPC.Tranquil.Beetle.BeetleRefactor;
using _Project.Code.Utilities.Utility;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleLineOfSight : MonoBehaviour
    {
        [SerializeField] float fieldOfView;
        public float viewDistance;
        [SerializeField] float eyeOffset;
        public List<GameObject> players = new List<GameObject>();
        [SerializeField] float fieldOfViewCheckFrequency;
        [SerializeField] LayerMask viewCastLayerMask;
        [SerializeField] BeetleHealth beetleHealthScript;
        public BeetleStateMachine StateMachine { get; private set; }

        private Timer _fovCheckTimer;
        private bool _isActive = true;

        public void Awake()
        {
            StateMachine = GetComponent<BeetleStateMachine>();
            _fovCheckTimer = new Timer(fieldOfViewCheckFrequency);
            _fovCheckTimer.Start();
        }

        private void Update()
        {
            if (!_isActive) return;

            _fovCheckTimer.TimerUpdate(Time.deltaTime);
            if (_fovCheckTimer.IsComplete)
            {
                _fovCheckTimer.Reset();
                CheckFOV();
            }
        }

        public void OnDeath()
        {
            _isActive = false;
        }

        public void OnKnockout()
        {
            _isActive = false;
        }
        public void AddPlayerInProximity(GameObject playerToAdd)
        {
            if (!players.Contains(playerToAdd))
            {
                players.Add(playerToAdd);
            }
        }

        public void RemovePlayerFromProximity(GameObject playerToRemove)
        {
            players.Remove(playerToRemove);
        }
            private void CheckFOV()
        {
            foreach (var player in players)
            {
                if (player == null) continue;

                if (InFOV(player) && HasLineOfSight(player))
                {
                    if (beetleHealthScript.IsPlayerHostile(player))
                    {
                        //player is hostile - RUN!
                        StateMachine.HandleRunFromPlayer(player);
                    }
                    else
                    {
                        StateMachine.HandleFollowPlayer(player);
                        //player is friendly. Follow
                    }
                }
            }
        }
        public bool CheckForHostiles()
        {
            bool hasHostile = false;
            foreach (var player in players)
            {
                if (beetleHealthScript.IsPlayerHostile(player) && HasLineOfSight(player))
                {
                    hasHostile = true;
                }
            }
            return hasHostile;
        }

        private bool InFOV(GameObject player)
        {
            Vector3 dirToTarget = ((player.transform.position + new Vector3(0, 0.5f, 0)) - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle < fieldOfView / 2 && Vector3.Distance(transform.position, player.transform.position) < viewDistance)
            {
                return true;
            }
            return false;
        }
        private bool HasLineOfSight(GameObject player)
        {
            Vector3 dirToTarget = ((player.transform.position + new Vector3(0, 0.5f, 0)) - transform.position).normalized;
            
            if (Physics.Raycast(transform.position + new Vector3(0, eyeOffset, 0), dirToTarget, out RaycastHit hit, viewDistance, ~viewCastLayerMask))
            {
                if (hit.transform.gameObject == player)
                {
                    // Player is visible (LOS confirmed)
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

    }
}
