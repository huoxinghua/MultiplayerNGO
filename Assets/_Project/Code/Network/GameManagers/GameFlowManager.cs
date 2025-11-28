using System;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.EnemySpawning;
using _Project.Code.Gameplay.Interactables.Truck;
using _Project.Code.Gameplay.Market.Buy;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Code.Network.GameManagers
{
    public class GameFlowManager : NetworkSingleton<GameFlowManager>
    {
        [SerializeField] private GameObject hubPlayerPrefab;
        [SerializeField] private GameObject missionPlayerPrefab;
        [SerializeField] private Transform[] truckSpawnPoints;
        [SerializeField] private GameObject _loadMenu;
        [SerializeField] private float showTime = 1f;
        #region Manager all the scene
        public static class SceneName
        {
            public const string MainMenu = "NetworkMainMenu";
            public const string MainMenuUTP = "NetworkMainMenuUTP";
            public const string HubScene = "HubScene";
            public const string MissionHospital = "SecondShowcase_v1_Build";
        }
        #endregion

        #region Loading image
        private void Start()
        {
            HideLoadMenu();
        }
        public void ShowLoadMenu()
        {
            ShowLoadMenuLocal();
            ShowLoadMenuClientRpc();
          
        }

        [ClientRpc]
        private void ShowLoadMenuClientRpc()
        {
            if (IsServer) return; 
            ShowLoadMenuLocal();
        }

        private void ShowLoadMenuLocal()
        {
            _loadMenu.SetActive(true);
         //   Invoke("HideLoadMenu", showTime);
        }
        public void HideLoadMenu()
        {
            HideLoadMenuLocal();
            HideLoadMenuClientRpc();
        }

        private void HideLoadMenuLocal()
        {
            _loadMenu.SetActive(false);
        }

        [ClientRpc]
        private void HideLoadMenuClientRpc()
        {
            if (IsServer) return;
            HideLoadMenuLocal();
        }
        #endregion
        
        public override void OnNetworkSpawn()
        {
            if (_loadMenu == null)
            {
                _loadMenu = transform.GetChild(0).gameObject;
            }
            var networkManager = NetworkManager.Singleton;
            if (networkManager == null || networkManager.SceneManager == null)
            {
                return;
            }
            networkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }
     
        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.SceneManager.OnSceneEvent -=
                    OnSceneEvent;
        }

        [ClientRpc]
        private void SyncPlayerPositionClientRpc(ulong clientId, Vector3 pos, Quaternion rot)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;

            var player = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (player == null) return;

            var psm = player.GetComponent<PlayerStateMachine>();
            psm.ForceSetPosition(pos, rot);
        }
        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!IsServer)
            {
                HideLoadMenu(); 
                return;
            }

            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {

                if (sceneEvent.SceneName == SceneName.HubScene)
                {
                    if (_isMissionFailed)
                    {
                      PlayerListManager.Instance.AlivePlayers.Clear();
                        SpawnHubPlayers();
                        _isMissionFailed = false;
                    }
                    
                    handleHubPlayerPositions(sceneEvent.ClientId);
                }
                else if (sceneEvent.SceneName == SceneName.MissionHospital)
                {
                    PlayerListManager.Instance.AlivePlayers.Clear();
                    SpawnHubPlayers();
                    HandleMissionPlayersPositions();
                }
                else  
                {
                    PlayerListManager.Instance.AlivePlayers.Clear();
                    SpawnHubPlayers();
                    HandleMissionPlayersPositions();
                }
          }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                RequestStartMission();
            }
        }

        public void LoadScene(string sceneName)
        {
            if (IsServer)
            {
                ShowLoadMenu();
               
            }
            else
            {
                ShowLoadMenu();
            }
            NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        [ClientRpc]
        private void ShowLoadMenuClientRPC()
        {
            ShowLoadMenu();
        }
        
        public void RequestStartMission()
        {
            if (IsServer)
            {
                StartMission();
            }
            else
            {
                RequestStartMissionServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestStartMissionServerRpc()
        {
           
            StartMission();
        }
        public void StartMission()
        {
            ShowLoadMenu();
            LoadScene(SceneName.MissionHospital);
        }

        private bool _isMissionFailed = false;
        public void ReturnToHub()
        {
            //this is when all players died will return to hub
            _isMissionFailed = true;
            LoadScene(SceneName.HubScene);
        }
        private void SpawnHubPlayers()
        {
            Debug.Log("mission faild spawn hub players");
            var clients = NetworkManager.Singleton.ConnectedClientsIds;

            foreach (var clientId in clients)
            {
                var oldPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                if (oldPlayer != null)
                {
                    oldPlayer.Despawn();
                }
                var newPlayer = Instantiate(hubPlayerPrefab);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }
        private void handleHubPlayerPositions(ulong id)
        {
            var vanSpawner = FindAnyObjectByType<TruckSpawnPointsForPlayers>();
            if (vanSpawner != null)
                truckSpawnPoints = vanSpawner.spawnPoints;
            var clients = NetworkManager.Singleton.ConnectedClientsIds;
            for (int i = 0; i < clients.Count; i++)
            {
                var clientId = clients[i];
                var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                if (playerObj == null) continue;

                var spawn = truckSpawnPoints[i % truckSpawnPoints.Length];
        
                playerObj.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

                SyncPlayerPositionClientRpc(clientId, spawn.position, spawn.rotation);
            }
        }

        private void HandleMissionPlayersPositions()
        {
            var vanSpawner = FindAnyObjectByType<TruckSpawnPointsForPlayers>();
            if (vanSpawner != null)
                truckSpawnPoints = vanSpawner.spawnPoints;

            var clients = NetworkManager.Singleton.ConnectedClientsIds;
            for (int i = 0; i < clients.Count; i++)
            {
                var clientId = clients[i];
                var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                if (playerObj == null) continue;

                var spawn = truckSpawnPoints[i % truckSpawnPoints.Length];
           
                playerObj.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
          
                SyncPlayerPositionClientRpc(clientId, spawn.position, spawn.rotation);
            }
        }
        
    }
}
