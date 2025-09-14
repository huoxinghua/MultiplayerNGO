using UnityEngine;

public class BeetleState : MonoBehaviour
{
    BeetleStates currentState;
    private enum BeetleStates
    {
        Idle,
        MovePosition,
        RunAway
    }
    public void Awake()
    {
        currentState = BeetleStates.MovePosition;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
