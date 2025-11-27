using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.UI
{
    public class SwitchScreens : NetworkBehaviour
    {

        public GameObject StorePage;
        public GameObject MissionPage;

        // Use a NetworkVariable to track the current state, writable only by the server
        private NetworkVariable<ScreenState> CurrentScreen = new NetworkVariable<ScreenState>(
            ScreenState.Missions, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CurrentScreen.OnValueChanged += HandleScreenStateChange;
            
            HandleScreenStateChange(CurrentScreen.Value, CurrentScreen.Value);
        }
    

        private void HandleScreenStateChange(ScreenState oldState, ScreenState newState)
        {
            StorePage.SetActive(newState == ScreenState.Store);
            MissionPage.SetActive(newState == ScreenState.Missions);
        }
        
        public void SwitchToStore()
        {
            RequestScreenChangeServerRpc(ScreenState.Store);
        }

        public void SwitchToMission()
        {
            RequestScreenChangeServerRpc(ScreenState.Missions);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestScreenChangeServerRpc(ScreenState newState)
        {
            if (CurrentScreen.Value != newState)
            {
                CurrentScreen.Value = newState;
            }
        }

    }
    public enum ScreenState
    {
        Missions = 0,
        Store = 1
    }
}
