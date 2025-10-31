using DunGen;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.Level
{
    public class MapNetworkSync : NetworkBehaviour
    {
        [SerializeField] private DungeonGenerator generator;
        
        private NetworkVariable<int> syncedSeed = new NetworkVariable<int>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            if (generator == null)
                generator = GetComponent<DungeonGenerator>();

            if (IsServer)
            {
            
                int seed = Random.Range(0, int.MaxValue);
                syncedSeed.Value = seed;
                Debug.Log($"[Server] Chosen seed = {seed}");

                
                generator.ShouldRandomizeSeed = false;
                generator.Seed = seed;
                generator.Generate();
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

            generator.ShouldRandomizeSeed = false;
            generator.Seed = newSeed;
            generator.Generate();

            Debug.Log("[Client] Dungeon generated from shared seed.");
        }

        private void ClearOldDungeonTiles()
        {
            var oldTiles = GameObject.FindGameObjectsWithTag("DungeonTile");
            foreach (var t in oldTiles)
                Destroy(t);
        }
     
    }
}