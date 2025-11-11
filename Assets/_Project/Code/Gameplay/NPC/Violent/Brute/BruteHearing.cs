using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteHearing : MonoBehaviour
    {
        [SerializeField] BruteSO _bruteSO;
        //  [SerializeField] BruteStateController _stateController;
        //  [SerializeField] BruteMovement _bruteMovement;
        /*private float _walkingHearDistance => _bruteSO.WalkHearingDistance;
    private float _runningHearDistance => _bruteSO.RunHearingDistance;
    private float _landingHearDistance => _bruteSO.LandingHearingDistance;
    //private float _instantAggroDistance => _bruteSO.InstantAggroDistance;*/
        private float _hearingCooldownTime => _bruteSO.HearingCooldown;
        private bool _isOnHearingCooldown;
        private int _timesAlerted = 0;
        private int _maxTimesAlerted = 3;
        //private HashSet<PlayerMovement> _subscribedPlayers = new();
        private HashSet<PlayerStateMachine> _subscribedPlayers = new();
        [SerializeField] BruteStateMachine _stateMachine;

        void OnEnable()
        {
            PlayerStateMachine.OnPlayerAdded += HandlePlayerAdded;
            PlayerStateMachine.OnPlayerRemoved += HandlePlayerRemoved;

            foreach (var player in PlayerStateMachine.AllPlayers)
            {
                HandlePlayerAdded(player); // centralize logic
            }
        }
        private void OnDestroy()
        {
            PlayerStateMachine.OnPlayerAdded -= HandlePlayerAdded;
            PlayerStateMachine.OnPlayerRemoved -= HandlePlayerRemoved;
            //unsub from all existing players
            foreach (var player in PlayerStateMachine.AllPlayers)
            {
                if (player != null)
                    player.SoundMade -= PlayerSoundMade;
            }
        }
        void OnDisable()
        {
            PlayerStateMachine.OnPlayerAdded -= HandlePlayerAdded;
            PlayerStateMachine.OnPlayerRemoved -= HandlePlayerRemoved;

            foreach (var player in _subscribedPlayers.ToList()) // to avoid modification during enumeration
            {
                HandlePlayerRemoved(player);
            }

            _subscribedPlayers.Clear();
        }
        //player joined game, add to list
        void HandlePlayerAdded(PlayerStateMachine player)
        {
            if (_subscribedPlayers.Contains(player)) return;

            player.SoundMade += PlayerSoundMade;

            _subscribedPlayers.Add(player);
        }

        void HandlePlayerRemoved(PlayerStateMachine player)
        {
            if (!_subscribedPlayers.Contains(player)) return;

            player.SoundMade -= PlayerSoundMade;

            _subscribedPlayers.Remove(player);
        }

        void PlayerSoundMade(float range, GameObject player)
        {
            if (player == null)
            {
                Debug.LogWarning("PlayerWalking called with null player.");
                return;
            }
            if (Vector3.Distance(player.transform.position, transform.position) <= range)
            {
                HeardPlayer(player);
            }
        }
        public void HeardPlayer(GameObject player)
        {
            if(player != null)
            {
                Debug.Log(player.name);
            }
            else
            {
                return;
            }

            if (!_isOnHearingCooldown)
            {
                _stateMachine.OnHearPlayer(player);
                //replace with timer later
                StartCoroutine(HearingCooldown());
            }
        }

        IEnumerator HearingCooldown()
        {
            _isOnHearingCooldown = true;
            yield return new WaitForSeconds(_hearingCooldownTime);
            _isOnHearingCooldown = false;
        }
    }
    public struct AlertingSound : IEvent
    {
        public float SoundRange;
        public Transform SoundSource;
        public bool WasPlayerSound;
    }
}
