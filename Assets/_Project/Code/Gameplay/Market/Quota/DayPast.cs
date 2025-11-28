using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Quota
{
    public class DayPast : NetworkBehaviour
    {
        private Timer Timer = new Timer(2);
        private bool _hasPushedEvent = false;
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if(IsServer) Timer.Start();;
       
        }
        private void Update()
        {
            if (!IsServer || _hasPushedEvent) return;
            Timer.TimerUpdate(Time.deltaTime);
            if (Timer.IsComplete)
            {
                SendEventServerRpc();
                _hasPushedEvent  = true;
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void SendEventServerRpc()
        {
            EventBus.Instance.Publish<DayStartEvent>(new DayStartEvent());
        }
    }
}
