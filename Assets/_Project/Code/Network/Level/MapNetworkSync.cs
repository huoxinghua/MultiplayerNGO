using DunGen;
using Unity.Netcode;
using UnityEngine;

public class MapNetworkSync : NetworkBehaviour
{
    [SerializeField] private DungeonGenerator generator;

    private NetworkVariable<int> seed = new NetworkVariable<int>(
      default,
      NetworkVariableReadPermission.Everyone,
      NetworkVariableWritePermission.Server
  );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int randomSeed = Random.Range(0, int.MaxValue);
            seed.Value = randomSeed;

            generator.Seed = randomSeed;         
            generator.Generate();
        }
        else
        {
            seed.OnValueChanged += (_, newSeed) =>
            {
                generator.Seed = newSeed;         
                generator.Generate();
            };
        }
    }
}


