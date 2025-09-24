using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

//States handling what behaviour system to use
public enum BruteAttentionStates
{
    Unaware,
    Alert,
    Hurt,
    Dead,
    KnockedOut
}

//States for handling the behaviour
public enum BruteBehaviourStates
{
    Idle, //standing still - Occurs in unaware, alert, and hurt
    Wander, //For random patrol around heart - Occurs in Unaware and hurt
    Investigate, //Inspecting a heard sound, but not quite agrrod to the player - Only in Alert
    Chase, //Hunting down the player that made the last sound before going into state - Only in Alert
    None

}

public class BruteStateController : MonoBehaviour
{
   [SerializeField] private BruteAttentionStates _currentBruteAttentionState;
    [SerializeField] private BruteBehaviourStates _currentBruteBehaviour;
    [SerializeField] float firstAlertDelayTime;
    [SerializeField] private GameObject _heartPrefab;
    [SerializeField] BruteHearing _bruteHearing;
    private GameObject _spawnedHeart;
    [SerializeField] private BruteMovement _bruteMovementScript;
    public GameObject PlayerToChase;
    [SerializeField] BruteAnimation _bruteAnimation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HandleHeartSpawn();
        TransitionToAttentionState(BruteAttentionStates.Unaware);
        TransitionToBehaviourState(BruteBehaviourStates.Wander);
    }
    public void HandleHeartSpawn()
    {
        _spawnedHeart = Instantiate(_heartPrefab, transform);
        _spawnedHeart.GetComponent<BruteHeart>()?.SetStateController(this);
        _spawnedHeart.transform.SetParent(null);
        _bruteMovementScript.SetHeartTransform(_spawnedHeart.transform);
    }
    public void TransitionToAttentionState(BruteAttentionStates newState)
    {
        if (_currentBruteAttentionState == newState || _currentBruteAttentionState == BruteAttentionStates.Dead || _currentBruteBehaviour == BruteBehaviourStates.None) return;
        if (_currentBruteAttentionState == BruteAttentionStates.Hurt && (newState == BruteAttentionStates.Unaware || newState == BruteAttentionStates.Alert)) return;
        _currentBruteAttentionState = newState;
        OnEnterAttentionState(_currentBruteAttentionState);
    }
    public void OnEnterAttentionState(BruteAttentionStates state)
    {
        switch (state)
        {
            case BruteAttentionStates.Unaware:
                _bruteHearing.OnExitAlertState();
                _bruteAnimation.PlayNormal();
                break;
            case BruteAttentionStates.Alert:
                _bruteAnimation.PlayAlert();
                break;
            case BruteAttentionStates.Hurt:
                _bruteHearing.OnExitAlertState();
                _bruteAnimation.PlayInjured();
                TransitionToBehaviourState(BruteBehaviourStates.Idle);
                break;
            case BruteAttentionStates.Dead:
                _bruteMovementScript.OnDeathKO();
                break;
            case BruteAttentionStates.KnockedOut:

                break;
        }
    }
    public BruteAttentionStates GetAttentionState()
    {
        return _currentBruteAttentionState;
    }
    public void TransitionToBehaviourState(BruteBehaviourStates newState)
    {
        if (_currentBruteBehaviour == newState || _currentBruteAttentionState == BruteAttentionStates.Dead || _currentBruteBehaviour == BruteBehaviourStates.None) return;
        if (_currentBruteAttentionState == BruteAttentionStates.Hurt && (newState == BruteBehaviourStates.Investigate || newState == BruteBehaviourStates.Chase)) return;
        _currentBruteBehaviour = newState;
        OnEnterBehaviourState(_currentBruteBehaviour);
    }
    public void OnEnterBehaviourState(BruteBehaviourStates state)
    {
        switch (state)
        {
            case BruteBehaviourStates.Idle:
                _bruteMovementScript.OnStartIdle();
                _bruteMovementScript.OnStopChase();
                break;
            case BruteBehaviourStates.Wander:
                _bruteMovementScript.OnStartWander();
                _bruteMovementScript.OnStopChase();
                break;
            case BruteBehaviourStates.Investigate:
                _bruteMovementScript.OnStopChase();
                break;
            case BruteBehaviourStates.Chase:
                _bruteMovementScript.OnStartChase();
                break;

            case BruteBehaviourStates.None:

                break;
        }
    }
    public void OnFirstAlert(GameObject player)
    {
        StartCoroutine(FirstAlertDelay(player));
    }
    IEnumerator FirstAlertDelay(GameObject player)
    {
        yield return new WaitForSeconds(firstAlertDelayTime);
        TransitionToBehaviourState(BruteBehaviourStates.Investigate);
        _bruteMovementScript.OnInvestigate(player);
    }
    public void OnSubsequentAlert(GameObject player)
    {
        TransitionToBehaviourState(BruteBehaviourStates.Investigate);
        _bruteMovementScript.OnInvestigate(player);
    }
    public void StartChasePlayer(GameObject playerToChase)
    {
        PlayerToChase = playerToChase;
        TransitionToAttentionState(BruteAttentionStates.Alert);
        TransitionToBehaviourState(BruteBehaviourStates.Chase);
    }
    public BruteBehaviourStates GetBehaviourState()
    {
        return _currentBruteBehaviour;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
