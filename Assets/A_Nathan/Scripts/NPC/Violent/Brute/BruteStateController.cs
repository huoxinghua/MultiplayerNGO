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
    Chase //Hunting down the player that made the last sound before going into state - Only in Alert
}

public class BruteStateController : MonoBehaviour
{
    private BruteAttentionStates _currentBruteAttentionState;
    private BruteBehaviourStates _currentBruteBehaviour;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void TransitionToAttentionState(BruteAttentionStates newState)
    {
        if (_currentBruteAttentionState == newState || _currentBruteAttentionState == BruteAttentionStates.Dead) return;

        _currentBruteAttentionState = newState;
        OnEnterAttentionState(_currentBruteAttentionState);
    }
    public void OnEnterAttentionState(BruteAttentionStates state)
    {
        switch (state)
        {
            case BruteAttentionStates.Unaware:
                
                break;
            case BruteAttentionStates.Alert:
                
                break;
            case BruteAttentionStates.Hurt:
               
                break;
            case BruteAttentionStates.Dead:
                
                break;
            case BruteAttentionStates.KnockedOut:
                
                break;
        }
    }

    public void TransitionToBehaviourState(BruteBehaviourStates newState)
    {
        if (_currentBruteBehaviour == newState || _currentBruteAttentionState == BruteAttentionStates.Dead) return;

        _currentBruteBehaviour = newState;
        OnEnterBehaviourState(_currentBruteBehaviour);
    }
    public void OnEnterBehaviourState(BruteBehaviourStates state)
    {
        switch (state)
        {
            case BruteBehaviourStates.Idle:

                break;
            case BruteBehaviourStates.Wander:

                break;
            case BruteBehaviourStates.Investigate:

                break;
            case BruteBehaviourStates.Chase:

                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
