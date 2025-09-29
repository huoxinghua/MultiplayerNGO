using System.Collections;
using UnityEngine;

public class BruteHeart : MonoBehaviour , IHitable
{
    private BruteStateController _controller;
    [SerializeField] float heartBeatFrequency;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void Awake()
    {
        StartCoroutine(HeartBeat());
    }
    public void SetStateController(BruteStateController stateController)
    {
        _controller = stateController;
    }
    public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
    {
        StopCoroutine(HeartBeat());
        _controller.TransitionToAttentionState(BruteAttentionStates.Hurt);
        Destroy(gameObject);
    }
    IEnumerator HeartBeat()
    {
        while(true)
        {
            yield return new WaitForSeconds(heartBeatFrequency);
            AudioManager.Instance.PlayByKey3D("BruteHeartBeat",transform.position);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
