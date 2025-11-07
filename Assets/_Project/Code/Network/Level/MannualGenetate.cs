using _Project.Code.Gameplay.Interactables.NoUse;
using DunGen;
using Unity.Netcode;
using UnityEngine;
namespace _Project.Code.Network.Level
{
    public class MannualGenetate : NetworkBehaviour
    {
        private RuntimeDungeon generator;

        private void Start()
        {
            generator = GetComponent<RuntimeDungeon>();
            
        }
   
        
        private NetworkVariable<int> syncedSeed = new NetworkVariable<int>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            if (generator == null)
                generator = GetComponent<RuntimeDungeon>();
      

            if (IsServer)
            {
            
                int seed = Random.Range(0, int.MaxValue);
                syncedSeed.Value = seed;
                Debug.Log($"[Server] Chosen seed = {seed}");

                generator.Generator.ShouldRandomizeSeed = false;
                generator.Generator.Seed = seed;
                /*generator.ShouldRandomizeSeed = false;
                generator.Seed = seed;*/
                generator.Generate();
                OnDungeonGenerated(generator);

            }
            else
            {

                syncedSeed.OnValueChanged += OnSeedReceived;

      
                if (syncedSeed.Value != default)
                {
                    OnSeedReceived(default, syncedSeed.Value);
                }
            }
        }

        private void OnSeedReceived(int oldSeed, int newSeed)
        {
            Debug.Log($"[Client] Received dungeon seed = {newSeed}");


            ClearOldDungeonTiles();

            generator.Generator.ShouldRandomizeSeed = false;
            generator.Generator.Seed = newSeed ;
            generator.Generate();

            Debug.Log("[Client] Dungeon generated from shared seed.");
            foreach (var door in Object.FindObjectsByType<SwingDoors>(FindObjectsSortMode.None))
            {
                var netObj = door.GetComponent<NetworkObject>();
                if (netObj == null || !netObj.IsSpawned)
                {
                    Destroy(door.gameObject);
                }
            }

        }

        private void ClearOldDungeonTiles()
        {
            var oldTiles = GameObject.FindGameObjectsWithTag("DungeonTile");
            foreach (var t in oldTiles)
                Destroy(t);
        }
        void OnDungeonGenerated(RuntimeDungeon dungeon)
        {
            foreach (var door in Object.FindObjectsByType<SwingDoors>(FindObjectsSortMode.None))
            {
                var netObj = door.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                {
                    netObj.Spawn();
                    Debug.Log($"[Server] Auto-spawned network door: {door.name}");
                }
            }
        }
    }
}