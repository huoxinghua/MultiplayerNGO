using UnityEngine;

public class BruteHeart : MonoBehaviour , IHitable
{
    private BruteStateController _controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void SetStateController(BruteStateController stateController)
    {
        _controller = stateController;
    }
    public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
    {
        _controller.TransitionToAttentionState(BruteAttentionStates.Hurt);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
