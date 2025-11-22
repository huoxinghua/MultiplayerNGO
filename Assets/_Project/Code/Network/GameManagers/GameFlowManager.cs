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
        private void Start()
        {
            HideLoadMenu();
        }
        public void ShowLoadMenu()
        {
            _loadMenu.SetActive(true);
            Invoke("HideLoadMenu", showTime);
        }

        public void HideLoadMenu()
        {
            Debug.Log("Hide load menu:."+ _loadMenu.name);
            _loadMenu.SetActive(false);
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

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!IsServer)
            {
                Debug.Log(" OnSceneEvent not server skip");
                return;
            }

            if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                Debug.Log($"[OnSceneEvent] LoadComplete: {sceneEvent.SceneName}");

                if (sceneEvent.SceneName == SceneName.HubScene)
                {
                    if (_isMissionFailed)
                    {
                      
                        SpawnHubPlayers();
                        _isMissionFailed = false;
                    }

                 
                    handleHubPlayerPostions();
                }
                else if (sceneEvent.SceneName == SceneName.MissionHospital)
                {
                    HandleMissionPlayersPositions();
                }
          }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                StartMission(SceneName.MissionHospital);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (IsServer)
            {
                ShowLoadMenu();
                NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            }
        }
   
        public void StartMission(string missionScene)
        {
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
        private void handleHubPlayerPostions()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsIds;
            for (int i = 0; i < clients.Count; i++)
            {
                var clientId = clients[i];
                var playerObj =
                    NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

                var vanSpawner = FindAnyObjectByType<TruckSpawnPointsForPlayers>();
                if (vanSpawner != null)
                    truckSpawnPoints = vanSpawner.spawnPoints;
                Debug.Log("SpawnHubPlayers length：" + truckSpawnPoints.Length);
                var spawn = truckSpawnPoints[i % truckSpawnPoints.Length];
                playerObj.GetComponent<PlayerStateMachine>().SetPositionServerRpc(spawn.position, spawn.rotation);
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
                var playerObj =
                    NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                var spawn = truckSpawnPoints[i % truckSpawnPoints.Length];
                playerObj.GetComponent<PlayerStateMachine>().SetPositionServerRpc(spawn.position, spawn.rotation);
            }
        }
    }
}
