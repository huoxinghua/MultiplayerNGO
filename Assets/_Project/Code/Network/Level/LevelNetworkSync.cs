using DunGen;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Collections;
public class LevelNetworkSync : NetworkBehaviour
{
    [SerializeField] private DungeonGenerator generator;
    private bool hasGenerated = false;
    private string lastDungeonJson = string.Empty;
    [Serializable]
    public struct RoomInfo : INetworkSerializable
    {
        public string prefabName;
        public Vector3 position;
        public Quaternion rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref prefabName);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
        }
    }
    [Serializable]
    public class RoomInfoList

    {
        public List<RoomInfo> rooms;
    }
    private void Start()
    {
        Debug.Log("LevelNetworkSync initialized (Scene Object)");

        if (generator == null)
        {
            generator = GetComponent<DungeonGenerator>();
            Debug.LogError(" DungeonGenerator missing!");
            return;
        }

        generator.OnGenerationComplete += OnDungeonGenerated;

        Debug.Log(" dungon start="+ generator.Status);
        StartCoroutine(WaitForNetworkAndGenerate());
    }

    private IEnumerator WaitForNetworkAndGenerate()
    {
        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening);

        yield return new WaitForSeconds(0.2f); 

        if (IsServer)
        {
            Debug.Log(" Host is generating dungeon now...generator="+ generator.Root);
             NetworkManager.OnClientConnectedCallback += OnClientConnected;
            // generator.OnGenerationComplete += OnDungeonGenerated;
            generator.Generate();


            Debug.Log(" dungon start=" + generator.Status);
        }
    }


    private void Disnable()
    {
        if (generator != null)
            generator.OnGenerationComplete -= OnDungeonGenerated;
    }

    private void OnDungeonGenerated(DungeonGenerator gen)
    {
         Debug.Log("OnDungeonGenerated");
        if (!IsServer)
            return; 

        if (hasGenerated)
        {
            Debug.Log("[Server] Dungeon already synced once, skipping duplicate generation.");
            return;
        }
        Debug.Log("hasGenerated="+ hasGenerated);
        hasGenerated = true;
        Debug.Log("hasGenerated=" + hasGenerated);
        Debug.Log("[Server] Dungeon generated automatically, preparing JSON sync...");

        List<RoomInfo> rooms = new List<RoomInfo>();

        foreach (var tile in gen.CurrentDungeon.AllTiles)
        {
            if (tile == null) continue;

            string prefabName = "DungeonTiles/" + tile.name.Replace("(Clone)", "").Trim();
            rooms.Add(new RoomInfo
            {
                prefabName = prefabName,
                position = tile.transform.position,
                rotation = tile.transform.rotation
            });
        }

        RoomInfoList wrapper = new RoomInfoList { rooms = rooms };
        string json = JsonUtility.ToJson(wrapper);
        lastDungeonJson = json;
        Debug.Log($"[Server] Sending dungeon JSON to {NetworkManager.Singleton.ConnectedClientsList.Count} clients...");
    }



    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        if (!hasGenerated) return;
        if (string.IsNullOrEmpty(lastDungeonJson)) return;

        Debug.Log($"[Server] Sending dungeon JSON to newly joined client {clientId}");
        SendDungeonDataClientRpc(lastDungeonJson,clientId);
    }
 
    [ClientRpc]
    private void SendDungeonDataClientRpc(string json,ulong id)
    {
        Debug.Log($"[ClientRpc] Received call! IsServer={IsServer}, IsClient={IsClient}, length={json?.Length}");
        if (IsServer)
        {
            Debug.Log("server return");
            return;
        }
           
       

        RoomInfoList wrapper = JsonUtility.FromJson<RoomInfoList>(json);
        if (wrapper == null || wrapper.rooms == null)
        {
            Debug.LogError("Failed to parse dungeon JSON!");
            return;
        }

        foreach (var room in wrapper.rooms)
        {
            
            GameObject prefab = Resources.Load<GameObject>(room.prefabName);
            if (prefab == null)
            {
                Debug.LogError($" Could not find prefab: {room.prefabName} in Resources!");
                continue;
            }

            GameObject instance = Instantiate(prefab, room.position, room.rotation);
      
        }

        Debug.Log(" Dungeon reconstruction complete on client!");
    }
}
