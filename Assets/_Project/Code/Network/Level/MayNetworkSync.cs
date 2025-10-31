using DunGen;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.Level
{
    public class MapNetworkSync : NetworkBehaviour
    {
        [SerializeField] private DungeonGenerator generator;

        private NetworkVariable<int> seed = new NetworkVariable<int>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
        private void Start()
        {
          
            //generator.RandomizeSeed() = false;
            generator.Seed = 70;
            seed.Value = 70;
            Debug.Log($"[After Awake] generator.Seed = {generator.Seed}");
        }
        public override void OnNetworkSpawn()
        {
            Debug.Log("Start() called, generator.Seed = " + generator.Seed);
            if (IsServer)
            {
                generator.ShouldRandomizeSeed = false;
                generator.DungeonFlow = generator.DungeonFlow; 
                int randomSeed = Random.Range(1, int.MaxValue);
                generator.Seed = randomSeed;
                seed.Value = randomSeed;

                Debug.Log($"[Server] seed = {randomSeed}");
                generator.Generate();
                /*var serialized = generator.SerializeLayout();
                SendDungeonLayoutClientRpc(serialized);*/
            }
            else//client
            {
                seed.OnValueChanged += OnSeedChanged;

                
                if (seed.Value != default)
                {
                    OnSeedChanged(default, seed.Value);
                }
            }
        }
        private void OnSeedChanged(int oldSeed, int newSeed)
        {
            generator.Seed = newSeed;
            generator.DungeonFlow = generator.DungeonFlow; 
            generator.Generate();
        }
     
    }
}