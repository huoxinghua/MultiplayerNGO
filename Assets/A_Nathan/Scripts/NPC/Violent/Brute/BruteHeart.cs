using System.Collections;
using UnityEngine;

public class BruteHeart : MonoBehaviour , IHitable
{
    private BruteStateMachine _controller;
    [SerializeField] float heartBeatFrequency;

    //add health
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void Awake()
    {
        StartCoroutine(HeartBeat());
    }
    public void SetStateController(BruteStateMachine stateController)
    {
        _controller = stateController;
    }
    public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
    {
        StopCoroutine(HeartBeat());
        _controller.TransitionTo(_controller.bruteHurtIdleState);
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
