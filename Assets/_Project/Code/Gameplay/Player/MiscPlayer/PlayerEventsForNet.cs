using System;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.Code.Utilities.Singletons;
using Unity.Netcode;
using Unity.VisualScripting;
using EventBus = _Project.Code.Utilities.EventBus.EventBus;

namespace _Project.Code.Gameplay.Player.MiscPlayer
{
    public class PlayerEventsForNet : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EventBus.Instance.Subscribe<TeleDoorEvent>(this, HandleTeleDoorEvent);
            EventBus.Instance.Subscribe<PlayerDiedEvent>(this, HandlePlayerDeathEvent);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<TeleDoorEvent>(this);
            EventBus.Instance.Unsubscribe<PlayerDiedEvent>(this);
        }

        public void HandleTeleDoorEvent(TeleDoorEvent teleDoorEvent)
        {
                RequestToggleListStateServerRpc();
        }

        [ServerRpc(RequireOwnership = true)]
        public void RequestToggleListStateServerRpc()
        {
            PlayerCamerasNetSingleton.Instance.RequestTogglePlayerCamServerRpc(new NetworkObjectReference(NetworkObject));
        }

        public void HandlePlayerDeathEvent(PlayerDiedEvent playerDeathEvent)
        {
            if (playerDeathEvent.deadPlayer == gameObject)
            {
                RequestRemoveFromListServerRpc();
            }
        }
        [ServerRpc(RequireOwnership = true)]
        public void RequestRemoveFromListServerRpc()
        {
            PlayerCamerasNetSingleton.Instance.RequestRemovePlayerCamServerRpc(new NetworkObjectReference(NetworkObject));
        }
    }

    public struct TeleDoorEvent : IEvent
    {
        
    }
}