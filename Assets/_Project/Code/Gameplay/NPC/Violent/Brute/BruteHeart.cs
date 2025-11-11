using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.Audio;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteHeart : NetworkBehaviour , IHitable 
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
                var netObj = GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn(true);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            StopAllCoroutines();

            // play the heard break sound if have
            //AudioManager.Instance.PlayByKey3D("HeartBreak", transform.position);
            if (_controller != null)
                _controller.OnHeartDestroyed();
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

    }
}
