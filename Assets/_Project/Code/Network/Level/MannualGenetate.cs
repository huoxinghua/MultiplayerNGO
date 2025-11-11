using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Interactables.Network;
using DunGen;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.Level
{
    public class ManualGenerate : NetworkBehaviour
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
                generator.Generator.ShouldRandomizeSeed = false;
                generator.Generator.Seed = seed;
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
            ClearOldDungeonTiles();

            generator.Generator.ShouldRandomizeSeed = false;
            generator.Generator.Seed = newSeed;
            generator.Generate();
            
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
                }
            }
        }
    }
}