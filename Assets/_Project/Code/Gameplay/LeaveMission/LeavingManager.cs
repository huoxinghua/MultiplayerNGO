using System;
using _Project.Code.Gameplay.Market.Quota;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Network.GameManagers;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.LeaveMission
{
    public class LeavingManager : NetworkBehaviour
    {
        [SerializeField] private GameObject _truckCam;
        [SerializeField] private float _leaveTime = 5f;
        private Timer _leaveTimer;
        private bool _isLeaving = false;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        private void Awake()
        {
            EventBus.Instance.Subscribe<SuccessfulDayEvent>(this, HandleSuccessfulDayEvent);
            _leaveTimer =  new Timer(_leaveTime);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<SuccessfulDayEvent>(this);
        }

        private void FixedUpdate()
        {
            if (!_isLeaving || !IsServer) return;
            _leaveTimer.TimerUpdate(Time.deltaTime);
            if (_leaveTimer.IsComplete)
            {
                GameFlowManager.Instance.LoadScene(GameFlowManager.SceneName.HubScene);
            }
        }

        private void HandleSuccessfulDayEvent(SuccessfulDayEvent e)
        {
            RequestLeaveMissionServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        public void RequestLeaveMissionServerRpc()
        {
        HandleCameraShiftClientRpc();
        _leaveTimer.Start();
        }
        [ClientRpc(RequireOwnership = false)]
        public void HandleCameraShiftClientRpc()
        {
        EventBus.Instance.Publish<LeavingMissionEvent>(new LeavingMissionEvent());
        _isLeaving = true;
        _truckCam.SetActive(true);
        }
    }

    public struct LeavingMissionEvent : IEvent
    {
    
    }
}