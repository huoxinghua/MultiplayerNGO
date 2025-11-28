using System;
using System.Collections.Generic;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Singletons;
using _Project.ScriptableObjects.ScriptObjects.GameTime;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay
{
    public class GameTime : NetworkBehaviour
    {
        public NetworkVariable<int>  GameTimeIntNet = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        //local gametime
        private float _gameTimeFloat;
        /// <summary>
        /// After how many seconds will an event attempt to be called (for performance)
        /// </summary>
        [field: SerializeField] float EventCallFrequency;
        
        /// <summary>
        /// How many attempts at calling an event. Works with EventCallFrequency
        /// </summary>
        private int AttemptsToEvent = 0;
        [SerializeField] private List<GameTimeEventSO> _gameTimeEventsSO;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
           
            GameTimeIntNet.OnValueChanged += HandleGameTimeChange;
            
        }

        public void Update()
        {
            if (IsServer)
            {
                _gameTimeFloat += Time.deltaTime;
                //if gametime divided by the call frequency is more than the events called to avoid calling on start,
                //call event, increase attempts
                if (_gameTimeFloat / EventCallFrequency >= AttemptsToEvent)
                {
                    AttemptsToEvent++;
                    GameTimeEventSO usedEvent = null;
                    foreach (GameTimeEventSO gtEventSO in _gameTimeEventsSO)
                    {
                        if (_gameTimeFloat >= gtEventSO.TimeForEvent)
                        {
                            EventBus.Instance.Publish<ElapsedGameTimeEvent>(new ElapsedGameTimeEvent
                            {
                                EvntGameTimeEnum = gtEventSO.EventType, TimeOfEvent = gtEventSO.TimeForEvent
                            });
                            usedEvent =  gtEventSO;
                        }
                    }

                    if (usedEvent != null)
                    {
                        _gameTimeEventsSO.Remove(usedEvent);
                    }
                }

                RequestSetGameTimeServerRpc(_gameTimeFloat);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestSetGameTimeServerRpc(float time)
        {
            GameTimeIntNet.Value = Mathf.FloorToInt(time);
        }
        private void HandleGameTimeChange(int lastTime, int newTime)
        {
            EventBus.Instance.Publish<GameTimeTickedEvent>(new GameTimeTickedEvent { GameTime = newTime });
        }
    }
    public enum GameTimeEnum
    {
        FirstEvent,
        SecondEvent,
        ThirdEvent,
        PassedTime
    
    }

    public struct ElapsedGameTimeEvent : IEvent
    {
        public GameTimeEnum EvntGameTimeEnum;
        public float TimeOfEvent;
    }
    public struct GameTimeTickedEvent : IEvent
    {
        public int GameTime;
    }
    
}