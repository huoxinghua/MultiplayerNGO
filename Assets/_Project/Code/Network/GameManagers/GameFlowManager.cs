using System;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Interactables.Truck;
using _Project.Code.Gameplay.Market.Buy;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Code.Network.GameManagers
{
    public class GameFlowManager : NetworkSingleton<GameFlowManager>
    {
         public string SelectedMissionScene;
    [SerializeField] private GameObject hubPlayerPrefab;
    [SerializeField] private GameObject missionPlayerPrefab;
    [SerializeField] private Transform[] truckSpawnPoints;


    public override void OnNetworkSpawn()
    {
        Debug.Log(" game flow manager Start");
      //  NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;
        var networkManager = NetworkManager.Singleton;
        if (networkManager == null || networkManager.SceneManager == null)
        {
            Debug.LogWarning("NetworkManager.SceneManager not ready yet.");
            return;
        }
        Debug.LogWarning("NetworkManager.SceneManager ready");
        networkManager.SceneManager.OnSceneEvent += OnSceneEvent;
       

        SpawnHubPlayers();

    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnSceneEvent -=
                OnSceneEvent;
    }
    

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        Debug.Log(" OnSceneEvent");
        if (!IsServer)
        {
            Debug.Log(" OnSceneEvent not server skip");
            return;
        }

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            if (  sceneEvent.SceneName == "HubScene")
            {
                SpawnHubPlayers();
            }
            else
            {
                SpawnMissionPlayers();
            }
        }
            
    
        
       
        
      
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartMission(SelectedMissionScene);
        }
    }*/

    public void LoadScene(string sceneName)
    {
        if (IsServer)
            NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        if (sceneName!="HubScene")
        {
            SpawnMissionPlayers();
        }
        else
        {
            SpawnHubPlayers();
            
        }

    }

    public void StartMission(string missionScene)
    {
        Debug.Log("StartMission"+missionScene);
        SelectedMissionScene = missionScene;
       // DespawnAllPlayers();
        LoadScene("SecondShowcase_v1_Build");
     

    }

    public void ReturnToHub()
    {
        Debug.Log("ReturnToHub");
        LoadScene("HubScene");
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (!IsServer) return;

        if (sceneName == "HubScene")
        {
            SpawnHubPlayers();
        }
           
        else if (sceneName == SelectedMissionScene)
            SpawnMissionPlayers();

    }

    private void SpawnHubPlayers()
    {
        Debug.Log("SpawnHubPlayers");
        var clients = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < clients.Count; i++)
        {
            var clientId = clients[i];
            var playerObj =
                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObj == null) continue;
            var vanSpawner = FindAnyObjectByType<TruckSpawnPointsForPlayers>();
            if (vanSpawner != null)
                truckSpawnPoints = vanSpawner.spawnPoints;
            Debug.Log("SpawnHubPlayers length："+truckSpawnPoints.Length);
            var spawn = truckSpawnPoints[i % truckSpawnPoints.Length];
            // playerObj.transform.SetPositionAndRotation(spawn.position,
            //     spawn.rotation);
            playerObj.GetComponent<PlayerStateMachine>().SetPositionServerRpc(spawn.position, spawn.rotation);

        }
    }

    private void SpawnMissionPlayers()
    {
        Debug.Log("SpawnMissionPlayers");
        var vanSpawner = FindAnyObjectByType<TruckSpawnPointsForPlayers>();
        if (vanSpawner != null)
            truckSpawnPoints = vanSpawner.spawnPoints;
        Debug.Log("SpawnHubPlayers length："+truckSpawnPoints.Length);
        
        var clients = NetworkManager.Singleton.ConnectedClientsIds;
        for (int i = 0; i < clients.Count; i++)
        {
            var clientId = clients[i];
            var playerObj =
                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (playerObj == null) continue;
          
            var spawn = truckSpawnPoints[i % truckSpawnPoints.Length];
            DespawnAllNonPlayerObjects();
             var newPlayer = Instantiate(missionPlayerPrefab, spawn.position,
                 spawn.rotation);

            playerObj.GetComponent<PlayerStateMachine>().SetPositionServerRpc(spawn.position, spawn.rotation);

        }
    }

    private void DespawnAllNonPlayerObjects()
    {
        foreach (var netObj in
                 NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (netObj.IsPlayerObject)
                continue;

            netObj.Despawn(true);
        }
    }

    }
}
