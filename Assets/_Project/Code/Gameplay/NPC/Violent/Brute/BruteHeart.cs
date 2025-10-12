using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BruteHeart : MonoBehaviour , IHitable 
{
    private BruteStateMachine _controller;
    private List<PlayerList> _players = PlayerList.AllPlayers;


    [SerializeField] private float _heartBeatFrequency;
    [Header("Heart Defense")]
    [SerializeField] private float _health;
    [SerializeField] private float _playerCheckFrequency;
    [SerializeField] private float _defendDistance;
   
    //add health
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
   
    public void Awake()
    {
        StartCoroutine(HeartBeat());
        StartCoroutine(CheckPlayerProximity());
    }
    public void SetStateController(BruteStateMachine stateController)
    {
        _controller = stateController;
    }
    public void OnHit(GameObject attackingPlayer, float damage, float knockoutPower)
    {
        _health -= damage;
        if(_health < 0)
        {
            StopCoroutine(HeartBeat());
            StopCoroutine(CheckPlayerProximity());
            _controller.TransitionTo(_controller.BruteHurtIdleState);
            Destroy(gameObject);
        }
    }
    IEnumerator HeartBeat()
    {
        while(true)
        {
            yield return new WaitForSeconds(_heartBeatFrequency);
            AudioManager.Instance.PlayByKey3D("BruteHeartBeat",transform.position);
        }
    }
    IEnumerator CheckPlayerProximity()
    {
        while(true)
        {
            yield return new WaitForSeconds(_playerCheckFrequency);
            foreach (var player in _players)
            {
                if(Vector3.Distance(player.transform.position, transform.position) <= _defendDistance)
                {
                    _controller.HandleDefendHeart(player.gameObject);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
