using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BruteHearing : MonoBehaviour
{
    [SerializeField] BruteSO _bruteSO;
    [SerializeField] BruteStateController _stateController;
    [SerializeField] BruteMovement _bruteMovement;
    private float _walkingHearDistance => _bruteSO.WalkHearingDistance;
    private float _runningHearDistance => _bruteSO.RunHearingDistance;
    private float _landingHearDistance => _bruteSO.LandingHearingDistance;
    private float _instantAggroDistance => _bruteSO.InstantAggroDistance;
    private float _hearingCooldownTime => _bruteSO.HearingCooldown;
    private bool _isOnHearingCooldown;
    private int _timesAlerted = 0;
    private int _maxTimesAlerted = 3;
    private HashSet<PlayerMovement> _subscribedPlayers = new();
    [SerializeField] BruteStateMachine _stateMachine;
    /*void OnEnable()
    {
        PlayerMovement.OnPlayerAdded += HandlePlayerAdded;
        PlayerMovement.OnPlayerRemoved += HandlePlayerRemoved;

        // Subscribe to existing players
        foreach (var player in PlayerMovement.AllPlayers)
        {
            player.OnWalking += PlayerWalking;
            player.OnFalling += PlayerLanded;
            player.OnRunning += PlayerRunning;
        }
    }*/
    void OnEnable()
    {
        PlayerMovement.OnPlayerAdded += HandlePlayerAdded;
        PlayerMovement.OnPlayerRemoved += HandlePlayerRemoved;

        foreach (var player in PlayerMovement.AllPlayers)
        {
            HandlePlayerAdded(player); // centralize logic
        }
    }
    private void OnDestroy()
    {
        PlayerMovement.OnPlayerAdded -= HandlePlayerAdded;
        PlayerMovement.OnPlayerRemoved -= HandlePlayerRemoved;
        //unsub from all existing players
        foreach (var player in PlayerMovement.AllPlayers)
        {
            if (player != null)
                player.OnWalking -= PlayerWalking;
            player.OnLand -= PlayerLanded;
            player.OnRunning -= PlayerRunning;
        }
    }
    void OnDisable()
    {
        PlayerMovement.OnPlayerAdded -= HandlePlayerAdded;
        PlayerMovement.OnPlayerRemoved -= HandlePlayerRemoved;

        foreach (var player in _subscribedPlayers.ToList()) // to avoid modification during enumeration
        {
            HandlePlayerRemoved(player);
        }

        _subscribedPlayers.Clear();
    }
    //player joined game, add to list
    void HandlePlayerAdded(PlayerMovement player)
    {
        if (_subscribedPlayers.Contains(player)) return;

        player.OnWalking += PlayerWalking;
        player.OnLand += PlayerLanded;
        player.OnRunning += PlayerRunning;

        _subscribedPlayers.Add(player);
    }

    void HandlePlayerRemoved(PlayerMovement player)
    {
        if (!_subscribedPlayers.Contains(player)) return;

        player.OnWalking -= PlayerWalking;
        player.OnLand -= PlayerLanded;
        player.OnRunning -= PlayerRunning;

        _subscribedPlayers.Remove(player);
    }

    void PlayerWalking(GameObject player)
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= _walkingHearDistance)
        {
            HeardPlayer(player);
        }
    }
    void PlayerRunning(GameObject player)
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= _runningHearDistance)
        {
            HeardPlayer(player);
        }
    }
    void PlayerLanded(GameObject player)
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= _landingHearDistance)
        {
            HeardPlayer(player);
        }
    }
    public void HeardPlayer(GameObject player)
    {
        if (!_isOnHearingCooldown)
        {
            _stateMachine.OnHearPlayer(player);
            //replace with timer later
            StartCoroutine(HearingCooldown());
        }
        //All below is old and out dated
       /* if (_stateController.GetAttentionState() == BruteAttentionStates.Hurt ||
            _stateController.GetAttentionState() == BruteAttentionStates.Dead ||
            _stateController.GetAttentionState() == BruteAttentionStates.KnockedOut) return;


        if (Vector3.Distance(player.transform.position, transform.position) <= _instantAggroDistance)
        {
            _stateController.StartChasePlayer(player);
        }
        if (_stateController.GetAttentionState() == BruteAttentionStates.Unaware)
        {

            if (_timesAlerted <= _maxTimesAlerted)
            {
                StartCoroutine(HearingCooldown());
                _stateController.TransitionToAttentionState(BruteAttentionStates.Alert);
                _stateController.OnFirstAlert(player);
                _timesAlerted++;
            }
            else
            {
                _stateController.StartChasePlayer(player);
            }

        }
        if (_stateController.GetAttentionState() == BruteAttentionStates.Alert && !_isOnHearingCooldown &&
            _stateController.GetBehaviourState() != BruteBehaviourStates.Chase)
        {

            if (_timesAlerted <= _maxTimesAlerted)
            {
                StartCoroutine(HearingCooldown());
                _stateController.OnSubsequentAlert(player);
                _timesAlerted++;
            }
            else
            {
                _stateController.StartChasePlayer(player);
            }
        }
        if (_stateController.GetAttentionState() == BruteAttentionStates.Alert && _stateController.GetBehaviourState() == BruteBehaviourStates.Chase)
        {
            _bruteMovement.OnHearInChase();
        }*/
    }
   /* public void OnExitAlertState()
    {
        _timesAlerted = 0;
        StopAllCoroutines();
        _isOnHearingCooldown = false;
    }*/
    IEnumerator HearingCooldown()
    {
        _isOnHearingCooldown = true;
        yield return new WaitForSeconds(_hearingCooldownTime);
        _isOnHearingCooldown = false;
    }
}
