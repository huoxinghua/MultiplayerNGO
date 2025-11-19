using System.Collections.Generic;
using _Project.Code.Gameplay.Interactables.Network;
using _Project.Code.Network.RegisterNetObj;
using DunGen;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.Level
{
    public class DoorwayNetworkSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject _doorPrefab;
        private readonly List<NetworkObject> spawnedDoors = new();
        private RuntimeDungeon runtimeDungeon;

        private void Awake()
        {
            runtimeDungeon = GetComponent<RuntimeDungeon>();
        }

        public void SpawnDoors(RuntimeDungeon dungeonOverride)
        {
            if (!IsServer || dungeonOverride == null)
                return;

            var spawnPoints = FindObjectsOfType<DoorDummySpawnPoint>();

            foreach (var sp in spawnPoints)
            {
                
                var door = Instantiate(_doorPrefab);

                door.transform.SetPositionAndRotation(
                    sp.transform.position,
                    sp.transform.rotation
                );
                var doorNetObj = door.GetComponent<NetworkObject>();
             
                doorNetObj.Spawn();

               
                Destroy(sp.gameObject);
            }

        }
    }
}